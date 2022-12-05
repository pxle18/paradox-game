using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Sync;

namespace VMP_CNR.Module.Menu
{
    public class DialogClientMenu
    {
        public uint MenuId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> ClientItems;
    }

    public class DialogMigratorItem
    {
        public string Label { get; set; }

        public string Description { get; set; }

        public DialogMigratorItem(string label, string description)
        {
            Label = label;
            Description = description;
        }
    }

    public static class DialogMigrator
    {
        public static void OpenUserMenu(DbPlayer dbPlayer, uint MenuID, bool nofreeze = false)
        {
            ShowMenu(dbPlayer.Player, MenuID);
            dbPlayer.WatchMenu = MenuID;
        }

        public static void CloseUserMenu(Player player, uint MenuID, bool noHide = false)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;
            if (!noHide) HideMenu(player, MenuID);
            dbPlayer.WatchMenu = 0;
        }

        public static void CreateMenu(Player player, uint menuid, string name = "", string description = "")
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;
            dbPlayer.SetData("clientBuildDialogMenu", new DialogClientMenu()
            {
                MenuId = menuid,
                ClientItems = new List<string>(),
                Name = name,
                Description = description
            });
        }

        public static void AddMenuItem(Player player, uint menuid, string label, string description)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.HasData("clientBuildDialogMenu")) return;

            DialogClientMenu dialogClientMenu = dbPlayer.GetData("clientBuildDialogMenu");
            if (dialogClientMenu == null) return;

            dialogClientMenu.ClientItems.Add(label);
        }

        public static void ShowMenu(Player player, uint menuid)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            DialogClientMenu dialogClientMenu = dbPlayer.GetData("clientBuildDialogMenu");
            if (dialogClientMenu == null) return;

            player.TriggerNewClient(
                "componentServerEvent",
                "NativeMenu",
                "createMenu",
                dialogClientMenu.Name,
                dialogClientMenu.Description
            );

            var idx = -1;
            foreach (var item in dialogClientMenu.ClientItems)
            {
                idx++;

                player.TriggerNewClient(
                    "componentServerEvent",
                    "NativeMenu",
                    "addItem",
                    JsonConvert.SerializeObject(new DialogMigratorItem(item, "")),
                    idx
                );
            }

            player.TriggerNewClient("componentServerEvent", "NativeMenu", "show", menuid);
        }

        private static void HideMenu(Player player, uint menuid)
        {
            player.TriggerNewClient("componentServerEvent", "NativeMenu", "hide");
        }

        public static void CloseUserDialog(Player player, uint dialogid)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            dbPlayer.WatchDialog = 0;
            player.TriggerNewClient("deleteDialog");

            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
        }
    }
}