using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Phone;

namespace VMP_CNR.Module.Telefon.App
{
    public class TelefonApp : SimpleApp
    {
        public TelefonApp() : base("Telefon")
        {
        }

        public DbPlayer GetPlayerByPhoneNumber(int p_PhoneNumber)
        {
            foreach (var l_Player in Players.Players.Instance.GetValidPlayers())
            {
                if ((int)l_Player.handy[0] != p_PhoneNumber)
                    continue;

                return l_Player;
            }

            return null;
        }
    }
}