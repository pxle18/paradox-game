using GTANetworkAPI;
using System;
using VMP_CNR.Handler;
using VMP_CNR.Module.Freiberuf;
using VMP_CNR.Module.Freiberuf.Mower;
using VMP_CNR.Module.Government;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles.Data;
using VMP_CNR.Module.Vehicles.Garages;

namespace VMP_CNR.Module.Gangwar
{
    public class GangwarVehicleMenu : MenuBuilder
    {
        public GangwarVehicleMenu() : base(PlayerMenu.GangwarVehicleMenu) { }

        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            Menu.Menu menu = new Menu.Menu(Menu, "Gangwar - Fahrzeuge");

            menu.Add(GlobalMessages.General.Close());
            menu.Add("Waffenkits");
            menu.Add("Revolter");

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
                uint model;
                GangwarTown gangwarTown = GangwarTownModule.Instance.FindActiveByTeam(dbPlayer.Team);
                int garageID = dbPlayer.Team.Equals(gangwarTown.AttackerTeam) ? gangwarTown.AttackerGarageID : gangwarTown.DefenderGarageID;
                GangwarGarage gangwarGarage = GangwarGarageModule.Instance.GetGarageByID((uint) garageID);

                switch (index)
                {
                    case 0: return true;
                    case 1:
                        Menu.Menu menu = MenuManager.Instance.Build(PlayerMenu.GangwarWeaponMenu, dbPlayer);
                        menu.Show(dbPlayer);
                        return false;
                    case 2: 
                        model = 686; 
                        break;
                    default: 
                        return true;
                }

                GangwarGarageSpawn spawn = gangwarGarage.GetFreeSpawnPosition();
                if (spawn == null)
                {
                    dbPlayer.SendNewNotification("Kein freier Ausparkpunkt.");
                    return false;
                }
                else
                {
                    try
                    {

                        NAPI.Task.Run(async () =>
                        {

                            SxVehicle vehicle = VehicleHandler.Instance.CreateServerVehicle(model, true, spawn.Position, spawn.Heading, dbPlayer.Team.ColorId, dbPlayer.Team.ColorId,
                                GangwarModule.Instance.DefaultDimension, true, teamid: dbPlayer.TeamId, plate: dbPlayer.Team.ShortName);

                            while (vehicle == null || vehicle.Entity == null)
                            {
                                await NAPI.Task.WaitForMainThread(100);
                            }

                            if(vehicle != null) gangwarTown.Vehicles.Add(vehicle);
                        });
                    }
                    catch(Exception e)
                    {
                        Logging.Logger.Crash(e);
                    }
                }

                return true;
            }
        }
    }
}