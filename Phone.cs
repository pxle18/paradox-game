using Newtonsoft.Json;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class PlayerPhoneData
    {
        public int Credit { get; set; }
        public uint Number { get; set; }
    }

    public static class Phone
    {
        public static void SetPlayerPhoneData(DbPlayer dbPlayer)
        {
            var data = new PlayerPhoneData {Credit = dbPlayer.guthaben[0], Number = dbPlayer.handy[0]};
            dbPlayer.Player.TriggerNewClient("RESPONSE_PHONE_SETTINGS", JsonConvert.SerializeObject(data));
        }
    }
}