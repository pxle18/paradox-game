using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR.Module.Stadthalle
{
    public class StadthalleModule : Module<StadthalleModule>
    {
        public static int PhoneNumberChangingMonths = 4;

        public static Vector3 MenuPosition = new Vector3(-555.256, -197.16, 38.2224);
        private readonly Vector3 NameChangePosition = new Vector3(-551.2791, -193.87358, 38.469006);

        public static ColShape MenuColShape = null;

        protected override bool OnLoad()
        {
            MenuColShape = ColShapes.Create(MenuPosition, 3.0f, 0);
            MenuColShape.SetData("stadthalle_menu", true);

            Main.ServerBlips.Add(
                Blips.Create(NameChangePosition, "PARADOX Stadthalle", 267, 1.0f, color: 42)
            );

            MenuManager.Instance.AddBuilder(new StadtHalleMenu());
            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E) return false;

            if (dbPlayer.HasData("stadthalle_menu"))
            {
                MenuManager.Instance.Build(PlayerMenu.StadtHalleMenu, dbPlayer).Show(dbPlayer);
                return true;
            }

            return false;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {

            if (colShape.HasData("stadthalle_menu"))
            {
                if (colShapeState == ColShapeState.Enter)
                {
                    dbPlayer.SetData("stadthalle_menu", true);
                    dbPlayer.SendNewNotification(title: "Stadthalle", text: "Drücke E um eine das Menü zu öffnen.");
                    return true;
                }
                else if (dbPlayer.HasData("stadthalle_menu"))
                {
                    dbPlayer.ResetData("stadthalle_menu");
                    return true;
                }
            }
            return false;
        }

        public void SavePlayerLastPhoneNumberChange(DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync("UPDATE player SET `lasthandychange` = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE id = '" + dbPlayer.Id + "';");
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.LastPhoneNumberChange = reader.GetDateTime("lasthandychange");
        }


        public bool IsPhoneNumberAvailable(int number)
        {
            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT id FROM player WHERE handy = '{number}' LIMIT 1";
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            return false;
                        }
                    }
                    conn.Close();
                }
            }

            DbPlayer searchPlayer = Players.Players.Instance.GetPlayerByPhoneNumber((uint)number);
            if (searchPlayer != null) return false;

            return true;
        }
    }
    public class StadthalleEvents : Script
    {
        [RemoteEvent]
        public static void DoNamechangeRelease(Player p_Player, string newName, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            DbPlayer playerToNameChange = dbPlayer;
            if (playerToNameChange == null || !playerToNameChange.IsValid()) return;

            var split = newName.Split("_");
            if (split[0].Length < 3 || split[1].Length < 3)
            {
                dbPlayer.SendNewNotification("Der Vor & Nachname muss jeweils mindestens 3 Buchstaben beinhalten!");
                return;
            }

            int count = newName.Count(f => f == '_');
            if (!Regex.IsMatch(newName, @"^[a-zA-Z_-]+$") || count != 1)
            {
                dbPlayer.SendNewNotification("Bitte gib einen Namen in dem Format Max_Mustermann an.");
                return;
            }

            if (newName.Length > 40 || newName.Length < 7)
            {
                dbPlayer.SendNewNotification("Der Name ist zu lang oder zu kurz!");
                return;
            }
            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT name FROM player WHERE LOWER(name) = '{newName.ToLower()}' LIMIT 1";
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            dbPlayer.SendNewNotification("Dieser Name ist bereits vergeben.");
                            conn.Close();
                            return;
                        }
                    }
                    conn.Close();
                }
            }

            if (playerToNameChange.OwnHouse[0] != 0)
            {
                House house = HouseModule.Instance.GetByOwner(playerToNameChange.Id);
                house.OwnerName = newName;
                house.SaveOwner();
            }

            Logger.AddNameChangeLog(playerToNameChange.Id, playerToNameChange.Level, playerToNameChange.GetName(), newName, false);

            Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                playerToNameChange.GetName() + $"({playerToNameChange.Id}) hat den Namen zu {newName} geändert");

            MySQLHandler.ExecuteAsync($"UPDATE player SET name = '{newName}' WHERE id = '{playerToNameChange.Id}'");

            playerToNameChange.SendNewNotification($"Du hast deinen Namen erfolgreich zu {newName} geändert! Bitte beende nun das Spiel und trag deinen neuen Namen in den RAGE:MP Launcher ein!", PlayerNotification.NotificationType.ADMIN, duration: 30000);
            playerToNameChange.Kick("Namensaenderung");

        }

        [RemoteEvent]
        public void changePhoneNumberRandom(Player player, string returnString, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (returnString.Length < 0 || returnString.Length > 20 || returnString.ToLower() != "kaufen")
            {
                return;
            }

            int money = 10000 * dbPlayer.Level;

            if (!dbPlayer.TakeBankMoney(money, "Telefonnummer Änderung"))
            {
                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(money));
                return;
            }

            Random rnd = new Random();

            int number = 0;
            while (number == 0)
            {
                number = rnd.Next(10000, 9999999);
                if (!StadthalleModule.Instance.IsPhoneNumberAvailable(number))
                {
                    number = 0;
                }
            }

            uint oldnumber = dbPlayer.handy[0];

            dbPlayer.handy[0] = Convert.ToUInt32(number);
            dbPlayer.Save();
            dbPlayer.SendNewNotification("Deine Nummer wurde geändert! (Neue Nummer: " + number + ")");

            MySQLHandler.ExecuteAsync($"INSERT INTO `log_phonenumberchange` (`player_id`, `handy_old`, `handy_new`) VALUES ('{dbPlayer.Id}', '{oldnumber}', '{number}');");

            dbPlayer.LastPhoneNumberChange = DateTime.Now;
            StadthalleModule.Instance.SavePlayerLastPhoneNumberChange(dbPlayer);
            return;
        }

        [RemoteEvent]
        public void changePhoneNumber(Player player, string returnString, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!UInt32.TryParse(returnString, out uint phoneNumber) || phoneNumber < 1000 || phoneNumber > 9999999)
            {
                dbPlayer.SendNewNotification("Die angegebene Telefonnummer ist ungültig!");
                return;
            }

            if (!StadthalleModule.Instance.IsPhoneNumberAvailable((int)phoneNumber))
            {
                dbPlayer.SendNewNotification("Die angegebene Telefonnummer ist bereits vergeben!");
                return;
            }

            int price = 0;
            if (phoneNumber > 1000 && phoneNumber < 9999) price = 200000 * dbPlayer.Level;
            else price = 25000 * dbPlayer.Level;

            if (!dbPlayer.TakeBankMoney(price, "Telefonnummer Änderung"))
            {
                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(price));
                return;
            }

            uint oldnumber = dbPlayer.handy[0];

            dbPlayer.handy[0] = Convert.ToUInt32(phoneNumber);
            dbPlayer.Save();
            dbPlayer.SendNewNotification("Deine Nummer wurde geändert! (Neue Nummer: " + phoneNumber + ", Kosten: $" + price + ")");

            MySQLHandler.ExecuteAsync($"INSERT INTO `log_phonenumberchange` (`player_id`, `handy_old`, `handy_new`) VALUES ('{dbPlayer.Id}', '{oldnumber}', '{phoneNumber}');");

            dbPlayer.LastPhoneNumberChange = DateTime.Now;
            StadthalleModule.Instance.SavePlayerLastPhoneNumberChange(dbPlayer);
        }
    }
}
