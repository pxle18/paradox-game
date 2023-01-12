using GTANetworkAPI;
using System;
using System.Collections.Generic;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Service
{
    public class Service
    {
        public Vector3 Position { get; }
        public string Message { get; set; }
        public uint TeamId { get; }
        public DbPlayer Player { get; }
        public HashSet<string> Accepted { get; }
        public string Telnr { get; }

        public DateTime Created { get; set; }
        
        public Service(Vector3 position, string message, uint teamId, DbPlayer dbPlayer, string description = "", string telefon = "0")
        {
            Position = position;
            Message = message;
            TeamId = teamId;
            Player = dbPlayer;
            Telnr = telefon;
            Accepted = new HashSet<string>();
            Created = DateTime.Now;
        }
    }

    public class ServiceEvaluation
    {
        public uint id { get; set; }

        public string name { get; set; }

        public int amount { get; set; }

        public DateTime timestr { get; set; }

        public ServiceEvaluation(uint PlayerId, int Amount)
        {
            Logging.Logger.Debug("new eval " + PlayerId + " " + Amount);
            string nstr = "";
            PlayerName.PlayerNameModel pName = PlayerName.PlayerNameModule.Instance.Get(PlayerId);
            if(pName != null)
            {
                nstr = pName.Name;
            }
            id = PlayerId;
            name = nstr;
            amount = Amount;
            timestr = DateTime.Now;
        }
    }

    public class ServiceEvaluationJson
    {
        public uint id { get; set; }

        public string name { get; set; }

        public int amount { get; set; }

        public string timestr { get; set; }

    }
}