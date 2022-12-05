using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Business
{
    public static class BusinessPlayer
    {
        public static bool IsMemberOfBusiness(this DbPlayer dbPlayer)
        {
            return dbPlayer.ActiveBusinessId != 0 && BusinessModule.Instance.GetAll().Values.Where(b => b.Id == dbPlayer.ActiveBusinessId).FirstOrDefault() != null;
        }

        public static Business GetVisitedBusiness(this DbPlayer dbPlayer)
        {
            return (from business in BusinessModule.Instance.GetAll()
                where business.Value.Visitors.Contains(dbPlayer)
                select business.Value).FirstOrDefault();
        }
        
        public static Business.Member GetActiveBusinessMember(this DbPlayer player)
        {
            Business biz = player.GetActiveBusiness();
            if (biz == null) return null;

            return biz.GetMember(player.Id); ;
        }

        public static Business GetActiveBusiness(this DbPlayer player)
        {
            return BusinessModule.Instance.GetAll().Values.Where(b => b.Id == player.ActiveBusinessId).FirstOrDefault();
        }

        public static void SaveBusinessMemberships(this DbPlayer player)
        {
            player.SaveBusinessMembership(player.GetActiveBusinessMember());
        }

        public static void SaveBusinessMembership(this DbPlayer player, Business.Member member)
        {
            try
            {
                var query =
                string.Format(
                    $"UPDATE `business_members` SET `business_id` = {member.BusinessId}, `manage` = {(member.Manage ? 1 : 0)}, " +
                    $"`money` = {(member.Money ? 1:0)}, `inventory` = {(member.Inventory ? 1:0)}, `gehalt` = {member.Salary}, `owner` = {(member.Owner ? 1:0)}, " +
                    $"`fuelstation` = {(member.Fuelstation ? 1:0)}, `raffinery` = {(member.Raffinery ? 1:0)}, `nightclub` = {(member.NightClub ? 1 : 0)}, " +
                    $"`tattoo` = {(member.Tattoo ? 1:0)} " +
                    $"WHERE `player_id` = {member.PlayerId} AND `business_id` = {member.BusinessId}");

                MySQLHandler.ExecuteAsync(query);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public static void RemoveBusinessMembership(this DbPlayer player, Business business)
        {
            if (player == null || !player.IsValid() || !player.IsMemberOfBusiness()) return;
            var query = string.Format(
                $"DELETE FROM `business_members` WHERE `player_id` = {player.Id} AND business_id = {business.Id};");
            MySQLHandler.ExecuteAsync(query);
            
            if (!BusinessModule.Instance.Contains(business.Id)) return;
            Business biz = BusinessModule.Instance.GetById(business.Id);
            if (biz == null) return;

            biz.RemoveMember(player.Id);
        }

        public static void AddBusinessMembership(this DbPlayer player, Business business)
        {
            if (player.IsMemberOfBusiness()) return;
            if (business == null) return;

            var member = new Business.Member()
            {
                BusinessId = business.Id,
                PlayerId = player.Id,
                Manage = false,
                Money = false,
                Inventory = false,
                Salary = 0,
                Owner = false,
                Raffinery = false,
                Fuelstation = false,
                Tattoo = false
            };
            player.InsertBusinessMemberShip(business, member);
        }

        public static void AddBusinessOwnership(this DbPlayer player, Business business)
        {
            if (business == null) return;

            Business.Member member = new Business.Member() {
                PlayerId = player.Id,
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
            player.InsertBusinessMemberShip(business, member);
        }

        private static void InsertBusinessMemberShip(this DbPlayer player, Business business, Business.Member member)
        {
            if (player == null || !player.IsValid() || business == null || member == null) return;

            player.ActiveBusinessId = business.Id;
            player.UpdateApps();

            business.AddMember(member, player);

            var query =
                string.Format(
                    $"INSERT INTO `business_members` (`player_id`, `business_id`, `manage`, `money`, `inventory`, `gehalt`, `owner`, `raffinery`, `fuelstation`, `nightclub`, `tattoo`) " +
                    $"VALUES ('{player.Id}', '{member.BusinessId}', '{(member.Manage?1:0)}', '{(member.Money?1:0)}', '{(member.Inventory ?1:0)}', '{member.Salary}', '{(member.Owner? 1:0)}', '{(member.Raffinery?1:0)}', '{(member.Fuelstation?1:0)}', '{(member.NightClub ? 1 : 0)}', ' {(member.Tattoo?1:0)}');");
            MySQLHandler.ExecuteAsync(query);
        }
    }
}