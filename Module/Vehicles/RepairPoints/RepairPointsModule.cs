using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR.Module.Vehicles.RepairPoints
{
    public class RepairPointsModule : SqlModule<RepairPointsModule, RepairPoint, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `repairpoints`;";
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if(key == Key.E && dbPlayer != null && dbPlayer.IsValid() && dbPlayer.RageExtension.IsInVehicle)
            {
                RepairPoint repairPoint = RepairPointsModule.Instance.GetAll().Values.Where(rp => rp.Position.DistanceTo(dbPlayer.Player.Position) < 4.0f).FirstOrDefault();

                if (repairPoint == null) return false;

                var sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVehicle == null || !sxVehicle.IsValid()) return true;

                if (sxVehicle.Data == null || (sxVehicle.Data.ClassificationId != 8 && sxVehicle.Data.ClassificationId != 9))
                {
                    dbPlayer.SendNewNotification("Du kannst nur Helikopter und Flugzeuge hier reparieren!");
                    return false;
                }

                // Fahrzeug keine Frakkarre oder nicht Fahrer oder keine Berechtigung
                if (!sxVehicle.IsTeamVehicle() ||  sxVehicle.teamid != dbPlayer.TeamId || dbPlayer.Player.VehicleSeat != 0 || !repairPoint.Teams.Contains(dbPlayer.TeamId)) return true;

                if (dbPlayer.CanInteract() && sxVehicle.CanInteract)
                {
                    Task.Run(async () =>
                    {

                        Chats.sendProgressBar(dbPlayer, (5000));

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);
                        sxVehicle.CanInteract = false;
                        sxVehicle.SyncExtension.SetEngineStatus(false);

                        await NAPI.Task.WaitForMainThread(5000);

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        sxVehicle.CanInteract = true;
                        sxVehicle.SyncExtension.SetEngineStatus(true);

                        sxVehicle.Repair();
                        dbPlayer.SendNewNotification("Fahrzeug erfolgreich repariert!");
                        return;
                    });
                }


            }

            return false;
        }
    }

    public class RepairPoint : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
   
        public HashSet<uint> Teams { get; set; }

        public RepairPoint(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));

            var teamString = reader.GetString("teams");

            Teams = new HashSet<uint>();
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId) || teamId == 0 || Teams.Contains(teamId)) continue;
                    Teams.Add(teamId);
                }
            }

            PlayerNotifications.Instance.Add(Position,
                "Werkstatt",
                "Hier kannst du dein Fahrzeug reparieren, nutze `E`!");

            Markers.CreateSimple(22, new Vector3(Position.X, Position.Y, Position.Z + 1.5f), 1.0f, 255, 255, 0, 255, 0);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
