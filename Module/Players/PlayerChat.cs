using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players
{
    public static class PlayerChat
    {
        public static void SendChatMessage(this DbPlayer dbPlayer, string msg, bool ignorelogin = false)
        {
            if (!dbPlayer.IsValid(ignorelogin)) return;

            dbPlayer.SendNewNotification(msg);
        }
        
        public static void ClearChat(this DbPlayer dbPlayer)
        {
            for (int i = 0; i < 10; i++)
            {
                dbPlayer.SendNewNotification("");
                i++;
            }
        }
    }
}