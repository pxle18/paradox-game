using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Shops;
using VMP_CNR.Module.ShopTakeover.Models;

namespace VMP_CNR.Module.ShopTakeover
{
    /**
     * This is part of the Void Game-Rework.
     * Made by module@jabber.ru
     */
    public sealed class ShopTakeoverModule : SqlModule<ShopTakeoverModule, ShopTakeoverModel, uint>
    {
        public Dictionary<uint, ShopTakeoverModel> ActiveTakeovers = new Dictionary<uint, ShopTakeoverModel>();

        private readonly int MinimumAlliesInRange = 0;

        protected override string GetQuery() => "SELECT * FROM `shop_takeovers`;";

        public override Type[] RequiredModules()
        {
            return new[] { typeof(ShopModule) };
        }
        
        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.RageExtension.IsInVehicle) return false;

            if (!dbPlayer.TryData("shopTakeoverId", out uint shopTakeoverId)) return false;

            var shopTakeoverModel = Instance.Get(shopTakeoverId);
            if (shopTakeoverModel == null) return false;

            if (dbPlayer.Player.Position.DistanceTo(shopTakeoverModel.Shop.Position) > 5.0f)
                return false;

            /**
             * Shop-Takeover Handling
             */

            if (dbPlayer.Player.CurrentWeapon != WeaponHash.Pistol && dbPlayer.Player.CurrentWeapon != WeaponHash.Pistol50 && dbPlayer.Player.CurrentWeapon != WeaponHash.Combatpistol)
                return false;

            var alliedPlayersInRange = Players.Players.Instance.GetPlayersInRange(shopTakeoverModel.Shop.Position, 15)
                .Where(player => player.Team.Id == dbPlayer.Team.Id);

            if(alliedPlayersInRange.Count() < MinimumAlliesInRange)
            {
                dbPlayer.Team.SendNotificationInRange($"Der Übernahmeprozess auf Shop {shopTakeoverModel.Name} kann aufgrund mangelnder Verbündete in Reichweite nicht gestartet werden. Bitte versuchen Sie es erneut.",
                    shopTakeoverModel.Shop.Position, 20, (int)7.5 * 1000);

                return false;
            }

            Menu.MenuManager.Instance.Build(Menu.PlayerMenu.ShopTakeoverAttackMenu, dbPlayer).Show(dbPlayer);
            
            return true;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!colShape.TryData("shopId", out uint shopId)) return false;

            if (!dbPlayer.Team.IsGangster) return false;

            var shopModel = ShopModule.Instance.Get(shopId);
            if (shopModel == null) return false;

            var shopTakeoverModel = GetAll().Values.FirstOrDefault(shopTakeover => shopTakeover.Shop.Id == shopModel.Id);
            if (shopTakeoverModel == null) return false;

            if (dbPlayer.Player.CurrentWeapon != WeaponHash.Pistol && dbPlayer.Player.CurrentWeapon != WeaponHash.Pistol50 && dbPlayer.Player.CurrentWeapon != WeaponHash.Combatpistol)
                return false;

            switch (colShapeState)
            {
                case ColShapeState.Enter:
                    dbPlayer.SetData("shopTakeoverId", shopTakeoverModel.Id);

                    return false;
                case ColShapeState.Exit:
                    if (!dbPlayer.HasData("shopTakeoverId")) return false;
                    dbPlayer.ResetData("shopTakeoverId");

                    return false;
                default:
                    return false;
            }
        }
    }
}
