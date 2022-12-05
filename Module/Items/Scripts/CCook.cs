using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Camper;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> CCook(DbPlayer dbPlayer, ItemModel ItemData)
        {

            if (dbPlayer.RageExtension.IsInVehicle) return false;
            CampingPlace campingPlace = CampingModule.Instance.CampingPlaces.ToList().Where(cp => cp.Position.DistanceTo(dbPlayer.Player.Position) < 7.0f).FirstOrDefault();
            if (campingPlace != null)
            {
                if (campingPlace.GrillPosition == new Vector3(0, 0, 0)) return false;

                if (campingPlace.GrillPosition.DistanceTo(dbPlayer.Player.Position) < 2.0f)
                {
                    string[] args = ItemData.Script.Split('_');
                    if (!UInt32.TryParse(args[1], out uint newItemId)) return false;
                    ItemModel newItem = ItemModelModule.Instance.Get(newItemId);
                    if (newItem == null) return false;

                    if(!dbPlayer.Container.CanInventoryItemAdded(newItem, 1))
                    {
                        dbPlayer.SendNewNotification("Du hast nicht genug platz im Inventar!");
                        return false;
                    }

                    Attachments.AttachmentModule.Instance.AddAttachment(dbPlayer, 64, true);

                    dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_drinking@coffee@female@base", "base");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                    dbPlayer.SetData("userCannotInterrupt", true);

                    Chats.sendProgressBar(dbPlayer, 20000);
                    await Task.Delay(20000);

                    dbPlayer.ResetData("userCannotInterrupt");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                    dbPlayer.StopAnimation();

                    campingPlace.SetLastUsedNow();

                    dbPlayer.SendNewNotification($"Du hast {ItemData.Name} gegrillt!");

                    dbPlayer.Container.AddItem(newItemId, 1);
                    return true;
                }
            }

            return false;
        }
    }
}