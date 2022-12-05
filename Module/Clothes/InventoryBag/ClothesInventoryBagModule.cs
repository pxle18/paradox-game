using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Clothes.Shops;
using VMP_CNR.Module.Clothes.Slots;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Clothes.InventoryBag
{
    public class ClothesInventoryBagModule : Module<ClothesInventoryBagModule>
    {
        public static int MaxClothesInBag = 10;

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.InventoryClothesBag = new PlayerInventoryBag();

            if(reader.GetString("clothesbag") != "")
            {
                dbPlayer.InventoryClothesBag = NAPI.Util.FromJson<PlayerInventoryBag>(reader.GetString("clothesbag"));
            }
        }
    }

    public static class ClothesInventoryBagPlayerExtension
    {
        public static void SaveClothesBag(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE player SET clothesbag = '{NAPI.Util.ToJson(dbPlayer.InventoryClothesBag)}' WHERE id = '{dbPlayer.Id}';");
        }
    }

    public class PlayerInventoryBag
    {
        public List<uint> Clothes { get; set; }
        
        public List<uint> Props { get; set; }

        public PlayerInventoryBag()
        {
            Clothes = new List<uint>();
            Props = new List<uint>();
        }
    }
}
