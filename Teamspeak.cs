using GTANetworkAPI;
using VMP_CNR.Module.Players;

namespace VMP_CNR
{
    public class Teamspeak
    {
        public static void ChangeVoiceRange(Player player, float range = 15.0f)
        {
            player.SetSharedData("VOICE_RANGE", range);
        }

        public static void Connect(Player player, string characterName)
        {
            player.TriggerNewClient("ConnectTeamspeak");
        }
    }
}