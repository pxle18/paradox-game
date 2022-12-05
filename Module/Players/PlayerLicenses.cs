using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.FIB;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Swat;

namespace VMP_CNR.Module.Players
{
    public static class PlayerLicenses
    {
        //ToDo: Add Custom Window

        public static void ShowLicenses(this DbPlayer dbPlayer, Player destinationPlayer)
        {
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (destinationDbPlayer == null || !destinationDbPlayer.IsValid()) return;

            dbPlayer.ShowLicenses(destinationDbPlayer);
        }

        public static void ShowLicenses(this DbPlayer dbPlayer, DbPlayer destinationDbPlayer)
        {
            if (destinationDbPlayer == null || !destinationDbPlayer.IsValid()) return;
            string l_Name = dbPlayer.GetName();
            if ((dbPlayer.IsSwatDuty() || (dbPlayer.IsACop() && dbPlayer.IsInDuty())) && !dbPlayer.IsUndercover())
            {
                l_Name = "Unbekannt";
            }

            destinationDbPlayer.Player.TriggerNewClient("showLicense", l_Name, dbPlayer.Lic_FirstAID[0], dbPlayer.Lic_Gun[0], dbPlayer.Lic_Car[0], dbPlayer.Lic_LKW[0], dbPlayer.Lic_Bike[0], dbPlayer.Lic_Boot[0], dbPlayer.Lic_PlaneA[0], dbPlayer.Lic_PlaneB[0], dbPlayer.Lic_Taxi[0], dbPlayer.Lic_Transfer[0], 0, dbPlayer.marryLic, dbPlayer.grade[0], dbPlayer.zwd[0]);
        }
    }
}