using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Injury.InjuryMove.Menu
{
    public class InjuryMoveMenuBuilder : MenuBuilder
    {
        public InjuryMoveMenuBuilder() : base(PlayerMenu.InjuryMoveMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            if (!p_DbPlayer.HasData("playerToMove")) return null;

            var l_Menu = new Module.Menu.Menu(Menu, "Krankentransport");
            l_Menu.Add($"Schließen");

            InjuryMovePoint actualMovePoint = InjuryMoveModule.Instance.GetAll().Values.Where(ip => ip.Position.DistanceTo(p_DbPlayer.Player.Position) < 2.0f && ip.Dimension == p_DbPlayer.Player.Dimension).FirstOrDefault();

            if (actualMovePoint == null) return null;

            foreach (InjuryMovePoint injuryMovePoint in InjuryMoveModule.Instance.GetAll().Values.Where(i => i.Grouping == actualMovePoint.Grouping).ToList())
            {
                l_Menu.Add(injuryMovePoint.Name);
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
                if (!dbPlayer.HasData("playerToMove")) return true;

                DbPlayer target = Players.Players.Instance.GetByDbId(dbPlayer.GetData("playerToMove"));
                if (target == null || !target.IsValid()) return true;

                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else
                {
                    int idx = 1;

                    InjuryMovePoint actualMovePoint = InjuryMoveModule.Instance.GetAll().Values.Where(ip => ip.Position.DistanceTo(dbPlayer.Player.Position) < 2.0f && ip.Dimension == dbPlayer.Player.Dimension).FirstOrDefault();

                    if (actualMovePoint == null) return true;

                    foreach (InjuryMovePoint injuryMovePoint in InjuryMoveModule.Instance.GetAll().Values.Where(i => i.Grouping == actualMovePoint.Grouping).ToList())
                    {
                        if(idx == index)
                        {
                            if (Players.Players.Instance.GetValidPlayers().Where(p => p.IsInjured() && p.Player.Position.DistanceTo(injuryMovePoint.Position) < 1.2f).Count() > 0)
                            {
                                dbPlayer.SendNewNotification("Dieses Bett ist bereits in Benutzung!");
                                return true;
                            }

                            // Deliver him to Desk
                            target.Player.SetPosition(injuryMovePoint.Position);
                            target.Player.SetRotation(injuryMovePoint.Heading);

                            target.SetDimension(injuryMovePoint.Dimension);

                            // Resett time in KH and redo pos save
                            target.dead_x[0] = target.Player.Position.X;
                            target.dead_y[0] = target.Player.Position.Y;
                            target.dead_z[0] = target.Player.Position.Z;
                            target.deadtime[0] = 0;

                            target.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "combat@damage@rb_writhe", "rb_writhe_loop");

                            Task.Run(async () =>
                            {
                                await Task.Delay(2000);

                                // Deliver him to Desk

                                NAPI.Task.Run(() => { target.Player.SetRotation(injuryMovePoint.Heading); });
                                await Task.Delay(500);

                                target.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "combat@damage@rb_writhe", "rb_writhe_loop");
                            });

                            dbPlayer.ResetData("playerToMove");
                            return true;
                        }
                        idx++;
                    }
                }
                return true;
            }
        }
    }
}
