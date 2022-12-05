
using GTANetworkAPI;
using System;
using System.Threading;
using System.Threading.Tasks;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Phone;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Players
{
    //Todo: maybe make vehicle check here as well instead of parameter
    public static class PlayerState
    {
        private const string CuffedMedic = "CuffedMedic";
        private const string AdminDutyEvent = "updateAduty";
        private const string CuffedEvent = "updateCuffed";
        private const string TiedEvent = "upadeTied";
        private const string DutyEvent = "updateDuty";

        public static void SetCuffed(this DbPlayer dbPlayer, bool cuffed, bool inVehicle = false)
        {
            if (dbPlayer.HasData("SMGkilledPos"))
            {
                dbPlayer.SetStunned(false);
            }
            if (cuffed)
            {
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "mp_arresting", inVehicle ? "sit" : "idle");
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);

                dbPlayer.CancelPhoneCall();
                Voice.VoiceModule.Instance.turnOffFunk(dbPlayer);
            }
            else
            {
                dbPlayer.StopAnimation();
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                if (dbPlayer.HasData("follow"))
                    dbPlayer.ResetData("follow");
            }

            if (cuffed)
            {
                dbPlayer.SetData("vehicleCuffed", true);
            }
            else
            {
                if (dbPlayer.HasData("vehicleCuffed"))
                {
                    dbPlayer.ResetData("vehicleCuffed");
                }
            }

            dbPlayer.IsCuffed = cuffed;
            dbPlayer.Player.TriggerNewClient(CuffedEvent, cuffed);
        }

        public static void SetStunned(this DbPlayer dbPlayer, bool stunned)
        {
            if (stunned)
            {
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor3_beatup", "guard_beatup_kickidle_dockworker");
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);

                dbPlayer.CancelPhoneCall();
                Voice.VoiceModule.Instance.turnOffFunk(dbPlayer);
                dbPlayer.IsCuffed = stunned;
                dbPlayer.Player.TriggerNewClient(CuffedEvent, stunned);
            }
            else if (dbPlayer.HasData("SMGkilledPos"))
            {
                dbPlayer.StopAnimation();
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.IsCuffed = stunned;
                dbPlayer.Player.TriggerNewClient(CuffedEvent, stunned);
                dbPlayer.ResetData("SMGkilledPos");
            }
        }

        public static void SetMedicCuffed(this DbPlayer dbPlayer, bool cuffed, bool inVehicle = false)
        {
            if (cuffed)
            {
                dbPlayer.CancelPhoneCall();
                dbPlayer.SetData(CuffedMedic, true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "mp_arresting", inVehicle ? "sit" : "idle");
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                //dbPlayer.Player.Freeze(true);
            }
            else
            {
                dbPlayer.ResetData(CuffedMedic);
                dbPlayer.StopAnimation();
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                //dbPlayer.Player.Freeze(false);
            }
        }

        public static bool IsMedicCuffed(this Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            return dbPlayer.HasData(CuffedMedic);
        }

        public static void SetTied(this DbPlayer dbPlayer, bool tied, bool inVehicle = false)
        {
            if (tied)
            {
                dbPlayer.CancelPhoneCall();
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "mp_arresting", inVehicle ? "sit" : "idle");
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                //dbPlayer.Player.Freeze(true);
                Voice.VoiceModule.Instance.turnOffFunk(dbPlayer);
                Voice.VoiceModule.Instance.turnOffFunk(dbPlayer);

                // Cancel Phonecall
                dbPlayer.Player.TriggerNewClient("hangupCall");
                dbPlayer.Player.TriggerNewClient("cancelPhoneCall");
                dbPlayer.ResetData("current_caller");

                NSAObservationModule.CancelPhoneHearing((int)dbPlayer.handy[0]);
            }
            else
            {
                dbPlayer.StopAnimation();
                //To make sure player can move
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                //dbPlayer.Player.Freeze(false);
            }

            dbPlayer.IsTied = tied;
            dbPlayer.Player.TriggerNewClient(TiedEvent, tied);
        }

        public static void SetDuty(this DbPlayer dbPlayer, bool duty)
        {
            if (dbPlayer.AccountStatus != AccountStatus.LoggedIn) return;
            dbPlayer.Duty = duty;
            dbPlayer.Player.TriggerNewClient(DutyEvent, duty);
        }

        public static bool IsInDuty(this DbPlayer dbPlayer)
        {
            return dbPlayer.Duty;
        }

        public static bool HasCopInsurance(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsInDuty()) return false;

            if (dbPlayer.IsACop() || dbPlayer.IsAMedic()) return true;

            return false;
        }

        public static void SetNames(this DbPlayer dbPlayer, bool names)
        {
            dbPlayer.CanSeeNames = names;
            dbPlayer.Player.TriggerNewClient("setPlayerNametags", names);
        }

        public static void SetAdminDuty(this DbPlayer dbPlayer, bool aduty)
        {
            if (aduty)
            {
                dbPlayer.SetData("armorbefore", dbPlayer.Player.Armor);
            }
            else
            {
                if (dbPlayer.HasData("armorbefore"))
                {
                    dbPlayer.SetArmorPlayer(dbPlayer.GetData("armorbefore"));
                    dbPlayer.ResetData("armorbefore");
                }
            }
            dbPlayer.RankDuty = aduty ? DbPlayer.RankDutyStatus.AdminDuty : DbPlayer.RankDutyStatus.OffDuty;
            dbPlayer.Player.TriggerNewClient("setPlayerAduty", aduty);
            dbPlayer.ApplyArmorVisibility();
        }

        public static bool IsInAdminDuty(this DbPlayer dbPlayer)
        {
            return dbPlayer.RankDuty == DbPlayer.RankDutyStatus.AdminDuty;
        }

        public static void SetCasinoDuty(this DbPlayer dbPlayer, bool state)
        {
            dbPlayer.RankDuty = state ? DbPlayer.RankDutyStatus.CasinoDuty : DbPlayer.RankDutyStatus.OffDuty;
            dbPlayer.Player.TriggerNewClient("setPlayerCduty", state);
        }

        public static bool IsInCasinoDuty(this DbPlayer dbPlayer)
        {
            return dbPlayer.RankDuty == DbPlayer.RankDutyStatus.CasinoDuty;
        }

        public static void SetGuideDuty(this DbPlayer dbPlayer, bool state)
        {
            dbPlayer.RankDuty = state ? DbPlayer.RankDutyStatus.GuideDuty : DbPlayer.RankDutyStatus.OffDuty;

            if (state)
            {
                dbPlayer.SetData("armorbefore", dbPlayer.Player.Armor);

                dbPlayer.SetSkin(dbPlayer.Customization.Gender == 0 ? PedHash.FilmDirector : PedHash.ShopMidSFY);
            }
            else
            {
                if (dbPlayer.HasData("armorbefore"))
                {
                    dbPlayer.SetArmorPlayer(dbPlayer.GetData("armorbefore"));
                    dbPlayer.ResetData("armorbefore");
                }

                dbPlayer.ApplyCharacter();
                dbPlayer.ApplyArmorVisibility();
            }
        }

        public static void SetGameDesignDuty(this DbPlayer dbPlayer, bool state)
        {
            dbPlayer.RankDuty = state ? DbPlayer.RankDutyStatus.GameDesignDuty : DbPlayer.RankDutyStatus.OffDuty;
            dbPlayer.Player.TriggerNewClient("setPlayerAduty", state);
            if (state)
            {
                dbPlayer.SetData("armorbefore", dbPlayer.Player.Armor);
                int rnd = Utils.RandomNumber(0, 1);
                if (rnd == 1)
                {
                    dbPlayer.SetSkin(dbPlayer.Customization.Gender == 0 ? PedHash.Construct01SMY : PedHash.ShopMidSFY);
                }
                else
                {
                    dbPlayer.SetSkin(dbPlayer.Customization.Gender == 0 ? PedHash.Construct02SMY : PedHash.ShopMidSFY);
                }                
            }
            else
            {
                if (dbPlayer.HasData("armorbefore"))
                {
                    dbPlayer.SetArmorPlayer(dbPlayer.GetData("armorbefore"));
                    dbPlayer.ResetData("armorbefore");
                }                
                dbPlayer.ApplyCharacter();
                dbPlayer.ApplyArmorVisibility();
            }
        }

        public static bool IsInGuideDuty(this DbPlayer dbPlayer)
        {
            return dbPlayer.RankDuty == DbPlayer.RankDutyStatus.GuideDuty;
        }

        public static bool IsInGameDesignDuty(this DbPlayer dbPlayer)
        {
            return dbPlayer.RankDuty == DbPlayer.RankDutyStatus.GameDesignDuty;
        }
    }
}