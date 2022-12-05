using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR.Module.Stadthalle
{
    public class StadthalleModule : Module<StadthalleModule>
    {
        public static int PhoneNumberChangingMonths = 4;

        public static Vector3 MenuPosition = new Vector3(-555.256, -197.16, 38.2224);
        public static ColShape MenuColShape = null;

        protected override bool OnLoad()
        {
            MenuColShape = ColShapes.Create(MenuPosition, 3.0f, 0);
            MenuColShape.SetData("stadthalle_menu", true);
            MenuManager.Instance.AddBuilder(new StadtHalleMenu());
            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;

            if (!dbPlayer.HasData("stadthalle_menu"))
                return false;

            if (key == Key.E)
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
                else if(dbPlayer.HasData("stadthalle_menu"))
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
        public void changePhoneNumberRandom(Player player, string returnString, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            
            if(returnString.Length < 0 || returnString.Length > 20 || returnString.ToLower() != "kaufen")
            {
                return;
            }

            int money = 10000 * dbPlayer.Level;

            if (!dbPlayer.TakeBankMoney(money, "Telefonnummer Änderung"))
            {
                dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(money));
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

            if(!UInt32.TryParse(returnString, out uint phoneNumber) || phoneNumber < 1000 || phoneNumber > 9999999)
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

            if(!dbPlayer.TakeBankMoney(price, "Telefonnummer Änderung"))
            {
                dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(price));
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
