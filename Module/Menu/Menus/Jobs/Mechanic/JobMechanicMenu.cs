
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Helper;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tuning;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR
{
    public class JobMechanmicMenuBuilder : MenuBuilder
    {
        public JobMechanmicMenuBuilder() : base(PlayerMenu.MechanicTune)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            if (!dbPlayer.HasData("tuneVeh")) return null;

            SxVehicle sxVeh = VehicleHandler.Instance.GetByVehicleDatabaseId(dbPlayer.GetData("tuneVeh"));
            if (sxVeh == null || !sxVeh.IsValid()) return null;

            dbPlayer.SetData("isTuning", false);

            Dictionary<int, int> l_Dic = new Dictionary<int, int>();

            var menu = new Menu(Menu, $"Tuning");
            menu.Add(GlobalMessages.General.Close(), "");
            menu.Add("Anbringen", "");
            menu.Add("Standard", "");
            if (!dbPlayer.TryData("tuneSlot", out int tuneSlot)) return menu;
            Console.WriteLine("TuneSlot: " + tuneSlot);
            
            Tuning tuning = Helper.m_Mods.Values.ToList().Where(tun => tun.ID == tuneSlot).FirstOrDefault();
            if (tuning == null) return menu;
            
            int i = 0;
            for (var l_Itr = tuning.StartIndex + 1; l_Itr <= tuning.MaxIndex; l_Itr++)
            {
                i++;
                menu.Add($"Teil {i.ToString()}", "");
                l_Dic.Add(l_Itr + 3, l_Itr);
            }
            
            dbPlayer.SetData("tuningList", l_Dic);
            dbPlayer.SetData("tuneIndex", 0);

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
                if (!dbPlayer.HasData("tuneSlot"))
                {
                    dbPlayer.SendNewNotification($"TuneSlot nicht gesetzt!");
                    return false;
                }

                if (!dbPlayer.HasData("tuneVeh"))
                {
                    dbPlayer.SendNewNotification($"TuneVeh nicht gesetzt!");
                    return false;
                }

                if (!dbPlayer.HasData("tuneIndex"))
                {
                    dbPlayer.SendNewNotification($"tuneIndex nicht gesetzt!");
                    return false;
                }

                Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                {
                    // get vehicle
                    if (!dbPlayer.HasData("tuneVeh"))
                        return;

                    // Irgendwie war die Data obwohl sie als uint gesetzt wurde, kein Uint mehr. Illuminati?
                    string l_DataString = dbPlayer.GetData("tuneVeh").ToString();
                    if (!uint.TryParse(l_DataString, out uint l_VehID))
                        return;

                    SxVehicle sxVeh = VehicleHandler.Instance.GetByVehicleDatabaseId(l_VehID);
                    if (sxVeh == null)
                    {
                        dbPlayer.SendNewNotification($"Fehler bei der Fahrzeugauswahl!");
                        return;
                    }

                    if (!dbPlayer.HasData("tuneSlot"))
                    {
                        dbPlayer.SendNewNotification($"Async tuneSlot");
                        return;
                    }

                    if (dbPlayer.GetData("isTuning"))
                        return;

                    int l_TuneSlot = (int)dbPlayer.GetData("tuneSlot");
                    int l_TuneIndex = (int)dbPlayer.GetData("tuneIndex");

                    Tuning tuning = Helper.m_Mods.Values.ToList().Where(tun => tun.ID == l_TuneSlot).FirstOrDefault();

                    if (tuning == null)
                    {
                        dbPlayer.SendNewNotification("tuning not found!");
                        return;
                    }

                    if (index == 0)
                    {
                        dbPlayer.ResetData("tuneIndex");
                        dbPlayer.ResetData("tuneSlot");
                        dbPlayer.ResetData("tuneVeh");
                        MenuManager.DismissMenu(dbPlayer.Player, (int)PlayerMenu.MechanicTune);
                        return;
                    }

                    if (index == 1)
                    {
                        MenuManager.DismissMenu(dbPlayer.Player, (int)PlayerMenu.MechanicTune);

                        if(dbPlayer.TeamRank < 10 && !sxVeh.InTuningProcess)
                        {
                            dbPlayer.SendNewNotification("Fahrzeug ist nicht im Tuning Besitz!");
                            return;
                        }

                        int time = 10;
                        if(dbPlayer.HasData("inTuning"))
                        {
                            dbPlayer.SendNewNotification("Sie bringen gerade ein Tuningteil an!");
                            return;
                        }
                        dbPlayer.SetData("inTuning", true);

                        Chats.sendProgressBar(dbPlayer, (time * 1000));
                        
                        await GTANetworkAPI.NAPI.Task.WaitForMainThread(time * 1000);

                        sxVeh.AddSavedMod(l_TuneSlot, l_TuneIndex);
                        dbPlayer.ResetData("tuneIndex");
                        dbPlayer.ResetData("tuneSlot");
                        dbPlayer.ResetData("isTuning");
                        dbPlayer.ResetData("inTuning");
                    }

                    if (index == 2)
                    {
                        dbPlayer.SetData("isTuning", false);
                        dbPlayer.SetData("tuneIndex", tuning.StartIndex);
                        sxVeh.SetMod(l_TuneSlot, tuning.StartIndex);
                    }

                    if (!dbPlayer.HasData("tuningList"))
                        return;

                    Dictionary<int, int> l_Dic = dbPlayer.GetData("tuningList");
                    if (l_Dic.ContainsKey(index))
                    {
                        dbPlayer.SetData("isTuning", false);
                        dbPlayer.SetData("tuneIndex", l_Dic[index]);
                        sxVeh.SetMod(l_TuneSlot, l_Dic[index]);
                    }
                }));

                return false;
            }
        }
    }
}
