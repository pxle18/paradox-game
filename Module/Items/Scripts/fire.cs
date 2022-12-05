using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Camper;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Fire(DbPlayer dbPlayer)
        {
            if (!dbPlayer.CanInteract() || dbPlayer.RageExtension.IsInVehicle) return false;


            CampingPlace campingPlace = CampingModule.Instance.CampingPlaces.ToList().Where(cp => cp.Position.DistanceTo(dbPlayer.Player.Position) < 10.0f).FirstOrDefault();
            if (campingPlace != null)
            {
                // Fire Bed
                if(dbPlayer.Player.Position.DistanceTo2D(campingPlace.Position.Add(CampingModule.AdjustmentBed)) < 1.5f && dbPlayer.Player.Position.DistanceTo(campingPlace.Position.Add(CampingModule.AdjustmentBed)) < 7f)
                {
                    if(campingPlace.FireStateBed == 0)
                    {
                        dbPlayer.SetCannotInteract(true);
                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                        Chats.sendProgressBar(dbPlayer, 5000);
                        await NAPI.Task.WaitForMainThread(5000);

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.StopAnimation();
                        dbPlayer.SetCannotInteract(false);

                        campingPlace.FireStateBed = 5;

                        campingPlace.RefreshObjectsForPlayerInRange();
                        return true;
                    }
                }

                // Fire Tent
                if (dbPlayer.Player.Position.DistanceTo2D(campingPlace.Position.Add(CampingModule.AdjustmentTent)) < 1.5f && dbPlayer.Player.Position.DistanceTo(campingPlace.Position.Add(CampingModule.AdjustmentTent)) < 7f)
                {
                    if (campingPlace.FireStateTent == 0)
                    {
                        dbPlayer.SetCannotInteract(true);
                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                        Chats.sendProgressBar(dbPlayer, 5000);
                        await NAPI.Task.WaitForMainThread(5000);

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.StopAnimation();
                        dbPlayer.SetCannotInteract(false);

                        campingPlace.FireStateTent = 5;

                        campingPlace.RefreshObjectsForPlayerInRange();
                        return true;
                    }
                }

                // Fire Table
                if (campingPlace.IsCocain)
                {
                    if (dbPlayer.Player.Position.DistanceTo2D(campingPlace.Position.Add(CampingModule.AdjustmentTable)) < 1.5f && dbPlayer.Player.Position.DistanceTo(campingPlace.Position.Add(CampingModule.AdjustmentTable)) < 7f)
                    {
                        if (campingPlace.FireStateTable == 0)
                        {
                            dbPlayer.SetCannotInteract(true);
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                            Chats.sendProgressBar(dbPlayer, 5000);
                            await NAPI.Task.WaitForMainThread(5000);

                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.StopAnimation();
                            dbPlayer.SetCannotInteract(false);

                            campingPlace.FireStateTable = 5;

                            campingPlace.RefreshObjectsForPlayerInRange();
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}