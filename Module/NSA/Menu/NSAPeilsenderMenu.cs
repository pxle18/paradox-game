using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Telefon.App;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.NSA.Menu
{
    public class NSAPeilsenderMenuBuilder : MenuBuilder
    {
        public NSAPeilsenderMenuBuilder() : base(PlayerMenu.NSAPeilsenderMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "NSA Aktive Peilsender");
            l_Menu.Add($"Schließen");

            foreach (NSAPeilsender nSAPeilsender in NSAObservationModule.NSAPeilsenders)
            {
                l_Menu.Add($"{nSAPeilsender.Name}");
            }

            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                int i = 1;

                foreach (NSAPeilsender nSAPeilsender in NSAObservationModule.NSAPeilsenders)
                {
                    if(i == index)
                    {
                        if(nSAPeilsender.VehicleId != 0)
                        {
                            SxVehicle sxVeh = VehicleHandler.Instance.GetByVehicleDatabaseId(nSAPeilsender.VehicleId);
                            if (sxVeh == null || !sxVeh.IsValid()) return true;

                            if (dbPlayer.HasData("nsaOrtung"))
                            {
                                dbPlayer.ResetData("nsaOrtung");
                            }

                            dbPlayer.SetData("nsaPeilsenderOrtung", (uint)nSAPeilsender.VehicleId);

                            // Orten
                            dbPlayer.Player.TriggerNewClient("setPlayerGpsMarker", sxVeh.entity.Position.X, sxVeh.entity.Position.Y);
                            dbPlayer.SendNewNotification("Peilsender geortet!");
                            return true;
                        }
                        
                        return true;
                    }
                    i++;
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
