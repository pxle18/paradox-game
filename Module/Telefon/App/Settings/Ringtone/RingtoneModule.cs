using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Telefon.App.Settings.Ringtone
{
    public class RingtoneModule : SqlModule<RingtoneModule, Ringtone, uint>
    {
        protected override string GetQuery()
        {
            return $"SELECT * FROM `phone_ringtones{(Configurations.Configuration.Instance.DevMode ? "_dev" : "")}`;";
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.Ringtone = Instance.Get(
                reader.GetUInt32("klingeltonId")
            );

            dbPlayer.PhoneSettings = new PhoneSetting(false, false, false);
            dbPlayer.PlayerWhoHearRingtone = new List<DbPlayer>();
        }

        public List<Ringtone> GetRingtonesForPlayer(DbPlayer dbPlayer)
        {
            List<Ringtone> liste = new List<Ringtone>();
            foreach (var item in this.GetAll().Values)
            {
                liste.Add(item);
            }

            return liste;
        }
    }
}
