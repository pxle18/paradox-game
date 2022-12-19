using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VMP_CNR.Module.Banks.BankHistory;
using VMP_CNR.Module.Banks.Windows;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Business
{
    public class BusinessModule : SqlModule<BusinessModule, Business, uint>
    {
        public static readonly Vector3 BusinessPosition = new Vector3(-78.9559, -829.376, 243.386);

        private static readonly Vector3 BusinessEnterPosition = new Vector3(-66.8634, -802.6156, 44.2273);

        //private static readonly Vector3 BusinessEnterPosition = new Vector3(-79.6059, -796.427, 44.2273);

        private static readonly Vector3 BusinessBankPosition = new Vector3(248.977, 212.425, 106.287);

        private static readonly Vector3 BusinessTresurePosition = new Vector3(-59.5738, -812.895, 243.386);

        public static Vector3 BusinessKeyInsertPosition = new Vector3(-83.051, -814.94, 36.1299);

        public static Vector3 BusinessKeyInsertAirport = new Vector3(-1279.88, -2615.68, 13.9449);

        public DateTime LastBusinessPayday = DateTime.Now.AddDays(-2);

        protected override string GetQuery()
        {
            return "SELECT * FROM `business`;";
        }

        protected override void OnItemLoaded(Business business)
        {
            // Load Branches
            business.LoadBusinessBranch();

            // Load Keys
            business.LoadVehicleKeys();
            business.LoadStorageKeys();

            business.LoadBankHistory();
            business.LoadMembers();
        }

        protected override void OnLoaded()
        {
            ColShape colShape = Spawners.ColShapes.Create(BusinessEnterPosition, 2.5f);
            colShape.SetData("businessTower", true);


            PlayerNotifications.Instance.Add(BusinessKeyInsertAirport,
                "Business Fahrzeuge",
                "Hier kannst du Fahrzeugschlüssel für dein Business anfertigen lassen!");

            PlayerNotifications.Instance.Add(BusinessKeyInsertPosition,
                "Business Fahrzeuge",
                "Hier kannst du Fahrzeugschlüssel für dein Business anfertigen lassen!");

            /*
            var query = $"SELECT value FROM `configuration` WHERE `key` = 'last_business_payday';";
            using (var conn = new MySqlConnection(Configurations.Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if(DateTime.TryParse(reader.GetString("value"), out DateTime lastBusinessPayday))
                            {
                                LastBusinessPayday = lastBusinessPayday;
                            }
                        }
                    }
                }
                conn.Close();
            }*/

        }

        public override void OnPlayerLoggedIn(DbPlayer dbPlayer)
        {
            dbPlayer.LoadBusinessMembership();
        }

        public IEnumerable<Business> GetOpenBusinesses()
        {
            return from business in this.GetAll() where !business.Value.Locked select business.Value;
        }

        public Business GetById(uint id)
        {
            return Instance.GetAll().Where(b => b.Value.Id == id).FirstOrDefault().Value;
        }

        public Business GetByName(string name)
        {
            return this.GetAll().FirstOrDefault(b =>
                string.Equals(b.Value.Name, name, StringComparison.CurrentCultureIgnoreCase)
                || b.Value.Name.ToLower().Contains(name.ToLower())).Value;
        }
        
        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (colShapeState == ColShapeState.Enter)
            {
                if (colShape.HasData("businessTower"))
                {
                    dbPlayer.SendNewNotification("Drücke E um den Business-Tower zu betreten.", PlayerNotification.NotificationType.BUSINESS);
                    return true;
                }
            }
            return false;
        }

        public override void OnFifteenMinuteUpdate()
        {
            /*
            if(LastBusinessPayday.AddDays(1) < DateTime.Now)
            {
                LastBusinessPayday = DateTime.Now;
                MySQLHandler.ExecuteAsync($"UPDATE configuration SET value = '{LastBusinessPayday.ToString()}' WHERE key = 'last_business_payday';");
                BusinessPaydays();
            }*/
        }

        public void BusinessPaydays()
        {
            foreach(Business biz in GetAll().Values.ToList())
            {
                try
                {
                    if(biz != null)
                    {
                        OnBusinessPayday(biz);
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }
        }

        public void OnBusinessPayday(Business biz)
        {
            // Steuer, Rechnungen Funkfrequenzen etc hier abrechenbar
        }

        private async Task LoadBusinessManually(uint id)
        {
            var query = $"SELECT * FROM business WHERE id = '{id}';";
            using (var conn = new MySqlConnection(Configurations.Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();
                cmd.CommandText = @query;

                using (var reader2 = await cmd.ExecuteReaderAsync())
                {
                    if (reader2.HasRows)
                    {
                        if (await reader2.ReadAsync())
                        {
                            if (!BusinessModule.Instance.Contains(id))
                            {

                                Logger.Debug("add to business list");
                                BusinessModule.Instance.Add(id, new Business(reader2));
                            }
                        }
                    }
                }
                await conn.CloseAsync();
            }
        }

        public async Task CreatePlayerBusiness(DbPlayer dbPlayer)
        {
            var name = $"Business von {dbPlayer.GetName()}";
            var query = $"INSERT INTO `business` (`name`) VALUES ('{MySqlHelper.EscapeString(name)}'); select last_insert_id();";
            using (var conn = new MySqlConnection(Configurations.Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                await conn.OpenAsync();
                cmd.CommandText = @query;
                uint id = 0;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        if (await reader.ReadAsync())
                        {
                            id = reader.GetUInt32("last_insert_id()");
                        }
                    }
                }
                await conn.CloseAsync();

                await LoadBusinessManually(id);

                Business business = this.Get(id);
                if(business == null)
                {
                    Logger.Debug("BUSINESS KONNTE NACH ERSTELLEN NICHT GELADEN WERDEN, ID: " + id);
                }

                //dbPlayer.AddBusinessOwnership(business);

                var queryAddOwner =
                string.Format(
                    $"INSERT INTO `business_members` (`player_id`, `business_id`, `manage`, `money`, `inventory`, `gehalt`, `owner`, `raffinery`, `fuelstation`, `nightclub`, `tattoo`) " +
                    $"VALUES ('{dbPlayer.Id}', '{id}', '1', '1', '1', '0', '1', '1', '1', '1', '1');");
                MySQLHandler.ExecuteAsync(queryAddOwner);

                Business.Member member = new Business.Member()
                {
                    PlayerId = dbPlayer.Id,
                    BusinessId = business.Id,
                    Manage = true,
                    Money = true,
                    Inventory = true,
                    Salary = 0,
                    Owner = true,
                    Raffinery = true,
                    Fuelstation = true,
                    Tattoo = true
                };

                business.Members = new Dictionary<uint, Business.Member>();
                business.AllMembers = new Dictionary<uint, Business.ExtendedMember>();

                business.AddMember(member);

                dbPlayer.ActiveBusinessId = business.Id;
                business.LoadBusinessBranch();
                await conn.CloseAsync();
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key == Key.L)
            {
                if (!(dbPlayer.Player.Position.DistanceTo(BusinessPosition) < 3.0f)) return false;
                if (dbPlayer.DimensionType[0] != DimensionType.Business ||
                    !dbPlayer.IsMemberOfBusiness() && dbPlayer.ActiveBusinessId != dbPlayer.Player.Dimension) return false;
                var biz = GetById((uint)dbPlayer.Player.Dimension);

                if (biz == null) return true;
                if (biz.Locked)
                {
                    biz.Locked = false;
                    dbPlayer.SendNewNotification("Tür aufgeschlossen!", title: "Business", notificationType: PlayerNotification.NotificationType.SUCCESS);
                }
                else
                {
                    biz.Locked = true;
                    dbPlayer.SendNewNotification("Tür abgeschlossen!", title: "Business", notificationType: PlayerNotification.NotificationType.ERROR);
                }

                return true;
            }

            if (key == Key.E)
            {
                if (dbPlayer.Player.Position.DistanceTo(BusinessEnterPosition) < 3.0f)
                {
                    MenuManager.Instance.Build(PlayerMenu.BusinessEnter, dbPlayer).Show(dbPlayer);
                    return true;
                }

                if (dbPlayer.DimensionType[0] == DimensionType.Business &&
                    dbPlayer.Player.Position.DistanceTo(BusinessTresurePosition) < 3.0f)
                {
                    var biz = GetById((uint)dbPlayer.Player.Dimension);
                    if (biz == null || !dbPlayer.IsMemberOfBusiness() ||
                        dbPlayer.ActiveBusinessId != biz.Id) return true;

                    ComponentManager.Get<BankWindow>().Show()(dbPlayer, "Business Tresor", biz.Name, dbPlayer.Money[0], biz.Money, 0, biz.BankHistory);
                    return true;
                }

                if (dbPlayer.Player.Position.DistanceTo(BusinessPosition) < 3.0f)
                {
                    if (dbPlayer.DimensionType[0] == DimensionType.Business)
                    {
                        var biz = GetById((uint)dbPlayer.Player.Dimension);
                        biz.Visitors.Remove(dbPlayer);
                        dbPlayer.Player.SetPosition(BusinessEnterPosition);
                        dbPlayer.DimensionType[0] = DimensionType.World;
                        dbPlayer.SetDimension(0);
                        return true;
                    }

                    return true;
                }

                if (dbPlayer.Player.Position.DistanceTo(BusinessBankPosition) < 3.0f)
                {
                    MenuManager.Instance.Build(PlayerMenu.BusinessBank, dbPlayer).Show(dbPlayer);
                    return true;
                }

                return false;
            }
            return false;
        }
    }

    public class BusinessEvents : Script
    {
        //NameChangeBiz
        [RemoteEvent]
        public void NameChangeBiz(Player player, string returnString, string key)
        {
            int nameChangeBizPrice = 50000;
            
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsMemberOfBusiness() || !dbPlayer.GetActiveBusinessMember().Manage) return;

            if (!Regex.IsMatch(returnString, @"^[a-zA-Z_-]+$"))
            {
                dbPlayer.SendNewNotification("Bitte gib einen Namen mit Buchstaben (optional _ und -) an!.");
                return;
            }

            if (returnString.Length > 40 || returnString.Length < 7)
            {
                dbPlayer.SendNewNotification("Der Name ist zu lang oder zu kurz!");
                return;
            }

            if(!dbPlayer.TakeBankMoney(nameChangeBizPrice))
            {
                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(nameChangeBizPrice));
                return;
            }

            dbPlayer.SendNewNotification($"Der Name des Businesses wurde auf {returnString} gesetzt!");
            
            var business = BusinessModule.Instance.Get(dbPlayer.ActiveBusinessId);
            if (business == null)
            {
                dbPlayer.SendNewNotification($"Der Name des Businesses wurde konnte nicht geändert werden! #362");
                return;
            }

            BusinessModule.Instance.Update(business.Id, business, "business", $"id={business.Id}", "name", MySqlHelper.EscapeString(returnString));

            // Eintragen beim Business Besitzer im Banking Verlauf.
            dbPlayer.AddPlayerBankHistory(-nameChangeBizPrice, "Business umbenannt");
            return;
        }
    }
}
