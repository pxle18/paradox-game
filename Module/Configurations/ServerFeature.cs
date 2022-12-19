using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Configurations
{
    public sealed class ServerFeatures
    {
        public static ServerFeatures Instance { get; } = new ServerFeatures();
        public static List<string> inactiveServerFeatures;

        private ServerFeatures() 
        {
            inactiveServerFeatures = new List<string>();
            //inactiveServerFeatures.Add("acpupdate");
            //inactiveServerFeatures.Add("makler-fahrzeuge");
            //inactiveServerFeatures.Add("makler-haus");
            //inactiveServerFeatures.Add("makler-lager");
            //inactiveServerFeatures.Add("jahrmarkt-scooter");
            //inactiveServerFeatures.Add("jahrmarkt-jetski");
            //inactiveServerFeatures.Add("jahrmarkt-rc");
            //inactiveServerFeatures.Add("blackmoney");
            //inactiveServerFeatures.Add("schweissgeraet");

            inactiveServerFeatures.Add("ac-teleport");
            //inactiveServerFeatures.Add("ac-vehicleinventory");
            inactiveServerFeatures.Add("ac-armor");
            inactiveServerFeatures.Add("ac-randomizedNames");

            //inactiveServerFeatures.Add("fishing");
            //inactiveServerFeatures.Add("airflight");
            //inactiveServerFeatures.Add("anticheat-thread");
            inactiveServerFeatures.Add("hunting");
            //inactiveServerFeatures.Add("anticheat");
            inactiveServerFeatures.Add("vehicle-cleanup-exploded");
            inactiveServerFeatures.Add("vehicles-cleanup");
        }

        public static bool IsActive(string featureName)
        {
            return !inactiveServerFeatures.Contains(featureName);
        }

        public static void SetActive(string featureName, bool activate)
        {
            if (activate)
            {
                if (!IsActive(featureName)) inactiveServerFeatures.Remove(featureName);
            }
            else
            {
                inactiveServerFeatures.Add(featureName);
            }
        }
    }
}
