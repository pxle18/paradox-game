using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Attachments
{
    public class AttachmentItemModule : SqlModule<AttachmentItemModule, AttachmentItem, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `attachment_item`;";
        }

        public void Commandreloadattach(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();


            if (!Configurations.Configuration.Instance.DevMode)
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }
        }
    }

    public class AttachmentItem : Loadable<uint>
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "model")]
        public int ObjectId { get; set; }

        [JsonProperty(PropertyName = "bone")]
        public int Bone { get; set; }

        [JsonProperty(PropertyName = "offset")]
        public Vector3 Position { get; set; }

        [JsonProperty(PropertyName = "rotation")]
        public Vector3 Rotation { get; set; }

        [JsonProperty(PropertyName = "needsAnimation")]
        public bool NeedsAnimation { get; set; }

        [JsonProperty(PropertyName = "animationDict")]
        public string AnimDic1 { get; set; }

        [JsonProperty(PropertyName = "animationName")]
        public string AnimDic2 { get; set; }

        [JsonProperty(PropertyName = "animationFlag")]
        public int AnimFlag { get; set; }

        [JsonProperty(PropertyName = "isCarrying")]
        public bool IsCarry { get; set; }

        public AttachmentItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            ObjectId = reader.GetInt32("object_id");
            Bone = reader.GetInt32("bone");
            Position = new Vector3(reader.GetFloat("x"), reader.GetFloat("y"), reader.GetFloat("z"));
            Rotation = new Vector3(reader.GetFloat("rot_x"), reader.GetFloat("rot_y"), reader.GetFloat("rot_z"));
            NeedsAnimation = reader.GetInt32("needs_animation") == 1;
            AnimDic1 = reader.GetString("animdic_1");
            AnimDic2 = reader.GetString("animdic_2");
            AnimFlag = reader.GetInt32("anim_flag");
            IsCarry = reader.GetInt32("iscarry") == 1;

        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
