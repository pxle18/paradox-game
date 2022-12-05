using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> UnderArmor(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            dbPlayer.SetCannotInteract(true);

            Chats.sendProgressBar(dbPlayer, 4000);
            dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            dbPlayer.SetData("armorusing", true);

            await Task.Delay(4000);

            dbPlayer.ResetData("armorusing");
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();

            int type = -1;
            if (dbPlayer.VisibleArmorType != type)
                dbPlayer.SaveArmorType(type);
            dbPlayer.SetArmor(99, false);

            return true;
        }

        public static async Task<bool> Armor(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            dbPlayer.SetCannotInteract(true);

            Chats.sendProgressBar(dbPlayer, 4000);
            dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            dbPlayer.SetData("armorusing", true);

            await Task.Delay(4000);

            dbPlayer.ResetData("armorusing");
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();
            dbPlayer.SetArmor(100, true);

            return true;
        }

        public static async Task<bool> BArmor(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.IsCopPackGun() || !dbPlayer.IsInDuty()) return false;
            dbPlayer.SetCannotInteract(true);

            Chats.sendProgressBar(dbPlayer, 4000);
            dbPlayer.PlayAnimation(
                (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            dbPlayer.SetData("armorusing", true);

            await Task.Delay(4000);

            dbPlayer.ResetData("armorusing");
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();
            dbPlayer.SetArmor(100, true);
            
            return true;
        }

        public static async Task<bool> BUnderArmor(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle || !dbPlayer.IsCopPackGun() || !dbPlayer.IsInDuty()) return false;
            dbPlayer.SetCannotInteract(true);

            Chats.sendProgressBar(dbPlayer, 4000);
            dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            dbPlayer.SetData("armorusing", true);

            await Task.Delay(4000);

            dbPlayer.ResetData("armorusing");
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();

            int type = 30;
            if (dbPlayer.VisibleArmorType != type)
                dbPlayer.SaveArmorType(type);
            dbPlayer.SetArmor(99, false);

            return true;
        }

        public static async Task<bool> FArmor(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            if (!GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team) || dbPlayer.Player.Dimension != GangwarModule.Instance.DefaultDimension) return false;
            
            dbPlayer.SetCannotInteract(true);
            Chats.sendProgressBar(dbPlayer, 4000);
            dbPlayer.PlayAnimation(
                (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            dbPlayer.SetData("armorusing", true);

            await Task.Delay(4000);

            dbPlayer.ResetData("armorusing");
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.StopAnimation();
            if (dbPlayer.VisibleArmorType != 0)
                dbPlayer.SaveArmorType(0);
            dbPlayer.VisibleArmorType = 0;
            dbPlayer.SetArmor(120, true);

            return true;
        }
    }
}