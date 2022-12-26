using GTANetworkAPI;
using System;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Houses.Menu
{
    public class VoltageMenuBuilder : MenuBuilder
    {
        public VoltageMenuBuilder() : base(PlayerMenu.VoltageMenu)
        {
        }

        public override Module.Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new Module.Menu.NativeMenu(Menu, "Stromkasten", "");

            menu.Add($"Schließen");
            menu.Add($"Messung durchführen");

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
                // Close menu
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else 
                {

                    HousesVoltage housesVoltage = HousesVoltageModule.Instance.GetAll().Values.ToList().Where(hv => hv.Position.DistanceTo(dbPlayer.Player.Position) < 5.0f).FirstOrDefault();
                    if (housesVoltage == null) return false;

                    if(index == 1)
                    {
                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            Chats.sendProgressBar(dbPlayer, 45000);

                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.SetData("userCannotInterrupt", true);

                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                            await NAPI.Task.WaitForMainThread(45000);

                            dbPlayer.SetData("userCannotInterrupt", false);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.ResetData("userCannotInterrupt");
                            dbPlayer.StopAnimation();
                            
                            if (housesVoltage.DetectedHouses.Count() > 0)
                            {
                                dbPlayer.SendNewNotification($"Folgende Häuser haben einen hohen Verbrauch aufgewiesen {string.Join(',', housesVoltage.DetectedHouses.ToList())}!");
                                housesVoltage.DetectedHouses.Clear();
                                return;
                            }
                            dbPlayer.SendNewNotification($"Es konnte kein auffälliger Verbrauch festgestellt werden!");
                        }));
                    }
                }

                return false;
            }
        }
    }
}
