using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.FIB.Menu
{
    public class FIBPermitMenu : MenuBuilder
    {
        public FIBPermitMenu() : base(PlayerMenu.FIBPermitMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "FIB Ortungsverwaltung");
            l_Menu.Add($"Schließen");

            if (!p_DbPlayer.HasData("fib_permit_id"))
                return l_Menu;

            if (!uint.TryParse(p_DbPlayer.GetData("fib_permit_id").ToString(), out uint l_AgentID))
                return l_Menu;

            DbPlayer l_Agent = Players.Players.Instance.FindPlayerById(l_AgentID);
            if (l_Agent == null || !l_Agent.IsValid())
                return l_Menu;

            l_Menu.Add($"Beamte orten: {(l_Agent.FindFlags.HasFlag(FindFlags.Beamte) ? "Entziehen" : "Vergeben")}");
            l_Menu.Add($"Ohne Haftbefehl: {(l_Agent.FindFlags.HasFlag(FindFlags.WithoutWarrant) ? "Entziehen" : "Vergeben")}");
            l_Menu.Add($"Aktive Ortung: {(l_Agent.FindFlags.HasFlag(FindFlags.Continuous) ? "Entziehen" : "Vergeben")}");
            l_Menu.Add($"Phonehistory: {(l_Agent.FindFlags.HasFlag(FindFlags.Phonehistory) ? "Entziehen" : "Vergeben")}");

            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (dbPlayer == null || !dbPlayer.IsValid())
                    return false;

                // Keine AgentId gesetzt
                if (!dbPlayer.HasData("fib_permit_id"))
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }

                // AgentID kann nicht in eine Zahl umgewandelt werden
                if (!uint.TryParse(dbPlayer.GetData("fib_permit_id").ToString(), out uint l_AgentID))
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }

                // Agent mit der ID ist nicht online
                DbPlayer l_Agent = Players.Players.Instance.FindPlayerById(l_AgentID);
                if (l_Agent == null || !l_Agent.IsValid())
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }

                FindFlags l_Flags = l_Agent.FindFlags;

                switch (index)
                {
                    case 0: // Schließen
                        MenuManager.DismissCurrent(dbPlayer);
                        return true;
                    case 1: // Beamte Orten
                        bool canFindBeamte = l_Flags.HasFlag(FindFlags.Beamte);

                        if (canFindBeamte)
                            Helper.FlagsHelper.Unset<FindFlags>(ref l_Flags, FindFlags.Beamte);
                        else
                            Helper.FlagsHelper.Set<FindFlags>(ref l_Flags, FindFlags.Beamte);

                        dbPlayer.SendNewNotification($"Dem Agenten {l_Agent.GetName()} wurde die Lizenz für das Orten von Beamten {(canFindBeamte ? "entzogen" : "erteilt")}");
                        dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat dem Agenten {l_Agent.GetName()} die Lizenz für das Orten von Beamten {(canFindBeamte ? "entzogen" : "erteilt")}", rang: 10);
                        break;
                    case 2: // Ohne Haftbefehl
                        bool canFindWithoutWarrant = l_Flags.HasFlag(FindFlags.WithoutWarrant);

                        if (canFindWithoutWarrant)
                            Helper.FlagsHelper.Unset<FindFlags>(ref l_Flags, FindFlags.WithoutWarrant);
                        else
                            Helper.FlagsHelper.Set<FindFlags>(ref l_Flags, FindFlags.WithoutWarrant);

                        dbPlayer.SendNewNotification($"Dem Agenten {l_Agent.GetName()} wurde die Lizenz für das Orten ohne Haftbefehl {(canFindWithoutWarrant ? "entzogen" : "erteilt")}");
                        dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat dem Agenten {l_Agent.GetName()} die Lizenz für das Orten ohne Haftbefehl {(canFindWithoutWarrant ? "entzogen" : "erteilt")}", rang: 10);
                        break;
                    case 3: // Aktive Ortung
                        bool canContinuousFind = l_Flags.HasFlag(FindFlags.Continuous);

                        if (canContinuousFind)
                            Helper.FlagsHelper.Unset<FindFlags>(ref l_Flags, FindFlags.Continuous);
                        else
                            Helper.FlagsHelper.Set<FindFlags>(ref l_Flags, FindFlags.Continuous);

                        dbPlayer.SendNewNotification($"Dem Agenten {l_Agent.GetName()} wurde die Lizenz für die aktive Ortung {(canContinuousFind ? "entzogen" : "erteilt")}");
                        dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat dem Agenten {l_Agent.GetName()} die Lizenz für die aktive Ortung {(canContinuousFind ? "entzogen" : "erteilt")}", rang: 10);
                        break;
                    case 4: // Phonehistory
                        bool canUsePhonehistory = l_Flags.HasFlag(FindFlags.Phonehistory);

                        if (canUsePhonehistory)
                            Helper.FlagsHelper.Unset<FindFlags>(ref l_Flags, FindFlags.Phonehistory);
                        else
                            Helper.FlagsHelper.Set<FindFlags>(ref l_Flags, FindFlags.Phonehistory);

                        dbPlayer.SendNewNotification($"Dem Agenten {l_Agent.GetName()} wurde die Lizenz für die Phonehistory {(canUsePhonehistory ? "entzogen" : "erteilt")}");
                        dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat dem Agenten {l_Agent.GetName()} die Lizenz für die Phonehistory {(canUsePhonehistory ? "entzogen" : "erteilt")}", rang: 10);
                        break;
                }

                l_Agent.FindFlags = l_Flags;
                l_Agent.SaveFindFlags();

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
