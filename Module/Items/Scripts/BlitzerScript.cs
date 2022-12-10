using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Blitzer;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Blitzer70(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.IsCuffed || dbPlayer.IsTied ||
                dbPlayer.Team.Id != (uint)TeamTypes.TEAM_POLICE ||
                dbPlayer.RageExtension.IsInVehicle)
            {
                return false;
            }

            return true;
            // TESTE DEINEN CODE OB ER ÜBERHAUPT COMPILED MAAAAAAAN
            /*if(BlitzerModule.Instance.aufgestellt >= 4)
            {
                dbPlayer.SendNewNotification( "Maximale Anzahl an Blitzern erreicht!");
                return false;
            }

            if (PoliceObjectModule.Instance.IsMaxReached())
            {
                dbPlayer.SendNewNotification( "Maximale Anzahl an Polizeiabsperrungen erreicht!");
                return false;
            }

            PoliceObjectModule.Instance.Add(1382242693, dbPlayer.Player, ItemData, false);

            Vector3 pos = dbPlayer.Player.Position;
            pos.Z = pos.Z - 5.0f;
            BlitzerModule.Instance.AddBlitzer(pos, dbPlayer.GetName(), (int)dbPlayer.TeamId, 70);

            dbPlayer.SendNewNotification( ItemData.Name + " erfolgreich platziert!");
            
                dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                await NAPI.Task.WaitForMainThread(4000);
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();
            
            return true;*/
        }

        public static async Task<bool> Blitzer120(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.Team.Id != (uint)TeamTypes.TEAM_POLICE || dbPlayer.RageExtension.IsInVehicle)
                return false;

            return true;

            /*if (BlitzerModule.Instance.aufgestellt >= 4)
            {
                dbPlayer.SendNewNotification( "Maximale Anzahl an Blitzern erreicht!");
                return false;
            }

            if (PoliceObjectModule.Instance.IsMaxReached())
            {
                dbPlayer.SendNewNotification( "Maximale Anzahl an Polizeiabsperrungen erreicht!");
                return false;
            }

            PoliceObjectModule.Instance.Add(1382242693, dbPlayer.Player, ItemData, false);

            BlitzerModule.Instance.AddBlitzer(dbPlayer.Player.Position, dbPlayer.GetName(), (int)dbPlayer.TeamId, 120);

            dbPlayer.SendNewNotification( ItemData.Name + " erfolgreich platziert!");
            
                dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                await NAPI.Task.WaitForMainThread(4000);
                dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();
            
            return true;*/
        }
    }
}