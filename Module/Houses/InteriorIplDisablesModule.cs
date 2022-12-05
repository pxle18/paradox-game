using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Houses
{
    public sealed class InteriorIplDisablesModule : SqlModule<InteriorIplDisablesModule, InteriorIplDisable, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `interiors_ipl_disables` ORDER BY id;";
        }
    }

    public static class InteriorIplPlayerExtensions
    {
        public static void UnloadInteriorIPLs(this DbPlayer dbPlayer, uint InteriorID)
        {
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            foreach (var entry in InteriorIplDisablesModule.Instance.GetAll().Where(x => x.Value.InteriorID == InteriorID).ToList())
            {
                dbPlayer.Player.TriggerNewClient("unloadClientIpl", entry.Value.IPL);
            }
        }

        public static void LoadUnloadedInteriorIPLs(this DbPlayer dbPlayer, uint InteriorID)
        {
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            foreach (var entry in InteriorIplDisablesModule.Instance.GetAll().Where(x => x.Value.InteriorID == InteriorID).ToList())
            {
                dbPlayer.Player.TriggerNewClient("loadClientIpl", entry.Value.IPL);
            }
        }
    }
}
