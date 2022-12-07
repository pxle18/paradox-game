using GTANetworkAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Tuning
{
    public class TuningModule : Module<TuningModule>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemModelModule) };
        }

        public static List<uint> VehicleAvailableMods = new List<uint>();
        
        public override bool Load(bool reload = false)
        {
            VehicleAvailableMods = new List<uint>();

            // add only tunings what available
            foreach(ItemModel itemModel in ItemModelModule.Instance.GetAll().Values.ToList().Where(im => im.Script.ToLower().StartsWith("tune_")))
            {
                VehicleAvailableMods.Add(Convert.ToUInt32(itemModel.Script.ToLower().Split("tune_")[1]));
            }
            return true;
        }
        
    }

    public static class TuningVehicleExtension
    {
        private static Random l_Random = new Random();
        
        public static void SyncMods(this SxVehicle sxVehicle)
        {
            sxVehicle.ClearMods();
            foreach (KeyValuePair<int, int> kvp in sxVehicle.Mods.ToList())
            {
               sxVehicle.SetMod(kvp.Key, kvp.Value);
            }
        }

        public static ConcurrentDictionary<int, int> ConvertModsToDictonary(string tuning)
        {
            var mods = new ConcurrentDictionary<int, int>();
            if (string.IsNullOrEmpty(tuning)) return mods;
            var splittedmodsString = tuning.Split(',');
            foreach (var modString in splittedmodsString)
            {
                if (string.IsNullOrEmpty(modString)) continue;
                var parts = modString.Split(':');
                if (parts.Length < 2) continue;
                if (!int.TryParse(parts[0], out var slot)) continue;
                if (!int.TryParse(parts[1], out var mod)) continue;
                if (mods.ContainsKey(slot))
                {
                    mods[slot] = mod;
                }
                else
                {
                    mods.TryAdd(slot, mod);
                }
            }

            return mods;
        }

        public static string ConvertModsToString(ConcurrentDictionary<int, int> mods)
        {
            var modsString = new StringBuilder();
            var count = 0;
            var lengthEnd = mods.Count - 1;
            foreach (var mod in mods)
            {
                modsString.Append($"{mod.Key}:{mod.Value}");
                if (count != lengthEnd)
                {
                    modsString.Append(",");
                }
                count++;
            }

            return modsString.ToString();
        }

        public static void SetMod(this SxVehicle sxVehicle, int type, int mod)
        {
            if (type == 1337)
            {
                NAPI.Vehicle.SetVehicleWheelType(sxVehicle.Entity,mod);
                return;
            }
            if (type == 98)
            {
                NAPI.Vehicle.SetVehiclePearlescentColor(sxVehicle.Entity, mod);
                return;
            }
            if (type == 99)
            {
                NAPI.Vehicle.SetVehicleWheelColor(sxVehicle.Entity, mod);
                return;
            }
            
            if (type == 97)
            {
                // NOT IMPLEMENTED
                //NAPI.Vehicle.SetVehicleTyreSmokeColor(sxVehicle.entity, new Color(sxVehicle.Mods[95], sxVehicle.Mods[96], mod));
                return;
            }

            NAPI.Vehicle.SetVehicleMod(sxVehicle.Entity, type, mod);

        }

        public static void RemoveMod(this SxVehicle sxVehicle, int type)
        {

            if (type == 1337)
            {
                NAPI.Vehicle.SetVehicleWheelType(sxVehicle.Entity, -1);
                return;
            }
            if (type == 98)
            {
                NAPI.Vehicle.SetVehiclePearlescentColor(sxVehicle.Entity, 0);
                return;
            }
            if (type == 99)
            {
                NAPI.Vehicle.SetVehicleWheelColor(sxVehicle.Entity, 0);
                return;
            }

            if (type == 97)
            {
                // NOT IMPLEMENTED
                //NAPI.Vehicle.SetVehicleTyreSmokeColor(sxVehicle.entity, new Color(255, 255, 255));
                return;
            }

            NAPI.Vehicle.RemoveVehicleMod(sxVehicle.Entity,type);

        }

        public static void ClearMods(this SxVehicle sxVehicle)
        {
            foreach (var l_Tuning in Helper.Helper.m_Mods)
            {
                sxVehicle.RemoveMod((int)l_Tuning.Value.ID);
            }
            
            sxVehicle.Entity.PrimaryColor = sxVehicle.color1;
            sxVehicle.Entity.SecondaryColor = sxVehicle.color2;
        }

        public static void AddSavedMod(this SxVehicle sxVehicle, int type, int mod, bool sync = true)
        {        
            if (!sxVehicle.Mods.ContainsKey(type)) sxVehicle.Mods.TryAdd(type, -1);
            sxVehicle.Mods[type] = mod;
            if (sync)
            {
                sxVehicle.SetMod(type, mod);
            }

            sxVehicle.SaveMods();

        }

        public static void AddMod(this SxVehicle sxVehicle, int type, int mod)
        {
            if (!sxVehicle.Mods.ContainsKey(type)) sxVehicle.Mods.TryAdd(type, -1);
            sxVehicle.Mods[type] = mod;
        }



            public static void RemoveSavedMod(this SxVehicle sxVehicle, int type, int mod)
        {            
            if (sxVehicle.Mods.ContainsKey(type)) sxVehicle.Mods.Remove(type, out int save);
            sxVehicle.Mods[type] = -1;
            sxVehicle.RemoveMod(type);

            sxVehicle.SaveMods();
        }
        
        public static void SaveMods(this SxVehicle sxVehicle)
        {
            if (sxVehicle.IsPlayerVehicle())
            {
                var query = $"UPDATE `vehicles` SET tuning = '{ConvertModsToString(sxVehicle.Mods)}' WHERE id = '{sxVehicle.databaseId}';";
                MySQLHandler.ExecuteAsync(query);
            }
            else if (sxVehicle.IsTeamVehicle())
            {
                var query = $"UPDATE `fvehicles` SET tuning = '{ConvertModsToString(sxVehicle.Mods)}' WHERE id = '{sxVehicle.databaseId}';";
                MySQLHandler.ExecuteAsync(query);
            }
        }
    }
}