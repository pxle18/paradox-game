using GTANetworkMethods;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Email
{
    public class DbEmail : Loadable<uint>
    {
        public uint Id { get; }
        public uint PlayerId { get; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public bool Readed { get; set; }
        public DateTime Date { get; set; }

        public DbEmail(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            PlayerId = reader.GetUInt32("player_id");
            Subject = reader.GetString("subject");
            Body = reader.GetString("body");
            Readed = reader.GetInt32("readed") == 1;
            Date = reader.GetDateTime("date");
        }

        public DbEmail(uint id, uint playerid, string subject, string body, bool readed, DateTime date)
        {
            Id = id;
            PlayerId = playerid;
            Subject = subject;
            Body = body;
            Readed = readed;
            Date = date;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public void UpdateReadStatus()
        {
            MySQLHandler.ExecuteAsync($"UPDATE player_emails SET readed = '{(Readed ? '1' : '0')}' WHERE id = '{Id}' AND player_id = '{PlayerId}';");
        }
    }
}
