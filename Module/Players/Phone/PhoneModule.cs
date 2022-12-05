using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;

using VMP_CNR.Module.Players.Db;

using VMP_CNR.Module.Players.Phone.Apps;
using VMP_CNR.Module.Players.Phone.Contacts;
using VMP_CNR.Module.Players.PlayerAnimations;
using VMP_CNR.Module.Tasks;

namespace VMP_CNR.Module.Players.Phone
{
    public class PhoneModule : Module<PhoneModule>
    {
        public static Dictionary<int, int> ActivePhoneCalls = new Dictionary<int, int>();

        protected override bool OnLoad()
        {
            ActivePhoneCalls = new Dictionary<int, int>();
            return base.OnLoad();
        }

        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            dbPlayer.PhoneContacts = new PhoneContacts(dbPlayer.Id);
            dbPlayer.PhoneContacts.Populate();

            dbPlayer.PhoneApps = new PhoneApps(dbPlayer);
        }
        
    }
}