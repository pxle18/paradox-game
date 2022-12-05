using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Armory;
using VMP_CNR.Module.Banks;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles.Garages;

namespace VMP_CNR.Module.AsyncEventTasks
{
    public static partial class AsyncEventTasks
    {
        public static void ExitColShapeTask(ColShape shape, Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;
            if (!dbPlayer.IsValid()) return;

            if (Modules.Instance.OnColShapeEvent(dbPlayer, shape, ColShapeState.Exit)) return;

            if (shape.HasData("clothShopId"))
            {
                ClothModule.Instance.ResetClothes(dbPlayer);
                
                if (dbPlayer.HasData("clothShopId"))
                {
                    dbPlayer.ResetData("clothShopId");
                }
            }

            if (shape.HasData("teamWardrobe"))
            {
                ClothModule.Instance.ResetClothes(dbPlayer);
                
                if (dbPlayer.HasData("teamWardrobe"))
                {
                    dbPlayer.ResetData("teamWardrobe");
                }
            }

            if (shape.HasData("ammunationId"))
            {
                if (dbPlayer.HasData("ammunationId"))
                {
                    dbPlayer.ResetData("ammunationId");
                }
            }

            if (shape.HasData("garageId"))
            {
                if (dbPlayer.HasData("garageId"))
                {
                    dbPlayer.ResetData("garageId");
                }
            }
            
            if (shape.HasData("bankId"))
            {
                if (dbPlayer.HasData("bankId"))
                {
                    dbPlayer.ResetData("bankId");
                }
            }

            if (shape.HasData("ArmoryId"))
            {
                if (dbPlayer.HasData("ArmoryId"))
                {
                    dbPlayer.ResetData("ArmoryId");
                }
            }
        }
    }
}
