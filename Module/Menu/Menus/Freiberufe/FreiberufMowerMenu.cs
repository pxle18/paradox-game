using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Freiberuf;
using VMP_CNR.Module.Freiberuf.Mower;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR
{
    public class FreiberufMowerMenuBuilder : MenuBuilder
    {
        public FreiberufMowerMenuBuilder() : base(PlayerMenu.FreiberufMowerMenu)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Freiberuf Rasenarbeiten");
            menu.Add("Arbeit starten");
            menu.Add("Rückgabe");
            menu.Add(MSG.General.Close());
            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                switch (index)
                {
                    case 0:
                        if (MowerModule.PlayersInJob.Contains(dbPlayer))
                        {
                            dbPlayer.SendNewNotification("Job wurde bereits gestartet!");
                            break;
                        }

                        dbPlayer.RemoveJobVehicleIfExist(MowerModule.MowerJobVehMarkId);

                        if(!dbPlayer.IsJobVehicleAtPoint(MowerModule.MowerGetPoint))
                        {
                            NAPI.Task.Run(async () =>
                            {
                                // Spawning Vehicle
                                SxVehicle xVeh = VehicleHandler.Instance.CreateServerVehicle(VehicleDataModule.Instance.GetData((uint)VehicleHash.Mower).Id, false,
                                    MowerModule.MowerSpawnPoint, MowerModule.MowerSpawnRotation, Main.rndColor(),
                                    Main.rndColor(), 0, true, true, false, 0, dbPlayer.GetName(), 0, MowerModule.MowerJobVehMarkId, dbPlayer.Id);

                                while (xVeh.entity == null)
                                {
                                    await NAPI.Task.WaitForMainThread(100);
                                }

                                xVeh.entity.SetData<int>("loadage", 0);
                                MowerModule.PlayersInJob.Add(dbPlayer);
                                dbPlayer.SendNewNotification("Ihr Fahrzeug steht bereit, maehen sie den Rasen!");

                                return;
                            });
                        }
                        break;
                    case 1:
                        SxVehicle sxVehicle = dbPlayer.GetJobVehicle(MowerModule.MowerJobVehMarkId);
                        if(sxVehicle != null)
                        {
                            if(!sxVehicle.entity.HasData("loadage"))
                            {
                                dbPlayer.SendNewNotification($"Du hast keinen Rasenschnitt in deinem Mäher, daher gibts auch kein Geld!");
                                MowerModule.PlayersInJob.Remove(dbPlayer);
                                VehicleHandler.Instance.DeleteVehicle(sxVehicle, false);
                                return true;
                            }

                            int loadage = sxVehicle.entity.GetData<int>("loadage");
                            int verdienst = loadage * 10;

                            VehicleHandler.Instance.DeleteVehicle(sxVehicle, false);
                            dbPlayer.GiveMoney(verdienst);
                            dbPlayer.SendNewNotification("Sie wurden fuer Ihre Arbeit mit 9$/kg Schnitt belohnt!");
                            dbPlayer.SendNewNotification($"Verdienst: {verdienst}$");
                            MowerModule.PlayersInJob.Remove(dbPlayer);
                        }
                        break;
                    default:
                        MenuManager.DismissCurrent(dbPlayer);
                        break;
                }

                return true;
            }
        }
    }
}