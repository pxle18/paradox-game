using System;
using System.Collections.Generic;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Support
{
    public class Ticket
    {
        public DbPlayer Player { get; set; }
        public string Description { get; set; }
        public DateTime Created_at { get; set; }
        public HashSet<string> Accepted { get; set; }
        public bool ChatStatus { get; set; }

        public Ticket(DbPlayer dbPlayer, string description)
        {
            Player = dbPlayer;
            Description = description;
            Created_at = DateTime.Now;
            Accepted = new HashSet<string>();
            ChatStatus = false;
        }
    }
}
