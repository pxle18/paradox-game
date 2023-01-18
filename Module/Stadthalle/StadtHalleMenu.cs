using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Business.FuelStations;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Stadthalle
{
    public class StadtHalleMenu : MenuBuilder
    {

        public StadtHalleMenu() : base(PlayerMenu.StadtHalleMenu)
        {

        }


        public override Menu.NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu.NativeMenu(Menu, "Stadthalle");

            menu.Add($"Schließen");
            menu.Add("Mietvertrag kündigen");
            menu.Add("Zufalls-Handynummer");
            menu.Add("Wunsch-Handynummer");
            menu.Add("Namensänderung");

            /*
            if (dbPlayer.married[0] != 0)
            {
                menu.Add("Scheidung einreichen");
            }*/

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

                int kosten = 0;
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    if(dbPlayer.IsTenant())
                    {
                        HouseRent tenant = dbPlayer.GetTenant();
                        dbPlayer.SendNewNotification($"Sie haben Ihren Mietvertag der Immobilie {tenant.HouseId} gekündigt!");
                        dbPlayer.RemoveTenant();
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Sie besitzen keinen aktiven Mietvertrag!");
                        return false;
                    }

                }
                else if (index == 2)
                {
                    if(dbPlayer.LastPhoneNumberChange.AddMonths(StadthalleModule.PhoneNumberChangingMonths) > DateTime.Now)
                    {
                        dbPlayer.SendNewNotification("Du kannst deine Telefonnummer nur alle 4 Monate ändern!");
                        return false;
                    }

                    int money = 10000 * dbPlayer.Level;

                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject()
                    {
                        Title = $"Zufalls Nummer kaufen",
                        Callback = "changePhoneNumberRandom",
                        Message = $"Du kannst hier eine Telefonnummer automatisch generieren lassen. Dies kostetn 10.000$ * Visumsstufe (In deinem Fall {money}$)," +
                        $" gib [KAUFEN] ein um eine neue Nummer zu beantragen, um abzubrechen nutze [ABBRECHEN]:"
                    });
                }
                else if (index == 3)
                {
                    if (dbPlayer.LastPhoneNumberChange.AddMonths(StadthalleModule.PhoneNumberChangingMonths) > DateTime.Now)
                    {
                        dbPlayer.SendNewNotification("Du kannst deine Telefonnummer nur alle 4 Monate ändern!");
                        return false;
                    }

                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject()
                    {
                        Title = $"Handynummer ändern",
                        Callback = "changePhoneNumber",
                        Message = "Du kannst eine Nummer auswählen zwischen 4-7 Stellen. Die Preise sind wie folgt: 5-7 Stellen jeweils 25.000$ * Visumsstufe. Reine 4 Stellige Nummern kosten 200.000$ * Visumsstufe. " +
                            "Gib bitte deine Wunschnummer ein:"
                    });
                }
                else if (index == 4)
                {
                    kosten = dbPlayer.Level * 15000;
                    if (dbPlayer.Container.GetItemAmount(670) >= 1)
                    {
                        kosten = dbPlayer.Level * 5000;
                    }

                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Namensänderung", Callback = "DoNamechangeRelease", Message = $"Kosten ${kosten} | Gib den neuen Namen an (Vorname_Nachname):" });
                }
                /*
                else if (index == 4)
                {
                    //Scheidung einreichen
                    if (dbPlayer.married[0] != 0)
                    {

                        string marryName = "";
                        int marryLevel = 0;
                        using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                        using (MySqlCommand cmd = conn.CreateCommand())
                        {
                            conn.Open();
                            cmd.CommandText = $"SELECT name, level FROM player WHERE id = '{dbPlayer.married[0]}' LIMIT 1";
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (reader.HasRows)
                                    {
                                        marryName = reader.GetString("name");
                                        marryLevel = reader.GetInt32("level");
                                    }
                                }
                                conn.Close();
                            }
                            
                            kosten = (dbPlayer.Level + marryLevel) * 40000 / 2;
                            ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"Scheidung beantragen | Kosten: ${kosten}", "Die Kosten belaufen sich auf (Visumsstufen der Eheleute) 40.000 $ * (Ehepartner1 + Ehepartner2) / 2 . Beispiel : Visumsstufe 1 und Visumsstufe 50 -> 40.000 $ * (1 + 50) / 2 = 1020000 $! Diese Entscheidung ist einmalig & endgültig.", "DivorceConfirm", "", ""));
                        }
                    }
                }*/
                return true;
            }
        }

    }
}

