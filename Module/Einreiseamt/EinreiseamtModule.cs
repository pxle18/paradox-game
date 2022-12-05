using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using static VMP_CNR.Module.Chat.Chats;
using static VMP_CNR.Module.Players.PlayerNotification;

namespace VMP_CNR.Module.Einreiseamt
{
    public class EinreiseamtModule : Module<EinreiseamtModule>
    {
        public static Vector3 PositionPC1 = new Vector3(-1077.96, -2810.92, 27.7087);
        public static Vector3 PositionPC2 = new Vector3(-1067.82, -2811.16, 27.7087);
        public static Vector3 PositionPC3 = new Vector3(-1073.29, -2820.76, 27.7087);
        public static Vector3 PositionPC4 = new Vector3(-1083.56, -2820.45, 27.7087);

        public override bool Load(bool reload = false)
        {
            MenuManager.Instance.AddBuilder(new EinreiseAmtMenuBuilder());
            return reload;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            if (key == Key.E)
            {
                if (!dbPlayer.IsEinreiseAmt())
                {
                    return false;
                }
                if (dbPlayer.Player.Position.DistanceTo(PositionPC1) < 1.5f || dbPlayer.Player.Position.DistanceTo(PositionPC2) < 1.5f || dbPlayer.Player.Position.DistanceTo(PositionPC3) < 1.5f || dbPlayer.Player.Position.DistanceTo(PositionPC4) < 1.5f)
                {
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Einreiseamt-Formular", Callback = "EinreiseAmtPlayer", Message = "Geben Sie einen Namen ein" });
                    return true;
                }
            }
            return false;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandeinreiseamt(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.IsEinreiseAmt()) return;

            string names = "";
            foreach(DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers().Where(p => p.NeuEingereist()))
            {
                names += xPlayer.GetName() + " ,";
            }

            dbPlayer.SendNewNotification("Aktuelle Spieler: " + names);
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandemember(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.IsEinreiseAmt()) return;

            DialogMigrator.CreateMenu(player, Dialogs.menu_player, "Einreiseamt", "");

            DialogMigrator.AddMenuItem(player, Dialogs.menu_player, GlobalMessages.General.Close(), "");

            foreach (DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers().Where(p => p.IsEinreiseAmt()))
            {
                if (!xPlayer.IsValid()) continue;
                
                DialogMigrator.AddMenuItem(player, Dialogs.menu_player, xPlayer.GetName(), "");
            }

            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_player);

        }


        [CommandPermission(PlayerRankPermission = true, AllowedDeath = true)]
        [Command(GreedyArg = true)]
        public void Commande(Player player, string message)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsEinreiseAmt()) return;

            foreach (DbPlayer iPlayer in Players.Players.Instance.GetValidPlayers().Where(p => p.IsEinreiseAmt()))
            {
                iPlayer.SendNewNotification("[EA] " + dbPlayer.GetName() + ": " + message, NotificationType.STANDARD,"",8000);
            }
        }


        [CommandPermission(PlayerRankPermission = true, AllowedDeath = true)]
        [Command(GreedyArg = true)]
        public void Commandgiveea(Player player, string destplayer)
        {
            Logging.Logger.Debug("command giveea " + destplayer);
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsEinreiseAmt() || dbPlayer.Team.Id != (int)teams.TEAM_GOV || dbPlayer.TeamRank < 8) return;

            DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(destplayer);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;

            if(foundPlayer.Einwanderung == 1)
            {
                foundPlayer.Einwanderung = 0;
                foundPlayer.SendNewNotification($"{dbPlayer.GetName()} hat ihnen die Einreiselizenz entzogen!");
                dbPlayer.SendNewNotification($"Sie haben {foundPlayer.GetName()} die Einreiselizenz entzogen!");
                return;
            }
            else
            {
                foundPlayer.Einwanderung = 1;
                foundPlayer.SendNewNotification($"{dbPlayer.GetName()} hat ihnen die Einreiselizenz ausgestellt!");
                dbPlayer.SendNewNotification($"Sie haben {foundPlayer.GetName()} die Einreiselizenz ausgestellt!");
                return;
            }
        }
    }

    public static class EinreisePlayerExtension
    {
        public static bool IsEinreiseAmt(this DbPlayer dbPlayer)
        {
            return dbPlayer.Einwanderung == 1;
        }

        public static void EinreiseSpawn(this DbPlayer dbPlayer)
        {
            dbPlayer.SendNewNotification("Neuer Spieler im Einreiseamt | Insg: " + Players.Players.Instance.GetValidPlayers().Where(p => p.NeuEingereist()).Count());
        }

        public static bool NeuEingereist(this DbPlayer dbPlayer)
        {
            return dbPlayer.hasPerso[0] == 0;
        }
    }
}
