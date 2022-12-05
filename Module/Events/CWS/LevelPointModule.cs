using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Events.CWS
{
    public class LevelPointModule : SqlModule<LevelPointModule, LevelPoints, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `levelpoints`;";
        }

        public void MigrateLevelPoints(DbPlayer dbPlayer)
        {
            if(dbPlayer.CWS != null && !dbPlayer.CWS.ContainsKey((uint)CWSTypes.Level))
            {
                int migrateValue = 0;

                foreach(KeyValuePair<uint, LevelPoints> kvp in Instance.GetAll())
                {
                    if(kvp.Key <= dbPlayer.Level)
                    {
                        migrateValue += kvp.Value.Points;
                    }
                }

                dbPlayer.GiveCWS(CWSTypes.Level, migrateValue);

                dbPlayer.SendNewNotification($"Levelpunkte migriert, setted to {migrateValue}");
            }
        }
    }

    public class LevelPoints : Loadable<uint>
    {
        public uint Level { get; set; }
        public int Points { get; set; }
       
        public LevelPoints(MySqlDataReader reader) : base(reader)
        {
            Level = reader.GetUInt32("level");
            Points = reader.GetInt32("points");

        }

        public override uint GetIdentifier()
        {
            return Level;
        }
    }
}
