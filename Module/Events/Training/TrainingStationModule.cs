using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.Nutrition;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Events.Training
{
    public sealed class TrainingstationModule : SqlModule<TrainingstationModule, Trainingstation, uint>
    {

        protected override string GetQuery()
        {
            return "SELECT * FROM `trainingstations`;";
        }

        public Dictionary<Trainingstation, DbPlayer> TrainingsInUse = new Dictionary<Trainingstation, DbPlayer>();

        protected override void OnItemLoaded(Trainingstation u)
        {
            if(TrainingsInUse == null)
            {
                TrainingsInUse = new Dictionary<Trainingstation, DbPlayer>();
            }

            TrainingsInUse.Add(u, null);
        }

        public override void OnTenSecUpdate()
        {
            if (TrainingsInUse.Count <= 0) return;

            foreach(KeyValuePair<Trainingstation, DbPlayer> kvp in TrainingsInUse.ToList())
            {
                if(kvp.Value != null && kvp.Value.IsValid())
                {
                    int actualdata = 1;
                    if(kvp.Value.HasData("training"))
                    {
                        actualdata = kvp.Value.GetData("training");
                    }
                    kvp.Value.SetData("training", actualdata + 1);

                    if(!kvp.Value.GiveCWS(CWSTypes.Training, 8))
                    {
                        kvp.Value.SendNewNotification("Du hast deine heutigen Trainings-Punkte bereits erreicht!");
                        kvp.Value.StopTraining(kvp.Key);
                        continue;
                    }

                    if(kvp.Value.HasData("training") && kvp.Value.GetData("training") >= 24)
                    {
                        kvp.Value.SetData("trainingLast", DateTime.Now);
                        kvp.Value.ResetData("training");
                        kvp.Value.SendNewNotification("Du musst dich kurz erholen, bevor du weiter trainieren kannst. (1 Minute)");
                        kvp.Value.StopTraining(kvp.Key);
                    }
                }
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.RageExtension.IsInVehicle || key != Key.E) return false;

            // Trainingsstation
            Trainingstation trainingstation = TrainingstationModule.Instance.GetAll().Values.Where(ts => ts.Position.DistanceTo(dbPlayer.Player.Position) < 1.5f).FirstOrDefault();

            if (trainingstation != null)
            {
                if(dbPlayer.HasData("trainingLast"))
                {
                    DateTime trainingLast = dbPlayer.GetData("trainingLast");
                    if (trainingLast != null && trainingLast.AddMinutes(1) > DateTime.Now)
                    {
                        dbPlayer.SendNewNotification("Du bist dich noch am erholen, warte kurz bevor du weiter trainierst!");
                        return true;
                    }
                    else dbPlayer.ResetData("trainingLast");
                }

                if (!TrainingsInUse.ContainsKey(trainingstation)) return false;

                if (TrainingsInUse[trainingstation] != null && TrainingsInUse[trainingstation].IsValid())
                {

                    if (TrainingsInUse[trainingstation] == dbPlayer)
                    {
                        dbPlayer.StopTraining(trainingstation);
                        return true;
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Diese Trainingsstation wird bereits benutzt!");
                        return false;
                    }
                }

                if (dbPlayer.CapReached(CWSTypes.Training))
                {
                    dbPlayer.SendNewNotification("Du solltest deine Muskeln für heute erholen lassen, komm morgen wieder!");
                    return true;
                }

                if (!dbPlayer.CanInteract()) return false;
                dbPlayer.StartTraining(trainingstation);
                return true;
            }
            return false;
        }
    }

    public static class TrainingsPlayerExtension
    {
        public static void StartTraining(this DbPlayer dbPlayer, Trainingstation trainingstation)
        {
            TrainingstationModule.Instance.TrainingsInUse[trainingstation] = dbPlayer;

            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetData("userCannotInterrupt", true);
            Task.Run(async () =>
            {
                dbPlayer.Player.SetPosition(trainingstation.Position);
                dbPlayer.Player.SetRotation(trainingstation.Heading);

                await Task.Delay(500);

                dbPlayer.StopAnimation();
                await Task.Delay(500);

                dbPlayer.Player.SetPosition(trainingstation.Position);
                dbPlayer.Player.SetRotation(trainingstation.Heading);

                await Task.Delay(1500);

                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), trainingstation.Anim1, trainingstation.Anim2);
            });
            dbPlayer.SendNewNotification($"Training gestartet! ({trainingstation.Name})");

            //Nutrition
            dbPlayer.food[0] -= 1;
            dbPlayer.drink[0] -= 2;
            NutritionModule.Instance.PushNutritionToPlayer(dbPlayer);
        }

        public static void StopTraining(this DbPlayer dbPlayer, Trainingstation trainingstation)
        {
            TrainingstationModule.Instance.TrainingsInUse[trainingstation] = null;

            dbPlayer.StopAnimation();
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.ResetData("userCannotInterrupt");

            dbPlayer.SendNewNotification("Training beendet!");
            dbPlayer.StopAnimation();

        }
    }

    public class Trainingstation : Loadable<uint>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public string Anim1 { get; set; }
        public string Anim2 { get; set; }

        public Trainingstation(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            Anim1 = reader.GetString("anim1");
            Anim2 = reader.GetString("anim2");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
