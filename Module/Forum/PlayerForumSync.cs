using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Forum
{
    public static class PlayerForumSync
    {
        public static void SynchronizeJobForum(this DbPlayer dbPlayer)
        {
            if (Configuration.Instance.DevMode) return;
            if (!ServerFeatures.IsActive("forumsync")) return;

            string completeQuery = dbPlayer.GetJobRemoveQuery();
            if (dbPlayer.job[0] != 0)
            {
                uint forumGroup = dbPlayer.GetForumGroupForJob();

                if (forumGroup > 0)
                {
                    // Set user into group
                    completeQuery +=
                        $"INSERT INTO wcf1_user_to_group (`userID`, `groupID`) VALUES ('{dbPlayer.ForumId}', '{@forumGroup}');";
                }

                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnectionForum()))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = completeQuery;
                        cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        //Todo: can be made async
        public static void SynchronizeForum(this DbPlayer dbPlayer)
        {
            if (Configuration.Instance.DevMode) return;
            if (!ServerFeatures.IsActive("forumsync")) return;

            string completeQuery = dbPlayer.GetRemoveQuery();
            if (dbPlayer.TeamId > 0)
            {
                int forumGroup = dbPlayer.GetForumGroupForTeam();

                if (forumGroup > 0)
                {
                    // Set user into group
                    completeQuery +=
                        $"INSERT INTO wcf1_user_to_group (`userID`, `groupID`) VALUES ('{dbPlayer.ForumId}', '{@forumGroup}');";
                }
            }

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnectionForum()))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = completeQuery;
                    cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static string GetJobRemoveQuery(this DbPlayer dbPlayer)
        {
            var removeGroups = new List<int>();
            foreach (var kvp in JobForumSync.Instance.GetAll())
            {
                removeGroups.Add((int)kvp.Value.group_id);
            }

            string addRemSql = "";
            foreach (var group in removeGroups)
            {
                if (addRemSql == "") addRemSql = $"groupID = '{@group}'";
                else addRemSql = addRemSql + $" OR groupID = '{@group}'";
            }

            return $"DELETE FROM wcf1_user_to_group WHERE userID = '{dbPlayer.ForumId}' AND ({addRemSql});";
        }

        // Returns all Groups (used in group sync) for removing first
        private static string GetRemoveQuery(this DbPlayer dbPlayer)
        {
            var removeGroups = new List<int>();

            foreach (var kvp in TeamForumSync.Instance.GetAll())
            {
                removeGroups.Add(kvp.Value.LeaderGroup);
                removeGroups.Add(kvp.Value.MemberGroup);
            }

            string addRemSql = "";

            foreach (var group in removeGroups)
            {
                if (addRemSql == "") addRemSql = $"groupID = '{@group}'";
                else addRemSql = addRemSql + $" OR groupID = '{@group}'";
            }

            return $"DELETE FROM wcf1_user_to_group WHERE userID = '{dbPlayer.ForumId}' AND ({addRemSql});";
        }


        public static string GetRemoveQueryByForumId(uint forumId)
        {
            var removeGroups = new List<int>();

            foreach (var kvp in TeamForumSync.Instance.GetAll())
            {
                removeGroups.Add(kvp.Value.LeaderGroup);
                removeGroups.Add(kvp.Value.MemberGroup);
            }

            string addRemSql = "";

            foreach (var group in removeGroups)
            {
                if (addRemSql == "") addRemSql = $"groupID = '{@group}'";
                else addRemSql = addRemSql + $" OR groupID = '{@group}'";
            }

            return $"DELETE FROM wcf1_user_to_group WHERE userID = '{forumId}' AND ({addRemSql});";
        }

        private static int GetForumGroupForTeam(this DbPlayer dbPlayer)
        {
            if (!TeamForumSync.Instance.GetAll().ContainsKey(dbPlayer.TeamId)) return 0;

            switch (dbPlayer.TeamRankPermission.Manage)
            {
                case 2:
                    return TeamForumSync.Instance[dbPlayer.TeamId].LeaderGroup;
                case 1:
                    return TeamForumSync.Instance[dbPlayer.TeamId].LeaderGroup;
                default:
                    return TeamForumSync.Instance[dbPlayer.TeamId].MemberGroup;
            }
        }

        private static uint GetForumGroupForJob(this DbPlayer dbPlayer)
        {
            if (!JobForumSync.Instance.GetAll().ContainsKey((uint)dbPlayer.job[0])) return 0;

            return JobForumSync.Instance[(uint)dbPlayer.job[0]].group_id;
        }
    }
}