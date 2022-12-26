using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Handler;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tuning;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Carsell.Menu
{
    public class CarsellTuneWheelMenuBuilder : MenuBuilder
    {
        public static int FrontWheel = 23;

        public CarsellTuneWheelMenuBuilder() : base(PlayerMenu.CarsellTuneWheelMenu)
        {

        }

        public override Module.Menu.NativeMenu Build(DbPlayer p_DbPlayer)
        {
            if (!p_DbPlayer.RageExtension.IsInVehicle) return null;

            var l_Menu = new Module.Menu.NativeMenu(Menu, "Felgen ändern");

            l_Menu.Add($"Schließen");

            l_Menu.Add($"Standard Felge", "");

            Helper.Tuning tuning = Helper.Helper.m_Mods.Values.ToList().Where(tun => tun.ID == FrontWheel).FirstOrDefault();
            if (tuning == null) return null;

            int i = 0;
            for (var l_Itr = tuning.StartIndex + 1; l_Itr <= tuning.MaxIndex; l_Itr++)
            {
                i++;
                if (i > 20) break;
                l_Menu.Add($"Felge {i.ToString()}", "");
            }
            

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
                if(index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                
                if (!dbPlayer.RageExtension.IsInVehicle) return true;

                SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVeh == null || !sxVeh.IsValid()) return true;

                if (index == 1)
                {
                    dbPlayer.SetData("carsellTuneWheelId", -1);
                    sxVeh.SetMod(FrontWheel, -1);
                }

                Helper.Tuning tuning = Helper.Helper.m_Mods.Values.ToList().Where(tun => tun.ID == FrontWheel).FirstOrDefault();
                if (tuning == null) return true;
                
                int i = 1;
                for (var l_Itr = tuning.StartIndex + 1; l_Itr <= tuning.MaxIndex; l_Itr++)
                {
                    i++;
                    if (i > 20) break;
                    if (index == i+1)
                    {
                        sxVeh.SetMod(FrontWheel, i);
                        dbPlayer.SetData("carsellTuneWheelId", i);
                    }
                }                

                return false;
            }
        }
    }
}
