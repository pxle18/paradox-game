using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Robbery;
using static VMP_CNR.Module.Chat.Chats;
using VMP_CNR.Module.Schwarzgeld;
using VMP_CNR.Module.Dealer;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Injury;

namespace VMP_CNR.Module.Players.Commands
{
    public class AsyncCommands : Script
    {
        public static AsyncCommands Instance { get; } = new AsyncCommands();

        public async Task HandleGrab(Player player, string playerName)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (string.IsNullOrWhiteSpace(playerName))
            {
                dbPlayer.SendNewNotification( GlobalMessages.General.Usage("/grab", "[playerName]"));
                return;
            }
            
            var findPlayer = Players.Instance.FindPlayer(playerName);

            // Main Thread Stuff
            await NAPI.Task.WaitForMainThread();
            if (findPlayer == null)
                return;

            var findPlayerPosition = findPlayer.Player.Position;
            var dbPlayerPosition = dbPlayer.Player.Position;

            // Back to Worker Thread
            await Task.Delay(5);
            if (findPlayer == null || findPlayerPosition.DistanceTo(dbPlayerPosition) > 20.0f)
            {
                dbPlayer.SendNewNotification(
                                                "Person nicht gefunden oder außerhalb der Reichweite!");
                return;
            }

            if (!dbPlayer.RageExtension.IsInVehicle)
            {
                dbPlayer.SendNewNotification(
                                                "Sie muessen sich in einem Fahrzeug befinden!");
                return;
            }

            if (findPlayer.RageExtension.IsInVehicle)
            {
                dbPlayer.SendNewNotification(
                                                "Die Person darf sich nicht in einem Auto befinden!");
                return;
            }

            if (findPlayer.IsCuffed || findPlayer.IsTied)
            {
                if (!VehicleHandler.Instance.TrySetPlayerIntoVehicleOccupants(dbPlayer.Player.Vehicle.GetVehicle(), findPlayer))
                {
                    dbPlayer.SendNewNotification("Es sind keine freien Sitze mehr verfuegbar!", title: "Fahrzeug", notificationType: PlayerNotification.NotificationType.ERROR);
                    return;
                }
                if (findPlayer.IsCuffed)
                {
                    findPlayer.SetCuffed(true, true);
                }
                else
                {
                    findPlayer.SetTied(true, true);
                }
                dbPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " ins Fahrzeug gezogen.");
                findPlayer.SendNewNotification("Du wurdest ins Fahrzeug gezogen.");
            }
            else
            {
                dbPlayer.SendNewNotification("Du musst die Person erst fesseln.");
                return;
            }            
        }
        
        public async Task HandleTakeLic(Player p_Player, string p_Name)
        {
            
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (!dbPlayer.IsACop() || !dbPlayer.IsInDuty())
            {
                dbPlayer.SendNewNotification( GlobalMessages.Error.NoPermissions());
                return;
            }

            await NAPI.Task.WaitForMainThread(0);

            var findPlayer = Players.Instance.FindPlayer(p_Name);
            if (findPlayer == null || findPlayer.Player.Position.DistanceTo(p_Player.Position) > 5.0f)
            {
                dbPlayer.SendNewNotification("Spieler nicht gefunden oder außerhalb der Reichweite!");
                return;
            }

            if (findPlayer.IsInjured())
            {
                dbPlayer.SendNewNotification("Person darf nicht verletzt sein!");
                return;
            }

            dbPlayer.SetData("takeLic", findPlayer);

            DialogMigrator.CreateMenu(p_Player, Dialogs.menu_takelic, "Lizenzen",
                "Welche Lizenz wollen Sie " + findPlayer.GetName() + " entziehen...");
            if (findPlayer.Lic_Car[0] > 0)
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Car, "");
            else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Car, "");
            if (findPlayer.Lic_LKW[0] > 0)
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Lkw, "");
            else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Lkw, "");
            if (findPlayer.Lic_Bike[0] > 0)
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Bike, "");
            else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Bike, "");
            if (findPlayer.Lic_Boot[0] > 0)
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Boot, "");
            else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Boot, "");
            if (findPlayer.Lic_PlaneA[0] > 0)
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.PlaneA, "");
            else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.PlaneA, "");
            if (findPlayer.Lic_PlaneB[0] > 0)
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.PlaneB, "");
            else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.PlaneB, "");
            if (findPlayer.Lic_Biz[0] > 0)
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Biz, "");
            else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Biz, "");
            if (findPlayer.Lic_Gun[0] > 0)
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Gun, "");
            else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Gun, "");
            if (findPlayer.Lic_Hunting[0] > 0)
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Hunting, "");
            else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Hunting, "");
            if (findPlayer.Lic_Transfer[0] > 0)
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Transfer, "");
            else
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Transfer, "");
            DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, "Menu schließen", "");
            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_takelic);
            
        }

        public async Task HandleGiveLic(Player p_Player, string p_Name)
        {
            
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (dbPlayer.TeamId != (int)TeamTypes.TEAM_DRIVINGSCHOOL || !dbPlayer.IsInDuty() || dbPlayer.TeamRank < 1)
            {
                dbPlayer.SendNewNotification( GlobalMessages.Error.NoPermissions());
                return;
            }

            await NAPI.Task.WaitForMainThread(0);

            var findPlayer = Players.Instance.FindPlayer(p_Name);

            if (findPlayer == null || findPlayer.Player.Position.DistanceTo(p_Player.Position) > 5.0f)
            {
                dbPlayer.SendNewNotification(
                        
                    "Spieler nicht gefunden oder außerhalb der Reichweite!");
                return;
            }

            if (findPlayer == dbPlayer) return;

            if (findPlayer.IsHomeless())
            {
                dbPlayer.SendNewNotification("Ohne Wohnsitz kann diese Person keine Lizenz erhalten!");
                return;
            }

            if (dbPlayer.Player.Position.DistanceTo(new Vector3(-810.6085, -1347.864, 5.166561)) >= 20.0f)
            {
                dbPlayer.SendNewNotification("Du musst an dem Fahrschulgebaeude sein um Scheine auszustellen!");
                return;
            }

            dbPlayer.SetData("giveLic", findPlayer.Player);

            DialogMigrator.CreateMenu(dbPlayer.Player, Dialogs.menu_givelicenses, "Lizenzen",
                "Vergebe Lizenzen an " + findPlayer.GetName());
            DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                Content.License.Car + " " + Price.License.Car + "$", "");
            DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                Content.License.Lkw + " " + Price.License.Lkw + "$", "");
            DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                Content.License.Bike + " " + Price.License.Bike + "$", "");
            DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                Content.License.Boot + " " + Price.License.Boot + "$", "");
            DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                Content.License.PlaneA + " " + Price.License.PlaneA + "$", "");
            DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                Content.License.PlaneB + " " + Price.License.PlaneB + "$", "");
            DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                Content.License.Transfer + " " + Price.License.Transfer + "$", "");
            DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses, "Menu schließen", "");
            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_givelicenses);
            
        }

        public async Task HandleGiveMarryLic(Player p_Player, DbPlayer findPlayer)
        {

            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamId != (int)TeamTypes.TEAM_GOV || !dbPlayer.IsInDuty())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            if (dbPlayer.TeamRank < 9)
            {
                dbPlayer.SendNewNotification("Sie benötigen mind. Rang 9 um die Standesamt-Lizenz zu vergeben.", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            await NAPI.Task.WaitForMainThread(0);

            if (findPlayer == null || !findPlayer.IsValid() || findPlayer.Player.Position.DistanceTo(p_Player.Position) > 5.0f)
            {
                dbPlayer.SendNewNotification(
                    "Spieler nicht gefunden oder außerhalb der Reichweite!");
                return;
            }
            if (findPlayer.marryLic == 1)
            {
                dbPlayer.SendNewNotification(findPlayer.GetName() + " hat bereits die " + Content.License.marryLic);
            }
            else
            {                           
                MySQLHandler.ExecuteAsync($"UPDATE player SET marrylic = '1' WHERE id = '{findPlayer.Id}'");

                findPlayer.marryLic = 1;

                findPlayer.SendNewNotification("Sie haben die "+Content.License.marryLic+" erhalten!");
                dbPlayer.SendNewNotification("Sie haben "+findPlayer.GetName()+" die "+Content.License.marryLic+" gegeben.");

            }

        }


        public async Task HandleTakeMarryLic(Player p_Player, DbPlayer findPlayer)
        {

            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamId != (int)TeamTypes.TEAM_GOV || !dbPlayer.IsInDuty())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }
            if (dbPlayer.TeamRank < 9)
            {
                dbPlayer.SendNewNotification("Sie benötigen mind. Rang 9 um die Standesamt-Lizenz zu vergeben.", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            await NAPI.Task.WaitForMainThread(0);

            if (findPlayer == null || !findPlayer.IsValid() || findPlayer.Player.Position.DistanceTo(p_Player.Position) > 5.0f)
            {
                dbPlayer.SendNewNotification(
                    "Spieler nicht gefunden oder außerhalb der Reichweite!");
                return;
            }
            if (findPlayer.marryLic != 1)
            {
                dbPlayer.SendNewNotification(findPlayer.GetName() + " hat keine " + Content.License.marryLic);
            }
            else
            {
                MySQLHandler.ExecuteAsync($"UPDATE player SET marrylic = '0' WHERE id = '{findPlayer.Id}'");

                findPlayer.marryLic = 0;

                findPlayer.SendNewNotification("Sie haben die " + Content.License.marryLic + " entzogen bekommen!");
                dbPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " die " + Content.License.marryLic + " entzogen.");

            }

        }

        public void HandleSupport(string p_Name, int p_ForumID, string p_Message)
        {
                Players.Instance.SendMessageToAuthorizedUsers("support", $"[{p_Name}({p_ForumID})]: {p_Message}", time:20000);
        }

        public async Task SendGovMessage(DbPlayer player, string mesage)
        {
            var id = player.Team.Id;
            var from = "LSPD Nachricht";

            if (id == (uint)TeamTypes.TEAM_GOV)
                from = "Regierungsnachricht";
            else if (id == (uint)TeamTypes.TEAM_FIB)
                from = "FIB Nachricht";
            else if (id == (uint)TeamTypes.TEAM_ARMY)
                from = "Army Nachricht";
            else if (id == (uint)TeamTypes.TEAM_DPOS)
                from = "DPOS Nachricht";
            else if (id == (uint)TeamTypes.TEAM_NEWS)
                from = "WEAZLE NEWS";
            else if (id == (uint)TeamTypes.TEAM_MEDIC)
                from = "LSMC";

            await Chats.SendGlobalMessage($"{from}: {mesage}", COLOR.LIGHTBLUE, ICON.GOV);

            Players.Instance.SendMessageToAuthorizedUsers("GOV-Message", "von " + player.GetName());
            
        }

        public async Task SendCayoMessage(DbPlayer player, string mesage)
        {

            await Chats.SendCayoMessage($"Cayo Perico: {mesage}", COLOR.LIGHTBLUE, ICON.GOV);

            Players.Instance.SendMessageToAuthorizedUsers("CayoPerico-Message", "von " + player.Player.Name);

        }

        public async Task SetHandMoney(Player player, string commandParams)
        {
            await Task.Run(() =>
            {
                DbPlayer dbPlayer = player.GetPlayer();

                if (!dbPlayer.CanAccessMethod())
                {
                    dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                    return;
                }

                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

                if (command.Length < 2) return;

                var findPlayer = Players.Instance.FindPlayer(command[0]);

                if (findPlayer == null || !findPlayer.IsValid()) return;

                if (!int.TryParse(command[1], out int amount)) return;

                var name = findPlayer != null ? findPlayer.GetName() : command[0];

                Players.Instance.SendMessageToAuthorizedUsers("log",
                    "Admin " + dbPlayer.GetName() + " hat das Geld von " + name + " um $" + amount + " veraendert.");

                DatabaseLogging.Instance.LogAdminAction(player, name, AdminLogTypes.log, $"{amount}$ GivemoneyHand");


                if (amount > 0)
                {
                    if (findPlayer.GiveMoney(amount))
                    {

                    };

                    dbPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " $" + amount +
                                            " auf die Hand gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    findPlayer.SendNewNotification("Administrator " + dbPlayer.GetName() + " hat ihnen $" +
                                               amount + " auf die Hand gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    if (dbPlayer.RankId < (int)AdminLevelTypes.Projektleitung)
                        Players.Instance.SendMessageToAuthorizedUsers("log",
                            "Admin " + dbPlayer.GetName() + " hat " + findPlayer.GetName() + " $" + amount + " auf die Hand gegeben!");

                    DatabaseLogging.Instance.LogAdminAction(player, findPlayer.GetName(), AdminLogTypes.log, $"{amount}$ GivemoneyHand");
                    return;
                }

                var success = findPlayer.TakeAnyMoney(Math.Abs(amount));
                string kontotyp = "";

                switch (success)
                {
                    case -1:
                        dbPlayer.SendNewNotification($"Beim Entfernen des Geldes vom Spieler {findPlayer.GetName()} in Hoehe von {amount} ist ein Fehler aufgetreten. Pruefen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                        return;
                    case 0:
                        kontotyp = "Geldbörse";
                        break;
                    case 1:
                        kontotyp = "Bank";
                        break;
                    default:
                        dbPlayer.SendNewNotification($"Beim Entfernen des Geldes vom Spieler {findPlayer.GetName()} in Hoehe von {amount} ist ein Fehler aufgetreten. Pruefen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                        return;
                }

                dbPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " $" + amount +
                                        " von " + kontotyp + " entfernt.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                findPlayer.SendNewNotification("Administrator" + dbPlayer.GetName() + " hat ihnen $" +
                                           amount + " aus der " + kontotyp + " entfernt.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

                Players.Instance.SendMessageToAuthorizedUsers("log",
                    "Admin " + dbPlayer.GetName() + " hat " + findPlayer.GetName() + " $" + amount + " aus der Geldbörse entfernt!");

                DatabaseLogging.Instance.LogAdminAction(player, findPlayer.GetName(), AdminLogTypes.log, $"-{amount}$ GivemoneyHand");
            });
        }

        public async Task SetBlackMoney(Player player, string commandParams)
        {
            await Task.Run(() =>
            {
                DbPlayer dbPlayer = player.GetPlayer();

                if (!dbPlayer.CanAccessMethod())
                {
                    dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                    return;
                }

                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

                if (!int.TryParse(command[1], out int amount)) return;

                var findPlayer = Players.Instance.FindPlayer(command[0]);

                var name = findPlayer != null ? findPlayer.GetName() : command[0];

                Players.Instance.SendMessageToAuthorizedUsers("log",
                    "Admin " + dbPlayer.GetName() + " hat das Schwarzgeld von " + name + " um $" + amount + " veraendert.");

                DatabaseLogging.Instance.LogAdminAction(player, name, AdminLogTypes.log, $"{amount}$ GiveBlackMoneyHand");

                if (findPlayer == null) return;

                if (amount > 0)
                {
                    if (findPlayer.GiveBlackMoney(amount))
                    {

                    };

                    dbPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " $" + amount + " Schwarzgeld auf die Hand gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    findPlayer.SendNewNotification("Administrator " + dbPlayer.GetName() + " hat ihnen $" + amount + " Schwarzgeld auf die Hand gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    if (dbPlayer.RankId < (int)AdminLevelTypes.Projektleitung)
                        Players.Instance.SendMessageToAuthorizedUsers("log", "Admin " + dbPlayer.GetName() + " hat " + findPlayer.GetName() + " $" + amount + " Schwarzgeld auf die Hand gegeben!");

                    DatabaseLogging.Instance.LogAdminAction(player, findPlayer.GetName(), AdminLogTypes.log, $"{amount}$ GiveBlackMoneyHand");
                    return;
                }
                
            });
        }

        public async Task HandleFindRob(DbPlayer dbPlayer)
        {
            
            if (!dbPlayer.IsValid())
                return;

            await NAPI.Task.WaitForMainThread(0);

            if (RobberyModule.Instance.GetActiveRobs().Count <= 0)
            {
                dbPlayer.SendNewNotification( "Kein Raubueberfall gefunden.");
                return;
            }

            DialogMigrator.CreateMenu(dbPlayer.Player, Dialogs.menu_findrob, "Aktuelle Raube", "");
            foreach (Rob rob in RobberyModule.Instance.GetActiveRobs(true))
            {
                if (rob.Id == RobberyModule.Juwelier)
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_findrob, "Juwelier", "");
                else
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_findrob, "Shopraub", "");
            }

            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_findrob);
        }

        public async Task HandleFindDealer(DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsValid()) return;

            DialogMigrator.CreateMenu(dbPlayer.Player, Dialogs.menu_dealerhint, "Aktuelle Informationen", "");
            DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_dealerhint, "Schließen", "");
            int index = 1;
            foreach (Dealer.Dealer dealer in DealerModule.Instance.GetAll().Values)
            {
                if (dealer.Alert)
                {
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_dealerhint, "Dealertipp " + index, "");
                    index++;
                }
            }
            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_dealerhint);
        }

        public async Task HandleJail(DbPlayer dbPlayer)
        {
            
            if (!dbPlayer.IsValid())
                return;

            await NAPI.Task.WaitForMainThread(0);

            Dictionary<string, int> l_JailInhabits = new Dictionary<string, int>();

            foreach (var l_Player in Players.Instance.GetValidPlayers())
            {
                if (l_Player.JailTime[0] > 0)
                    l_JailInhabits.TryAdd(l_Player.GetName(), l_Player.JailTime[0]);
            }

            DialogMigrator.CreateMenu(dbPlayer.Player, Dialogs.menu_jailinhabits, "Insassen (Name - Verbleib. Monate)", "");
            foreach (var l_Pair in l_JailInhabits)
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_jailinhabits, $"{l_Pair.Key} - {l_Pair.Value}", "");

            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_jailinhabits);
            dbPlayer.SendNewNotification($"Es befinden sich insgesamt {l_JailInhabits.Count} Insassen im SG!");
            
        }
        
        public async Task HandleNews(string p_NewsMessage)
        {
            await Task.Run(() =>
            {
                foreach (var l_Player in Players.Instance.GetValidPlayers())
                {
                    if (!Main.newsActivated(l_Player.Player))
                        continue;

                    l_Player.SendNewNotification(Chats.MsgNews + p_NewsMessage);
                }
            });
        }
    }
}
