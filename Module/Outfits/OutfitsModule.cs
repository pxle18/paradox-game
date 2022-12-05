using GTANetworkAPI;
using System;
using System.Linq;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Space;

namespace VMP_CNR.Module.Outfits
{
    public enum OutfitTypes
    {
        Cocain = 73,
        Jail = 67,
    }

    public class OutfitsModule : Module<OutfitsModule>
    {
        public uint GetPropValue(uint id, bool prop = false)
        {
            return prop ? id*1000 : id * 50000;
        }

        public int GetOutfitComponentTexture(DbPlayer dbPlayer, int outfitId, int SlotId)
        {
            Player player = dbPlayer.Player;
            Outfit outfit = OutfitModule.Instance.GetAll().Values.FirstOrDefault(o => ((PedHash)player.Model == PedHash.FreemodeMale01 ? o.DataId : o.DataId - 1) == outfitId && o.Male == ((PedHash)player.Model == PedHash.FreemodeMale01) ? true : false);
            if (outfit == null) return 0;

            OutfitComponent outfitComponent = outfit.Components.Where(ic => ic.Slot == SlotId).FirstOrDefault();
            if (outfitComponent == null) return 0;

            return outfitComponent.Texture;
        }

        public int GetOutfitPropTexture(DbPlayer dbPlayer, int outfitId, int SlotId)
        {
            Player player = dbPlayer.Player;
            Outfit outfit = OutfitModule.Instance.GetAll().Values.FirstOrDefault(o => ((PedHash)player.Model == PedHash.FreemodeMale01 ? o.DataId : o.DataId - 1) == outfitId && o.Male == ((PedHash)player.Model == PedHash.FreemodeMale01) ? true : false);
            if (outfit == null) return 0;

            OutfitProp outfitComponent = outfit.Props.Where(ic => ic.Slot == SlotId).FirstOrDefault();
            if (outfitComponent == null) return 0;

            return outfitComponent.Texture;
        }

        public int GetOutfitComponentVariation(DbPlayer dbPlayer, int outfitId, int SlotId)
        {
            Player player = dbPlayer.Player;
            Outfit outfit = OutfitModule.Instance.GetAll().Values.FirstOrDefault(o => ((PedHash)player.Model == PedHash.FreemodeMale01 ? o.DataId : o.DataId - 1) == outfitId && o.Male == ((PedHash)player.Model == PedHash.FreemodeMale01) ? true : false);
            if (outfit == null) return 0;

            OutfitComponent outfitComponent = outfit.Components.Where(ic => ic.Slot == SlotId).FirstOrDefault();
            if (outfitComponent == null) return 0;

            return outfitComponent.Component;
        }

        public int GetOutfitPropVariation(DbPlayer dbPlayer, int outfitId, int SlotId)
        {
            Player player = dbPlayer.Player;
            Outfit outfit = OutfitModule.Instance.GetAll().Values.FirstOrDefault(o => ((PedHash)player.Model == PedHash.FreemodeMale01 ? o.DataId : o.DataId - 1) == outfitId && o.Male == ((PedHash)player.Model == PedHash.FreemodeMale01) ? true : false);
            if (outfit == null) return 0;

            OutfitProp outfitComponent = outfit.Props.Where(ic => ic.Slot == SlotId).FirstOrDefault();
            if (outfitComponent == null) return 0;

            return outfitComponent.Component;
        }

        public void SetPlayerOutfit(DbPlayer dbPlayer, int outfitId, bool overwrite = false)
        {
            Player player = dbPlayer.Player;

            // find outfit by id and actual gender
            Outfit outfit = OutfitModule.Instance.GetAll().Values.FirstOrDefault(o => ((PedHash)player.Model == PedHash.FreemodeMale01 ? o.DataId : o.DataId-1) == outfitId && o.Male == ((PedHash)player.Model == PedHash.FreemodeMale01) ? true: false);
            if (outfit == null) return;

            if(overwrite) dbPlayer.Character.Clothes.Clear();

            foreach(OutfitComponent kvp in outfit.Components)
            {
                dbPlayer.SetClothes(kvp.Slot, kvp.Component, kvp.Texture);
                if(overwrite) dbPlayer.Character.Clothes.Add(kvp.Slot, GetPropValue(kvp.Id));
            }

            NAPI.Task.Run(() =>
            {
                if (dbPlayer == null)
                    return;

                // clear all
                player.ClearAccessory(0);
                player.ClearAccessory(1);
                player.ClearAccessory(2);
                player.ClearAccessory(6);
                player.ClearAccessory(7);
                if (overwrite) dbPlayer.Character.EquipedProps.Clear();

                foreach (OutfitProp kvp in outfit.Props)
                {
                    player.ClearAccessory(kvp.Slot);
                    player.SetAccessories(kvp.Slot, kvp.Component, kvp.Texture);
                    if (overwrite) dbPlayer.Character.EquipedProps.Add(kvp.Slot, GetPropValue(kvp.Id, true));
                }

                // Fix Hair
                // Set Hair
                dbPlayer.SetClothes(2, dbPlayer.Customization.Hair.Hair, 0);
                NAPI.Player.SetPlayerHairColor(player, dbPlayer.Customization.Hair.Color, dbPlayer.Customization.Hair.HighlightColor);

                dbPlayer.SetData("outfitactive", outfitId);
            });
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandoutfit(Player player, string outfitstring)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            if (!dbPlayer.IsValid()) return;

            if (!Int32.TryParse(outfitstring, out int outfitId)) return;

            SetPlayerOutfit(dbPlayer, outfitId);
            dbPlayer.SendNewNotification("Outfit " + outfitId);
        }
    }
}
