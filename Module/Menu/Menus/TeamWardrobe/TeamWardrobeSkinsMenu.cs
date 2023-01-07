using GTANetworkAPI;
using System;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Clothes.Team;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class TeamWardrobeSkinsMenu : MenuBuilder
    {
        public TeamWardrobeSkinsMenu() : base(PlayerMenu.TeamWardrobeSkins)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Fraktionskleiderschrank");
            menu.Add(GlobalMessages.General.Close());
            menu.Add("Normal");
            foreach (var skin in TeamSkinModule.Instance.GetSkinsForTeam(dbPlayer.TeamId))
            {
                menu.Add(skin.Name);
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
                    MenuManager.DismissMenu(dbPlayer.Player, (uint) PlayerMenu.TeamWardrobeSkins);
                    ClothModule.SaveCharacter(dbPlayer);
                    return false;
                }

                if (index == 1)
                {
                    if (dbPlayer.Customization != null)
                    {
                        if (dbPlayer.Customization.Gender == 0)
                        {
                            dbPlayer.Character.Skin = PedHash.FreemodeMale01;
                            dbPlayer.SetSkin(PedHash.FreemodeMale01);
                        }
                        else
                        {
                            dbPlayer.Character.Skin = PedHash.FreemodeFemale01;
                            dbPlayer.SetSkin(PedHash.FreemodeFemale01);
                        }
                    }
                    else
                    {
                        dbPlayer.SetSkin(PedHash.FreemodeMale01);
                    }

                    dbPlayer.Character.Clothes?.Clear();
                    dbPlayer.Character.EquipedProps?.Clear();
                    ClothModule.Instance.ApplyPlayerClothes(dbPlayer);
                    return false;
                }

                index -= 2;
                var skins = TeamSkinModule.Instance.GetSkinsForTeam(dbPlayer.TeamId);
                if (skins.Count <= index)
                {
                    return false;
                }

                var currentSkin = skins[index];
                dbPlayer.Character.Skin = currentSkin.Hash;
                dbPlayer.Character.Clothes?.Clear();
                dbPlayer.Character.EquipedProps?.Clear();
                dbPlayer.SetSkin(currentSkin.Hash);
                ClothModule.Instance.ApplyPlayerClothes(dbPlayer);
                return false;
            }
        }
    }
}