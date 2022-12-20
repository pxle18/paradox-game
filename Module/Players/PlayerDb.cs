using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Buffs;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Swat;

namespace VMP_CNR.Module.Players
{
    public static class PlayerDb
    {
        public static DbPlayer GetPlayer(this Player client)
        {
            if (client == null) return null;
            if (!client.HasData("player")) return null;
            DbPlayer dbPlayer = client.GetData<DbPlayer>("player");

            if (dbPlayer is DbPlayer userlist)
            {
                return userlist;
            }

            return null;
        }

        public static bool CanInteractAntiFlood(this DbPlayer dbPlayer)
        {
            if (dbPlayer.LastInteracted.AddSeconds(10) > DateTime.Now)
            {
                dbPlayer.SendNewNotification("Bitte warte kurz!");
                return false;
            }
            else
            {
                dbPlayer.LastInteracted = DateTime.Now;
                return true;
            }
        }

        public static async Task<bool> CanPressE(this DbPlayer dbPlayer)
        {
            if (dbPlayer.LastEInteract.AddSeconds(3) > DateTime.Now)
                return false;
            else
            {
                dbPlayer.LastEInteract = DateTime.Now;
                return true;
            }
        }

        public static bool IsValid(this DbPlayer dbPlayer, bool ignorelogin = false)
        {
            if (dbPlayer == null)
                return false;

            if (!Players.Instance.DoesPlayerExists(dbPlayer.Id))
                return false;

            if (dbPlayer.Player.IsNull)
                return false;

            if (dbPlayer.Player == null)
                return false;

            if (ignorelogin || dbPlayer.AccountStatus == AccountStatus.LoggedIn)
                return true;

            return false;
        }

        public static void Save(this DbPlayer dbPlayer, bool disconnect = false)
        {
            if (dbPlayer.AccountStatus != AccountStatus.LoggedIn) return;
            var update = dbPlayer.GetUpdateQuery(disconnect);

            if (update == "") return;
            if (!update.Contains("UPDATE")) return;
            MySQLHandler.ExecuteAsync(update);
        }

        public static void SaveWeapons(this DbPlayer dbPlayer)
        {
            if (dbPlayer.AccountStatus != AccountStatus.LoggedIn) return;

            string update = "UPDATE `player` SET ";

            string xstr;
            if ((xstr = Helper.Helper.GetWeapons(dbPlayer)) != "")
            {
                update += xstr;
            }
            else return;

            update += " WHERE `id` = '" + dbPlayer.Id + "';";

            if (update == "") return;
            if (!update.Contains("UPDATE")) return;
            MySQLHandler.ExecuteAsync(update);

            dbPlayer.SaveWeaponComponents();
        }


        public static void SaveWeaponComponents(this DbPlayer dbPlayer)
        {
            if (dbPlayer.AccountStatus != AccountStatus.LoggedIn) return;

            string update = "UPDATE `player` SET ";

            string xstr;
            if ((xstr = Helper.Helper.GetWeaponComponents(dbPlayer)) != "")
            {
                update += xstr;
            }
            else return;

            update += " WHERE `id` = '" + dbPlayer.Id + "';";

            if (update == "") return;
            if (!update.Contains("UPDATE")) return;
            MySQLHandler.ExecuteAsync(update);
        }

        public static string GetUpdateQuery(this DbPlayer dbPlayer, bool disconnect = false)
        {
            string update = "UPDATE `player` SET ";
            
            dbPlayer.SaveJobSkill();
            
            List<string> ups = new List<string>();
            string xstr = "";

            string query;
            
            if (!dbPlayer.IsSwatDuty())
            {
                if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.TeamId, DbPlayer.Value.TeamId, out query))
                {
                    ups.Add(query);
                }

                if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.TeamRank, DbPlayer.Value.TeamRang, out query))
                {
                    ups.Add(query);
                }
            }
            
            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.Level, DbPlayer.Value.Level, out query))
            {
                ups.Add(query);
            }
                        
            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.Injury.Id, DbPlayer.Value.DeathStatus, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.Swat, DbPlayer.Value.Swat, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.UHaftTime, DbPlayer.Value.UHaftTime, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.Einwanderung, DbPlayer.Value.Einwanderung, out query))
            {
                ups.Add(query);
            }
            
            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.SwatDuty, DbPlayer.Value.SwatDuty, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.Teamfight, DbPlayer.Value.Teamfight, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.Paintball, DbPlayer.Value.Paintball, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.Suspension, DbPlayer.Value.Suspension, out query))
            {
                ups.Add(query);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Money, "Money")) != "")
            {
                dbPlayer.Money[1] = dbPlayer.Money[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.BlackMoney, "blackmoney")) != "")
            {
                dbPlayer.BlackMoney[1] = dbPlayer.BlackMoney[0];
                ups.Add(xstr);
            }


            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.BlackMoneyBank, "blackmoneybank")) != "")
            {
                dbPlayer.BlackMoneyBank[1] = dbPlayer.BlackMoneyBank[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.BankMoney, "BankMoney")) != "")
            {
                dbPlayer.BankMoney[1] = dbPlayer.BankMoney[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.PayDay, "payday")) != "")
            {
                dbPlayer.PayDay[1] = dbPlayer.PayDay[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.RP, "rp")) != "")
            {
                dbPlayer.RP[1] = dbPlayer.RP[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.OwnHouse, "ownHouse")) != "")
            {
                dbPlayer.OwnHouse[1] = dbPlayer.OwnHouse[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Wanteds, "wanteds")) != "")
            {
                dbPlayer.Wanteds[1] = dbPlayer.Wanteds[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Lic_Car, "Lic_Car")) != "")
            {
                dbPlayer.Lic_Car[1] = dbPlayer.Lic_Car[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Lic_LKW, "Lic_LKW")) != "")
            {
                dbPlayer.Lic_LKW[1] = dbPlayer.Lic_LKW[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Lic_Bike, "Lic_Bike")) != "")
            {
                dbPlayer.Lic_Bike[1] = dbPlayer.Lic_Bike[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Lic_PlaneA, "Lic_PlaneA")) != "")
            {
                dbPlayer.Lic_PlaneA[1] = dbPlayer.Lic_PlaneA[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Lic_PlaneB, "Lic_PlaneB")) != "")
            {
                dbPlayer.Lic_PlaneB[1] = dbPlayer.Lic_PlaneB[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Lic_Boot, "Lic_Boot")) != "")
            {
                dbPlayer.Lic_Boot[1] = dbPlayer.Lic_Boot[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Lic_Taxi, "Lic_Taxi")) != "")
            {
                dbPlayer.Lic_Taxi[1] = dbPlayer.Lic_Taxi[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Lic_Gun, "Lic_Gun")) != "")
            {
                dbPlayer.Lic_Gun[1] = dbPlayer.Lic_Gun[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Lic_Hunting, "Lic_Hunting")) != "") {
                dbPlayer.Lic_Hunting[1] = dbPlayer.Lic_Hunting[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Lic_Biz, "Lic_Biz")) != "")
            {
                dbPlayer.Lic_Biz[1] = dbPlayer.Lic_Biz[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Lic_FirstAID, "Lic_FirstAID")) != "")
            {
                dbPlayer.Lic_FirstAID[1] = dbPlayer.Lic_FirstAID[0];
                ups.Add(xstr);
            }


            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.Lic_Transfer, "lic_transfer")) != "")
            {
                dbPlayer.Lic_Transfer[1] = dbPlayer.Lic_Transfer[0];
                ups.Add(xstr);
            }


            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.spawnchange, "spawnchange")) != "")
            {
                dbPlayer.spawnchange[1] = dbPlayer.spawnchange[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.job, "job")) != "")
            {
                dbPlayer.job[1] = dbPlayer.job[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.JobSkill, "jobskills")) != "")
            {
                dbPlayer.JobSkill[1] = dbPlayer.JobSkill[0];
                ups.Add(xstr);
            }
                        
            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.JailTime, "jailtime")) != "")
            {
                dbPlayer.JailTime[1] = dbPlayer.JailTime[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.jailtimeReducing, "jailtime_reduce")) != "")
            {
                dbPlayer.jailtimeReducing[1] = dbPlayer.jailtimeReducing[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.uni_points, "uni_points")) != "")
            {
                dbPlayer.uni_points[1] = dbPlayer.uni_points[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.uni_economy, "uni_economy")) != "")
            {
                dbPlayer.uni_economy[1] = dbPlayer.uni_economy[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.uni_business, "uni_business")) != "")
            {
                dbPlayer.uni_business[1] = dbPlayer.uni_business[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.uni_workaholic, "uni_workaholic")) != "")
            {
                dbPlayer.uni_workaholic[1] = dbPlayer.uni_workaholic[0];
                ups.Add(xstr);
            }


            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.HasPerso, "Perso")) != "")
            {
                dbPlayer.HasPerso[1] = dbPlayer.HasPerso[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.donator, "Donator")) != "")
            {
                dbPlayer.donator[1] = dbPlayer.donator[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.deadtime, "Deadtime")) != "")
            {
                dbPlayer.deadtime[1] = dbPlayer.deadtime[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.fspawn, "fspawn")) != "")
            {
                dbPlayer.fspawn[1] = dbPlayer.fspawn[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataString(dbPlayer.hasPed, "hasPed")) != "")
            {
                dbPlayer.hasPed[1] = dbPlayer.hasPed[0];
                ups.Add(xstr);
            }
            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.timeban, "timeban")) != "")
            {
                dbPlayer.timeban[1] = dbPlayer.timeban[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataString(dbPlayer.job_skills, "job_skills")) != "")
            {
                dbPlayer.job_skills[1] = dbPlayer.job_skills[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.warns, "warns")) != "")
            {
                dbPlayer.warns[1] = dbPlayer.warns[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.fgehalt, "fgehalt")) != "")
            {
                dbPlayer.fgehalt[1] = dbPlayer.fgehalt[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.paycheck, "paycheck")) != "")
            {
                dbPlayer.paycheck[1] = dbPlayer.paycheck[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.handy, "handy")) != "")
            {
                if (!dbPlayer.HasData("nsaChangedNumber")) 
                {
                    dbPlayer.handy[1] = dbPlayer.handy[0];
                    ups.Add(xstr);
                }
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.guthaben, "guthaben")) != "")
            {
                dbPlayer.guthaben[1] = dbPlayer.guthaben[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.married, "married")) != "")
            {
                dbPlayer.married[1] = dbPlayer.married[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.grade, "grade")) != "")
            {
                dbPlayer.grade[1] = dbPlayer.grade[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.food, "food")) != "")
            {
                dbPlayer.food[1] = dbPlayer.food[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.drink, "drink")) != "")
            {
                dbPlayer.drink[1] = dbPlayer.drink[0];
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.ComplainPlayerDataInt(dbPlayer.zwd, "zwd")) != "")
            {
                dbPlayer.zwd[1] = dbPlayer.zwd[0];
                ups.Add(xstr);
            }


            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.Duty, DbPlayer.Value.Duty, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.IsCuffed, DbPlayer.Value.IsCuffed, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.IsTied, DbPlayer.Value.IsTied, out query))
            {
                ups.Add(query);
            }

            if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.MetaData.Health, DbPlayer.Value.Hp, out query))
            {
                ups.Add(query);
            }

            // niet update in pb or gangwar...
            if (dbPlayer.DimensionType[0] != DimensionType.Paintball && dbPlayer.DimensionType[0] != DimensionType.Gangwar && dbPlayer.Paintball == 0)
            {
                if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.MetaData.Armor, DbPlayer.Value.Armor, out query))
                {
                    ups.Add(query);
                }
            }
            // Wenn DC Speicher lastpos, duty, hp und Armor
            if (disconnect)
            {
                ups.Add("`tax_sum` = '" + dbPlayer.VehicleTaxSum + "'");

                if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.IsCuffed, DbPlayer.Value.IsCuffed, out query))
                {
                    ups.Add(query);
                }

                if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.IsTied, DbPlayer.Value.IsTied, out query))
                {
                    ups.Add(query);
                }

                if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.MetaData.Health, DbPlayer.Value.Hp, out query))
                {
                    ups.Add(query);
                }

                if (dbPlayer.DimensionType[0] != DimensionType.Paintball && dbPlayer.DimensionType[0] != DimensionType.Gangwar && dbPlayer.Paintball == 0)
                { 
                    if (Helper.Helper.CheckPlayerData(dbPlayer, dbPlayer.MetaData.Armor, DbPlayer.Value.Armor, out query))
                    {
                        ups.Add(query);
                    }
                }
            }
                        

            // Save immer Injured Position
            if (dbPlayer.IsInjured())
            {
                // Position Saving
                string px = "";
                string py = "";
                string pz = "";

                if (dbPlayer.dead_x[0] != dbPlayer.dead_x[1])
                {
                    px = dbPlayer.dead_x[0].ToString().Replace(",", ".");
                }

                if (dbPlayer.dead_y[0] != dbPlayer.dead_y[1])
                {
                    py = dbPlayer.dead_y[0].ToString().Replace(",", ".");
                }

                if (dbPlayer.dead_z[0] != dbPlayer.dead_z[1])
                {
                    pz = dbPlayer.dead_z[0].ToString().Replace(",", ".");
                }
                
                // Manuell die Deathpos
                if (px != "") ups.Add("`dead_x` = '" + px + "'");
                if (py != "") ups.Add("`dead_y` = '" + py + "'");
                if (pz != "") ups.Add("`dead_z` = '" + pz + "'");
            }

            // Immer Position aus dem MetaData Saven
            string lx = dbPlayer.MetaData.Position.X.ToString().Replace(",", ".");
            string ly = dbPlayer.MetaData.Position.Y.ToString().Replace(",", ".");
            string lz = dbPlayer.MetaData.Position.Z.ToString().Replace(",", ".");
            string heading = dbPlayer.MetaData.Heading.ToString().Replace(",", ".");
            
            // Manuell die lastpos
            if (lx != "") ups.Add("`pos_x` = '" + lx + "'");
            if (ly != "") ups.Add("`pos_y` = '" + ly + "'");
            if (lz != "") ups.Add("`pos_z` = '" + lz + "'");
            if (heading != "") ups.Add("`pos_heading` = '" + heading + "'");
            ups.Add("`dimension` = '" + dbPlayer.MetaData.Dimension + "'");
            
            if (dbPlayer.SocialClubName == "") ups.Add("`SCName` = '" + dbPlayer.Player.SocialClubName + "'");

            if ((xstr = Helper.Helper.GetWeapons(dbPlayer)) != "")
            {
                ups.Add(xstr);
            }

            if ((xstr = Helper.Helper.GetWeaponComponents(dbPlayer)) != "")
            {
                ups.Add(xstr);
            }

            string updateX = "";
            if (ups.Count > 0)
            {
                updateX = string.Join(", ", ups);
                if (!disconnect)
                {
                    updateX = updateX + ", `Online` = '1'";
                }
                else
                {
                    updateX = updateX + ", `Online` = '0'";
                }

                updateX = updateX + ", `lastSaved` = '" +
                          Helper.Helper.GetTimestamp(DateTime.Now) + "'";
                updateX = update + updateX + " WHERE `id` = '" + dbPlayer.Id + "';";
            }

            return updateX;
        }

        public static void SaveWallpaper(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync("UPDATE player SET `wallpaperId` = '" + dbPlayer.Wallpaper.Id + "' WHERE id = '" + dbPlayer.Id + "';");
        }

        public static void SaveRingtone(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync("UPDATE player SET `klingeltonId` = '" + dbPlayer.Ringtone.Id + "' WHERE id = '" + dbPlayer.Id + "';");
        }

        public static void SaveJobSkill(this DbPlayer dbPlayer)
        {
            string playerjob = Convert.ToString(dbPlayer.job[0]);
            string jobskills = dbPlayer.job_skills[0];
            int actualskill = Convert.ToInt32(dbPlayer.JobSkill[0]);
            string newinventory = "";
            bool found = false;

            if (jobskills != "")
            {
                string[] Items = jobskills.Split(',');
                foreach (string item in Items)
                {
                    string[] parts = item.Split(':');
                    if (parts[0] == playerjob)
                    {
                        if (newinventory == "")
                        {
                            newinventory = playerjob + ":" + Convert.ToString(actualskill);
                        }
                        else
                        {
                            newinventory =
                                newinventory + "," + playerjob + ":" +
                                Convert.ToString(actualskill);
                        }

                        found = true;
                    }
                    else
                    {
                        if (newinventory == "")
                        {
                            newinventory = parts[0] + ":" + parts[1];
                        }
                        else
                        {
                            newinventory = newinventory + "," + parts[0] + ":" + parts[1];
                        }
                    }
                }

                if (!found)
                {
                    if (newinventory == "")
                    {
                        newinventory = playerjob + ":" + Convert.ToString(actualskill);
                    }
                    else
                    {
                        newinventory =
                            newinventory + "," + playerjob + ":" + Convert.ToString(actualskill);
                    }
                }
            }
            else
            {
                newinventory = playerjob + ":" + Convert.ToString(actualskill);
            }

            dbPlayer.job_skills[0] = newinventory;
        }



    }
}