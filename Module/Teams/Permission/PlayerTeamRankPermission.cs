using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Teams.Permission
{
    public static class PlayerTeamRankPermission
    {
        public static void SetTeamRankPermission(this DbPlayer dbPlayer, bool bank, int manage, bool inventory, string title)
        {
            if (dbPlayer == null) return;
            dbPlayer.TeamRankPermission.Bank = bank;
            dbPlayer.TeamRankPermission.Manage = manage;
            dbPlayer.TeamRankPermission.Inventory = inventory;
            dbPlayer.TeamRankPermission.Title = title;

            dbPlayer.TeamRankPermission.Save();
        }
        
        public static void Save(this TeamRankPermission trp)
        {
            var query =
                string.Format(
                    $"UPDATE `player_rights` SET `r_bank` = {(trp.Bank ? 1 : 0)}, `r_manage` = {trp.Manage}, `r_inventory` = {(trp.Inventory ? 1 : 0)}, `title` = '{trp.Title}' WHERE `accountid` = '{trp.PlayerId}'");

            MySQLHandler.ExecuteAsync(query);
        }

        public static TeamRankPermission CreateTeamRankPermission(this DbPlayer dbPlayer)
        {
            var key = new TeamRankPermission(dbPlayer);
            var query =
                string.Format(
                    $"INSERT INTO `player_rights` (`accountid`, `r_bank`, `r_manage`, `r_inventory`, `title`) VALUES ('{dbPlayer.Id}', '0', '0', '0', '')");

            MySQLHandler.ExecuteAsync(query);
            return key;
        }
    }
}