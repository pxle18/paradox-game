using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.NSA.Observation
{
    public class NSAPeilsender
    {
        public uint CreatorId { get; set; }

        public string Name { get; set; }

        public uint VehicleId { get; set; }
        
        public DateTime Added { get; set; }
    }

    public class NSAWanze
    {
        public uint CreatorId { get; set; }

        public string Name { get; set; }

        public uint PlayerId { get; set; }

        public DateTime Added { get; set; }

        public List<DbPlayer> HearingAgents { get; set; }

        public bool active { get; set; }
    }
}
