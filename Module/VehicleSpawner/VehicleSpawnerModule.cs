using System;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.VehicleSpawner
{
    public sealed class VehicleSpawnerModule : Module<VehicleSpawnerModule>
    {

        public override void OnMinuteUpdate()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (SxVehicle sxVehicle in VehicleHandler.Instance.GetAllVehicles())
                {
                    if (sxVehicle == null || !sxVehicle.IsValid()) continue;

                    if (sxVehicle.IsPlayerVehicle() || sxVehicle.IsTeamVehicle())
                    {
                        if (sxVehicle.entity.HasData("lastSavedPos"))
                        {
                            if (sxVehicle.entity == null) continue;
                            Vector3 lastSavedPos = sxVehicle.entity.GetData<Vector3>("lastSavedPos");
                            if (lastSavedPos.DistanceTo(sxVehicle.entity.Position) > 5.0f)
                            {
                                SaveVehiclePosition(sxVehicle);
                            }
                        }
                        else
                        {
                            SaveVehiclePosition(sxVehicle);
                        }
                    }
                }
            }));
        }

        public void SaveVehiclePosition(SxVehicle sxVehicle)
        {
            string x = sxVehicle.entity.Position.X.ToString().Replace(",", ".");
            string y = sxVehicle.entity.Position.Y.ToString().Replace(",", ".");
            string z = sxVehicle.entity.Position.Z.ToString().Replace(",", ".");
            string rotation = sxVehicle.entity.Rotation.Z.ToString().Replace(",", ".");
            
            if (sxVehicle.databaseId == 0) return;

            if (sxVehicle.entity.Position.X == 0 && sxVehicle.entity.Position.Y == 0) return;

            if (sxVehicle.IsTeamVehicle())
            {
                MySQLHandler.ExecuteAsync($"UPDATE fvehicles SET pos_x = '{x}', pos_y = '{y}', pos_z = '{z}', `fuel` = '{sxVehicle.fuel}', `zustand` = '{Convert.ToInt32(sxVehicle.entity.Health)}', `km` = '{Convert.ToInt32(sxVehicle.Distance)}', `rotation` = '{rotation}' WHERE id = '{sxVehicle.databaseId}' AND team = '{sxVehicle.teamid}'");
            }
            else if (sxVehicle.IsPlayerVehicle())
            {
                MySQLHandler.ExecuteAsync($"UPDATE vehicles SET pos_x = '{x}', pos_y = '{y}', pos_z = '{z}', `fuel` = '{sxVehicle.fuel}', `zustand` = '{Convert.ToInt32(sxVehicle.entity.Health)}', `km` = '{Convert.ToInt32(sxVehicle.Distance)}', `heading` = '{rotation}' WHERE id = '{sxVehicle.databaseId}' AND owner = '{sxVehicle.ownerId}'");
            }
            
            sxVehicle.entity.SetData<Vector3>("lastSavedPos", sxVehicle.entity.Position);
        }
    }
}