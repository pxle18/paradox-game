using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.AnimationMenu
{
    public class AnimationItem : Loadable<uint>
    {
        public uint Id { get; }
        public uint CategoryId { get; }
        public string Name { get; }
        public string AnimDic { get; }
        public string AnimName { get; }
        public int AnimFlag { get; }

        public string Icon { get; }
        public int AttachmentId { get; }
        public HashSet<uint> RestrictedToTeams { get; }

        public AnimationItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            CategoryId = reader.GetUInt32("category_id");
            Name = reader.GetString("name");
            AnimDic = reader.GetString("anim_dic");
            AnimName = reader.GetString("anim_name");
            AnimFlag = reader.GetInt32("flag");
            Icon = reader.GetString("icon");
            AttachmentId = reader.GetInt32("attachment_id");

            var teamString = reader.GetString("restricted_teams");
            RestrictedToTeams = new HashSet<uint>();
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId)) continue;
                    RestrictedToTeams.Add(teamId);
                }
            }
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
