using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Crime.PoliceAkten;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.LeitstellenPhone;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using Logger = VMP_CNR.Module.Logging.Logger;

namespace VMP_CNR.Module.Computer.Apps.PoliceAktenSearchApp
{
    public class PoliceEditPersonApp : SimpleApp
    {
        public PoliceEditPersonApp() : base("PoliceEditPersonApp")
        {
        }


        [RemoteEvent]
        public async void requestPersonData(Player p_Client, string p_Name, string key)
        {
            if (!p_Client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;


            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, p_Name)) return;

            var foundPlayer = Players.Players.Instance.FindPlayer(p_Name);
            if (foundPlayer == null || !foundPlayer.IsValid())
            {
                try
                {
                    // Get Person CustomData
                    using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var keyCmd = keyConn.CreateCommand())
                    {
                        await keyConn.OpenAsync();
                        keyCmd.CommandText =
                            $"SELECT player.id, ownHouse, gov_level, hr.house_id, pc.membership, pc.phone  FROM `player` LEFT JOIN house_rents hr ON hr.player_id = player.id LEFT JOIN player_customdata pc ON pc.player_id = player.id WHERE Name LIKE '%{p_Name}%';";
                        using (var keyReader = await keyCmd.ExecuteReaderAsync())
                        {
                            if (keyReader.HasRows)
                            {
                                while (await keyReader.ReadAsync())
                                {
                                    uint playerId = keyReader.GetUInt32("id");
                                    uint ownHouse = keyReader.GetUInt32("ownHouse");
                                    string govLevel = keyReader.GetString("gov_level");

                                    uint rentHouse = 0;
                                    if (await keyReader.IsDBNullAsync(keyReader.GetOrdinal("house_id")) == false)
                                    {
                                        rentHouse = keyReader.GetUInt32("house_id");
                                    }


                                    string note = "";
                                    string adress = "";

                                    string membership = "";
                                    string phone = "";

                                    if (await keyReader.IsDBNullAsync(keyReader.GetOrdinal("membership")) == false)
                                    {
                                        membership = keyReader.GetString("membership");
                                    }

                                    if (await keyReader.IsDBNullAsync(keyReader.GetOrdinal("phone")) == false)
                                    {
                                        phone = keyReader.GetString("phone");
                                    }

                                    if ((dbPlayer.Team.IsCops() && dbPlayer.TeamRank >= 10) || govLevel.Length > 0)
                                    {
                                        note = "Sicherheitsstufe " + govLevel;
                                    }

                                    if (ownHouse > 0)
                                    {
                                        adress = "Haus " + ownHouse;
                                    }
                                    else if (rentHouse > 0)
                                    {
                                        adress = "Mieter " + rentHouse;
                                    }

                                    uint currBizId = 0;

                                    using (var connA = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                                    using (var cmdA = connA.CreateCommand())
                                    {
                                        await connA.OpenAsync();
                                        cmdA.CommandText = $"SELECT * FROM `business_members` WHERE `player_id` = '{dbPlayer.Id}';";
                                        using (var readerA = await cmdA.ExecuteReaderAsync())
                                        {
                                            if (readerA.HasRows)
                                            {
                                                while (await readerA.ReadAsync())
                                                {
                                                    currBizId = readerA.GetUInt32("business_id");
                                                    break;
                                                }
                                            }
                                        }
                                        await connA.CloseAsync();
                                    }

                                    Business.Business biz = Business.BusinessModule.Instance.GetAll().Values.ToList().Where(b => b.Id == currBizId).FirstOrDefault();
                                    if (biz != null && biz.GovRegisterState)
                                    {
                                        note += note.Length <= 0 ? "" : " " + "Business: " + biz.Name;
                                    }

                                    CustomData customData = new CustomData(playerId, adress, membership, phone, note, dbPlayer.CanAktenView());
                                    Logger.Debug(NAPI.Util.ToJson(customData));
                                    TriggerNewClient(p_Client, "responsePersonData", NAPI.Util.ToJson(customData));
                                }
                            }
                            else return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
                return;
            }
            else
            {
                foundPlayer.CustomData.CanAktenView = dbPlayer.CanAktenView();

                string note = "";

                if ((dbPlayer.Team.IsCops() && dbPlayer.TeamRank >= 10) || foundPlayer.GovLevel.Length > 0)
                {
                    note = "Sicherheitsstufe " + foundPlayer.GovLevel;
                }

                if (foundPlayer.OwnHouse[0] > 0)
                {
                    foundPlayer.CustomData.Address = "Haus " + foundPlayer.OwnHouse[0];
                }
                else if (foundPlayer.IsTenant())
                {
                    HouseRent tentant = foundPlayer.GetTenant();
                    if (tentant != null) foundPlayer.CustomData.Address = "Mieter " + tentant.HouseId;
                }

                if(foundPlayer.ActiveBusinessId != 0)
                {
                    Business.Business biz = Business.BusinessModule.Instance.GetAll().Values.ToList().Where(b => b.Id == foundPlayer.ActiveBusinessId).FirstOrDefault();
                    if(biz != null && biz.GovRegisterState)
                    {
                        note += note.Length <=0 ? "" : " " + "Business: " + biz.Name;
                    }
                }

                Logger.Debug(NAPI.Util.ToJson(new CustomDataJson(foundPlayer.CustomData, note)));
                TriggerNewClient(p_Client, "responsePersonData", NAPI.Util.ToJson(new CustomDataJson(foundPlayer.CustomData, note)));
                return;
            }
        }

        [RemoteEvent]
        public async Task requestAktenList(Player player, string searchQuery, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, searchQuery)) return;

            var foundPlayer = Players.Players.Instance.FindPlayer(searchQuery);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;

            await Task.Delay(500);

            if (!dbPlayer.CanAktenView())
            {
                return;
            }

            TriggerNewClient(player, "responseAktenList", NAPI.Util.ToJson(PoliceAktenModule.Instance.GetPlayerClientJsonAkten(foundPlayer)));
        }

        [RemoteEvent]
        public async Task requestLicenses(Player player, string searchQuery, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, searchQuery)) return;

            var foundPlayer = Players.Players.Instance.FindPlayer(searchQuery);
            if (foundPlayer == null || !foundPlayer.IsValid())
            {
                try
                {
                    List<LicenseJson> licenseJsons = new List<LicenseJson>();

                    // Get Person CustomData
                    using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var keyCmd = keyConn.CreateCommand())
                    {
                        await keyConn.OpenAsync();
                        keyCmd.CommandText =
                            $"SELECT Lic_Car, Lic_LKW, Lic_Bike, Lic_PlaneA, Lic_PlaneB, Lic_Boot, Lic_Gun, Lic_Biz, Lic_FirstAID, Lic_Hunting, lic_transfer, Lic_Taxi FROM `player` WHERE Name LIKE '%{searchQuery}%';";
                        using (var keyReader = await keyCmd.ExecuteReaderAsync())
                        {
                            if (keyReader.HasRows)
                            {
                                while (await keyReader.ReadAsync())
                                {
                                    licenseJsons.Add(new LicenseJson() { Name = "Motorradschein", Value = (keyReader.GetInt32("Lic_Bike") == 2 ? 1 : keyReader.GetInt32("Lic_Bike")) });
                                    licenseJsons.Add(new LicenseJson() { Name = "Führerschein", Value = (keyReader.GetInt32("Lic_Car") == 2 ? 1 : keyReader.GetInt32("Lic_Car")) });
                                    licenseJsons.Add(new LicenseJson() { Name = "Bootsschein", Value = (keyReader.GetInt32("Lic_Boot") == 2 ? 1 : keyReader.GetInt32("Lic_Boot")) });
                                    licenseJsons.Add(new LicenseJson() { Name = "LKW-Schein", Value = (keyReader.GetInt32("Lic_LKW") == 2 ? 1 : keyReader.GetInt32("Lic_LKW")) });
                                    licenseJsons.Add(new LicenseJson() { Name = "Flugschein A", Value = (keyReader.GetInt32("Lic_PlaneA") == 2 ? 1 : keyReader.GetInt32("Lic_PlaneA")) });
                                    licenseJsons.Add(new LicenseJson() { Name = "Flugschein B", Value = (keyReader.GetInt32("Lic_PlaneB") == 2 ? 1 : keyReader.GetInt32("Lic_PlaneB")) });
                                    licenseJsons.Add(new LicenseJson() { Name = "Pers. Beförderungsschein", Value = (keyReader.GetInt32("lic_transfer") == 2 ? 1 : keyReader.GetInt32("lic_transfer")) });
                                    licenseJsons.Add(new LicenseJson() { Name = "Waffenschein", Value = (keyReader.GetInt32("Lic_Gun") == 2 ? 1 : keyReader.GetInt32("Lic_Gun")) });
                                    licenseJsons.Add(new LicenseJson() { Name = "Jagdschein", Value = (keyReader.GetInt32("Lic_Hunting") == 2 ? 1 : keyReader.GetInt32("Lic_Hunting")) });
                                    licenseJsons.Add(new LicenseJson() { Name = "Erstehilfekurs", Value = (keyReader.GetInt32("Lic_FirstAID") == 2 ? 1 : keyReader.GetInt32("Lic_FirstAID")) });
                                }
                            }
                            else return;
                        }
                        await keyConn.CloseAsync();
                    }

                    // warsch zu schnell...
                    await Task.Delay(100);

                    TriggerNewClient(player, "responseLicenses", NAPI.Util.ToJson(licenseJsons));
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
                return;
            }
            else
            {
                List<LicenseJson> licenseJsons = new List<LicenseJson>();

                if (LeitstellenPhoneModule.Instance.GetByAcceptor(dbPlayer) == null)
                {
                    licenseJsons.Add(new LicenseJson() { Name = "Motorradschein", Value = (foundPlayer.Lic_Bike[0] == 2 ? 1 : foundPlayer.Lic_Bike[0]) });
                    licenseJsons.Add(new LicenseJson() { Name = "Führerschein", Value = (foundPlayer.Lic_Car[0] == 2 ? 1 : foundPlayer.Lic_Car[0]) });
                    licenseJsons.Add(new LicenseJson() { Name = "Bootsschein", Value = (foundPlayer.Lic_Boot[0] == 2 ? 1 : foundPlayer.Lic_Boot[0]) });
                    licenseJsons.Add(new LicenseJson() { Name = "LKW-Schein", Value = (foundPlayer.Lic_LKW[0] == 2 ? 1 : foundPlayer.Lic_LKW[0]) });
                    licenseJsons.Add(new LicenseJson() { Name = "Flugschein A", Value = (foundPlayer.Lic_PlaneA[0] == 2 ? 1 : foundPlayer.Lic_PlaneA[0]) });
                    licenseJsons.Add(new LicenseJson() { Name = "Flugschein B", Value = (foundPlayer.Lic_PlaneB[0] == 2 ? 1 : foundPlayer.Lic_PlaneB[0]) });
                    licenseJsons.Add(new LicenseJson() { Name = "Pers. Beförderungsschein", Value = (foundPlayer.Lic_Transfer[0] == 2 ? 1 : foundPlayer.Lic_Transfer[0]) });
                    licenseJsons.Add(new LicenseJson() { Name = "Waffenschein", Value = (foundPlayer.Lic_Gun[0] == 2 ? 1 : foundPlayer.Lic_Gun[0]) });
                    licenseJsons.Add(new LicenseJson() { Name = "Jagdschein", Value = (foundPlayer.Lic_Hunting[0] == 2 ? 1 : foundPlayer.Lic_Hunting[0]) });
                    licenseJsons.Add(new LicenseJson() { Name = "Erstehilfekurs", Value = (foundPlayer.Lic_FirstAID[0] == 2 ? 1 : foundPlayer.Lic_FirstAID[0]) });
                }
                else
                {
                    licenseJsons.Add(new LicenseJson() { Name = "Motorradschein", Value = foundPlayer.Lic_Bike[0] });
                    licenseJsons.Add(new LicenseJson() { Name = "Führerschein", Value = foundPlayer.Lic_Car[0] });
                    licenseJsons.Add(new LicenseJson() { Name = "Bootsschein", Value = foundPlayer.Lic_Boot[0] });
                    licenseJsons.Add(new LicenseJson() { Name = "LKW-Schein", Value = foundPlayer.Lic_LKW[0] });
                    licenseJsons.Add(new LicenseJson() { Name = "Flugschein A", Value = foundPlayer.Lic_PlaneA[0] });
                    licenseJsons.Add(new LicenseJson() { Name = "Flugschein B", Value = foundPlayer.Lic_PlaneB[0] });
                    licenseJsons.Add(new LicenseJson() { Name = "Pers. Beförderungsschein", Value = foundPlayer.Lic_Transfer[0] });
                    licenseJsons.Add(new LicenseJson() { Name = "Waffenschein", Value = foundPlayer.Lic_Gun[0] });
                    licenseJsons.Add(new LicenseJson() { Name = "Jagdschein", Value = foundPlayer.Lic_Hunting[0] });
                    licenseJsons.Add(new LicenseJson() { Name = "Erstehilfekurs", Value = foundPlayer.Lic_FirstAID[0] });
                }

                Logging.Logger.Debug(NAPI.Util.ToJson(licenseJsons));

                TriggerNewClient(player, "responseLicenses", NAPI.Util.ToJson(licenseJsons));
            }
        }

        [RemoteEvent]
        public void requestAkte(Player player, string playername, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, playername)) return;

            // GetValid Player by searchname
            DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(playername);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;


            if (!dbPlayer.CanAktenView())
            {
                return;
            }

            TriggerNewClient(player, "responseAkte", NAPI.Util.ToJson(PoliceAktenModule.Instance.GetOpenAkteOrNew(foundPlayer)));
        }

        [RemoteEvent]
        public void savePersonData(Player player, string playername, string address, string membership, string phone, string info, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, playername)) return;
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, address)) return;
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, membership)) return;
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, phone)) return;
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, info)) return;

            if (!dbPlayer.CanEditData())
            {
                dbPlayer.SendNewNotification("Keine Berechtigung!");
                return;
            }

            playername = MySqlHelper.EscapeString(playername);
            address = MySqlHelper.EscapeString(address);
            membership = MySqlHelper.EscapeString(membership);
            phone = MySqlHelper.EscapeString(phone);
            info = MySqlHelper.EscapeString(info);

            // GetValid Player by searchname
            DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(playername);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;

            foundPlayer.UpdateCustomData(address, membership, phone, info);
        }


        [RemoteEvent]
        public void requestOpenCrimes(Player p_Client, string p_Name, string key)
        {
            if (!p_Client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = Players.Players.Instance.FindPlayer(p_Name);
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, p_Name)) return;
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                List<CrimeJsonObject> l_List = new List<CrimeJsonObject>();
                TriggerNewClient(p_Client, "responseOpenCrimes", NAPI.Util.ToJson(l_List));
                return;
            }
            else
            {
                var l_Crimes = dbPlayer.Crimes;
                List<CrimeJsonObject> l_List = new List<CrimeJsonObject>();

                try
                {
                    foreach (var l_Reason in l_Crimes.ToList())
                    {
                        l_List.Add(new CrimeJsonObject() { id = (int)l_Reason.Id, name = l_Reason.Name, description = l_Reason.Notice });
                    }
                    TriggerNewClient(p_Client, "responseOpenCrimes", NAPI.Util.ToJson(l_List));
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }
        }

        [RemoteEvent]
        public void requestJailCosts(Player p_Client, string p_Name, string key)
        {
            if (!p_Client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = Players.Players.Instance.FindPlayer(p_Name);
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, p_Name)) return;

            TriggerNewClient(p_Client, "responseJailCosts", CrimeModule.Instance.CalcJailCosts(dbPlayer.Crimes, dbPlayer.EconomyIndex));
        }

        [RemoteEvent]
        public void requestJailTime(Player p_Client, string p_Name, string key)
        {
            if (!p_Client.CheckRemoteEventKey(key)) return;
            try
            {
                DbPlayer dbPlayer = Players.Players.Instance.FindPlayer(p_Name);
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (!MySQLHandler.IsValidNoSQLi(dbPlayer, p_Name)) return;

                TriggerNewClient(p_Client, "responseJailTime", CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes));
            }
            catch (Exception e)
            {
                Logging.Logger.Crash(e);
            }
        }
    }
    public class CustomDataJson
    {
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "membership")]
        public string Membership { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "info")]
        public string Info { get; set; }

        [JsonProperty(PropertyName = "canAktenView")]
        public bool CanAktenView { get; set; }

        [JsonProperty(PropertyName = "note")]
        public string Note { get; set; }


        public CustomDataJson(CustomData customData, string note = "")
        {
            Address = customData.Address;
            Membership = customData.Membership;
            Phone = customData.Phone;
            Info = customData.Info;
            CanAktenView = customData.CanAktenView;
            Note = note;
        }
    }

    public class LicenseJson
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }
    }

}

