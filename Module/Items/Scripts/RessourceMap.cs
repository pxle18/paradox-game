using System;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Farming;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool RessourceMap(DbPlayer dbPlayer, ItemModel ItemData)
        {
            DialogMigrator.CreateMenu(dbPlayer.Player, Dialogs.menu_ressourcemap, "Ressourcen Karte", "");
            DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_ressourcemap, MSG.General.Close(), "");

            foreach (var xFarm in FarmSpotModule.Instance.GetAll())
            {
                if (xFarm.Value.RessourceName != "")
                {
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_ressourcemap,
                        xFarm.Value.RessourceName,
                        "Ressourcenpunkt fuer " + xFarm.Value.RessourceName);
                }
            }

            foreach (var farmProcess in FarmProcessModule.Instance.GetAll())
            {
                if (farmProcess.Value.ProcessName != "")
                {
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_ressourcemap,
                        farmProcess.Value.ProcessName,
                        "Hersteller fuer " + ItemModelModule.Instance.Get(farmProcess.Value.RewardItemId).Name);
                }
            }

            dbPlayer.Player.TriggerNewClient("hideInventory");
            dbPlayer.watchDialog = 0;
            dbPlayer.ResetData("invType");

            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_ressourcemap);

            return true;
        }
    }
}
