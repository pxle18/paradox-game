using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Tattoo;
using VMP_CNR.Module.Teams.Shelter;

namespace VMP_CNR.Module.Teams.AmmoPackageOrder
{
    public class AmmoPackageOrderMenuBuilder : MenuBuilder
    {
        public AmmoPackageOrderMenuBuilder() : base(PlayerMenu.AmmoPackageOrderMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            if (dbPlayer.Team.Id != (int)TeamTypes.TEAM_HUSTLER && dbPlayer.Team.Id != (int)TeamTypes.TEAM_ICA) return null;
            if (dbPlayer.TeamRank < 8) return null;

            var menu = new Menu.Menu(Menu, "Munitionsbestellung");

            menu.Add($"Schließen");
            foreach(DbTeam dbTeam in TeamModule.Instance.GetAll().Values.Where(t => t.IsGangsters()))
            {
                menu.Add("Bestellung " + dbTeam.Name);
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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else // choose x.x
                {
                    int idx = 1;
                    foreach (DbTeam dbTeam in TeamModule.Instance.GetAll().Values.Where(t => t.IsGangsters()))
                    {
                        if (idx == index)
                        {
                            dbPlayer.SetData("orderedTeam", dbTeam.Id);
                            ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Kisten Anzahl", Callback = "AddAmmoPackageOrder", Message = "Geben Sie die Anzahl an Kisten an." });
                            return true;
                        }
                        else idx++;
                    }
                    return true;
                }
            }
        }
    }
}