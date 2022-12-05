using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.ReversePhone
{
    public class PhoneHistoryEntry
    {
        public int Number { get; set; }
        public int Dauer { get; set; }
        public DateTime Time { get; set; }

        public PhoneHistoryEntry(int number, int dauer)
        {
            Number = number;
            Dauer = dauer;
            Time = DateTime.Now;
        }
    }

    public sealed class ReversePhoneModule : Module<ReversePhoneModule>
    {
        public Dictionary<uint, List<PhoneHistoryEntry>> phoneHistory = new Dictionary<uint, List<PhoneHistoryEntry>>();

        public override bool Load(bool reload = false)
        {
            phoneHistory = new Dictionary<uint, List<PhoneHistoryEntry>>();
            return true;
        }

        public void AddPhoneHistory(DbPlayer dbPlayer, int number, int dauer)
        {
            if (!phoneHistory.ContainsKey(dbPlayer.Id)) phoneHistory.Add(dbPlayer.Id, new List<PhoneHistoryEntry>());

            phoneHistory[dbPlayer.Id].Add(new PhoneHistoryEntry(number, dauer));
        }
    }
}