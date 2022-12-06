using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Computer.Apps.FahrzeugUebersichtApp;
using VMP_CNR.Module.Computer.Apps.StreifenApp;
using VMP_CNR.Module.Email;
using VMP_CNR.Module.Export;
using VMP_CNR.Module.LeitstellenPhone;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Swat;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.VehicleRent;
using VMP_CNR.Module.Vehicles;
using VMP_CNR.Module.Voice;
using VMP_CNR.Module.Zone;

namespace VMP_CNR.Module.Computer.Apps.ExportApp.Apps
{
    public class Streife
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "vehid")]
        public uint VehicleId { get; set; }

        [JsonProperty(PropertyName = "vehname")]
        public string VehicleName { get; set; }

        [JsonProperty(PropertyName = "location")]
        public string Location { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "officers")]
        public List<StreifeOfficer> Officers { get; set; }

        [JsonIgnore]
        public List<DbPlayer> OfficersPlayers { get; set; }

        [JsonProperty(PropertyName = "state")]
        public int State { get; set; }


        [JsonProperty(PropertyName = "kfz")]
        public int kfz { get; set; }
        public Streife(uint id, string name, uint vehid)
        {
            Id = id;
            Name = name;
            VehicleId = vehid;
            Officers = new List<StreifeOfficer>();
            OfficersPlayers = new List<DbPlayer>();
            State = 2;
            kfz = 1;
        }
    }

    public class StreifeOfficer
    {
        [JsonIgnore]
        public uint PlayerId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "rank")]
        public uint Rank { get; set; }

        [JsonProperty(PropertyName = "funk")]
        public string Funk { get; set; }

        [JsonProperty(PropertyName = "handy")]
        public uint Handy { get; set; }
    }

    public class InfoData
    {
        [JsonProperty(PropertyName = "items")]
        public List<StreifenInfoItem> items { get; set; }

        [JsonProperty(PropertyName = "state")]
        public int State { get; set; }
    }

    public class StreifenInfoItem
    {
        [JsonProperty(PropertyName = "desc")]
        public string Desc { get; set; }

        [JsonProperty(PropertyName = "val")]
        public string Value { get; set; }

        public StreifenInfoItem(string desc, string val)
        {
            Desc = desc;
            Value = val;
        }
    }

    public class StreifenApp : SimpleApp
    {
        public StreifenApp() : base("StreifenApp") { }

        [RemoteEvent]
        public void requestCurrentStreifen(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (!StreifenAppModule.Instance.TeamStreifen.ContainsKey(dbPlayer.TeamId)) return;

            var cp = StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].ToList();

            List<Streife> ownStreife = new List<Streife>();
            List<Streife> vehicleStreifen = new List<Streife>();
            List<Streife> bikeStreifen = new List<Streife>();
            List<Streife> airStreifen = new List<Streife>();
            List<Streife> boatStreifen = new List<Streife>();

            foreach (Streife streife in cp)
            {
                SxVehicle streifeVeh = StreifenAppModule.Instance.GetVehicleByStreife(streife);
                if(streifeVeh == null || !streifeVeh.IsValid() || streifeVeh.teamid != dbPlayer.TeamId)
                {
                    streife.VehicleName = "Unbekannt";
                    streife.Location = "";
                    streife.kfz = 1;
                }
                else
                {
                    streife.VehicleName = streifeVeh.GetName();
                    Zone.Zone loc = ZoneModule.Instance.GetZone(streifeVeh.entity.Position);
                    if (loc != null)
                    {
                        streife.Location = loc.Name;
                    }
                    else streife.Location = "";

                    if (streifeVeh.Data.ClassificationId == 2 || streifeVeh.Data.ClassificationId == 7)
                    {
                        streife.kfz = 4;
                    }
                    else if (streifeVeh.Data.ClassificationId == 8 || streifeVeh.Data.ClassificationId == 9)
                    {
                        streife.kfz = 3;
                    }
                    else if (streifeVeh.Data.ClassificationId == 3)
                    {
                        streife.kfz = 2;
                    }
                    else streife.kfz = 1;
                }

                streife.Officers.Clear();

                foreach(DbPlayer officer in streife.OfficersPlayers.ToList())
                {
                    if (officer == null || !officer.IsValid()) continue;

                    double funk = VoiceModule.Instance.getPlayerFrequenz(officer);
                    string funkstr = funk.ToString().Replace(',', '.');
                    if (funk <= 0) funkstr = "Aus";

                    int handyNumber = (int)officer.handy[0];

                    if(officer.HasData("nsaChangedNumber"))
                    {
                        handyNumber = (int)officer.GetData("nsaChangedNumber");
                    }

                    streife.Officers.Add(
                        new StreifeOfficer() { 
                            PlayerId = officer.Id, 
                            Funk = funkstr, 
                            Handy = officer.handy[0], 
                            Name = officer.GetName(), 
                            Rank = officer.TeamRank 
                        });
                }

                if(streife.OfficersPlayers.Contains(dbPlayer))
                {
                    ownStreife.Add(streife);
                }
                else if(streife.kfz == 1)
                {
                    vehicleStreifen.Add(streife);
                }
                else if (streife.kfz == 2)
                {
                    boatStreifen.Add(streife);
                }
                else if (streife.kfz == 3)
                {
                    airStreifen.Add(streife);
                }
                else if (streife.kfz == 4)
                {
                    bikeStreifen.Add(streife);
                }
            }

            List<Streife> result = new List<Streife>();

            vehicleStreifen = vehicleStreifen.OrderBy(o => o.Name).ToList();
            bikeStreifen    = bikeStreifen.OrderBy(o => o.Name).ToList();
            airStreifen     = airStreifen.OrderBy(o => o.Name).ToList();
            boatStreifen    = boatStreifen.OrderBy(o => o.Name).ToList();

            result.AddRange(ownStreife);
            result.AddRange(vehicleStreifen);
            result.AddRange(bikeStreifen);
            result.AddRange(airStreifen);
            result.AddRange(boatStreifen);

            TriggerNewClient(client, "responseStreifenData", NAPI.Util.ToJson(result));
        }

        [RemoteEvent]
        public void createStreife(Player client, string name, string vehidObj, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (!UInt32.TryParse(vehidObj, out uint vehid)) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, name)) return;

            if (vehid < 0 || vehid > 999999) return;

            if (name.Contains('"') || name.Contains("'") || name.Contains('\\') || name.Contains('/') || name.Contains('\n'))
            {
                dbPlayer.SendNewNotification("Bitte gib einen Namen mit Buchstaben (optional _ ,+,& und -) an!.");
                return;
            }

            if (!StreifenAppModule.Instance.TeamStreifen.ContainsKey(dbPlayer.TeamId)) return;

            Streife streife = new Streife(StreifenAppModule.Instance.countId, name, vehid);
            StreifenAppModule.Instance.countId++;

            SxVehicle sxVehicle = null;
            if (vehid != 999)
                sxVehicle = VehicleHandler.Instance.GetTeamVehicles(dbPlayer.TeamId).Where(x => x.databaseId == vehid).FirstOrDefault();

            StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].Add(streife);
            StreifenAppModule.Instance.StreifenFahrzeuge[streife.Id] = sxVehicle;

            // for refresh LG
            this.requestCurrentStreifen(client, key);
        }

        [RemoteEvent]
        public void updateStreife(Player client, uint streifeId, string streifeName, string streifeVehIdObj, string streifeCode, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (!UInt32.TryParse(streifeVehIdObj, out uint streifeVehId)) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, streifeName)) return;
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, streifeCode)) return;

            if (streifeId < 0 || streifeId > 9999) return;
            if (streifeVehId < 0 || streifeVehId > 999999) return;


            if (streifeName.Contains('"') || streifeName.Contains("'") || streifeName.Contains('\\') || streifeName.Contains('/'))
            {
                dbPlayer.SendNewNotification("Bitte gib einen Namen mit Buchstaben (optional _ ,+,& und -) an!.");
                return;
            }

            if (streifeCode.Contains('"') || streifeCode.Contains("'") || streifeCode.Contains('\\') || streifeCode.Contains('/'))
            {
                dbPlayer.SendNewNotification("Bitte gib einen Namen mit Buchstaben (optional _ ,+,& und -) an!.");
                return;
            }

            if (!StreifenAppModule.Instance.TeamStreifen.ContainsKey(dbPlayer.TeamId)) return;


            Streife streife = StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].ToList().Where(s => s.Id == streifeId).FirstOrDefault();


            if (streife == null) return;

            // Remove Old
            StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].Remove(streife);

            streife.Name = streifeName;
            streife.VehicleId = streifeVehId;
            streife.Code = streifeCode;

            SxVehicle sxVehicle = null;
            if (streifeVehId != 999)
                sxVehicle = VehicleHandler.Instance.GetTeamVehicles(dbPlayer.TeamId).Where(x => x.databaseId == streifeVehId).FirstOrDefault();

            // Add new
            StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].Add(streife);
            StreifenAppModule.Instance.StreifenFahrzeuge[streife.Id] = sxVehicle;

            // for refresh LG
            this.requestCurrentStreifen(client, key);
        }

        [RemoteEvent]
        public void askSupportStreife(Player client, string streifeIdObj, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (!UInt32.TryParse(streifeIdObj, out uint streifeId)) return;

            if (streifeId < 0 || streifeId > 9999) return;
            if (!StreifenAppModule.Instance.TeamStreifen.ContainsKey(dbPlayer.TeamId)) return;


            Streife streife = StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].ToList().Where(s => s.Id == streifeId).FirstOrDefault();
            if (streife == null) return;

            foreach(DbPlayer iPlayer in streife.OfficersPlayers.ToList())
            {
                if (iPlayer == null || !iPlayer.IsValid()) continue;

                iPlayer.SendNewNotification($"Ihre Einheit wurde von {dbPlayer.GetName()} angefordert, GPS markiert!");
                iPlayer.Player.TriggerNewClient("setPlayerGpsMarker", dbPlayer.Player.Position.X, dbPlayer.Player.Position.Y);
            }

            dbPlayer.SendNewNotification($"Sie haben {streife.Name} als Unterstützung angefordert!");
        }

        [RemoteEvent]
        public void findStreife(Player client, string streifeIdObj, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (!UInt32.TryParse(streifeIdObj, out uint streifeId)) return;

            if (streifeId < 0 || streifeId > 9999) return;
            if (!StreifenAppModule.Instance.TeamStreifen.ContainsKey(dbPlayer.TeamId)) return;


            Streife streife = StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].ToList().Where(s => s.Id == streifeId).FirstOrDefault();
            if (streife == null) return;

            SxVehicle streifeVeh = StreifenAppModule.Instance.GetVehicleByStreife(streife);
            if (streifeVeh == null || !streifeVeh.IsValid() || streifeVeh.teamid != dbPlayer.TeamId) return;

            client.TriggerNewClient("setPlayerGpsMarker", streifeVeh.entity.Position.X, streifeVeh.entity.Position.Y);
            dbPlayer.SendNewNotification($"Sie haben die Streife {streife.Name} geortet!");
        }

        [RemoteEvent]
        public void deleteStreife(Player client, string streifeIdObj, string key )
        {
            if (!client.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (!UInt32.TryParse(streifeIdObj, out uint streifeId)) return;

            if (streifeId < 0 || streifeId > 9999) return;
            if (!StreifenAppModule.Instance.TeamStreifen.ContainsKey(dbPlayer.TeamId)) return;


            Streife streife = StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].ToList().Where(s => s.Id == streifeId).FirstOrDefault();
            if (streife == null) return;

            // Remove
            StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].Remove(streife);
            StreifenAppModule.Instance.StreifenFahrzeuge.TryRemove(streife.Id, out SxVehicle sxVehicle);

            // for refresh LG
            this.requestCurrentStreifen(client, key);
        }

        [RemoteEvent]
        public void addOfficerToStreife(Player client, string streifeIdObj, string OfficerName, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (!UInt32.TryParse(streifeIdObj, out uint streifeId)) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, OfficerName)) return;

            if (streifeId < 0 || streifeId > 9999) return;
            DbPlayer target = Players.Players.Instance.FindPlayer(OfficerName);
            if(target == null || !target.IsValid() || !target.IsInDuty() ||
                (
                target.TeamId != (int)TeamTypes.TEAM_POLICE && 
                target.TeamId != (int)TeamTypes.TEAM_ARMY && 
                target.TeamId != (int)TeamTypes.TEAM_FIB &&
                target.TeamId != (int)TeamTypes.TEAM_DPOS &&
                target.TeamId != (int)TeamTypes.TEAM_SWAT &&
                target.TeamId != (int)TeamTypes.TEAM_MEDIC
                ))
            {
                return;
            }

            if (!StreifenAppModule.Instance.TeamStreifen.ContainsKey(dbPlayer.TeamId)) return;

            if (target.TeamId != dbPlayer.TeamId) // Beamter möchte einen Beamten einer anderen Behörde hinzufügen
            {
                if (dbPlayer.IsSwatDuty()) // SWAT soll FIB, LSPD, SWAT hinzufügen können
                {
                    if (target.TeamId != (int)TeamTypes.TEAM_POLICE && target.TeamId != (int)TeamTypes.TEAM_FIB && !target.IsSwatDuty())
                        return;
                }
                else
                {
                    // LSPD, FIB soll gegenseitig nur hinzufügen können, falls der Beamte der anderen Behörde im SWAT Dienst ist
                    if ((dbPlayer.TeamId == (int)TeamTypes.TEAM_POLICE || dbPlayer.TeamId == (int)TeamTypes.TEAM_FIB) && !target.IsSwatDuty())
                        return;
                }
            }


            Streife streife = StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].ToList().Where(s => s.Id == streifeId).FirstOrDefault();
            if (streife == null) return;

            // Remove Old
            StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].Remove(streife);

            if (!streife.OfficersPlayers.Contains(target)) streife.OfficersPlayers.Add(target);

            // Add new
            StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].Add(streife);

            // for refresh LG
            this.requestCurrentStreifen(client, key);
        }


        [RemoteEvent]
        public void removeOfficerFromStreife(Player client, string streifeIdObj, string OfficerName, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (!UInt32.TryParse(streifeIdObj, out uint streifeId)) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, OfficerName)) return;

            if (streifeId < 0 || streifeId > 9999) return;
            DbPlayer target = Players.Players.Instance.FindPlayer(OfficerName);
            if (target == null || !target.IsValid())
            {
                return;
            }

            if (!StreifenAppModule.Instance.TeamStreifen.ContainsKey(dbPlayer.TeamId)) return;


            Streife streife = StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].ToList().Where(s => s.Id == streifeId).FirstOrDefault();
            if (streife == null) return;

            // Remove Old
            StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].Remove(streife);

            if (streife.OfficersPlayers.Contains(target)) streife.OfficersPlayers.Remove(target);

            // Add new
            StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].Add(streife);

            // for refresh LG
            this.requestCurrentStreifen(client, key);
        }


        [RemoteEvent]
        public void setStreifenState(Player client, string stateObj, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (!Int32.TryParse(stateObj, out int state)) return;

            if (state < 1 || state > 3) return;

            if (!StreifenAppModule.Instance.TeamStreifen.ContainsKey(dbPlayer.TeamId)) return;


            Streife streife = StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].ToList().Where(s => s.OfficersPlayers.Contains(dbPlayer)).FirstOrDefault();
            if (streife == null) return;

            // Remove Old
            StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].Remove(streife);

            streife.State = state;

            // Add new
            StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].Add(streife);

            // for refresh LG
            this.requestStreifenInfo(client, key);
            this.requestCurrentStreifen(client, key);
        }

        [RemoteEvent]
        public void requestStreifenInfo(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            InfoData infoData = new InfoData();

            List<StreifenInfoItem> Infos = new List<StreifenInfoItem>();

            Infos.Add(new StreifenInfoItem("Aktuelle Defconstufe", "" + Government.GovernmentModule.Defcon.Level));

            string Leitstelle = "Nicht besetzt";
            TeamLeitstellenObject tlo = LeitstellenPhoneModule.Instance.GetLeitstelle(dbPlayer.TeamId);

            if (tlo != null && tlo.Acceptor != null && tlo.Acceptor.IsValid())
            {
                Leitstelle = tlo.Acceptor.GetName();
            }

            Infos.Add(new StreifenInfoItem("Leitstelle", Leitstelle));

            if (dbPlayer.TeamId == (int)TeamTypes.TEAM_POLICE)
            {
                Infos.Add(new StreifenInfoItem("LSPD", "" + TeamModule.Instance.GetById((int)dbPlayer.TeamId).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));

                if(dbPlayer.TeamRank >= 2)
                {
                    Infos.Add(new StreifenInfoItem("FIB", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_FIB).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                    Infos.Add(new StreifenInfoItem("US ARMY", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_ARMY).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                    Infos.Add(new StreifenInfoItem("LSMC", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_MEDIC).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                }
                if(dbPlayer.TeamRank >= 6)
                {
                    Infos.Add(new StreifenInfoItem("SWAT", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_SWAT).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                }
            }
            else if(dbPlayer.TeamId == (int)TeamTypes.TEAM_MEDIC)
            {
                Infos.Add(new StreifenInfoItem("LSMC", "" + TeamModule.Instance.GetById((int)dbPlayer.TeamId).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));

                if (dbPlayer.TeamRank >= 6)
                {
                    Infos.Add(new StreifenInfoItem("LSPD", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_POLICE).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                }
            }
            else if (dbPlayer.TeamId == (int)TeamTypes.TEAM_ARMY)
            {
                Infos.Add(new StreifenInfoItem("ARMY", "" + TeamModule.Instance.GetById((int)dbPlayer.TeamId).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));

                if (dbPlayer.TeamRank >= 2)
                {
                    Infos.Add(new StreifenInfoItem("LSPD", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_POLICE).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                    Infos.Add(new StreifenInfoItem("FIB", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_FIB).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                    Infos.Add(new StreifenInfoItem("LSMC", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_MEDIC).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                }

                if (dbPlayer.TeamRank >= 6)
                {
                    Infos.Add(new StreifenInfoItem("SWAT", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_SWAT).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                }
            }
            else if (dbPlayer.TeamId == (int)TeamTypes.TEAM_FIB)
            {
                Infos.Add(new StreifenInfoItem("FIB", "" + TeamModule.Instance.GetById((int)dbPlayer.TeamId).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));

                if (dbPlayer.TeamRank >= 1)
                {
                    Infos.Add(new StreifenInfoItem("LSPD", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_POLICE).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                    Infos.Add(new StreifenInfoItem("US Army", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_ARMY).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                    Infos.Add(new StreifenInfoItem("LSMC", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_MEDIC).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                }

                if (dbPlayer.TeamRank >= 6)
                {
                    Infos.Add(new StreifenInfoItem("SWAT", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_SWAT).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                }
            }
            else if (dbPlayer.TeamId == (int)TeamTypes.TEAM_DPOS)
            {
                Infos.Add(new StreifenInfoItem("DPOS", "" + TeamModule.Instance.GetById((int)dbPlayer.TeamId).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));

                if (dbPlayer.TeamRank >= 6)
                {
                    Infos.Add(new StreifenInfoItem("LSPD", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_POLICE).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                }
            }
            else if (dbPlayer.TeamId == (int)TeamTypes.TEAM_SWAT)
            {
                Infos.Add(new StreifenInfoItem("SWAT", "" + TeamModule.Instance.GetById((int)dbPlayer.TeamId).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));

                Infos.Add(new StreifenInfoItem("FIB", "" + TeamModule.Instance.GetById((int)dbPlayer.TeamId).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                Infos.Add(new StreifenInfoItem("LSPD", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_POLICE).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                Infos.Add(new StreifenInfoItem("US Army", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_ARMY).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
                Infos.Add(new StreifenInfoItem("LSMC", "" + TeamModule.Instance.GetById((int)TeamTypes.TEAM_MEDIC).GetTeamMembers().Where(t => t.IsInDuty() && !t.IsInAdminDuty()).Count()));
            }

            infoData.State = 2;

            if (StreifenAppModule.Instance.TeamStreifen.ContainsKey(dbPlayer.TeamId))
            {
                Streife streife = StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].ToList().Where(s => s.OfficersPlayers.Contains(dbPlayer)).FirstOrDefault();
                if (streife != null)
                {
                    infoData.State = streife.State;
                }
            }


            infoData.items = Infos;

            TriggerNewClient(client, "responseStreifenInfo", NAPI.Util.ToJson(infoData));
        }
    }
}
