using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Computer.Apps.FahrzeugUebersichtApp;
using VMP_CNR.Module.Email;
using VMP_CNR.Module.PlayerName;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.VehicleRent;

namespace VMP_CNR.Module.Computer.Apps.EmailApp.Apps
{
    public class Email
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }

        [JsonProperty(PropertyName = "readed")]
        public bool Readed { get; set; }

        [JsonIgnore]
        public DateTime Date { get; set; }


        public Email(uint id, string subject, string body, bool readed, DateTime date)
        {
            Id = id;
            Subject = subject;
            Body = body;
            Readed = readed;
            Date = date;
        }

        public Email(DbEmail dbEmail)
        {
            Id = dbEmail.Id;
            Subject = dbEmail.Subject;
            Body = dbEmail.Body;
            Readed = dbEmail.Readed;
            Date = dbEmail.Date;
        }

    }

    public class EmailApp : SimpleApp
    {
        public EmailApp() : base("EmailApp") { }

        [RemoteEvent]
        public void requestEmails(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer p_DbPlayer = client.GetPlayer();
            if (p_DbPlayer == null || !p_DbPlayer.IsValid())
                return;

            List<Email> Emails = new List<Email>();

            foreach(DbEmail dbEmail in p_DbPlayer.Emails.Values.ToList())
            {
                Emails.Add(new Email(dbEmail));
            }

            Emails = Emails.OrderByDescending(x => x.Date).ToList();

            Logging.Logger.Debug(NAPI.Util.ToJson(Emails));
            TriggerNewClient(client, "responseEmails", NAPI.Util.ToJson(Emails));
        }

        [RemoteEvent]
        public void markEmailAsRead(Player client, uint EmailId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer p_DbPlayer = client.GetPlayer();
            if (p_DbPlayer == null || !p_DbPlayer.IsValid())
                return;

            if(p_DbPlayer.Emails.ContainsKey(EmailId))
            {
                p_DbPlayer.Emails[EmailId].Readed = true;
                p_DbPlayer.Emails[EmailId].UpdateReadStatus();
            }
        }

    }
}
