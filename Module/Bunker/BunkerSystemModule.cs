using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMP_CNR.Module.Bunker.Menu;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Schwarzgeld;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Bunker
{
    public class BunkerSystemModule : Module<BunkerSystemModule>
    {
        public static bool deactivated = true;

        public override Type[] RequiredModules()
        {
            return new[] { typeof(BunkerModule), typeof(StaticContainerModule), typeof(ItemModelModule), typeof(JumpPointModule) };
        }

        protected override bool OnLoad()
        {

            // Add Menus

            MenuManager.Instance.AddBuilder(new BunkerDealerSellMenu());
            MenuManager.Instance.AddBuilder(new BunkerRessourceBuyMenu());

            Bunker bunker = BunkerModule.Instance.GetAll().FirstOrDefault().Value;
            if (bunker != null)
            {
                // Load Containers
                bunker.BunkerBlackMoneyContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.BUNKERBLACKMONEY);
                bunker.BunkerRessourceOrderContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.BUNKERRESSOURCEORDER);
                                
                // Remove other Jumppoints
                foreach (KeyValuePair<int, JumpPoint> kvp in JumpPointModule.Instance.jumpPoints.ToList().Where(jp => jp.Value.DestinationId == bunker.jpExitId && jp.Value.Id != bunker.jpEnterId))
                {
                    NAPI.Task.Run(() =>
                    {
                        JumpPoint jumpPoint = kvp.Value;

                        if (jumpPoint != null)
                        {
                            if (jumpPoint.ColShape != null)
                            {
                                jumpPoint.ColShape.ResetData("jumpPointId");
                                NAPI.ColShape.DeleteColShape(jumpPoint.ColShape);
                            }
                            /*if (jumpPoint.Object != null)
                            {
                                jumpPoint.Object.Delete();
                            }*/
                            JumpPointModule.Instance.jumpPoints.Remove(kvp.Key);
                        }
                    });
                }

                
                bunker.jpEnter = JumpPointModule.Instance.Get(bunker.jpEnterId);
                bunker.jpExit = JumpPointModule.Instance.Get(bunker.jpExitId);

                bunker.jpEnter.Locked = false;
                bunker.jpExit.Locked = false;
                bunker.jpEnter.DestinationId = bunker.jpExit.Id;
                bunker.jpEnter.Destination = bunker.jpExit;

                bunker.jpExit.DestinationId = bunker.jpEnter.Id;
                bunker.jpExit.Destination = bunker.jpEnter;

                NAPI.Task.Run(() =>
                {
                    NAPI.Marker.CreateMarker(25, (BunkerModule.BunkerBlackMoneyWithdraw - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, BunkerModule.BunkerDimension);
                    NAPI.Marker.CreateMarker(25, (BunkerModule.BunerDealerSellMenu - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, BunkerModule.BunkerDimension);
                    NAPI.Marker.CreateMarker(25, (BunkerModule.BlackMoneyContainer - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, BunkerModule.BunkerDimension);
                    NAPI.Marker.CreateMarker(25, (BunkerModule.RessourceOrderContainer - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, BunkerModule.BunkerDimension);

                    // Load Ipls
                    NAPI.World.RequestIpl(bunker.IPL);
                });

            }
            return base.OnLoad();
        }

        public override void OnMinuteUpdate()
        {
            if (deactivated) return;

            Bunker bunker = BunkerModule.Instance.GetAll().FirstOrDefault().Value;
            if (bunker == null) return;

            bunker.CheckControllers();

            foreach (BunkerOrder bunkerOrder in BunkerModule.Instance.RessourceOrders.ToList())
            {
                if (bunkerOrder == null) continue;
                if (bunkerOrder.OrderDate.AddMinutes(5) < DateTime.Now)
                {
                    if (bunker.BunkerRessourceOrderContainer.Container.CanInventoryItemAdded(bunkerOrder.ItemId, bunkerOrder.Amount))
                    {
                        bunker.BunkerRessourceOrderContainer.Container.AddItem(bunkerOrder.ItemId, bunkerOrder.Amount);
                        bunkerOrder.Amount = 0;
                    }
                }
            }

            BunkerModule.Instance.RessourceOrders.RemoveAll(isAmount);

        }

        private bool isAmount(BunkerOrder order)
        {
            return order.Amount == 0;
        }

        public override void OnFiveMinuteUpdate()
        {
            if (deactivated) return;

            // Schwarzgeld waschen
            Bunker bunker = BunkerModule.Instance.GetAll().FirstOrDefault().Value;
            if (bunker == null) return;

            if (BunkerModule.Instance.ActualChestSellings > 0) BunkerModule.Instance.ActualChestSellings = 0;

            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                if (bunker.OwnerTeamId == 0) return;

                // Check BankNotes
                if (bunker.BunkerBlackMoneyContainer.Container.GetItemAmount(SchwarzgeldModule.SchwarzgeldId) > 0)
                {
                    // Amount to wash 200k - 5 min
                    var l_Amount = bunker.BunkerBlackMoneyContainer.Container.GetItemAmount(SchwarzgeldModule.SchwarzgeldId);
                    if (l_Amount > 200000)
                        l_Amount = 200000;

                    bunker.BunkerBlackMoneyContainer.Container.RemoveItem(SchwarzgeldModule.SchwarzgeldId, l_Amount);

                    int realmoney = (l_Amount / 100) * 90; // 0,9 Kurs

                    bunker.BlackMoneyAmount += realmoney;
                    bunker.SaveBlackMoney();
                }
            }));
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (deactivated) return false;

            if (key != Key.E || dbPlayer.RageExtension.IsInVehicle || dbPlayer.Player.Dimension != 100) return false;

            if (dbPlayer.Dimension[0] == BunkerModule.BunkerDimension && dbPlayer.Player.Position.DistanceTo(BunkerModule.BunkerBlackMoneyWithdraw) < 2.0f)
            {
                Bunker bunker = BunkerModule.Instance.GetAll().FirstOrDefault().Value;
                if (bunker == null || !bunker.IsControlledByTeam(dbPlayer.TeamId)) return false;

                // Withdraw
                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Gelddruckmaschine", Callback = "BlackmoneyWithdrawBunker", Message = $"Aktueller Betrag {bunker.BlackMoneyAmount}$, wie viel möchten Sie entnehmen:" });
                return true;
            }

            if (dbPlayer.Dimension[0] == BunkerModule.BunkerDimension && dbPlayer.Player.Position.DistanceTo(BunkerModule.BunerDealerSellMenu) < 2.0f)
            {
                Bunker bunker = BunkerModule.Instance.GetAll().FirstOrDefault().Value;
                if (bunker == null || !bunker.IsControlledByTeam(dbPlayer.TeamId)) return false;


                MenuManager.Instance.Build(PlayerMenu.BunkerDealerSellMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            if (dbPlayer.Dimension[0] == BunkerModule.BunkerDimension && dbPlayer.Player.Position.DistanceTo(BunkerModule.RessourceOrderMenu) < 2.0f)
            {
                Bunker bunker = BunkerModule.Instance.GetAll().FirstOrDefault().Value;
                if (bunker == null || !bunker.IsControlledByTeam(dbPlayer.TeamId)) return false;


                MenuManager.Instance.Build(PlayerMenu.BunkerRessourceBuyMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            return false;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandbsetowner(Player player, string arg)
        {
            DbPlayer dbPlayer = player.GetPlayer();

            if (!Configurations.Configuration.Instance.DevMode)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!uint.TryParse(arg, out uint id)) return;
            Bunker bunker = BunkerModule.Instance.GetAll().FirstOrDefault().Value;
            if (bunker == null) return;

            if (id == 0)
            {
                bunker.OwnerTeamId = 0;
                bunker.IsOwnerSetted = false;
                dbPlayer.SendNewNotification("Bunker Owner resettet!");
            }
            else
            {
                bunker.IsOwnerSetted = true;

                bunker.SetOwnerTeam(id);

                dbPlayer.SendNewNotification($"Bunker Owner ist nun Team {id}!");
            }
            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandgotobunker(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();

            if (!Configurations.Configuration.Instance.DevMode)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            Bunker bunker = BunkerModule.Instance.GetAll().FirstOrDefault().Value;
            if (bunker == null) return;

            dbPlayer.Player.SetPosition(bunker.jpExit.Position);
            dbPlayer.SetDimension(bunker.jpExit.Dimension);
            return;
        }

    }
}
