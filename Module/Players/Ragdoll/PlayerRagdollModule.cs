using System;
using GTANetworkAPI;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.Ragdoll
{
    public class PlayerRagdollModule : SqlModule<PlayerRagdollModule, PlayerRagdoll, uint>
    {
        private uint PLACEHOLDER_DIMENSION = 123;

        protected override string GetQuery()
        {
            return "SELECT * FROM player_ragdoll;";
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            /*if (!colShape.HasData("ragdoll_id") || !dbPlayer.IsValid() || colShapeState != ColShapeState.Enter)
                return false;

            dbPlayer.Player.TriggerNewClient("triggerRagdoll");
            return true;*/

            if (!colShape.HasData("ragdoll_id") || !dbPlayer.IsValid())
                return false;


            if (colShapeState == ColShapeState.Enter)
                dbPlayer.SetDimensionAsync(PLACEHOLDER_DIMENSION, true);

            return true;
        }
    }
}