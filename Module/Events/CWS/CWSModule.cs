using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Events.CWS
{
    public class CWSModule : SqlModule<CWSModule, CWS, uint>
    {
        public List<DbPlayer> ToSavingPlayer = new List<DbPlayer>();

        protected override string GetQuery()
        {
            return "SELECT * FROM `cws_data`;";
        }

        protected override bool OnLoad()
        {
            ToSavingPlayer = new List<DbPlayer>();

            return base.OnLoad();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandsetcws(Player player, string args)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if(!Configurations.Configuration.Instance.DevMode) return;

            if (!args.Contains(' ')) return;

            string[] argsSplit = args.Split(' ');
            if (argsSplit.Length < 2) return;

            if(!Int32.TryParse(argsSplit[0], out int cwsid))
            {
                return;
            }

            if (!Int32.TryParse(argsSplit[1], out int value))
            {
                return;
            }

            dbPlayer.GiveCWS((CWSTypes)cwsid, value);
            dbPlayer.SendNewNotification($"{value} cws added {CWSModule.Instance.Get((uint)cwsid).Name}");

            return;
        }

        public override void OnMinuteUpdate()
        {
            List<DbPlayer> duplicate = ToSavingPlayer.ToList();
            ToSavingPlayer.Clear();

            foreach (DbPlayer dbPlayer in duplicate)
            {
                if(dbPlayer != null && dbPlayer.IsValid())
                    dbPlayer.SaveCWS();
            }
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if(ToSavingPlayer.Contains(dbPlayer))
            {
                dbPlayer.SaveCWS();
                ToSavingPlayer.Remove(dbPlayer);
            }
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.CWS = JsonConvert.DeserializeObject<Dictionary<uint, PlayerCWS>>(reader.GetString("cws")) ??
                                              new Dictionary<uint, PlayerCWS>();

            bool edited = false;
            // resett if older than today
            foreach(KeyValuePair<uint, PlayerCWS> kvp in dbPlayer.CWS) 
            {
                if(kvp.Value.DailyDate < DateTime.Today)
                {
                    kvp.Value.DailyDate = DateTime.Today;
                    kvp.Value.DailyValue = 0;
                    edited = true;
                }
            }

            if(edited)
            {
                dbPlayer.UpdateCWS();
            }

            LevelPointModule.Instance.MigrateLevelPoints(dbPlayer);

            return;
        }
    }

    public enum CWSTypes
    {
        Jahrmarkt = 1,
        Level = 2,
        Training = 3,
        Halloween = 4,
    }

    public class CWS : Loadable<uint>
    {
        public uint Id { get; set; }

        public string Name { get; set; }

        public int DailyCap { get; set; }

        public CWS(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");

            Name = reader.GetString("name");

            DailyCap = reader.GetInt32("daily_cap");
        }
        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class PlayerCWS
    {
        public int Value { get; set; }
        public int DailyValue { get; set; }
        public DateTime DailyDate { get; set; }
        public dynamic CustomData { get; set; }

    }

    public static class PlayerExtension
    {
        public static void SaveCWS(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE player SET cws = '{(JsonConvert.SerializeObject(dbPlayer.CWS))}' WHERE id = '{dbPlayer.Id}'");
        }
        public static void UpdateCWS(this DbPlayer dbPlayer)
        {
            if (!CWSModule.Instance.ToSavingPlayer.Contains(dbPlayer)) CWSModule.Instance.ToSavingPlayer.Add(dbPlayer);
        }

        public static bool TakeCWS(this DbPlayer dbPlayer, CWSTypes Type, int cws)
        {
            if(dbPlayer.CWS.ContainsKey((uint)Type))
            {
                if(dbPlayer.CWS[(uint)Type].Value >= cws)
                {
                    dbPlayer.CWS[(uint)Type].Value -= cws;
                    dbPlayer.UpdateCWS();
                    return true;
                }
            }
            return false;
        }

        public static bool CapReached(this DbPlayer dbPlayer, CWSTypes Type)
        {
            if (dbPlayer.CWS.ContainsKey((uint)Type))
            {
                return dbPlayer.CWS[(uint)Type].DailyValue >= CWSModule.Instance.Get((uint)Type).DailyCap && CWSModule.Instance.Get((uint)Type).DailyCap > 0;
            }
            return false;
        }

        public static bool GiveCWS(this DbPlayer dbPlayer, CWSTypes Type, int cws)
        {
            if (dbPlayer.CWS.ContainsKey((uint)Type))
            {
                if ((dbPlayer.CWS[(uint)Type].DailyValue+cws) >= CWSModule.Instance.Get((uint)Type).DailyCap && CWSModule.Instance.Get((uint)Type).DailyCap > 0)
                {

                    int diff = CWSModule.Instance.Get((uint)Type).DailyCap - dbPlayer.CWS[(uint)Type].DailyValue;

                    dbPlayer.CWS[(uint)Type].DailyValue += diff;
                    dbPlayer.CWS[(uint)Type].Value += diff;
                    dbPlayer.UpdateCWS();
                    return false;
                }

                dbPlayer.CWS[(uint)Type].Value += cws;
                dbPlayer.CWS[(uint)Type].DailyValue += cws;
                dbPlayer.UpdateCWS();
            }
            else
            {
                dbPlayer.CWS.Add((uint)Type, new PlayerCWS() { Value = cws, DailyDate = DateTime.Today, DailyValue = cws });
                dbPlayer.UpdateCWS();
            }
            return true;
        }
    }
}
