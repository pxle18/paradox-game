using System;
using System.Text.RegularExpressions;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Support
{
    public class Konversation
    {
        public DbPlayer Player { get; set; }
        public bool Receiver { get; set; }
        public string Message { get; set; }
        public DateTime Created_at { get; set; }

        public Konversation(DbPlayer dbPlayer, bool receiver, string message)
        {
            Player = dbPlayer;
            Receiver = receiver;
            Message = Regex.Replace(message, @"[^a-zA-Z0-9\s]", ""); ;
            Created_at = DateTime.Now;
        }
    }
}