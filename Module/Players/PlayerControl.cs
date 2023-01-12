using System;
using System.Linq;
using VMP_CNR.Handler;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Farming;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.VehicleRent;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Players
{
    public static class PlayerControl
    {
        public static void ChangeSeat(this DbPlayer dbPlayer, int newSeat)
        {
            if (!dbPlayer.RageExtension.IsInVehicle) return;

            GTANetworkAPI.Vehicle vehicle = dbPlayer.Player.Vehicle;
            if (vehicle == null || !vehicle.IsSeatFree(newSeat)) return;
            // Carzy Workaroung
            dbPlayer.WarpOutOfVehicle();
            dbPlayer.Player.SetIntoVehicleSave(vehicle, newSeat);
        }

        public static bool CanControl(this DbPlayer dbPlayer, SxVehicle sxVehicle)
        {
            if (sxVehicle == null || !sxVehicle.IsValid()) return false;
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            try
            {
                if (sxVehicle.InTuningProcess == true && dbPlayer.TeamId == (int)TeamTypes.TEAM_LSC)
                {
                    return true;
                }
                if (sxVehicle.InTuningProcess == true && dbPlayer.TeamId != (int)TeamTypes.TEAM_LSC)
                {
                    return false;
                }

                if (sxVehicle.IsTeamVehicle())
                {
                    if (sxVehicle.teamid == (int)TeamTypes.TEAM_GOV && dbPlayer.TeamId == (int)TeamTypes.TEAM_FIB)
                    {
                        return true;
                    }


                    if (sxVehicle.teamid == (uint)TeamTypes.TEAM_IAA && dbPlayer.IsNSADuty && dbPlayer.IsNSAState >= (int)NSA.NSARangs.LIGHT)
                    {
                        return true;
                    }
                     
                    if (sxVehicle.Team.IsCops() && dbPlayer.IsNSADuty && dbPlayer.IsNSAState >= (int)NSA.NSARangs.NORMAL)
                    {
                        return true;
                    }

                    if (sxVehicle.Team.Id == (int)TeamTypes.TEAM_MEDIC && dbPlayer.IsNSADuty && dbPlayer.IsNSAState >= (int)NSA.NSARangs.NORMAL)
                    {
                        return true;
                    }

                    if (sxVehicle.Team.IsCops() && dbPlayer.TeamId == (int)TeamTypes.TEAM_SWAT)
                    {
                        return true;
                    }

                    if (sxVehicle.teamid == (int)TeamTypes.TEAM_ARMY)
                    {
                        return dbPlayer.TeamId == sxVehicle.teamid && (dbPlayer.TeamRank >= 1 || sxVehicle.Data.Id == 185); // Rang 0 only freecrawler
                    }

                    return dbPlayer.TeamId == sxVehicle.teamid && (!dbPlayer.Team.IsStaatsfraktion() || dbPlayer.Duty);
                }

                if (sxVehicle.ownerId == dbPlayer.Id && VehicleRentModule.PlayerVehicleRentKeys.ToList().Where(k => k.VehicleId == sxVehicle.databaseId).Count() <= 0)
                {
                    return true;
                }

                if (dbPlayer.VehicleKeys != null && dbPlayer.VehicleKeys.Count > 0 && dbPlayer.VehicleKeys.ContainsKey(sxVehicle.databaseId))
                {
                    return true;
                }

                // Business Keys
                if (dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusiness() != null && dbPlayer.GetActiveBusiness().VehicleKeys != null &&
                    dbPlayer.GetActiveBusiness().VehicleKeys.Count > 0 && dbPlayer.GetActiveBusiness().VehicleKeys.ContainsKey(sxVehicle.databaseId))
                {
                    return true;
                }

                // Vehicle Rent
                if (VehicleRentModule.PlayerVehicleRentKeys.ToList().Where(k => k.VehicleId == sxVehicle.databaseId && k.PlayerId == dbPlayer.Id).Count() > 0)
                {
                    return true;
                }
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
            }
            return false;
        }

        public static bool IsOwner(this DbPlayer dbPlayer, SxVehicle sxVehicle)
        {
            if (sxVehicle.ownerId != dbPlayer.Id) return false;
            return sxVehicle.teamid == 0 && sxVehicle.jobid == 0;
        }

        public static void SetCannotInteract(this DbPlayer dbPlayer, bool status)
        {
            if(status) dbPlayer.SetData("userCannotInterrupt", true);
            else dbPlayer.ResetData("userCannotInterrupt");
        }
        public static bool CanInteract(this DbPlayer dbPlayer, bool ignoreFarming = false)
        {
            if (!dbPlayer.IsValid()) return false;
            if (dbPlayer.IsInAnimation()) return false;
            if (dbPlayer.IsCuffed || dbPlayer.IsTied) return false;
            if (dbPlayer.IsInjured()) return false;
            if (dbPlayer.HasData("follow")) return false;
            if (dbPlayer.HasData("userCannotInterrupt") && Convert.ToBoolean(dbPlayer.GetData("userCannotInterrupt"))) return false;
            if (FarmingModule.FarmingList.Contains(dbPlayer) && !ignoreFarming) return false;
            //Todo: can be removed soon
            if (dbPlayer.HasData("disableAnim")) return false;
            return true;
        }
    }
}