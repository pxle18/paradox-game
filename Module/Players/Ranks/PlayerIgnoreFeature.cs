using System.Collections.Generic;
using GTANetworkAPI;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.Ranks
{
    public static class PlayerIgnoreFeature
    {
        public static bool HasFeatureIgnored(this Player client, string name)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            if (!dbPlayer.HasData("ignored_features")) return false;
            HashSet<string> features = dbPlayer.GetData("ignored_features");
            return features.Contains(name);
        }

        public static void SetFeatureIgnored(this Player client, string name)
        {

            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            HashSet<string> features;
            if (!dbPlayer.HasData("ignored_features"))
            {
                features = new HashSet<string>();
                dbPlayer.SetData("ignored_features", features);
            }
            else
            {
                features = dbPlayer.GetData("ignored_features");
            }

            if (features.Contains(name)) return;
            features.Add(name);
        }

        public static void RemoveFeatureIgnored(this Player client, string name)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!dbPlayer.HasData("ignored_features")) return;
            HashSet<string> features = dbPlayer.GetData("ignored_features");
            if (!features.Contains(name)) return;
            features.Remove(name);
        }
    }
}