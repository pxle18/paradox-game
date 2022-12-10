using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Banks.BankHistory;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Business.Tasks;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Jails;
using VMP_CNR.Module.Players.Buffs;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Events;
using VMP_CNR.Module.Staatsgefaengnis;
using VMP_CNR.Module.Staatskasse;
using VMP_CNR.Module.Storage;
using VMP_CNR.Module.Swat;
using VMP_CNR.Module.Tasks;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Shelter;
using VMP_CNR.Module.UHaft;

namespace VMP_CNR.Module.Players.Sync
{
    public sealed class PlayerSyncModule : Module<PlayerSyncModule>
    {
        private const int RpMultiplikator = 4;

        public static void CheckSalary(DbPlayer dbPlayer)
        {
            // Wenn DutyPaycheck dann prüfe auf onduty
            if (!dbPlayer.Team.HasDuty || !dbPlayer.IsInDuty()) return;

            int salary = dbPlayer.Team.Salary[(int)dbPlayer.TeamRank];
            if (dbPlayer.IsSwatDuty() && dbPlayer.HasData("swatOld_team") && dbPlayer.HasData("swatOld_rang"))
            {
                uint originalTeamId = dbPlayer.GetData("swatOld_team");
                uint originalRang = dbPlayer.GetData("swatOld_rang");
                Team originalTeam = TeamModule.Instance.GetById((int)originalTeamId);

                // gebe den höchsten Payday (von normaler Frak ODER Swat, falls zb lspd chief im swat nur rang 8..)
                int normalSalary = originalTeam.Salary[(int)originalRang];
                if(normalSalary > salary)
                {
                    salary = normalSalary;
                }

                // Swat gefahrenbonus
                salary += 40; // 40 * 60 = 2.400$ / H 
            }

            if(salary > 0) 
            {
                // Nachts doppeltes Gehalt
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour < 10 && dbPlayer.Team.GetsExtraNightPayday())
                    dbPlayer.GiveEarning((salary / 60) + 66); //4k nightbonus
                else
                    dbPlayer.GiveEarning(salary / 60);
            }
            else
            {
                dbPlayer.GiveEarning(dbPlayer.fgehalt[0] / 60);
            }
        }
        
        public static void PlayerPayday(DbPlayer dbPlayer)
        {
            // Start Bankhistories
            var bankHistories = new List<Banks.BankHistory.BankHistory>();

            // Level System
            if (dbPlayer.RP[0] >= dbPlayer.Level * RpMultiplikator)
            {
                dbPlayer.Level++;
                dbPlayer.uni_points[0]++;
                dbPlayer.RP[0] = 0;
                dbPlayer.SendNewNotification($"Glueckwunsch, Sie haben nun Level {dbPlayer.Level} erreicht!", title: "Level aufgestiegen!", notificationType:PlayerNotification.NotificationType.SERVER);

                LevelPoints lp = LevelPointModule.Instance.Get((uint)dbPlayer.Level);

                if (lp != null && lp.Points > 0)
                {
                    dbPlayer.GiveCWS(CWSTypes.Level, lp.Points);
                    dbPlayer.SendNewNotification($"Durch Ihr Levelup haben Sie {lp.Points} erhalten!");
                }
            }
            else
            {
                dbPlayer.RP[0]++;
            }

            //Stuff to do on Payday
            dbPlayer.PayDay[0] = 1;
            Main.lowerPlayerJobSkill(dbPlayer);

            // Money Money Money
            var total = 0;
            
            int salary = 0;

            // Gebe Server Gehalt für nicht Duty Fraktionen & staatlichen Gehalt
            if (!dbPlayer.Team.HasDuty && dbPlayer.Team.Salary[(int)dbPlayer.TeamRank] > 0)
            {
                salary += dbPlayer.Team.Salary[(int)dbPlayer.TeamRank];
            }

            // Gebe verdienst durch jbos gehalt etc
            if (dbPlayer.paycheck[0] > 0)
            {
                if(dbPlayer.Team.IsBusinessTeam)
                {
                    var teamShelter = TeamShelterModule.Instance.GetByTeam(dbPlayer.TeamId);
                    if (teamShelter != null)
                    {
                        if (teamShelter.Money - dbPlayer.paycheck[0] > 0)
                        {
                            teamShelter.TakeMoney(dbPlayer.paycheck[0]);
                            salary += dbPlayer.paycheck[0];
                            dbPlayer.Team.AddBankHistory(-dbPlayer.paycheck[0], $"Gehaltszahlung an {dbPlayer.GetName()}");
                        }
                    }
                } 
                else
                {
                    salary = salary + dbPlayer.paycheck[0];
                }
            }

            // Staatskasse abrechnung
            if (salary != 0 && dbPlayer.Team.IsStaatsfraktion())
            {
                KassenModule.Instance.StaatsKassenPaycheckAmountAll += salary;
            }

            //Reset Gehaltsanrechnung
            dbPlayer.paycheck[0] = 0;

            if (dbPlayer.IsMemberOfBusiness())
            {
                var business = dbPlayer.GetActiveBusiness();

                var member = dbPlayer.GetActiveBusinessMember();

                if (business != null && member != null)
                {
                    if (business.Money > member.Salary)
                    {
                        SynchronizedTaskManager.Instance.Add(
                            new BusinessSalaryTask(business, dbPlayer, member.Salary));

                        bankHistories.Add(new Banks.BankHistory.BankHistory
                        {
                            Name = "Business Lohn",
                            Value = member.Salary
                        });

                        if (member.Salary > 0)
                            business.AddBankHistory(-member.Salary, "Lohn - " + dbPlayer.GetName());
                    }
                }
            }
            
            //Gebe FGehalt für alle NICHT Duty Fraktionen (Gangs)
            if (dbPlayer.Team.Id != (int)TeamTypes.TEAM_CIVILIAN && dbPlayer.fgehalt[0] > 0 && !dbPlayer.Team.HasDuty)
            {
                var teamShelter = TeamShelterModule.Instance.GetByTeam(dbPlayer.TeamId);
                if (teamShelter != null)
                {
                    var usersalary = dbPlayer.fgehalt[0];
                    if (teamShelter.Money - usersalary > 0)
                    {
                        teamShelter.TakeMoney(usersalary);
                        dbPlayer.Team.AddBankHistory(-usersalary, $"Gehalt {dbPlayer.GetName()}");

                        if (dbPlayer.IsAGangster())
                        {
                            total += usersalary;

                            bankHistories.Add(new Banks.BankHistory.BankHistory
                            {
                                Name = "Fraktion - Entlohnung",
                                Value = usersalary
                            });
                        }
                        else
                        {
                            salary += usersalary;
                        }
                    }
                }
            }

            total += salary;
            bankHistories.Add(new Banks.BankHistory.BankHistory { Name = "Einkommen", Value = salary });

            if (dbPlayer.Rank.Salary > 0)
            {
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "GVMP Bonus",
                    Value = dbPlayer.Rank.Salary
                });
                total += dbPlayer.Rank.Salary;
            }

            if (dbPlayer.married[0] > 0 && dbPlayer.Team.IsStaatsfraktion())
            {
                var steuern = Convert.ToInt32(salary * 0.01);
                total -= steuern;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Steuern (Klasse 4 | 1%)",
                    Value = -steuern
                });
            }
            else if (dbPlayer.married[0] > 0)
            {
                var steuern = Convert.ToInt32(salary * 0.03);
                total -= steuern;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Steuern (Klasse 3 | 3%)",
                    Value = -steuern
                });
            }
            else if (dbPlayer.Team.IsStaatsfraktion())
            {
                var steuern = Convert.ToInt32(salary * 0.03);
                total -= steuern;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Steuern (Klasse 2 | 3%)",
                    Value = -steuern
                });
            }
            else
            {
                var steuern = Convert.ToInt32(salary * 0.15);
                total -= steuern;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Steuern (Klasse 1 | 15%)",
                    Value = -steuern
                });
            }

            int storageTax = 0;
            foreach(KeyValuePair<uint, StorageRoom> kvp in dbPlayer.GetStoragesOwned()) {
                storageTax += StorageRoomAusbaustufenModule.Instance.Get((uint)kvp.Value.Ausbaustufe).Tax;
            }

            if(storageTax != 0)
            {
                total -= storageTax;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Lagerraum Steuer",
                    Value = -storageTax
                });
            }

            //KFZ Steuer
            var steuer = dbPlayer.VehicleTaxSum;
            if (steuer > 0)
            {
                total -= steuer;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "KFZ Steuer",
                    Value = -steuer
                });
            }
            dbPlayer.VehicleTaxSum = 0;

            if(dbPlayer.InsuranceType > 0)
            {
                if (dbPlayer.InsuranceType == 1)
                {
                    total -= 1275;
                    bankHistories.Add(new Banks.BankHistory.BankHistory
                    {
                        Name = "Krankenversicherung",
                        Value = -1275
                    });
                }
                else if (dbPlayer.InsuranceType == 2)
                {
                    total -= steuer;
                    bankHistories.Add(new Banks.BankHistory.BankHistory
                    {
                        Name = "private Krankenversicherung",
                        Value = -5000
                    });
                }
            }

            var newsShelter = TeamShelterModule.Instance.GetByTeam((uint)TeamTypes.TEAM_NEWS);
            if (newsShelter != null)
            {
                newsShelter.GiveMoney(50);
            }

            total -= 50;
            bankHistories.Add(
                new Banks.BankHistory.BankHistory { Name = "Rundfunkbeitrag", Value = -50 });

            // HausSteuer
            if (dbPlayer.OwnHouse[0] > 0)
            {
                var tax = 0;
                var wasser = 22;
                var strom = 68;
                House iHouse;
                if ((iHouse = HouseModule.Instance.Get(dbPlayer.OwnHouse[0])) != null)
                {

                    float taxRate = 0.003f;

                    switch(iHouse.Type)
                    {
                        case 1:
                            taxRate = 0.0005f;
                            break;
                        case 2:
                            taxRate = 0.0003f;
                            break;
                        case 3:
                            taxRate = 0.0008f;
                            break;
                        case 4:
                            taxRate = 0.0005f;
                            break;
                    }

                    tax = tax + Convert.ToInt32(iHouse.Price * taxRate);



                    int activeTenants = iHouse.GetTenantAmountUsed();
                    wasser = wasser * activeTenants;
                    strom = strom * activeTenants;
                }

                //bla 30 prozent weniger Steuern bla
                tax = Convert.ToInt32(tax * 0.7);

                total -= tax;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Haussteuern",
                    Value = -tax
                });

                total -= strom + wasser;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Nebenkosten",
                    Value = -(strom + wasser)
                });
                
                KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, (strom + wasser + steuer + tax));
            }
            else if (dbPlayer.IsTenant())
            {
                House iHouse;
                HouseRent tenant = dbPlayer.GetTenant();
                if ((iHouse = HouseModule.Instance.Get(tenant.HouseId)) != null)
                {

                    if (iHouse.OwnerId == 0)
                    {
                        dbPlayer.RemoveTenant();
                    }                    
                    else if (iHouse.OwnerId == dbPlayer.married[0])
                    {
                        bankHistories.Add(new Banks.BankHistory.BankHistory
                        {
                            Name = "Miete",
                            Value = 0
                        });
                    }
                    else
                    {
                        if (dbPlayer.BankMoney[0] < -500000)
                        {
                            // No Money to Rent??!
                            dbPlayer.RemoveTenant();
                            dbPlayer.SendNewNotification(
                                "Aufgrund nicht gezahlter Miete, wurde ihre Mietwohnung gekuendigt!");
                        }
                        else
                        {
                            var wasser = 25;
                            var strom = 12;

                            wasser = wasser * iHouse.Maxrents / 2;
                            strom = strom * iHouse.Maxrents / 2;

                            if (dbPlayer.uni_economy[0] > 0)
                            {
                                var nachlass = 2 * dbPlayer.uni_economy[0];
                                strom = Convert.ToInt32((strom * (100 - nachlass)) / 100);
                                wasser = Convert.ToInt32((wasser * (100 - nachlass)) / 100);
                            }

                            total -= tenant.RentPrice;
                            bankHistories.Add(new Banks.BankHistory.BankHistory
                            {
                                Name = "Miete",
                                Value = -tenant.RentPrice
                            });

                            total -= strom + wasser;
                            bankHistories.Add(new Banks.BankHistory.BankHistory
                            {
                                Name = "Nebenkosten",
                                Value = -(strom + wasser)
                            });

                            iHouse.InventoryCash += tenant.RentPrice;
                            iHouse.SaveHouseBank();
                        }
                    }
                }
            }
            // Levelbonus
            if (!dbPlayer.IsHomeless())
            {
                var bonus = 150 * (dbPlayer.Level - 1);
                total += bonus;
                bankHistories.Add(new Banks.BankHistory.BankHistory {Name = "Sozialbonus", Value = bonus});
            }

            dbPlayer.SendNewNotification("Sie haben ihren Payday erhalten, schauen Sie auf Ihrem Konto fuer mehr Informationen!", title: "Kontoveraenderung", notificationType:PlayerNotification.NotificationType.SUCCESS);
            dbPlayer.TakeOrGiveBankMoney(total, true);
            
            bankHistories.Add(new BankHistory { Name = $"Neuer Kontostand nach Payday", Value = dbPlayer.BankMoney[0] });
            dbPlayer.AddPlayerBankHistories(bankHistories);

            dbPlayer.LogVermoegen();
        }


        public static void CheckPayDay(DbPlayer dbPlayer)
        {
            //Bei Neuling keinen PayDay hochzählen
            if (dbPlayer.HasPerso[0] == 0) return;

            //Payday System
            if (dbPlayer.PayDay[0] < 60 && dbPlayer.JailTime[0] == 0)
            {
                    dbPlayer.PayDay[0]++;
            }
            //The Payday
            else if (dbPlayer.PayDay[0] >= 60)
            {
                PlayerPayday(dbPlayer);
                dbPlayer.Save();
            }
        }

        public override void OnPlayerMinuteUpdate(DbPlayer dbPlayer)
        {
            // Staatskassenshit
            KassenModule.Instance.StaatsKassenPaycheckAmountAll = 0;

            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (dbPlayer.AccountStatus != AccountStatus.LoggedIn) return;

            // Führerschein Sperre
            if (dbPlayer.Lic_Bike[0] < 0) dbPlayer.Lic_Bike[0]++;
            if (dbPlayer.Lic_Biz[0] < 0) dbPlayer.Lic_Biz[0]++;
            if (dbPlayer.Lic_Boot[0] < 0) dbPlayer.Lic_Boot[0]++;
            if (dbPlayer.Lic_Taxi[0] < 0) dbPlayer.Lic_Taxi[0]++;
            if (dbPlayer.Lic_Car[0] < 0) dbPlayer.Lic_Car[0]++;
            if (dbPlayer.Lic_FirstAID[0] < 0) dbPlayer.Lic_FirstAID[0]++;
            if (dbPlayer.Lic_Gun[0] < 0) dbPlayer.Lic_Gun[0]++;
            if (dbPlayer.Lic_Hunting[0] < 0) dbPlayer.Lic_Hunting[0]++;
            if (dbPlayer.Lic_PlaneA[0] < 0) dbPlayer.Lic_PlaneA[0]++;
            if (dbPlayer.Lic_PlaneB[0] < 0) dbPlayer.Lic_PlaneB[0]++;
            if (dbPlayer.Lic_LKW[0] < 0) dbPlayer.Lic_LKW[0]++;
            if (dbPlayer.Lic_Transfer[0] < 0) dbPlayer.Lic_Transfer[0]++;
                        
            //Jailtime
            if (dbPlayer.JailTime[0] == 1)
            {
                // Erneute Wanteds?
                if(CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes) > 0)
                {
                    dbPlayer.JailTime[0] += CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes);
                    dbPlayer.SendNewNotification($"Durch erneute Verbrechen, haben Sie eine Haftzeitverlängerung von {CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes)} Minuten!");
                    dbPlayer.RemoveAllCrimes();
                    
                }
                else
                {
                    //ReleasePlayerFromJail
                    dbPlayer.SendNewNotification("Sie wurden aus dem Gefaengnis entlassen.");

                    // Remove From Trainingsinteraction
                    SportItem spItem = dbPlayer.GetPlayerSGSportsItem();
                    if (spItem != null)
                    {
                        dbPlayer.StopSGTraining(spItem);
                    }

                    dbPlayer.RemoveItemsOnUnjail();

                    if (dbPlayer.HasData("inJailGroup") || dbPlayer.Player.Position.DistanceTo(JailModule.PrisonZone) <= JailModule.Range)
                    {
                        dbPlayer.JailTime[0] = 0;

                        // SG
                        if (dbPlayer.Player.Position.DistanceTo(JailModule.PrisonZone) <= JailModule.Range)
                        {
                            dbPlayer.SetSkin(dbPlayer.IsMale() ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);
                            dbPlayer.Player.SetPosition(JailModule.PrisonSpawn);
                            dbPlayer.Player.SetRotation(266.862f);
                            dbPlayer.SetDimension(0);
                            dbPlayer.Dimension[0] = 0;
                            dbPlayer.DimensionType[0] = DimensionType.World;
                            dbPlayer.SetCuffed(false);
                            if (dbPlayer.HasData("outfitactive")) dbPlayer.ResetData("outfitactive");
                            dbPlayer.ApplyCharacter(false, true);
                        }
                        // In Zellen
                        else if(dbPlayer.HasData("inJailGroup"))
                        {
                            // Get JailSpawn
                            JailSpawn JailSpawn = JailSpawnModule.Instance.GetAll().Values.Where(js => js.Group == dbPlayer.GetData("inJailGroup")).FirstOrDefault();
                            if (JailSpawn != null)
                            {
                                dbPlayer.Player.SetPosition(JailSpawn.Position);
                                dbPlayer.Player.SetRotation(JailSpawn.Heading);
                                dbPlayer.SetDimension(0);
                                dbPlayer.Dimension[0] = 0;
                                dbPlayer.DimensionType[0] = DimensionType.World;
                                dbPlayer.SetCuffed(false);
                                if (dbPlayer.HasData("outfitactive")) dbPlayer.ResetData("outfitactive");
                                dbPlayer.ApplyCharacter(false, true);
                            }
                        }

                        dbPlayer.SendNewNotification("Sie haben ihre Haftzeit nun vollständig abgesessen!");
                    }
                    else // Check Manually
                    {

                        JailCell JailCell = JailCellModule.Instance.GetAll().Values.Where(js => js.Position.DistanceTo(dbPlayer.Player.Position) < js.Range+2).FirstOrDefault();

                        if(JailCell != null)
                        {
                            // Get JailSpawn
                            JailSpawn JailSpawn = JailSpawnModule.Instance.GetAll().Values.Where(js => js.Group == JailCell.Group).FirstOrDefault();
                            if (JailSpawn != null)
                            {
                                dbPlayer.Player.SetPosition(JailSpawn.Position);
                                dbPlayer.Player.SetRotation(JailSpawn.Heading);
                                dbPlayer.SetDimension(0);
                                dbPlayer.Dimension[0] = 0;
                                dbPlayer.DimensionType[0] = DimensionType.World;
                                dbPlayer.SetCuffed(false);
                                if (dbPlayer.HasData("outfitactive")) dbPlayer.ResetData("outfitactive");
                                dbPlayer.ApplyCharacter(false, true);
                            }
                        }

                        dbPlayer.JailTime[0] = 0;
                        dbPlayer.SendNewNotification("Sie haben ihre Haftzeit nun vollständig abgesessen!");
                    }
                }
            }
            else if (dbPlayer.JailTime[0] > 1 && !dbPlayer.IsInjured())
            {
                dbPlayer.JailTime[0]--;
            }

            CheckSalary(dbPlayer);
            CheckPayDay(dbPlayer);
            
            // Minus Staatskasse
            KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, -KassenModule.Instance.StaatsKassenPaycheckAmountAll);
            KassenModule.Instance.StaatsKassenPaycheckAmountAll = 0;
        }
    }
}
