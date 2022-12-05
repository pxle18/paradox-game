using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Shops;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Robbery
{
    public class Rob
    {
        public int Id { get; set; }
        public DbPlayer Player { get; set; }
        public int Interval { get; set; }
        public int CopInterval { get; set; }
        public int EndInterval { get; set; }
        public bool Disabled { get; set; }
        public RobType Type { get; set; }
    }

    public enum RobType
    {
        Shop,
        Juwelier,
        Staatsbank,
        VespucciBank
    }
}