using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Extensions;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Shops;
using VMP_CNR.Module.ShopTakeover.Models;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.ShopTakeover
{
    /**
     * This is part of the PARADOX Game-Rework.
     * Made by module@jabber.ru
     */
    public sealed class ShopTakeoverAttackModule : Module<ShopTakeoverAttackModule>
    {
        public Dictionary<uint, ShopTakeoverModel> ActiveTakeovers = new Dictionary<uint, ShopTakeoverModel>();

        public const int ShopTakeoverDimension = 5000;
        public const int TimeLimitInMinutes = 15;

        public override Type[] RequiredModules()
        {
            return new[] { typeof(ShopTakeoverModule) };
        }

        public void Attack(DbPlayer dbPlayer, ShopTakeoverModel shopTakeoverModel)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (shopTakeoverModel.IsInTakeover)
            {
                dbPlayer.SendNewNotification("Dieser Shop wird bereits übernommen.");
                return;
            }

            if (!shopTakeoverModel.CanAttacked())
            {
                dbPlayer.SendNewNotification("Der Übernahmeprozess kann aufgrund der 72-Stunden-Regel nicht gestartet werden. Bitte versuchen Sie es demnächst erneut.");
                return;
            }

            if(shopTakeoverModel.Team.Id == dbPlayer.Team.Id)
            {
                dbPlayer.SendNewNotification("öhm... WIESO WILLST DU DICH SELBST ANGREIFEN?! BIST DU DOOOF?????");
                return;
            }

            shopTakeoverModel.Marker = Markers.Create(
                (int)MarkerType.VerticalCylinder, shopTakeoverModel.Shop.Position, new Vector3(), new Vector3(), 50f,
                200, 0, 255, 255,
                ShopTakeoverDimension);

            shopTakeoverModel.ColShape = ColShapes.Create(shopTakeoverModel.Shop.Position, 50f * 1.2f);
            shopTakeoverModel.ColShape.SetData("attackShopTakeoverId", shopTakeoverModel.Id);

            shopTakeoverModel.IsInTakeover = true;
            shopTakeoverModel.Attacker = TeamModule.Instance[dbPlayer.Team.Id];
            shopTakeoverModel.LastRob = DateTime.Now;

            Instance.ActiveTakeovers.Add(shopTakeoverModel.Id, shopTakeoverModel);
        }

        public override void OnMinuteUpdate()
        {
            foreach (var shopTakeoverModel in ActiveTakeovers.Values)
            {
                shopTakeoverModel.Players.Values.ForEach(player => player.SendNewNotification("Du bist im ShopTakeover"));

                if (shopTakeoverModel.LastRob.AddMinutes(TimeLimitInMinutes) < DateTime.Now)
                    Surrender(shopTakeoverModel, "Zeitlimit überschritten.");

                var attackerPlayersInsideShop = shopTakeoverModel.Players.Values.Where(player => player.Player.Position.DistanceTo(shopTakeoverModel.Shop.Position) <= 15f);

                if (attackerPlayersInsideShop.Count() < 1)
                    Surrender(shopTakeoverModel, "Keiner der Angreifer im Shop.");
            }
        }

        public void Surrender(ShopTakeoverModel shopTakeoverModel, string finishReason = "Unbekannt.")
        {

        }

        public void Clean(ShopTakeoverModel shopTakeoverModel)
        {
            Instance.ActiveTakeovers.Remove(shopTakeoverModel.Id);

            shopTakeoverModel.Marker.Delete();
            shopTakeoverModel.Marker = null;

            shopTakeoverModel.ColShape.Delete();
            shopTakeoverModel.ColShape = null;

            foreach(var player in shopTakeoverModel.Players.Values)
            {
                player.SetDimension(0);
            }
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!colShape.TryData("attackShopTakeoverId", out uint shopTakeoverId)) return false;

            if (!dbPlayer.Team.IsGangster) return false;

            var shopTakeoverModel = ShopTakeoverModule.Instance.Get(shopTakeoverId);
            if (shopTakeoverModel == null) return false;

            if (shopTakeoverModel.TeamsCanAccess.FirstOrDefault(team => team.Id == dbPlayer.Team.Id) == null)
            {
                dbPlayer.SendNewNotification("DEV: ShopTakeover nicht zugreifbar (Team)");
                return false;
            }

            switch (colShapeState)
            {
                case ColShapeState.Enter:
                    dbPlayer.SetData("attackShopTakeoverId", shopTakeoverModel.Id);

                    dbPlayer.DimensionType[0] = DimensionTypes.ShopTakeover;
                    dbPlayer.SetDimension(ShopTakeoverDimension);

                    shopTakeoverModel.Players.TryAdd(dbPlayer.Id, dbPlayer);

                    dbPlayer.SendNewNotification("Enter ShopTakeover");
                    return false;
                case ColShapeState.Exit:
                    if (!dbPlayer.HasData("attackShopTakeoverId")) return false;
                    dbPlayer.ResetData("attackShopTakeoverId");

                    dbPlayer.DimensionType[0] = DimensionTypes.World;
                    dbPlayer.SetDimension(0);

                    shopTakeoverModel.Players.Remove(dbPlayer.Id);
                    dbPlayer.SendNewNotification("Leave ShopTakeover");

                    return false;
                default:
                    return false;
            }
        }
    }
}
