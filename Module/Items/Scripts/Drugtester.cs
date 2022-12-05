using System;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Laboratories;
using VMP_CNR.Module.Meth;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> DrugtestAir(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle || dbPlayer.TeamId != (int)teams.TEAM_FIB) return false;
        
            if (dbPlayer.HasData("lastDrugAirUsed"))
            {
                DateTime lastUsed = dbPlayer.GetData("lastDrugAirUsed");
                if(lastUsed.AddMinutes(1) > DateTime.Now)
                {
                    dbPlayer.SendNewNotification("Es kann nur eine Messung innerhalb 1 Minuten durchgeführt werden!");
                    return true;
                }
                else
                    dbPlayer.SetData("lastDrugAirUsed", DateTime.Now);
            }
            else
                dbPlayer.SetData("lastDrugAirUsed", DateTime.Now);

            Chat.Chats.sendProgressBar(dbPlayer, 30000);
            await NAPI.Task.WaitForMainThread(30000);

            foreach (SxVehicle sxvehicle in HeroinModule.CookingVehicles.ToList())
            {
                if (sxvehicle == null || !sxvehicle.IsValid()) continue;
                float Distance = dbPlayer.Player.Position.DistanceTo(sxvehicle.entity.Position);
                if (Distance < HeroinModule.CamperDrugAirRange)
                {
                    // get percent
                    int percDist = 10-((int)((Distance/60) * 10));
                    int Stickstoff = Convert.ToInt32(0.78f * (100-percDist));
                    int Sauerstoff = Convert.ToInt32(0.20f * (100-percDist));
                    dbPlayer.SendNewNotification($"Lufttest: Stickstoff {Stickstoff}%, Sauerstoff {Sauerstoff}%, Aldehyde {percDist}%");
                    return true;
                }
            }
            
            foreach(Methlaboratory methlaboratory in Laboratories.MethlaboratoryModule.Instance.GetAll().Values.Where(m => m.ProzessingPlayers.Count() > 0))
            {
                float Distance = dbPlayer.Player.Position.DistanceTo(methlaboratory.JumpPointEingang.Position);
                float maxDistance = methlaboratory.ProzessingPlayers.Count() * HeroinModule.DrugLabIncreaseRange;
                if (maxDistance > 150) maxDistance = 150;
                if(Distance < maxDistance)
                {
                    // get percent
                    int percDist = 10 - ((int)((Distance / 60) * 10));
                    int Stickstoff = Convert.ToInt32(0.78 * (100 - percDist));
                    int Sauerstoff = Convert.ToInt32(0.20 * (100 - percDist));
                    dbPlayer.SendNewNotification($"Lufttest: Stickstoff {Stickstoff}%, Sauerstoff {Sauerstoff}%, Aldehyde {percDist}%");
                    return true;
                }
            }

            foreach (Cannabislaboratory cannabislab in Laboratories.CannabislaboratoryModule.Instance.GetAll().Values.Where(m => m.ActingPlayers.Count > 0))
            {
                float Distance = dbPlayer.Player.Position.DistanceTo(cannabislab.JumpPointEingang.Position);
                float maxDistance = cannabislab.ActingPlayers.Count() * HeroinModule.DrugLabIncreaseRange;
                if (maxDistance > 150) maxDistance = 150;
                if (Distance < maxDistance)
                {
                    int percDist = 10 - ((int)((Distance / 60) * 10));
                    int Stickstoff = Convert.ToInt32(0.78 * (100 - percDist));
                    int Sauerstoff = Convert.ToInt32(0.20 * (100 - percDist));
                    dbPlayer.SendNewNotification($"Lufttest: Stickstoff {Stickstoff}%, Sauerstoff {Sauerstoff}%, THC-Gehalt {percDist}%");
                    return true;
                }
            }

            foreach (Weaponlaboratory cannabislab in Laboratories.WeaponlaboratoryModule.Instance.GetAll().Values.Where(m => m.ActingPlayers.Count > 0))
            {
                float Distance = dbPlayer.Player.Position.DistanceTo(cannabislab.JumpPointEingang.Position);
                float maxDistance = cannabislab.ActingPlayers.Count() * HeroinModule.DrugLabIncreaseRange;
                if (maxDistance > 150) maxDistance = 150;
                if (Distance < maxDistance)
                {
                    int percDist = 10 - ((int)((Distance / 60) * 10));
                    int Stickstoff = Convert.ToInt32(0.78 * (100 - percDist));
                    int Sauerstoff = Convert.ToInt32(0.20 * (100 - percDist));
                    dbPlayer.SendNewNotification($"Lufttest: Stickstoff {Stickstoff}%, Sauerstoff {Sauerstoff}%, Schwefel {percDist}%");
                    return true;
                }
            }

            foreach (Heroinlaboratory cannabislab in Laboratories.HeroinlaboratoryModule.Instance.GetAll().Values.Where(m => m.ActingPlayers.Count > 0))
            {
                float Distance = dbPlayer.Player.Position.DistanceTo(cannabislab.JumpPointEingang.Position);
                float maxDistance = cannabislab.ActingPlayers.Count() * HeroinModule.DrugLabIncreaseRange;
                if (maxDistance > 150) maxDistance = 150;
                if (Distance < maxDistance)
                {
                    int percDist = 10 - ((int)((Distance / 60) * 10));
                    int Stickstoff = Convert.ToInt32(0.78 * (100 - percDist));
                    int Sauerstoff = Convert.ToInt32(0.20 * (100 - percDist));
                    dbPlayer.SendNewNotification($"Lufttest: Stickstoff {Stickstoff}%, Sauerstoff {Sauerstoff}%, Schwefel {percDist}%");
                    return true;
                }
            }

            dbPlayer.SendNewNotification($"Lufttest: Stickstoff 78%, Sauerstoff 22%, Aldehyde 0%");
            return true;
        }
    }
}