using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Jails
{
    public sealed class JailCellModule : SqlModule<JailCellModule, JailCell, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `jail_cells`;";
        }
    }

    public sealed class JailSpawnModule : SqlModule<JailSpawnModule, JailSpawn, uint>
    {

        protected override string GetQuery()
        {
            return "SELECT * FROM `jail_spawns`;";
        }
    }

    public class JailModule : Module<JailModule>
    {
        public static Vector3 PrisonZone = new Vector3(1681, 2604, 44);
        public static float Range = 200.0f;

        public static Vector3 PrisonSpawn = new Vector3(1836.71, 2587.8, 45.891);
        
        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;

            if (colShape == null || !colShape.HasData("jailGroup")) return false;

            if (colShapeState == ColShapeState.Enter)
            {
                if (dbPlayer.IsACop() && dbPlayer.IsInDuty()) return false;

                var wanteds = dbPlayer.wanteds[0];
                if (dbPlayer.TempWanteds > 0 && dbPlayer.wanteds[0] < 30) wanteds = 30;

                if(dbPlayer.jailtime[0] > 0)
                {
                    // already inhaftiert
                    return false;
                }

                int jailtime = CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes);
                int jailcosts = CrimeModule.Instance.CalcJailCosts(dbPlayer.Crimes, dbPlayer.EconomyIndex);

                // Checke auf Jailtime
                if (jailtime > 0 && jailtime <= 29 && colShape.GetData<int>("jailGroup") != 5)
                {
                    dbPlayer.jailtime[0] = jailtime;
                    dbPlayer.jailtimeReducing[0] = Convert.ToInt32(dbPlayer.jailtime[0] / 3);
                    dbPlayer.ArrestPlayer(null, false);
                    dbPlayer.ApplyCharacter();
                    dbPlayer.SetData("inJailGroup", colShape.GetData<int>("jailGroup"));
                } // group 5 == sg
                else if(colShape.GetData<int>("jailGroup") == 5 && jailtime >= 30)
                {
                    dbPlayer.jailtime[0] = jailtime;
                    dbPlayer.jailtimeReducing[0] = Convert.ToInt32(dbPlayer.jailtime[0] / 3);
                    dbPlayer.ArrestPlayer(null, false);
                    dbPlayer.ApplyCharacter();
                    dbPlayer.SetData("inJailGroup", colShape.GetData<int>("jailGroup"));
                }
                
            }
            else
            {
                dbPlayer.ResetData("inJailGroup");
            }

            return false;
        }


    }
}
