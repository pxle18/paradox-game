using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;

namespace VMP_CNR.Module.TeamSubgroups.Models
{
    public class DbTeamSubgroup : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public string ShortName { get; }
        public int ColorId { get; }
        public int MaxMembers { get; }
        public Color RgbColor { get; }
        public DateTime LastFight { get; set; }

        public DbTeamSubgroup(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            ShortName = reader.GetString("shortName");
            MaxMembers = reader.GetInt32("maxMembers");
            LastFight = reader.GetDateTime("lastFight");

            ColorId = reader.GetInt32("colorId");
            var rgb = reader.GetString("rgb").Split(",");
            RgbColor = new Color(Int32.Parse(rgb[0]), Int32.Parse(rgb[1]), Int32.Parse(rgb[2]));
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
