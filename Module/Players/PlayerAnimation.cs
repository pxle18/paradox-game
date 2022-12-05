using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using GTANetworkMethods;
using Newtonsoft.Json;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Players
{
    public static class PlayerAnimation
    {
        public static async void PlayAnimation(this DbPlayer dbPlayer, AnimationScenarioType Type, string Context1,
            string Context2 = "", int lifetime = 5, bool repeat = false,
            AnimationLevels AnimationLevel = AnimationLevels.User, int specialflag = 0, bool noFreeze = false)
        {
            
                if (((int)AnimationLevel < (int)dbPlayer.AnimationScenario.AnimationLevel))
                {
                    return;
                }

                dbPlayer.AnimationScenario.Context1 = Context1;
                dbPlayer.AnimationScenario.Context2 = Context2;
                dbPlayer.AnimationScenario.Lifetime = lifetime;
                dbPlayer.AnimationScenario.AnimationLevel = AnimationLevel;
                dbPlayer.AnimationScenario.StartTime = DateTime.Now;
                dbPlayer.AnimationScenario.Repeat = repeat;
                dbPlayer.AnimationScenario.SpecialFlag = specialflag;

                if (Type == AnimationScenarioType.Animation)
                {
                    // do animation
                    dbPlayer.Player.PlayAnimation(Context1, Context2, specialflag);
                }
                else
                {
                    //do Scenario
                    dbPlayer.Player.PlayScenario(Context1);
                }

                dbPlayer.AnimationScenario.Active = true;
            
        }

        //public static async void StopAnimation(this DbPlayer dbPlayer, AnimationLevels AnimationLevel = AnimationLevels.User)
        //{
            
        //        if (!dbPlayer.AnimationScenario.Active)
        //        {
        //            if ((int)dbPlayer.AnimationScenario.AnimationLevel > (int)AnimationLevel)
        //            {
        //                return;
        //            }
        //        }
        //        else dbPlayer.AnimationScenario.Active = false;

        //        dbPlayer.Player.StopAnimation();
        //        //dbPlayer.Player.FreezePosition = false;
        //        dbPlayer.AnimationScenario.Active = false;
        //        dbPlayer.AnimationScenario.AnimationLevel = 0;
            
        //}

        public static void StopAnimation(this DbPlayer dbPlayer, AnimationLevels AnimationLevel = AnimationLevels.User, bool dontRemoveAttachments = false)
        {
            dbPlayer.PlayingAnimation = false;
            dbPlayer.Player.TriggerNewClient("SetOwnAnimData", JsonConvert.SerializeObject(new AnimationSyncItem(dbPlayer)));

            // Sync für den Fall, dass man durch eine Tür geht. Damit die Anim für andere nicht wieder startet
            List<DbPlayer> nearPlayers = Players.Instance.GetPlayersListInRange(dbPlayer.Player.Position);

            foreach (DbPlayer iPlayer in nearPlayers)
            {
                if (iPlayer == null || !iPlayer.IsValid()) continue;
                iPlayer.Player.TriggerNewClient("SetAnimDataNear", dbPlayer.Player, JsonConvert.SerializeObject(new AnimationSyncItem(dbPlayer)));
            }

            if(!dontRemoveAttachments) Attachments.AttachmentModule.Instance.ClearAllAttachments(dbPlayer);

            dbPlayer.SyncAttachmentOnlyItems();

            if (!dbPlayer.AnimationScenario.Active)
            {
                if ((int)dbPlayer.AnimationScenario.AnimationLevel > (int)AnimationLevel)
                {
                    return;
                }
            }
            else
                dbPlayer.AnimationScenario.Active = false;

            NAPI.Task.Run(() =>
            {
                if (!dbPlayer.RageExtension.IsInVehicle)
                {
                    dbPlayer.Player.StopAnimation();
                }
            });

            dbPlayer.AnimationScenario.Active = false;
            dbPlayer.AnimationScenario.AnimationLevel = 0;
            dbPlayer.Player.TriggerNewClient("VisibleWindowBug");
        }

    public static bool IsInAnimation(this DbPlayer dbPlayer)
        {
            return (dbPlayer.AnimationScenario.Active &&
                    dbPlayer.AnimationScenario.AnimationLevel > AnimationLevels.NonRelevant);
        }
    }
}