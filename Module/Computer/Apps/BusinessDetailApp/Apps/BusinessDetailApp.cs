using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Configurations;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Business.FuelStations;
using static VMP_CNR.Module.Business.Apps.BusinessListApp;
using System.Linq;
using MySql.Data.MySqlClient.Memcached;
using VMP_CNR.Module.Business.Apps;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Business.Raffinery;
using VMP_CNR.Module.Business.NightClubs;

namespace VMP_CNR.Module.Computer.Apps.BusinessDetailApp.Apps
{
  
    
    public class FuelstationObject
    {
        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }

        [JsonProperty(PropertyName = "price")]
        public int Price { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

        [JsonProperty(PropertyName = "Log")]
        public List<FuelstationLogObject> Log { get; set; }

    }

    public class RaffineryObject
    {

        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }
        [JsonProperty(PropertyName = "Log")]
        public List<RaffineryLogObject> Log { get; set; }

    }

    public class NightclubObject
    {

        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }
        [JsonProperty(PropertyName = "items")]
        public List<NightclubItemObject> Items { get; set; }

    }

    public class NightclubItemObject
    {
        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }
        [JsonProperty(PropertyName = "price")]
        public int Price { get; set; }
        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }
    }
    
    public class SimpleBusinessMemberObject
    {
        [JsonProperty(PropertyName = "id")] public uint Id { get; set; }
        
        [JsonProperty(PropertyName = "name")] public string Name { get; set; }
        
        [JsonProperty(PropertyName = "last_login")] public int LastLogin { get; set; }
    }

    public class SimpleBusinessMemberManageObject
    {
        public List<SimpleBusinessMemberObject> BusinessMemberList { get; set; }
        
        public int ManagePermission { get; set; }
    }

    public class BusinessDetailApp : SimpleApp
    {
        public void SendAllBusinessMembers(DbPlayer dbPlayer)
        {
            Business.Business business = dbPlayer.GetActiveBusiness();

            // check if someone is in business
            if (business.GetMembers().Count <= 0) return;

            // check if requesting member is in business
            if (business.GetMember(dbPlayer.Id) == null) return;

            var members = business
                .GetAllMembers()
                .Values
                .ToList()
                .ConvertAll(
                    x => new SimpleBusinessMemberObject
                    {
                        Id = x.PlayerId,
                        Name = x.Name, 
                        LastLogin = x.LastLogin
                    }
                )
                .ToList();

            int manage = 0;
            if (business.GetMember(dbPlayer.Id).Owner) manage = 2;
            else if (business.GetMember(dbPlayer.Id).Manage) manage = 1;

            var membersManageObject = new SimpleBusinessMemberManageObject
            {
                BusinessMemberList = members,
                ManagePermission = manage
            };
            var membersJson = JsonConvert.SerializeObject(membersManageObject);

            if (!string.IsNullOrEmpty(membersJson))
            {
                TriggerNewClient(dbPlayer.Player, "responseBusinessDetail", membersJson, business.Money);
            }
        }

        [RemoteEvent]
        public void requestBusinessDetailAllMembers(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsMemberOfBusiness()) return;

            SendAllBusinessMembers(dbPlayer);
        }        
        
        public BusinessDetailApp() : base("BusinessDetailApp") { }

            [RemoteEvent]
            public void requestBusinessDetailMembers(Player client, string key)
            {
                if (!client.CheckRemoteEventKey(key)) return;
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid())
                    return;

                var members = new List<BusinessApp.BusinessMember>();
                var business = dbPlayer.GetActiveBusiness();

                if (business.GetMembers().Count <= 0) return;
                if (business.GetMember(dbPlayer.Id) == null) return;

                foreach (var member in business.GetMembers().ToList())
                {
                    if (member.Value == null) continue;
                    var findplayer = Players.Players.Instance.FindPlayerById(member.Value.PlayerId);
                    if (findplayer == null || !findplayer.IsValid() || findplayer.IsInAdminDuty() || findplayer.IsInGuideDuty() || findplayer.IsInGameDesignDuty()) continue;

                    var businessMember = member.Value;
                    var currentDbPlayer = Players.Players.Instance.GetByDbId(member.Key);
                    if (currentDbPlayer == null || !currentDbPlayer.IsValid()) continue;

                    members.Add(new BusinessApp.BusinessMember(currentDbPlayer.Id, currentDbPlayer.GetName(), businessMember.Money, businessMember.Manage, businessMember.Owner, businessMember.Salary, (int)currentDbPlayer.handy[0], businessMember.Raffinery, businessMember.Fuelstation, businessMember.NightClub, businessMember.Tattoo));
                }

                int manage = 0;
                if (business.GetMember(dbPlayer.Id).Owner) manage = 2;
                else if (business.GetMember(dbPlayer.Id).Manage) manage = 1;

                var membersManageObject = new MembersManageObject { BusinessMemberList = members, ManagePermission = manage };
                var membersJson = JsonConvert.SerializeObject(membersManageObject);

                if (!string.IsNullOrEmpty(membersJson))
                {
                    TriggerNewClient(client,"responseBusinessDetail", membersJson,business.Money);
                }
  
            }

            [RemoteEvent]
            public void requestBusinessDetailFuelstation(Player client, string key)
            {
                if (!client.CheckRemoteEventKey(key)) return;
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid())
                    return;

            if (dbPlayer.GetActiveBusiness().BusinessBranch.hasFuelstation())
            {
                FuelStation fuelStation = FuelStationModule.Instance.Get(dbPlayer.GetActiveBusiness().BusinessBranch.FuelstationId);

                FuelstationObject FuelstationObject = new FuelstationObject
                {
                    Name = fuelStation.Name,
                    Price = fuelStation.Price,
                    Amount = fuelStation.Container.GetItemAmount(537),
                    Log = fuelStation.GetLogFuelstationFilled(),
                };

                TriggerNewClient(client,"responseBusinessDetail", JsonConvert.SerializeObject(FuelstationObject));
            }


            }

            [RemoteEvent]
            public void requestBusinessDetailRaffinery(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid())
                    return;

                if (dbPlayer.GetActiveBusiness().BusinessBranch.hasRaffinerie())
                {
                    Raffinery raffinery = RaffineryModule.Instance.Get(dbPlayer.GetActiveBusiness().BusinessBranch.RaffinerieId);

                    RaffineryObject raffineryObject = new RaffineryObject
                    {
                        Amount = raffinery.Container.GetItemAmount(536),
                        Log = raffinery.GetLogRaffinery(),
                    };

                    TriggerNewClient(client,"responseBusinessDetail", JsonConvert.SerializeObject(raffineryObject));
                }

            }

            [RemoteEvent]
            public void requestBusinessDetailNightclub(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid())
                    return;
                if (dbPlayer.GetActiveBusiness().BusinessBranch.hasNightClub())
                {
                    List<NightclubItemObject> nightclubItemObject = new List<NightclubItemObject>();
                    NightClub nightClub = NightClubModule.Instance.Get(dbPlayer.GetActiveBusiness().BusinessBranch.NightClubId);
                    foreach (NightClubItem nightClubItem in nightClub.NightClubShopItems)
                    {
                        NightclubItemObject data = new NightclubItemObject
                        {
                            Name = nightClubItem.Name,
                            Price = nightClubItem.Price,
                            Amount = nightClub.Container.GetItemAmount(nightClubItem.ItemId),
                        };

                        nightclubItemObject.Add(data);
                    }

                    NightclubObject nightclubObject = new NightclubObject
                    {
                        Name = nightClub.Name,
                        Items = nightclubItemObject,
                    };

                    TriggerNewClient(client,"responseBusinessDetail", JsonConvert.SerializeObject(nightclubObject));
                }

            }
            [RemoteEvent]
            public void requestBusinessDetail(Player client, string key)
            {
                if (!client.CheckRemoteEventKey(key)) return;
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid())return;
                List<string> test = new List<string>();
                if (dbPlayer.GetActiveBusiness() == null) { TriggerNewClient(client, "responseBusinessDetailLinks", "[]"); return;}

                if (dbPlayer.GetActiveBusiness().GetMember(dbPlayer.Id) != null && dbPlayer.GetActiveBusiness().GetMembers().Count > 0)
                {
                    test.Add("Mitglieder");

                    if (dbPlayer.GetActiveBusiness().BusinessBranch.hasFuelstation())
                    {
                        test.Add("Tankstelle");
                    }
                    if (dbPlayer.GetActiveBusiness().BusinessBranch.hasRaffinerie())
                    {
                        test.Add("Oelpumpe");
                    }
                    if (dbPlayer.GetActiveBusiness().BusinessBranch.hasNightClub())
                    {
                        test.Add("Club");
                    }

                    TriggerNewClient(client, "responseBusinessDetailLinks", JsonConvert.SerializeObject(test));
                }
                else
                {
                    TriggerNewClient(client, "responseBusinessDetailLinks", "[]");
                }
            }


        }
}
