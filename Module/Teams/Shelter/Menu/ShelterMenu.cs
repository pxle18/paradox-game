using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Banks.BankHistory;
using VMP_CNR.Module.Banks.Windows;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Dealer;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Heist.Planning;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Laboratories;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Buffs;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teamfight;
using VMP_CNR.Module.Weapons.Component;
using VMP_CNR.Module.Weapons.Data;
using VMP_CNR.Module.Workstation;

namespace VMP_CNR.Module.Teams.Shelter
{
    public class ShelterMenuBuilder : MenuBuilder
    {
        public ShelterMenuBuilder() : base(PlayerMenu.ShelterMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.HasData("teamShelterMenuId")) return null;
            var menu = new Menu.Menu(Menu, "Fraktionslager");

            menu.Add($"Schließen"); // 0
            menu.Add($"Fraktionsbank"); // 1
            if (!dbPlayer.Team.IsBusinessTeam)
            {
                menu.Add("Equip kaufen (900$)"); // 2
                menu.Add("Schwarzgeldbank"); // 3

                if (dbPlayer.Team.IsBadOrga())
                {
                    menu.Add("Pistolen Herstellung"); // 4

                    if (!dbPlayer.InParamedicDuty && dbPlayer.ParamedicLicense)
                        menu.Add("In den Frakmedic-Dienst gehen"); // 5
                    else if (dbPlayer.InParamedicDuty)
                        menu.Add("Frakmedic-Dienst verlassen"); // 5
                }
                else if (dbPlayer.Team.IsGangsters() && !dbPlayer.Team.IsBadOrga())
                {
                    menu.Add("Dealer suchen (25.000$)"); // 4
                    menu.Add("Gangwar beitreten"); // 5
                    menu.Add("Fraktionsdroge herstellen"); // 6
                    menu.Add("Planningroom Workstation (2.500$)"); // 7

                    if (dbPlayer.Team.IsMethTeam()) menu.Add("Methlabor suchen"); // 8
                    else if (dbPlayer.Team.IsWeaponTeam()) menu.Add("Waffenfabrik suchen"); // 8
                    else if (dbPlayer.Team.IsWeedTeam()) menu.Add("Cannabislabor suchen"); // 8
                    else if (dbPlayer.Team.IsHeroinTeam()) menu.Add("Heroinlabor suchen"); // 8
                    else menu.Add(""); // leer... 8


                    menu.Add("Waffe vergolden"); // 9

                    int FingerPrintedWeaponCount = dbPlayer.Container.GetItemsByDataKey("fingerprint").Count;
                    if (FingerPrintedWeaponCount > 0)
                    {
                        int price = FingerPrintedWeaponCount * 25000;
                        menu.Add($"{FingerPrintedWeaponCount} Fingerabdrücke für ${price} entfernen"); // 10
                    }

                    if (!dbPlayer.InParamedicDuty && dbPlayer.ParamedicLicense)
                        menu.Add("In den Frakmedic-Dienst gehen"); // 10, 11
                    else if (dbPlayer.InParamedicDuty)
                        menu.Add("Frakmedic-Dienst verlassen"); // 10, 11
                }
            }
            
            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (!dbPlayer.HasData("teamShelterMenuId")) return true;
                var teamShelter = TeamShelterModule.Instance.Get(dbPlayer.GetData("teamShelterMenuId"));
                if (teamShelter == null || teamShelter.Team.Id != dbPlayer.TeamId) return true;

                // Close menu
                if (index == 0)
                {
                    return true;
                }
                else if (index == 1)
                {
                    //Open FBank
                    if (dbPlayer.HasData("swbank"))
                        dbPlayer.ResetData("swbank");

                    MenuManager.DismissCurrent(dbPlayer);

                    // Only Show History with Bank Permissions
                    if (!dbPlayer.TeamRankPermission.Bank)
                    {
                        ComponentManager.Get<BankWindow>().Show()(dbPlayer, "Fraktionskonto", dbPlayer.Team.Name, dbPlayer.Money[0], teamShelter.Money, 0, new List<BankHistory>());
                    }
                    else
                    {
                        ComponentManager.Get<BankWindow>().Show()(dbPlayer, "Fraktionskonto", dbPlayer.Team.Name, dbPlayer.Money[0], teamShelter.Money, 0, dbPlayer.Team.BankHistory);
                    }
                    return true;
                }
                else if (dbPlayer.Team.IsGangster)
                {
                    if (index == 2)
                    {
                        //Equip

                        if (dbPlayer.CanWeaponAdded(WeaponHash.Heavypistol))
                        {
                            if (!dbPlayer.TakeMoney(900))
                            {
                                dbPlayer.SendNewNotification(
                                    GlobalMessages.Money.NotEnoughMoney(900));
                                return true;
                            }

                            if (dbPlayer.CanWeaponAdded(WeaponHash.Golfclub))
                            {
                                switch (dbPlayer.TeamId)
                                {
                                    case (int)teams.TEAM_LCN:
                                        dbPlayer.GiveWeapon(WeaponHash.Golfclub, 0);
                                        break;
                                    case (int)teams.TEAM_HOH:
                                        dbPlayer.GiveWeapon(WeaponHash.Stone_hatchet, 0);
                                        break;
                                    case (int)teams.TEAM_BOSOZOKUKAI:
                                    case (int)teams.TEAM_BRATWA:
                                        dbPlayer.GiveWeapon(WeaponHash.Wrench, 0);
                                        break;
                                    case (int)teams.TEAM_ORGANISAZIJA:
                                        dbPlayer.GiveWeapon(WeaponHash.Hammer, 0);
                                        break;
                                    case (int)teams.TEAM_LOST:
                                        dbPlayer.GiveWeapon(WeaponHash.Battleaxe, 0);
                                        break;
                                    case (int)teams.TEAM_BALLAS:
                                        dbPlayer.GiveWeapon(WeaponHash.Knuckle, 0);
                                        break;
                                    case (int)teams.TEAM_YAKUZA:
                                        dbPlayer.GiveWeapon(WeaponHash.Switchblade, 0);
                                        break;
                                    case (int)teams.TEAM_GROVE:
                                        dbPlayer.GiveWeapon(WeaponHash.Bat, 0);
                                        break;
                                    case (int)teams.TEAM_TRIADEN:
                                    case (int)teams.TEAM_IRISHMOB:
                                        dbPlayer.GiveWeapon(WeaponHash.Poolcue, 0);
                                        break;
                                    // case (int)teams.TEAM_MIDNIGHT:
                                    //     dbPlayer.GiveWeapon(WeaponHash.Knife, 0);
                                    //     break;
                                    case (int)teams.TEAM_MARABUNTA:
                                    case (int)teams.TEAM_VAGOS:
                                        dbPlayer.GiveWeapon(WeaponHash.Machete, 0);
                                        break;
                                    case (int)teams.TEAM_REDNECKS:
                                        dbPlayer.GiveWeapon(WeaponHash.Bottle, 0);
                                        break;
                                    case (int)teams.TEAM_HUSTLER:
                                        dbPlayer.GiveWeapon(WeaponHash.Knife, 0);
                                        break;
                                    case (int)teams.TEAM_ICA:
                                        dbPlayer.GiveWeapon(WeaponHash.Dagger, 0);
                                        break;
                                    case (int)teams.TEAM_MADRAZO:
                                        dbPlayer.GiveWeapon(WeaponHash.Hatchet, 0);
                                        break;
                                    default:
                                        dbPlayer.GiveWeapon(WeaponHash.Bat, 0);
                                        break;
                                }

                                dbPlayer.SendNewNotification(
                                    "Sie haben ihre Waffen aus dem Arsenal genommen! (900$ Kosten)", title: "Fraktion", notificationType: PlayerNotification.NotificationType.FRAKTION);
                            }
                        }
                    }
                    else if (index == 3)
                    {
                        //Open Blackmoneybank

                        dbPlayer.SetData("swbank", 1);
                        MenuManager.DismissCurrent(dbPlayer);
                        ComponentManager.Get<BankWindow>().Show()(dbPlayer, "Schwarzgeldkonto", "Schwarzgeldkonto", dbPlayer.BlackMoney[0], dbPlayer.BlackMoneyBank[0], 0, new List<Banks.BankHistory.BankHistory>());
                    }
                    else if (index == 4)
                    {
                        if(dbPlayer.Team.IsBadOrga())
                        {
                            Module.Menu.MenuManager.Instance.Build(VMP_CNR.Module.Menu.PlayerMenu.PistoleCreateMenu, dbPlayer).Show(dbPlayer);
                            return false;
                        }

                        //Show random dealer

                        if (teamShelter.DealerPosition == null)
                        {
                            if (dbPlayer.TeamRankPermission.Manage < 1)
                            {
                                dbPlayer.SendNewNotification("Du musst Leader deiner Fraktion sein!");
                                return true;
                            }

                            var dealer = DealerModule.Instance.GetRandomDealer();
                            teamShelter.DealerPosition = Utils.GenerateRandomPosition(dealer.Position);
                            teamShelter.TakeMoney(25000);
                        }
                        float X = teamShelter.DealerPosition.X;
                        float Y = teamShelter.DealerPosition.Y;
                        dbPlayer.SetWaypoint(X, Y);
                        dbPlayer.SendNewNotification("Hier irgendwo...");
                    }
                    else if (index == 5)
                    {
                        // Medic Dienst für Orgas
                        if (dbPlayer.Team.IsBadOrga())
                        {
                            if (!dbPlayer.InParamedicDuty)
                            {
                                bool success = dbPlayer.SetPlayerInMedicDuty();
                                if (success)
                                    dbPlayer.SendNewNotification($"Du bist nun im Medic-Dienst deiner Fraktion!");
                                else
                                    dbPlayer.SendNewNotification("$Du kannst nicht in den Medic-Dienst gehen. (Max-Medics im Dienst erreicht)");
                            }
                            else
                            {
                                dbPlayer.SetPlayerOffMedicDuty();
                                dbPlayer.SendNewNotification($"Du bist nun nicht mehr im Medic-Dienst deiner Fraktion!");
                            }
                        }
                        else
                        {
                            //Join Gangwar
                            if (GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team))
                            {
                                if (CanPlayerJoinGangwar(dbPlayer))
                                {
                                    dbPlayer.Player.SetPosition(GangwarTownModule.Instance.GetGangwarTownSpawnByTeam(dbPlayer.Team));
                                    TeamfightFunctions.SetToGangware(dbPlayer);
                                }
                                else
                                {
                                    dbPlayer.SendNewNotification("Es sind bereits 20 Mitglieder im Gangwar");
                                }
                            }
                            else
                            {
                                dbPlayer.SendNewNotification("Kein Gangwar aktiv!");
                            }
                        }
                    }
                    else if (index == 6)
                    {
                        DateTime actualDate = DateTime.Now;
                        if (dbPlayer.DrugCreateLast.Day < actualDate.Day || dbPlayer.DrugCreateLast.Month < actualDate.Month || dbPlayer.DrugCreateLast.Year < actualDate.Year)
                        {
                            if (dbPlayer.Team.IsHeroinTeam()) //FRAKDROGEN
                            {
                                uint RequiredItemId = 1443; //Kiste veredeltes Heroin
                                int ResultAmount = 30;

                                if (dbPlayer.Container.GetItemAmount(RequiredItemId) > 0)
                                {
                                    uint resultItemId = 0;

                                    //Add Fraktion Item
                                    switch (dbPlayer.TeamId)
                                    {
                                        case (int)teams.TEAM_BOSOZOKUKAI:
                                            resultItemId = 1432;
                                            break;
                                        case (int)teams.TEAM_IRISHMOB:
                                            resultItemId = 1312;
                                            break;
                                        case (int)teams.TEAM_BALLAS:
                                            resultItemId = 999;
                                            break;
                                        case (int)teams.TEAM_GROVE:
                                            resultItemId = 1000;
                                            break;
                                        case (int)teams.TEAM_MARABUNTA:
                                            resultItemId = 1001;
                                            break;
                                        // case (int)teams.TEAM_MIDNIGHT:
                                        //     resultItemId = 1002;
                                        //     break;
                                        case (int)teams.TEAM_LOST:
                                            resultItemId = 1003;
                                            break;
                                        case (int)teams.TEAM_REDNECKS:
                                            resultItemId = 1004;
                                            break;
                                        case (int)teams.TEAM_HOH:
                                            resultItemId = 1311;
                                            break;
                                        case (int)teams.TEAM_VAGOS:
                                            resultItemId = 998;
                                            break;
                                        case (int)teams.TEAM_MADRAZO:
                                            resultItemId = 646;
                                            ResultAmount = 15;
                                            break;
                                        case (int)teams.TEAM_TRIADEN:
                                            resultItemId = 1005;
                                            ResultAmount = 15;
                                            break;
                                        case (int)teams.TEAM_YAKUZA:
                                            resultItemId = 1006;
                                            ResultAmount = 15;
                                            break;
                                        case (int)teams.TEAM_LCN:
                                            resultItemId = 1007;
                                            ResultAmount = 15;
                                            break;
                                        case (int)teams.TEAM_BRATWA:
                                            resultItemId = 1132;
                                            ResultAmount = 15;
                                            break;
                                        default:
                                            resultItemId = 998;
                                            break;
                                    }

                                    if (!dbPlayer.Container.CanInventoryItemAdded(resultItemId, ResultAmount))
                                    {
                                        dbPlayer.SendNewNotification(GlobalMessages.Inventory.NotEnoughSpace());
                                        return true;
                                    }

                                    dbPlayer.Container.RemoveItem(RequiredItemId, 1);
                                    dbPlayer.Container.AddItem(resultItemId, ResultAmount);
                                    dbPlayer.SendNewNotification($"Du hast aus {ItemModelModule.Instance.Get(RequiredItemId).Name}  {ResultAmount} {ItemModelModule.Instance.Get(RequiredItemId).Name} hergestellt.");

                                    dbPlayer.DrugCreateLast = DateTime.Now;
                                    dbPlayer.SaveCustomDrugsCreation();
                                    return true;
                                }
                            }
                            else if (dbPlayer.Team.IsWeedTeam())
                            {
                                uint RequiredItemId = 980;
                                int ResultAmount = 30;

                                if (dbPlayer.Container.GetItemAmount(RequiredItemId) > 0)
                                {
                                    uint resultItemId = 0;

                                    // Add Fraktion item
                                    switch (dbPlayer.TeamId)
                                    {
                                        case (int)teams.TEAM_BOSOZOKUKAI:
                                            resultItemId = 1432;
                                            break;
                                        case (int)teams.TEAM_IRISHMOB:
                                            resultItemId = 1312;
                                            break;
                                        case (int)teams.TEAM_BALLAS:
                                            resultItemId = 999;
                                            break;
                                        case (int)teams.TEAM_GROVE:
                                            resultItemId = 1000;
                                            break;
                                        case (int)teams.TEAM_MARABUNTA:
                                            resultItemId = 1001;
                                            break;
                                        // case (int)teams.TEAM_MIDNIGHT:
                                        //     resultItemId = 1002;
                                        //     break;
                                        case (int)teams.TEAM_LOST:
                                            resultItemId = 1003;
                                            break;
                                        case (int)teams.TEAM_REDNECKS:
                                            resultItemId = 1004;
                                            break;
                                        case (int)teams.TEAM_HOH:
                                            resultItemId = 1311;
                                            break;
                                        case (int)teams.TEAM_VAGOS:
                                            resultItemId = 998;
                                            break;
                                        default:
                                            resultItemId = 998;
                                            break;
                                    }

                                    if (!dbPlayer.Container.CanInventoryItemAdded(resultItemId, ResultAmount))
                                    {
                                        dbPlayer.SendNewNotification(GlobalMessages.Inventory.NotEnoughSpace());
                                        return true;
                                    }

                                    dbPlayer.Container.RemoveItem(RequiredItemId, 1);
                                    dbPlayer.Container.AddItem(resultItemId, ResultAmount);
                                    dbPlayer.SendNewNotification($"Du hast aus {ItemModelModule.Instance.Get(RequiredItemId).Name}  {ResultAmount} {ItemModelModule.Instance.Get(RequiredItemId).Name} hergestellt.");

                                    dbPlayer.DrugCreateLast = DateTime.Now;
                                    dbPlayer.SaveCustomDrugsCreation();
                                    return true;
                                }
                            }
                            else if (dbPlayer.Team.IsMethTeam())
                            {
                                uint RequiredItemId = 729;
                                int ResultAmount = 15;

                                if (dbPlayer.Container.GetItemAmount(RequiredItemId) > 0)
                                {
                                    uint resultItemId = 0;

                                    // Add Fraktion item
                                    switch (dbPlayer.TeamId)
                                    {
                                        case (int)teams.TEAM_MADRAZO:
                                            resultItemId = 646;
                                            break;
                                        case (int)teams.TEAM_TRIADEN:
                                            resultItemId = 1005;
                                            break;
                                        case (int)teams.TEAM_YAKUZA:
                                            resultItemId = 1006;
                                            break;
                                        case (int)teams.TEAM_LCN:
                                            resultItemId = 1007;
                                            break;
                                        case (int)teams.TEAM_BRATWA:
                                            resultItemId = 1132;
                                            break;
                                        default:
                                            resultItemId = 1005;
                                            break;
                                    }

                                    if (!dbPlayer.Container.CanInventoryItemAdded(resultItemId, ResultAmount))
                                    {
                                        dbPlayer.SendNewNotification(GlobalMessages.Inventory.NotEnoughSpace());
                                        return true;
                                    }

                                    dbPlayer.Container.RemoveItem(RequiredItemId, 1);
                                    dbPlayer.Container.AddItem(resultItemId, ResultAmount);
                                    dbPlayer.SendNewNotification($"Du hast aus {ItemModelModule.Instance.Get(RequiredItemId).Name}  {ResultAmount} {ItemModelModule.Instance.Get(RequiredItemId).Name} hergestellt.");

                                    dbPlayer.DrugCreateLast = DateTime.Now;
                                    dbPlayer.SaveCustomDrugsCreation();
                                    return true;
                                }
                            }
                            else
                            {
                                dbPlayer.SendNewNotification("Das können Sie nicht!");
                                return true;
                            }
                        }
                        else
                        {
                            dbPlayer.SendNewNotification("Du hast heute bereits deine Fraktionsdroge hergestellt!");
                            return true;
                        }
                    }
                    else if (index == 7)
                    {
                        Workstation.Workstation workstation = WorkstationModule.Instance.GetAll().Where(w => w.Value.Dimension == dbPlayer.Team.Id && w.Value.SpecialType == WorkstationSpecialType.PlanningRoomStahlpatronen).FirstOrDefault().Value;
                        if (workstation != null)
                        {
                            if (!workstation.LimitTeams.Contains(dbPlayer.TeamId))
                            {
                                dbPlayer.SendNewNotification($"Du scheinst mir zu unseriös zu sein... Arbeitest du schon etwas anderes?");
                                return true;
                            }
                            if (dbPlayer.WorkstationId == workstation.Id)
                            {
                                dbPlayer.SendNewNotification($"Sie sind hier bereits eingemietet!");
                                return true;
                            }

                            // Planning room weaponstuff
                            if (workstation.SpecialType == WorkstationSpecialType.PlanningRoomStahlpatronen)
                            {
                                PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id);
                                if (room == null || room.BasementWeaponsLevel < 1)
                                {
                                    dbPlayer.SendNewNotification($"Sie müssen zuerst die Waffenfunktion ausbauen!");
                                    return true;
                                }
                            }

                            if (workstation.RequiredLevel > 0 && workstation.RequiredLevel > dbPlayer.Level)
                            {
                                dbPlayer.SendNewNotification($"Für diese Workstation benötigen Sie mind Level {workstation.RequiredLevel}!");
                                return true;
                            }

                            if (!dbPlayer.TakeMoney(2500))
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(2500));
                                return true;
                            }
                            dbPlayer.WorkstationEndContainer.ClearInventory();
                            dbPlayer.WorkstationFuelContainer.ClearInventory();
                            dbPlayer.WorkstationSourceContainer.ClearInventory();

                            dbPlayer.SendNewNotification($"Sie haben sich in {workstation.Name} eingemietet und können diese nun benutzen!");
                            dbPlayer.WorkstationId = workstation.Id;
                            dbPlayer.SaveWorkstation();
                            return true;
                        }
                    }
                    else if (index == 8)
                    {
                        //Methlabor suchen
                        if (teamShelter.LoboratoryPosition == null)
                        {
                            if (dbPlayer.TeamRankPermission.Manage < 1)
                            {
                                dbPlayer.SendNewNotification("Du musst Leader deiner Fraktion sein!");
                                return true;
                            }

                            if (dbPlayer.Team.IsMethTeam())
                            {
                                var methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByTeamId(dbPlayer.TeamId);
                                teamShelter.LoboratoryPosition = methlaboratory.JumpPointEingang.Position;
                            }
                            else if (dbPlayer.Team.IsWeaponTeam())
                            {
                                var weaponlab = WeaponlaboratoryModule.Instance.GetLaboratoryByTeamId(dbPlayer.TeamId);
                                teamShelter.LoboratoryPosition = weaponlab.JumpPointEingang.Position;
                            }
                            else if (dbPlayer.Team.IsWeedTeam())
                            {
                                var cannabislab = CannabislaboratoryModule.Instance.GetLaboratoryByTeamId(dbPlayer.TeamId);
                                teamShelter.LoboratoryPosition = cannabislab.JumpPointEingang.Position;
                            }
                            else if (dbPlayer.Team.IsHeroinTeam())
                            {
                                var heroinlab = HeroinlaboratoryModule.Instance.GetLaboratoryByTeamId(dbPlayer.TeamId);
                                teamShelter.LoboratoryPosition = heroinlab.JumpPointEingang.Position;
                            }
                        }

                        float X = teamShelter.LoboratoryPosition.X;
                        float Y = teamShelter.LoboratoryPosition.Y;
                        dbPlayer.SetWaypoint(X, Y);
                        dbPlayer.SendNewNotification("Hier ist das Labor!");
                    }
                    else if (index == 9)
                    {
                        if (GangwarTownModule.Instance.GetOwnedTownsCount(dbPlayer.Team) >= 3)
                        {
                            var gun = dbPlayer.Player.CurrentWeapon;
                            if (gun != 0)
                            {

                                var l_WeaponDatas = WeaponDataModule.Instance.GetAll();
                                var l_Weapon = l_WeaponDatas.Values.FirstOrDefault(data => data.Hash == (int)gun);
                                if (l_Weapon != null)
                                {

                                    Weapons.Component.WeaponComponent weaponComponent = WeaponComponentModule.Instance.GetAll().Values.Where(c => GangwarModule.GoldComponentIds.Contains(c.Id) && c.WeaponDataId == l_Weapon.Id).FirstOrDefault();
                                    if (weaponComponent != null)
                                    {

                                        dbPlayer.GiveWeaponComponent((uint)l_Weapon.Hash, weaponComponent.Hash);
                                        dbPlayer.SendNewNotification("Waffe vergoldet!");
                                        return true;
                                    }
                                }
                            }
                            dbPlayer.SendNewNotification("Diese Waffe kannst du nicht vergolden!");
                            return true;
                        }
                        else
                        {
                            dbPlayer.SendNewNotification("Dein Team besitzt keine 3 Gebiete!");
                            return true;
                        }
                    }
                    else if (index == 10)
                    {
                        //Fingerprint
                        int FingerPrintedWeaponCount = dbPlayer.Container.GetItemsByDataKey("fingerprint").Count;
                        if (FingerPrintedWeaponCount > 0)
                        {
                            int price = FingerPrintedWeaponCount * 25000;
                            if (!dbPlayer.TakeBlackMoney(price))
                            {
                                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughSWMoney(price));
                                return true;
                            }

                            foreach (Item item in dbPlayer.Container.GetItemsByDataKey("fingerprint"))
                            {
                                item.Data = new Dictionary<string, dynamic>();
                            }
                            dbPlayer.Container.SaveAll();

                            dbPlayer.SendNewNotification($"Sie haben {FingerPrintedWeaponCount} für ${price} entfernen lassen!");
                            return true;
                        }
                        else
                        {
                            if (!dbPlayer.InParamedicDuty)
                            {
                                bool success = dbPlayer.SetPlayerInMedicDuty();
                                if (success)
                                    dbPlayer.SendNewNotification($"Du bist nun im Medic-Dienst deiner Fraktion!");
                                else
                                    dbPlayer.SendNewNotification("Du kannst nicht in den Medic-Dienst gehen. (Max-Medics im Dienst erreicht)");
                            }
                            else
                            {
                                dbPlayer.SetPlayerOffMedicDuty();
                                dbPlayer.SendNewNotification($"Du bist nun nicht mehr im Medic-Dienst deiner Fraktion!");
                            }
                        }
                    }
                    else if (index == 11)
                    {
                        if (!dbPlayer.InParamedicDuty)
                        {
                            bool success = dbPlayer.SetPlayerInMedicDuty();
                            if (success)
                                dbPlayer.SendNewNotification($"Du bist nun im Medic-Dienst deiner Fraktion!");
                            else
                                dbPlayer.SendNewNotification("Du kannst nicht in den Medic-Dienst gehen. (Max-Medics im Dienst erreicht)");
                        }
                        else
                        {
                            dbPlayer.SetPlayerOffMedicDuty();
                            dbPlayer.SendNewNotification($"Du bist nun nicht mehr im Medic-Dienst deiner Fraktion!");
                        }
                    }

                    return true;
                }
                return false;
            }

            private static bool CanPlayerJoinGangwar(DbPlayer dbPlayer)
            {
                return dbPlayer.Team.Members.Values.Count(p => p.Player.Dimension == GangwarModule.Instance.DefaultDimension) < 20;
            }
        }
    }
}
