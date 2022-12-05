using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VMP_CNR.Module.Players.Db;
using Newtonsoft.Json;
using GTANetworkAPI;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Teams.Shelter;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Tattoo;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.NSA;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Boerse;

namespace VMP_CNR.Module.Banks.Windows
{
    public class BankWindow : Window<Func<DbPlayer, string, string, int, int, int, List<BankHistory.BankHistory>, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "title")] private string Title { get; }

            [JsonProperty(PropertyName = "balance")]
            private int Balance { get; }

            [JsonProperty(PropertyName = "history")]
            private List<BankHistory.BankHistory> History { get; }

            [JsonProperty(PropertyName = "playername")]
            private string Playername { get; }

            [JsonProperty(PropertyName = "money")] 
            private int Money { get; }

            [JsonProperty(PropertyName = "type")] 
            private int Type { get; }

            public ShowEvent(DbPlayer dbPlayer, string title, string playername, int money, int balance, int type, List<BankHistory.BankHistory> history) :
                base(dbPlayer)
            {
                Title = title;
                Playername = playername;
                Money = money;
                Balance = balance;
                Type = type;
                History = history;
            }
        }

        public BankWindow() : base("Bank")
        {
        }

        public override Func<DbPlayer, string, string, int, int, int, List<BankHistory.BankHistory>, bool> Show()
        {
            return (player, title, playername, money, balance, type, history) =>
                OnShow(new ShowEvent(player, title, playername, money, balance, type, history));
        }

        [RemoteEvent]
        public void bankPayout(Player player, int balance, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            BankTransaction(player, 0, balance);
        }

        [RemoteEvent]
        public void bankDeposit(Player player, int balance, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            BankTransaction(player, balance, 0);
        }

        [RemoteEvent]
        public void bankTransfer(Player player, int amount, string target, string transferPurpose, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            Regex regex = new Regex(@"([a-zA-Z0-9-_ ]*)");
            if (!regex.IsMatch(transferPurpose)) {
                dbPlayer.SendNewNotification("Dieser Überweisungsgrund ist ungültig!", PlayerNotification.NotificationType.ERROR);
                return;
            }

            // BusinessBank
            switch (dbPlayer.DimensionType[0])
            {
                case DimensionType.Business:
                    dbPlayer.SendNewNotification("Funktion nicht moeglich!");
                    break;
                default:
                    // Tattostudio
                    if (dbPlayer.TryData("tattooShopId", out uint tattooShopId))
                    {
                        TattooShop tattooShop = TattooShopModule.Instance.Get(tattooShopId);
                        if (tattooShop != null)
                        {
                            dbPlayer.SendNewNotification("Funktion nicht moeglich!");
                            return;
                        }
                    }

                    // TeamShelter
                    if (dbPlayer.TryData("teamShelterMenuId", out uint teamshelterId))
                    {
                        TeamShelter shelter = TeamShelterModule.Instance.Get(teamshelterId);
                        if (shelter != null)
                        {
                            dbPlayer.SendNewNotification("Funktion nicht moeglich!");
                            return;
                        }
                    }

                    // Gangwar Town
                    GangwarTown gangwarTown = GangwarTownModule.Instance.Get(dbPlayer.Player.Dimension);
                    if (gangwarTown != null && gangwarTown.OwnerTeam.Id == dbPlayer.TeamId &&
                        dbPlayer.Player.Position.DistanceTo(GangwarTownModule.BankPosition) < 1.5f)
                    {
                        dbPlayer.SendNewNotification("Funktion nicht moeglich!");
                        return;
                    }

                    DbPlayer targetPlayer = Players.Players.Instance.FindPlayer(target);

                    if (targetPlayer != null && targetPlayer.IsValid() && targetPlayer != dbPlayer)
                    {
                        if (amount <= 0) return;
                        if (dbPlayer.TakeBankMoney(amount))
                        {
                            targetPlayer.GiveBankMoney(amount);
                            dbPlayer.SendNewNotification(
                                "Sie haben " + amount + "$ an " + targetPlayer.GetName() + " ueberwiesen." + 
                                (string.IsNullOrWhiteSpace(transferPurpose) ? "" : " (Grund: " + transferPurpose + ")")
                            );
                            targetPlayer.SendNewNotification(
                                dbPlayer.GetName() + " hat ihnen " + amount + "$ ueberwiesen." +
                                (string.IsNullOrWhiteSpace(transferPurpose) ? "" : " (Grund: " + transferPurpose + ")")
                            );

                            // Bankhistory
                            targetPlayer.AddPlayerBankHistory(amount, "Ueberweisung von " + dbPlayer.GetName() + (string.IsNullOrWhiteSpace(transferPurpose) ? "" : " (Grund: " + transferPurpose + ")"));
                            dbPlayer.AddPlayerBankHistory(-amount, "Ueberweisung an " + targetPlayer.GetName() + (string.IsNullOrWhiteSpace(transferPurpose) ? "" : " (Grund: " + transferPurpose + ")"));
                            GiveMoneyWindow.SaveToPayLog(dbPlayer.Id.ToString(), targetPlayer.Id.ToString(), amount, TransferType.ÜBERWEISUNG);
                            return;
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Spieler nicht gefunden!");
                        return;
                    }

                    break;
            }
        }

        public void BankTransaction(Player player, int einzahlen, int auszahlen)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            // Get Type of Bank
            // BusinessBank
            switch (dbPlayer.DimensionType[0])
            {
                case DimensionType.Business:
                    Business.Business biz = BusinessModule.Instance.GetById((uint) dbPlayer.Player.Dimension);
                    if (biz == null || dbPlayer.GetActiveBusiness()?.Id != biz.Id) return;
                    if (auszahlen > 0 && auszahlen <= biz.Money)
                    {
                        if (dbPlayer.GetActiveBusinessMember() == null ||
                            dbPlayer.GetActiveBusinessMember()?.Money == false) return;
                        biz.Disburse(dbPlayer, auszahlen);

                        Logging.Logger.SaveToBusinessBank(biz.Id, auszahlen, dbPlayer.Id, dbPlayer.GetName(), false);
                        biz.AddBankHistory(-auszahlen, $"Auszahlung von {dbPlayer.GetName()}");
                    }

                    if (einzahlen > 0 && einzahlen <= dbPlayer.Money[0])
                    {
                        biz.Deposite(dbPlayer, einzahlen);
                        Logging.Logger.SaveToBusinessBank(biz.Id, einzahlen, dbPlayer.Id, dbPlayer.GetName(), true);
                        biz.AddBankHistory(einzahlen, $"Einzahlung von {dbPlayer.GetName()}");
                    }

                    break;
                default:

                    // TeamShelter
                    if (dbPlayer.TryData("teamShelterMenuId", out uint teamshelterId))
                    {
                        TeamShelter teamShelter = TeamShelterModule.Instance.Get(teamshelterId);
                        if (teamShelter != null)
                        {
                            if (teamShelter == null || dbPlayer.TeamId != teamShelter.Team.Id) return;

                            if (dbPlayer.HasData("swbank"))
                            {
                                if (auszahlen > 0 && auszahlen <= dbPlayer.BlackMoneyBank[0])
                                {
                                    dbPlayer.TakeBlackMoneyBank(auszahlen);
                                    dbPlayer.GiveBlackMoney(auszahlen);

                                    dbPlayer.SendNewNotification(
                                        "Sie haben " + auszahlen + "$ von Ihrem Schwarzgeldkonto abgehoben.",
                                        title: "Schwarzgeldkonto",
                                        notificationType: PlayerNotification.NotificationType.ERROR
                                    );
                                }

                                if (einzahlen > 0 && einzahlen <= dbPlayer.BlackMoney[0])
                                {
                                    if (dbPlayer.BlackMoneyBank[0] >= 3000000)
                                    {
                                        dbPlayer.SendNewNotification(
                                            "Ihr Schwarzgeldkonto kann maximal 3 Millionen fassen!"
                                        );
                                        return;
                                    }

                                    int diff = 3000000 - dbPlayer.BlackMoneyBank[0];
                                    if (einzahlen > diff) einzahlen = diff;

                                    dbPlayer.GiveBlackMoneyBank(einzahlen);
                                    dbPlayer.TakeBlackMoney(einzahlen);

                                    dbPlayer.SendNewNotification(
                                        "Sie haben " + einzahlen + "$ auf Ihr Schwarzgeldkonto eingezahlt.",
                                        title: "Schwarzgeldkonto",
                                        notificationType: PlayerNotification.NotificationType.SUCCESS
                                    );

                                    if (dbPlayer.HasData("packBlackMoney"))
                                    {
                                        Logging.Logger.LogToAcDetections(dbPlayer.Id, Logging.ACTypes.SWBankAbuse,
                                            $"BlackMoney {einzahlen} transfered while BM {dbPlayer.GetData("packBlackMoney")} packed");
                                    }
                                }

                                dbPlayer.ResetData("swbank");
                                return;
                            }
                            else
                            {
                                if (auszahlen > 0 && auszahlen <= teamShelter.Money)
                                {
                                    if (!dbPlayer.TeamRankPermission.Bank)
                                    {
                                        return;
                                    }

                                    teamShelter.Disburse(dbPlayer, auszahlen);
                                    dbPlayer.SendNewNotification(
                                        "Sie haben " + auszahlen + "$ von Ihrem Fraktionskonto abgehoben.",
                                        title: "Fraktionskasse",
                                        notificationType: PlayerNotification.NotificationType.ERROR
                                    );
                                    Logging.Logger.SaveToFbankLog(
                                        dbPlayer.TeamId, 
                                        auszahlen, 
                                        dbPlayer.Id,
                                        dbPlayer.GetName(), 
                                        false
                                    );
                                }

                                if (einzahlen > 0 && einzahlen <= dbPlayer.Money[0])
                                {
                                    teamShelter.Deposit(dbPlayer, einzahlen);
                                    dbPlayer.SendNewNotification(
                                        "Sie haben " + einzahlen + "$ auf Ihr Fraktionskonto eingezahlt.",
                                        title: "Fraktionskasse",
                                        notificationType: PlayerNotification.NotificationType.SUCCESS
                                    );
                                    Logging.Logger.SaveToFbankLog(
                                        dbPlayer.TeamId, 
                                        einzahlen, 
                                        dbPlayer.Id,
                                        dbPlayer.GetName(), 
                                        true
                                    );
                                }

                                return;
                            }
                        }
                    }

                    // Tattostudio
                    if (dbPlayer.TryData("tattooShopId", out uint tattooShopId))
                    {
                        TattooShop tattooShop = TattooShopModule.Instance.Get(tattooShopId);
                        if (tattooShop != null)
                        {
                            if (!dbPlayer.IsMemberOfBusiness() || !dbPlayer.GetActiveBusinessMember().Manage ||
                                dbPlayer.GetActiveBusinessMember().BusinessId != tattooShop.BusinessId) return;

                            if (einzahlen > 0)
                            {
                                dbPlayer.SendNewNotification("Nur Auszahlungen moeglich!");
                                return;
                            }

                            if (tattooShop.Bank < auszahlen)
                            {
                                dbPlayer.SendNewNotification("Konto nicht genügend gedeckt!");
                                return;
                            }

                            tattooShop.MinusBank(auszahlen);
                            Logger.AddTattoShopLog(tattooShop.Id, dbPlayer.Id, auszahlen, false);
                            dbPlayer.GiveMoney(auszahlen);

                            biz = BusinessModule.Instance.GetById((uint) tattooShop.BusinessId);
                            if (biz == null || dbPlayer.GetActiveBusiness()?.Id != biz.Id) return;

                            biz.AddBankHistory(-auszahlen, $"Auszahlung von {dbPlayer.GetName()}");

                            return;
                        }
                    }

                    // Gangwar Town
                    GangwarTown gangwarTown = GangwarTownModule.Instance.Get(dbPlayer.Player.Dimension);
                    if (gangwarTown != null && gangwarTown.OwnerTeam.Id == dbPlayer.TeamId &&
                        dbPlayer.Player.Position.DistanceTo(GangwarTownModule.BankPosition) < 1.5f)
                    {
                        if (auszahlen > 0 && auszahlen <= gangwarTown.Cash)
                        {
                            if (!dbPlayer.TeamRankPermission.Bank)
                            {
                                return;
                            }

                            gangwarTown.SetCash(gangwarTown.Cash - auszahlen);
                            dbPlayer.GiveMoney(auszahlen);
                            dbPlayer.SendNewNotification(
                                "Sie haben " + auszahlen + "$ von Ihrer Gangwarkasse abgehoben.", title: "Gangwarkasse",
                                notificationType: PlayerNotification.NotificationType.SUCCESS
                            );
                            Logging.Logger.SaveToFbankLog(
                                dbPlayer.TeamId, 
                                auszahlen, 
                                dbPlayer.Id,
                                dbPlayer.GetName() + " gw", 
                                false
                            );
                            return;
                        }

                        if (einzahlen > 0)
                        {
                            dbPlayer.SendNewNotification(
                                "Nur Auszahlungen moeglich!", title: "Gangwarkasse",
                                notificationType: PlayerNotification.NotificationType.SERVER
                            );
                            return;
                        }
                    }

                    if (dbPlayer.TryData("bankId", out uint bankId))
                    {
                        Bank bank = BankModule.Instance.Get(bankId);

                        if (!bank.CanMoneyWithdrawn(auszahlen))
                        {
                            dbPlayer.SendNewNotification(
                                "Dieser Bankautomat verfügt aktuell leider nicht über diese Geldsumme.",
                                title: "Bankautomat"
                            );
                            return;
                        }

                        if (auszahlen > 0 && auszahlen <= dbPlayer.BankMoney[0])
                        {
                            if (dbPlayer.TakeBankMoney(auszahlen))
                            {
                                dbPlayer.GiveMoney(auszahlen);
                                dbPlayer.AddPlayerBankHistory(-auszahlen, $"Geldtransfer ({bank.Name}) - Auszahlung");
                                bank.WithdrawMoney(auszahlen);
                                bank.SaveActMoneyToDb();
                                dbPlayer.SendNewNotification(
                                    "Sie haben " + GlobalMessages.Money.fnumber(auszahlen) + "$ von Ihrem Konto abgehoben.",
                                    title: "Konto", 
                                    notificationType: PlayerNotification.NotificationType.SUCCESS
                                );

                                if (dbPlayer.HasMoneyTransferWantedStatus())
                                {
                                    TeamModule.Instance.SendMessageToTeam(
                                        $"Finanz-Detection: Die Gesuchte Person {dbPlayer.GetName()} hat eine Auszahlung von ${auszahlen} getätigt! (Standort: {bank.Name})",
                                        teams.TEAM_FIB, 
                                        10000, 
                                        3
                                    );
                                    NSAPlayerExtension.AddTransferHistory(
                                        $"{dbPlayer.GetName()} Auszahlung {bank.Name}", 
                                        bank.Position
                                    );
                                }
                            }
                        }

                        if (!bank.CanMoneyDeposited(einzahlen))
                        {
                            dbPlayer.SendNewNotification(
                                "Der Speicher dieses Bankautomaten kann diese Geldsumme leider nicht mehr aufnehmen.",
                                title: "Bankautomat"
                            );
                            return;
                        }

                        if (einzahlen > 0 && einzahlen <= dbPlayer.Money[0])
                        {
                            if (dbPlayer.GiveBankMoney(einzahlen))
                            {
                                dbPlayer.TakeMoney(einzahlen);
                                dbPlayer.AddPlayerBankHistory(einzahlen, $"Geldtransfer ({bank.Name}) - Einzahlung");
                                bank.DepositMoney(einzahlen);
                                bank.SaveActMoneyToDb();
                                dbPlayer.SendNewNotification(
                                    "Sie haben " + GlobalMessages.Money.fnumber(einzahlen) + "$ auf Ihr Konto eingezahlt.",
                                    title: "Konto", 
                                    notificationType: PlayerNotification.NotificationType.SUCCESS
                                );

                                if (dbPlayer.HasMoneyTransferWantedStatus())
                                {
                                    TeamModule.Instance.SendMessageToTeam(
                                        $"Finanz-Detection: Die Gesuchte Person {dbPlayer.GetName()} hat eine Einzahlung von ${einzahlen} getätigt! (Standort: {bank.Name})",
                                        teams.TEAM_FIB, 
                                        10000, 
                                        3
                                    );
                                    NSAPlayerExtension.AddTransferHistory(
                                        $"{dbPlayer.GetName()} Einzahlung {bank.Name}", 
                                        bank.Position
                                    );
                                }
                            }
                        }
                    }

                    if (dbPlayer.Player.Position.DistanceTo(PlayerDepotModule.DepotManagementPosition) < 1.5f)
                    {
                        if (!dbPlayer.HasDepot()) return;
                        if (dbPlayer.HasDepotLimitReached(PlayerDepotExtensions.DepotOperation.Both))
                        {
                            dbPlayer.SendNewNotification(
                                $"Du hast bereits das tägliche Limit von {PlayerDepotModule.DepotDailyMaximum} $ für Ein- sowie Auszahlungen innerhalb deines Depots erreicht!"
                            );
                            return;
                        }

                        if (auszahlen > 0 && auszahlen <= dbPlayer.Depot.Amount)
                        {
                            if (dbPlayer.HasDepotLimitReached(PlayerDepotExtensions.DepotOperation.Subtract, (uint) auszahlen))
                            {
                                dbPlayer.SendNewNotification(
                                    $"Du hast bereits das tägliche Limit von {PlayerDepotModule.DepotDailyMaximum} $ für Auszahlungen innerhalb deines Depots erreicht!"
                                );
                                return;
                            }

                            uint tax = (uint) (auszahlen * 0.1);
                            dbPlayer.Depot.Subtract((uint) auszahlen);
                            dbPlayer.GiveMoney((int) (auszahlen - tax));
                            dbPlayer.SendNewNotification(
                                $"Sie haben {auszahlen} $ aus Ihrem Depot ausgezahlt! Es wurden 10% Steuer ({tax} $) verrechnet!"
                            );
                            return;
                        }

                        if (einzahlen > 0 && einzahlen <= dbPlayer.Money[0])
                        {
                            if (dbPlayer.HasDepotLimitReached(PlayerDepotExtensions.DepotOperation.Add, (uint) einzahlen))
                            {
                                dbPlayer.SendNewNotification(
                                    $"Du hast bereits das tägliche Limit von {PlayerDepotModule.DepotDailyMaximum} $ für Einzahlungen innerhalb deines Depots erreicht!"
                                );
                                return;
                            }

                            if (dbPlayer.TakeMoney(einzahlen))
                            {
                                uint tax = (uint) (einzahlen * 0.1);
                                dbPlayer.Depot.Add((uint) einzahlen - tax); // 10% weniger einzahlen zwecks Steuern
                                dbPlayer.SendNewNotification(
                                    $"Sie haben {einzahlen} $ in Ihr Depot eingezahlt! Es wurden 10% Steuer ({tax} $) verrechnet!"
                                );
                            }

                            return;
                        }

                        return;
                    }

                    break;
            }

            return;
        }
    }
}