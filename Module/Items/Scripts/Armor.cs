using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Progressbar.Extensions;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> UnderArmor(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            dbPlayer.SetCannotInteract(true);

            dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            dbPlayer.SetData("armorusing", true);

            bool finishedProgressbar = await dbPlayer.RunProgressBar(() =>
            {
                int type = -1;
                if (dbPlayer.VisibleArmorType != type)
                    dbPlayer.SaveArmorType(type);
                dbPlayer.SetArmor(99, false);

                return Task.CompletedTask;
            }, "Schutzweste", "Du ziehst eine Schutzweste.", 4 * 1000);

            dbPlayer.ResetData("armorusing");
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();

            return finishedProgressbar;
        }

        public static async Task<bool> Armor(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            dbPlayer.SetCannotInteract(true);

            dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            dbPlayer.SetData("armorusing", true);

            bool finishedProgressbar = await dbPlayer.RunProgressBar(() =>
            {
                dbPlayer.SetArmor(100, true);

                return Task.CompletedTask;
            }, "Schutzweste", "Du ziehst eine Schutzweste.", 4 * 1000);

            dbPlayer.ResetData("armorusing");
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();

            return finishedProgressbar;
        }

        public static async Task<bool> BArmor(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.IsCopPackGun() || !dbPlayer.IsInDuty()) return false;
            dbPlayer.SetCannotInteract(true);

            dbPlayer.PlayAnimation(
                (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            dbPlayer.SetData("armorusing", true);

            bool finishedProgressbar = await dbPlayer.RunProgressBar(() =>
            {
                dbPlayer.SetArmor(100, true);

                return Task.CompletedTask;
            }, "Schutzweste", "Du ziehst eine Schutzweste.", 4 * 1000);

            dbPlayer.ResetData("armorusing");
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();
            
            return finishedProgressbar;
        }

        public static async Task<bool> BUnderArmor(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.IsCopPackGun() || !dbPlayer.IsInDuty()) return false;
            dbPlayer.SetCannotInteract(true);

            dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            dbPlayer.SetData("armorusing", true);

            bool finishedProgressbar = await dbPlayer.RunProgressBar(() =>
            {
                int type = 30;
                if (dbPlayer.VisibleArmorType != type)
                    dbPlayer.SaveArmorType(type);
                dbPlayer.SetArmor(99, false);

                return Task.CompletedTask;
            }, "Schutzweste", "Du ziehst eine Schutzweste.", 4 * 1000);

            dbPlayer.ResetData("armorusing");
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();

            return finishedProgressbar;
        }

        public static async Task<bool> FArmor(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            if (!GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team) || dbPlayer.Player.Dimension != GangwarModule.Instance.DefaultDimension) return false;
            
            dbPlayer.SetCannotInteract(true);
            dbPlayer.PlayAnimation(
                (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            dbPlayer.SetData("armorusing", true);

            bool finishedProgressbar = await dbPlayer.RunProgressBar(() =>
            {
                if (dbPlayer.VisibleArmorType != 0)
                    dbPlayer.SaveArmorType(0);
                dbPlayer.VisibleArmorType = 0;
                dbPlayer.SetArmor(120, true);

                return Task.CompletedTask;
            }, "Schutzweste", "Du ziehst eine Schutzweste.", 4 * 1000);

            dbPlayer.ResetData("armorusing");
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();

            return finishedProgressbar;
        }
    }
}