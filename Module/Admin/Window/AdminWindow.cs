using System;
using VMP_CNR.Module.Players.Db;
using Newtonsoft.Json;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Windows;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.ClientUI.Components;
using System.Reflection;

namespace VMP_CNR.Module.Admin.Window
{
    public class AdminWindow : Window<Func<DbPlayer, bool>>
    {
        private class ShowEvent : Event
        {
            public ShowEvent(DbPlayer dbPlayer) : base(dbPlayer)
            {

            }
        }

        public AdminWindow() : base("AdminMenu")
        {
        }

        public override Func<DbPlayer, bool> Show()
        {
            return (player) => OnShow(new ShowEvent(player));
        }
        [RemoteEvent]
        public void requestAdminMenuCmd(Player player, string cmd, uint foundPlayer, string args, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid()||String.IsNullOrEmpty(cmd)) return;
            if (!dbPlayer.Rank.CanAccessCommand(cmd)||!dbPlayer.Rank.CanAccessFeature("adminmenu")) return;
            AdminModuleCommands admin = new Admin.AdminModuleCommands();
            MethodInfo method=admin.GetType().GetMethod(cmd);
            if (method!=null)
            {
                ParameterInfo[] pars = method.GetParameters();

                switch (pars.Count())
                {
                    case 1: method.Invoke(admin, new[] { player }); break;
                    case 2: 
                        string arg = foundPlayer + " " + args;
                        method.Invoke(admin, new object[] { player, arg}); break;
                }

            }
            else
            {
                return;
            }
        }

        [RemoteEvent]
        public void requestAdminMenu(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid()) return;
            if (!dbPlayer.Rank.CanAccessFeature("adminmenu")) return;

            /*
             * ShowPlayer = Soll ein Spieler für den Befehl ausgewählt werden
             * Server = Ist es ein Serverbefehl?
             * Input = Text für eine Confirmationmeldung und InputFeld
             * Confirm = Befehl bestätigen
             */

            List<AdminMenuCommands> amc = new List<AdminMenuCommands>();
            amc.Add(new AdminMenuCommands() { Title="Players",Cmd="showplayers",ShowPlayers=true,Server=false,Input="",Confirm=false});
            amc.Add(new AdminMenuCommands() { Title="Names",Cmd="names",ShowPlayers=false,Server=true,Input="",Confirm=false});
            amc.Add(new AdminMenuCommands() { Title="aDuty",Cmd="aduty",ShowPlayers=false,Server=true,Input="",Confirm=false});
            amc.Add(new AdminMenuCommands() { Title="NoClip",Cmd="noclip",ShowPlayers=false,Server=true,Input="",Confirm=false});
            amc.Add(new AdminMenuCommands() { Title="aRev",Cmd="arev",ShowPlayers=true,Server=true,Input="",Confirm=false});
            amc.Add(new AdminMenuCommands() { Title="aSpawn",Cmd="spawn",ShowPlayers=true,Server=true,Input="",Confirm=false});
            amc.Add(new AdminMenuCommands() { Title="aFrisk", Cmd="afrisk", ShowPlayers=true,Server=true,Input="",Confirm=false});
            amc.Add(new AdminMenuCommands() { Title="KickPlayer", Cmd="kickplayer", ShowPlayers=true,Server=true,Input="Grund des Kicks",Confirm=true});
            amc.Add(new AdminMenuCommands() { Title="freezePlayer", Cmd="freezeplayer", ShowPlayers=true,Server=true,Input="",Confirm=false});
            amc.Add(new AdminMenuCommands() { Title="unfreezePlayer", Cmd="unfreezeplayer", ShowPlayers=true,Server=true,Input="",Confirm=false});
            amc.Add(new AdminMenuCommands() { Title="RemoveVeh", Cmd="removeveh", ShowPlayers=false,Server=true,Input="",Confirm=false});
            amc.Add(new AdminMenuCommands() { Title="aFly", Cmd="afly", ShowPlayers=false,Server=true,Input="",Confirm=false});

            //Filter WhiteList 4 Admin
            foreach (var item in amc.ToList())
            {
                if(!dbPlayer.Rank.CanAccessCommand(item.Cmd)) amc.Remove(item);
            }

            List<AdminMenuPlayers> amp = new List<AdminMenuPlayers>();
            foreach (var l_Player in NAPI.Player.GetPlayersInRadiusOfPosition(70.0f, dbPlayer.Player.Position))
            {
                //if (player == l_Player) continue; SelfShow Activate
                DbPlayer iPlayer = l_Player.GetPlayer();
                if (iPlayer == null || !iPlayer.IsValid()) continue;
                amp.Add(new AdminMenuPlayers() { Id = iPlayer.Id, Realname= iPlayer.GetName(),Fakename=iPlayer.Player.Name});
            }


            dbPlayer.Player.TriggerNewClient("componentServerEvent", "AdminMenu", "responseAdminMenu", NAPI.Util.ToJson(amc), NAPI.Util.ToJson(amp));
        }
    }

    public class AdminMenuPlayers
    {
        [JsonProperty(PropertyName = "id")] public uint Id { get; set; }
        [JsonProperty(PropertyName = "realname")] public string Realname { get; set; }
        [JsonProperty(PropertyName = "fakename")] public string Fakename { get; set; }
    }

    public class AdminMenuCommands
    {
        [JsonProperty(PropertyName = "title")] public string Title { get; set; }
        [JsonProperty(PropertyName = "cmd")] public string Cmd { get; set; }
        [JsonProperty(PropertyName = "showplayers")] public bool ShowPlayers { get; set; }
        [JsonProperty(PropertyName = "server")] public bool Server { get; set; }
        [JsonProperty(PropertyName = "input")] public string Input { get; set; }
        [JsonProperty(PropertyName = "confirm")] public bool Confirm { get; set; }
    }

    public class AdminWindowHandler : Script
    {
        [RemoteEvent]
        public void openAdminMenu(Player player,  string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.Rank.CanAccessFeature("adminmenu")) return;
            ComponentManager.Get<AdminWindow>().Show()(dbPlayer);
        }

        [RemoteEvent]
        public void closeAdminMenu(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.Rank.CanAccessFeature("adminmenu")) return;
            dbPlayer.Player.TriggerNewClient("componentServerEvent", "AdminMenu", "responseCloseAdminMenu", true);
        }
    }

}
