using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Business.Apps
{
    public class BusinessApp : SimpleApp
    {
        public BusinessApp() : base("BusinessApp")
        {
        }

        internal class BusinessMember
        {
            [JsonProperty(PropertyName = "id")]
            public uint Id { get; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; }

            [JsonProperty(PropertyName = "bank")]
            public bool Bank { get; }

            [JsonProperty(PropertyName = "manage")]
            public bool Manage { get; }

            [JsonProperty(PropertyName = "salary")]
            public int Salary { get; }

            [JsonProperty(PropertyName = "owner")]
            public bool Owner { get; }

            [JsonProperty(PropertyName = "number")]
            public int Number { get; }

            [JsonProperty(PropertyName = "raffinery")]
            public bool Raffinery { get; }

            [JsonProperty(PropertyName = "fuelstation")]
            public bool Fuelstation { get; }

            [JsonProperty(PropertyName = "nightclub")]
            public bool NightClub { get; }
            [JsonProperty(PropertyName = "tattoo")]
            public bool Tattoo { get; }

            public BusinessMember(uint id, string name, bool bank, bool manage, bool owner, int salary, int number, bool raffinery, bool fuelstation, bool nightclub, bool tattoo)
            {
                Id = id;
                Name = name;
                Bank = bank;
                Manage = manage;
                Salary = salary;
                Owner = owner;
                Number = number;
                Raffinery = raffinery;
                Fuelstation = fuelstation;
                NightClub = nightclub;
                Tattoo = tattoo;
            }
        }

        public void SendBusinessMembers(DbPlayer dbPlayer)
        {
            var members = new List<BusinessMember>();
            var business = dbPlayer.GetActiveBusiness();

            if (business.GetMembers().Count <= 0) return;
            if (business.GetMember(dbPlayer.Id) == null) return;


            foreach (var member in business.GetMembers().ToList())
            {
                if (member.Value == null) continue;
                DbPlayer findplayer = Players.Players.Instance.FindPlayerById(member.Value.PlayerId);
                
                if (findplayer == null || !findplayer.IsValid() || findplayer.IsInAdminDuty() || findplayer.IsInGuideDuty() || findplayer.IsInGameDesignDuty()) continue;                
                
                Business.Member businessMember = member.Value;
                DbPlayer currentDbPlayer = Players.Players.Instance.GetByDbId(member.Key);
                if (currentDbPlayer == null || !currentDbPlayer.IsValid()) continue;
                
                var handyNumber = (int) currentDbPlayer.handy[0];
                if (currentDbPlayer.HasData("nsaChangedNumber"))
                {
                    handyNumber = (int)currentDbPlayer.GetData("nsaChangedNumber");
                }

                members.Add(new BusinessMember(currentDbPlayer.Id, currentDbPlayer.GetName(), businessMember.Money, businessMember.Manage, businessMember.Owner, businessMember.Salary, handyNumber, businessMember.Raffinery, businessMember.Fuelstation, businessMember.NightClub, businessMember.Tattoo));
            }

            int manage = 0;
            if (business.GetMember(dbPlayer.Id).Owner) manage = 2;
            else if (business.GetMember(dbPlayer.Id).Manage) manage = 1;

            var membersManageObject = new MembersManageObject { BusinessMemberList = members, ManagePermission = manage };
            var membersJson = JsonConvert.SerializeObject(membersManageObject);

            Logger.Debug("responseBusinessMembers " + membersJson);

            if (!string.IsNullOrEmpty(membersJson))
            {
                TriggerNewClient(dbPlayer.Player, "responseBusinessMembers", membersJson);
            }
        }

        [RemoteEvent]
        public void requestBusinessMembers(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsMemberOfBusiness()) return;
            SendBusinessMembers(dbPlayer);
        }

        [RemoteEvent]
        public void requestBusinessMOTD(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsMemberOfBusiness()) return;

            TriggerNewClient(dbPlayer.Player, "responseBusinessMOTD", dbPlayer.GetActiveBusiness().MessageOfTheDay);
        }
    }
    
    class MembersManageObject
    {
        public List<BusinessApp.BusinessMember> BusinessMemberList { get; set; }
        public int ManagePermission { get; set; }
    }
}