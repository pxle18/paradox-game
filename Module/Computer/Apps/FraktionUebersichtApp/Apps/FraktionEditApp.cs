using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Computer.Apps.FahrzeugUebersichtApp;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Forum;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Swat;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Permission;
using VMP_CNR.Module.Weapons.Component;
using VMP_CNR.Module.FIB;
using VMP_CNR.Module.Injury;

namespace VMP_CNR.Module.Computer.Apps.FraktionUebersichtApp.Apps
{
    public class FraktionEditApp : SimpleApp
    {
        public FraktionEditApp() : base("FraktionEditApp") { }

        public static uint MaxTeamGangstersPayday = 200000;

        public int GetAllOverPaydayAmount(uint TeamId)
        {
            int amount = 0;
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT SUM(fgehalt) FROM `player` WHERE team = '{TeamId}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            amount = reader.GetInt32(0);
                            break;
                        }
                    }
                }
            }
            return amount;
        }

        public int GetLowestAmountFromHigherRang(uint TeamId, uint rank)
        {
            int amount = 0;
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT fgehalt FROM `player` WHERE team = '{TeamId}' AND rang > '{rank}' AND fgehalt > 0 ORDER BY rang,fgehalt DESC LIMIT 1;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            amount = reader.GetInt32(0);
                            break;
                        }
                    }
                }
            }
            return amount;
        }

        [RemoteEvent]
        public void editFraktionMember(Player player, string playerIdObj, string memberRankObj, string paydayObj, string title, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.IsSwatDuty()) return;

            if (!Int32.TryParse(paydayObj, out int payday)) return;
            if (!UInt32.TryParse(memberRankObj, out uint memberRank)) return;
            if (!UInt32.TryParse(playerIdObj, out uint playerId)) return;

            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, title)) return;

            if (dbPlayer.TeamId == (uint) TeamList.Zivilist) return;
            var teamRankPermission = dbPlayer.TeamRankPermission;
            var editDbPlayer = Players.Players.Instance.GetByDbId(playerId);
            if (teamRankPermission.Manage < 1) return;

            title = MySqlHelper.EscapeString(title);

            if (title.Length < 0 || title.Length > 50)
            {
                dbPlayer.SendNewNotification("Diese Beschreibung ist zu nicht zulässig!");
                return;
            }

            if (editDbPlayer != null)
            {
                if (dbPlayer.Id == editDbPlayer.Id && dbPlayer.TeamRank == 12)
                {
                    memberRank = dbPlayer.TeamRank;
                }
                else
                {
                    if (memberRank >= dbPlayer.TeamRank || editDbPlayer.TeamRank >= dbPlayer.TeamRank)
                    {
                        dbPlayer.SendNewNotification("Du kannst niemandem mit deinem oder einem höheren Rang veraendern!");
                        dbPlayer.SendNewNotification("Du kannst nur bis zu einem Rang unter deinem befördern!");
                        return;
                    }
                    if (memberRank > 11)
                    {
                        dbPlayer.SendNewNotification("Rang 12 kann nicht auf der Insel vergeben werden!");
                        return;
                    };
                }

                if (!dbPlayer.Team.HasDuty && (payday < 0 || payday > 30000))
                {
                    return;
                }

                if (!dbPlayer.Team.HasDuty && memberRank < 12 && payday > 0 && (payday+500) > GetLowestAmountFromHigherRang(dbPlayer.TeamId, memberRank))
                {
                    dbPlayer.SendNewNotification($"Das eingestellte Gehalt muss niedriger sein, als der nächst höhere Rang!");
                    return;
                }

                if(!dbPlayer.Team.HasDuty && (payday + GetAllOverPaydayAmount(dbPlayer.TeamId)) > MaxTeamGangstersPayday)
                {
                    dbPlayer.SendNewNotification($"Die Summe aller Gehälter darf nicht ${MaxTeamGangstersPayday} überschreiten!");
                    return;
                }

                // 5 days not 4 Mining Corp.
                var exceptTeamId = new List<uint> { 28, 29 };

                if (!dbPlayer.Team.HasDuty && payday > 0 && editDbPlayer.fgehalt[0] != payday && editDbPlayer.LastPaydayChanged.AddDays(4) > DateTime.Now && !exceptTeamId.Contains(editDbPlayer.TeamId))
                {
                    dbPlayer.SendNewNotification($"Gehalt kann nur alle 5 Tage angepasst werden!");
                    return;
                }

                editDbPlayer.SynchronizeForum();

                if (dbPlayer.Team.HasDuty && dbPlayer.Team.Salary[(int)dbPlayer.TeamRank] > 0)
                {
                    dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {editDbPlayer.GetName()} auf Rang {memberRank} gesetzt.");
                    payday = 0;
                }
                else
                {
                    dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {editDbPlayer.GetName()} auf Rang {memberRank} gesetzt und den Payday auf {payday} $ angepasst.");
                    editDbPlayer.fgehalt[0] = payday;
                }


                editDbPlayer.LastPaydayChanged = DateTime.Now;
                editDbPlayer.SaveLastPaydayChanged();

                editDbPlayer.TeamRankPermission.Title = title;
                editDbPlayer.TeamRankPermission.Save();

                editDbPlayer.TeamRank = memberRank;
                editDbPlayer.Save();

            }
            else
            {
                using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();

                    cmd.CommandText = "SELECT player.id, player.name, player.rang, player.fgehalt, player_rights.* FROM player INNER JOIN player_rights ON player_rights.accountid = player.id WHERE player.id = @id ORDER BY rang DESC";
                    cmd.Parameters.AddWithValue("@id", playerId);
                    cmd.Prepare();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Frakmember overview = new Frakmember
                            {
                                Id = reader.GetUInt32("id"),
                                Name = reader.GetString("name"),
                                Rang = reader.GetUInt32("rang"),
                                Payday = reader.GetInt32("fgehalt"),
                                Title = "",
                                Bank = reader.GetInt32("r_bank") == 1,
                                Manage = reader.GetInt32("r_manage") >= 1,
                                Storage = reader.GetInt32("r_inventory") == 1
                            };

                            if (memberRank >= dbPlayer.TeamRank || overview.Rang >= dbPlayer.TeamRank)
                            {
                                dbPlayer.SendNewNotification("Du kannst niemandem mit deinem oder einem höheren Rang veraendern!");
                                dbPlayer.SendNewNotification("Du kannst nur bis zu einem Rang unter deinem befördern!");
                                return;
                            }
                            if (memberRank > 11)
                            {
                                dbPlayer.SendNewNotification("Rang 12 kann nicht auf der Insel vergeben werden!");
                                return;
                            };

                            if (!dbPlayer.Team.HasDuty && (payday < 0 || payday > 30000))
                            {
                                dbPlayer.SendNewNotification($"Das Gehalt darf nicht größer als 30.000$ sein!");
                                return;
                            }

                            if (!dbPlayer.Team.HasDuty && payday > 0 && memberRank < 12 && (payday + 1000) > GetLowestAmountFromHigherRang(dbPlayer.TeamId, memberRank))
                            {
                                dbPlayer.SendNewNotification($"Das eingestellte Gehalt muss niedriger sein, als der nächst höhere Rang!");
                                return;
                            }

                            if (dbPlayer.Team.IsGangster && payday > 0 && (payday + GetAllOverPaydayAmount(dbPlayer.TeamId)) > MaxTeamGangstersPayday)
                            {
                                dbPlayer.SendNewNotification($"Die Summe aller Gehälter darf nicht ${MaxTeamGangstersPayday} überschreiten!");
                                return;
                            }


                            if (dbPlayer.Team.HasDuty && dbPlayer.Team.Salary[(int)dbPlayer.TeamRank] > 0)
                            {
                                MySQLHandler.ExecuteAsync($"UPDATE player SET rang = '{memberRank}' WHERE id = '{playerId}'");
                                MySQLHandler.ExecuteAsync($"UPDATE player_rights SET title = '{title}' WHERE accountid = '{playerId}'");
                                dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {overview.Name} auf Rang {memberRank} gesetzt.");
                            }
                            else
                            {
                                MySQLHandler.ExecuteAsync($"UPDATE player SET rang = '{memberRank}' WHERE id = '{playerId}'");
                                MySQLHandler.ExecuteAsync($"UPDATE player SET fgehalt = '{payday}' WHERE id = '{playerId}' AND `lastpaydaychanged` < NOW() - INTERVAL 4 DAY;");
                                MySQLHandler.ExecuteAsync($"UPDATE player_rights SET title = '{title}' WHERE accountid = '{playerId}'");
                                dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {overview.Name} auf Rang {memberRank} gesetzt und den Payday auf {payday} $ angepasst.");
                            }
                        }
                    }
                    conn.Close();
                }
            }

        }

        [RemoteEvent]
        public void kickFraktionMember(Player player, uint playerId, int rang, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.IsSwatDuty()) return;
            if (dbPlayer.TeamId == (uint)TeamList.Zivilist) return;
            var teamRankPermission = dbPlayer.TeamRankPermission;
            var editDbPlayer = Players.Players.Instance.GetByDbId(playerId);

            if (teamRankPermission.Manage < 1) return;

            if (editDbPlayer != null)
            {
                if (dbPlayer.Id == editDbPlayer.Id || rang == 12)
                {
                    dbPlayer.SendNewNotification("Du kannst niemandem mit deinem oder einem höheren Rang veraendern!");
                    return;
                }

                editDbPlayer.SynchronizeForum();
                
                dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {editDbPlayer.GetName()} aus der Fraktion entlassen.");

                if (editDbPlayer.Team.IsGangsters())
                {
                    if (editDbPlayer.Team.IsInTeamfight())
                    {
                        editDbPlayer.RemoveWeapons();
                        editDbPlayer.ResetAllWeaponComponents();
                    }


                    editDbPlayer.LastUninvite = DateTime.Now;
                    editDbPlayer.SaveLastUninvite();
                }


                editDbPlayer.Team.RemoveMember(editDbPlayer);
                editDbPlayer.SetTeam((uint)TeamList.Zivilist);
                editDbPlayer.TeamRank = 0;
                editDbPlayer.fgehalt[0] = 0;
                editDbPlayer.Player.TriggerNewClient("updateDuty", false);
                editDbPlayer.UpdateApps();

                if (dbPlayer.TeamId == (int)TeamTypes.TEAM_FIB)
                {
                    editDbPlayer.ResetUndercover();
                    editDbPlayer.FindFlags = FindFlags.None;
                    editDbPlayer.SaveFindFlags();
                }

                editDbPlayer.RemoveParamedicLicense();
                editDbPlayer.SynchronizeForum();
                editDbPlayer.Save();

                LogHandler.LogFactionAction(editDbPlayer.Id, editDbPlayer.GetName(), dbPlayer.Team.Id, false);
            }
            else
            {
                PlayerName.PlayerName playerName = PlayerName.PlayerNameModule.Instance.Get(playerId);
                // TODO: Edit before release
                //if (!Configurations.Configuration.Instance.DevMode)
                //{
                //    if (playerName != null)
                //    {
                //        using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnectionForum()))
                //        {
                //            conn.Open();
                //            using (var cmd = conn.CreateCommand())
                //            {
                //                cmd.CommandText = PlayerForumSync.GetRemoveQueryByForumId(playerName.ForumId);
                //                cmd.ExecuteNonQueryAsync();
                //            }
                //        }
                //    }
                //}

                using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();

                    cmd.CommandText = "SELECT player.id, player.name, player.rang, player.fgehalt, player_rights.* FROM player INNER JOIN player_rights ON player_rights.accountid = player.id WHERE player.id = @id ORDER BY rang DESC";
                    cmd.Parameters.AddWithValue("@id", playerId);
                    cmd.Prepare();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Frakmember overview = new Frakmember
                            {
                                Id = reader.GetUInt32("id"),
                                Name = reader.GetString("name"),
                                Rang = reader.GetUInt32("rang"),
                                Payday = reader.GetInt32("fgehalt"),
                                Title = "",
                                Bank = reader.GetInt32("r_bank") == 1,
                                Manage = reader.GetInt32("r_manage") >= 1,
                                Storage = reader.GetInt32("r_inventory") == 1
                            };

                            if (dbPlayer.Id == overview.Id || overview.Rang == 12)
                            {
                                dbPlayer.SendNewNotification("Du kannst niemandem mit deinem oder einem höheren Rang veraendern!");
                                return;
                            }
                            
                            MySQLHandler.ExecuteAsync($"UPDATE player SET rang = '0', team = '0', mediclic = 0 WHERE id = '{playerId}'");
                            MySQLHandler.ExecuteAsync($"UPDATE player_rights SET title = '', r_bank = 0, r_inventory = 0, r_manage = 0 WHERE accountid = '{playerId}'");
                            dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {overview.Name} aus der Fraktion entlassen.");

                            if (dbPlayer.TeamId == (int)TeamTypes.TEAM_FIB)
                            {
                                MySQLHandler.ExecuteAsync($"UPDATE player SET `ucname` = '', `fib_find_flags` = 0 WHERE id = '{playerId}'");
                            }

                            LogHandler.LogFactionAction(overview.Id, overview.Name, dbPlayer.Team.Id, false);
                        }
                    }
                    conn.Close();
                }
            }

        }
    }
}