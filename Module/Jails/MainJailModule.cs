using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Jails
{
    public class MainJailModule : SqlModule<MainJailModule, SportItem, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `staatsg_sportitems`;";
        }

        public Dictionary<SportItem, DbPlayer> SportItemUsed = new Dictionary<SportItem, DbPlayer>();

        protected override bool OnLoad()
        {
            return base.OnLoad();
        }

        protected override void OnItemLoaded(SportItem u)
        {
            if (SportItemUsed == null)
            {
                SportItemUsed = new Dictionary<SportItem, DbPlayer>();
            }

            SportItemUsed.Add(u, null);
        }


        public override void OnMinuteUpdate()
        {
            foreach (KeyValuePair<SportItem, DbPlayer> kvp in SportItemUsed.ToList())
            {
                if (kvp.Value != null && kvp.Value.IsValid())
                {
                    if(kvp.Value.jailtime[0] > 5)
                    {
                        if(kvp.Value.jailtimeReducing[0] > 0)
                        {
                            int actualdata = 1;
                            if (kvp.Value.HasData("sgtraining"))
                            {
                                actualdata = kvp.Value.GetData("sgtraining");
                            }
                            actualdata++;
                            kvp.Value.SetData("sgtraining", actualdata);


                            kvp.Value.jailtime[0]--;
                            kvp.Value.jailtimeReducing[0]--;

                            if (actualdata >= 5)
                            {
                                kvp.Value.SetData("sgtrainingLast", DateTime.Now);
                                kvp.Value.ResetData("sgtraining");
                                kvp.Value.SendNewNotification("Du musst dich kurz erholen, bevor du weiter trainieren kannst. (2 Minuten)");
                                kvp.Value.StopSGTraining(kvp.Key);
                                continue;
                            }
                        }
                    }
                    else
                    {
                        kvp.Value.SendNewNotification("Du solltest dich auf deine Entlassung nun vorbereiten!");
                        kvp.Value.StopSGTraining(kvp.Key);
                        continue;
                    }
                }
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.RageExtension.IsInVehicle || key != Key.E) return false;


            // First search for player in Interaction
            SportItem sportItem = SportItemUsed.ToList().Where(i => i.Value == dbPlayer).FirstOrDefault().Key;
            if (sportItem != null)
            {
                if (SportItemUsed[sportItem] != null && SportItemUsed[sportItem].IsValid() && SportItemUsed[sportItem] == dbPlayer)
                {
                    dbPlayer.StopSGTraining(sportItem);
                    return true;
                }
            }


            // InteractionItem
            sportItem = Instance.GetAll().Values.Where(ts => ts.Position.DistanceTo(dbPlayer.Player.Position) < 1.5f &&
            (!SportItemUsed.ContainsKey(ts) || Instance.SportItemUsed[ts] == null)).FirstOrDefault();

            if (sportItem != null)
            {
                if (!SportItemUsed.ContainsKey(sportItem)) return false;

                if (!dbPlayer.CanInteract()) return false;

                if (dbPlayer.HasData("sgtrainingLast"))
                {
                    DateTime trainingLast = dbPlayer.GetData("sgtrainingLast");
                    if (trainingLast != null && trainingLast.AddMinutes(2) > DateTime.Now)
                    {
                        dbPlayer.SendNewNotification("Du bist dich noch am erholen, warte kurz bevor du weiter trainierst!");
                        return true;
                    }
                    else dbPlayer.ResetData("sgtrainingLast");
                }

                if(dbPlayer.jailtime[0] < 5)
                {
                    dbPlayer.SendNewNotification("Du solltest dich auf deine Entlassung nun vorbereiten!");
                    return true;
                }

                dbPlayer.StartSGTraining(sportItem);

                return true;
            }
            return false;
        }
    }

    public static class MainJailModulePlayerExtension
    {
        public static SportItem GetPlayerSGSportsItem(this DbPlayer dbPlayer)
        {
            foreach (KeyValuePair<SportItem, DbPlayer> kvp in MainJailModule.Instance.SportItemUsed.ToList())
            {
                if(kvp.Value != null && kvp.Value == dbPlayer)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        public static void StartSGTraining(this DbPlayer dbPlayer, SportItem sportItem)
        {
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            dbPlayer.SetData("userCannotInterrupt", true);

            MainJailModule.Instance.SportItemUsed[sportItem] = dbPlayer;

            Task.Run(async () =>
            {

                dbPlayer.Player.SetPosition(sportItem.Position);
                dbPlayer.Player.SetRotation(sportItem.Heading);

                await NAPI.Task.WaitForMainThread(400);

                // first
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), sportItem.Anim1, sportItem.Anim2);

            });

            //Nutrition
            dbPlayer.food[0] -= 1;
            dbPlayer.drink[0] -= 2;
            Nutrition.NutritionModule.Instance.PushNutritionToPlayer(dbPlayer);
        }

        public static void StopSGTraining(this DbPlayer dbPlayer, SportItem sportItem)
        {
            MainJailModule.Instance.SportItemUsed[sportItem] = null;

            dbPlayer.StopAnimation();
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.ResetData("userCannotInterrupt");

            dbPlayer.StopAnimation();
            return;
        }
    }


    public class SportItem : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }

        public string Anim1 { get; set; }
        public string Anim2 { get; set; }

        public SportItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");

            Anim1 = reader.GetString("anim1");
            Anim2 = reader.GetString("anim2");

            if (Configurations.Configuration.Instance.DevMode) Spawners.Markers.Create(1, Position, new Vector3(), new Vector3(), 0.7f, 255, 255, 0, 0);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

}
