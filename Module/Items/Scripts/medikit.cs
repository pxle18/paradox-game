using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Progressbar.Extensions;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> medikit(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.Player.Health > 98) return false;

            if (dbPlayer.RageExtension.IsInVehicle)
            {
                dbPlayer.SendNewNotification("Du kannst waehrend der Fahrt keinen Verbandskasten benutzen");
                return false;
            }

            dbPlayer.SetCannotInteract(true);
            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            dbPlayer.SetData("mediusing", true);

            bool finishedProgressbar = await dbPlayer.RunProgressBar(() =>
            {
                dbPlayer.SetHealth(100);

                return Task.CompletedTask;
            }, "Verbandskasten", "Du ziehst ein Verbandskasten.", 4 * 1000);

            dbPlayer.ResetData("mediusing");
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();
            dbPlayer.SetCannotInteract(false);

            return finishedProgressbar;
        }

        public static async Task<bool> FMedikit(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.RageExtension.IsInVehicle || dbPlayer.Player.Health > 99) return false;
            if (!GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team) || dbPlayer.Player.Dimension != GangwarModule.Instance.DefaultDimension) return false;

            dbPlayer.SetCannotInteract(true);
            dbPlayer.PlayAnimation(
                (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1]);
            dbPlayer.Player.TriggerNewClient("freezePlayer", true);

            dbPlayer.SetData("mediusing", true);

            bool finishedProgressbar = await dbPlayer.RunProgressBar(() =>
            {
                dbPlayer.SetHealth(100);

                return Task.CompletedTask;
            }, "Verbandskasten", "Du ziehst ein Verbandskasten.", 4 * 1000);

            dbPlayer.ResetData("mediusing");

            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();
            dbPlayer.SetCannotInteract(false);

            return finishedProgressbar;
        }
    }
}