using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players.Phone.Contacts
{
    public class PhoneContacts
    {
        // Number, Contact
        private Dictionary<uint, PhoneContact> Contacts { get; }

        private uint PlayerId { get; }

        public PhoneContacts(uint playerId)
        {
            PlayerId = playerId;
            Contacts = new Dictionary<uint, PhoneContact>();
        }

        public void Populate()
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"SELECT * FROM `phone_numbers` WHERE player_id = '{PlayerId}' ORDER BY name;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return;
                        while (reader.Read())
                        {
                            var phoneContact = new PhoneContact(reader);
                            if (!Contacts.ContainsKey(phoneContact.Number))
                            {
                                Contacts.Add(phoneContact.Number, phoneContact);
                            }
                        }
                    }
                }
            }));
        }
        
        public string TryGetPhoneContactNameByNumber(uint number)
        {
            Contacts.TryGetValue(number, out PhoneContact value);
            if (value == null) return null;
            else return value.Name;
        }

        public void Add(string name, uint number)
        {
            if (Contacts.ContainsKey(number)) return;
            Contacts.Add(number, new PhoneContact(name, number));
            MySQLAdd(name, number);
        }

        public void Remove(uint number)
        {
            if (!Contacts.ContainsKey(number)) return;
            Contacts.Remove(number);
            MySQLDelete(number);
        }

        public void Update(uint oldNumber, uint number, string name)
        {
            if (oldNumber != number) 
            {
                Contacts.Remove(oldNumber);
                MySQLDelete(oldNumber);

                if (Contacts.ContainsKey(number)) return;
                Contacts.Add(number, new PhoneContact(name, number));
                MySQLAdd(name, number);
            } 
            else if (Contacts.ContainsKey(number)) 
            {
                var contact = Contacts[number];
                contact.Name = name;
                contact.Number = number;
                MySQLUpdate(oldNumber, number, name);
            }
        }

        private void MySQLAdd(string name, uint number)
        {
            MySQLHandler.ExecuteAsync($"INSERT INTO `phone_numbers` (`name`, `number`, `player_id`) VALUES ('{name}', '{number}', '{this.PlayerId}')");
        }

        private void MySQLDelete(uint number)
        {
            MySQLHandler.ExecuteAsync($"DELETE FROM `phone_numbers` WHERE number = {number} AND player_id = '{this.PlayerId}'");
        }

        private void MySQLUpdate(uint oldnumber, uint newnumber, string name)
        {
            MySQLHandler.ExecuteAsync($"UPDATE `phone_numbers` SET number = '{newnumber}', name = '{name}' WHERE number = '{oldnumber}' AND player_id = '{this.PlayerId}'");
        }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(Contacts.Values.ToList());
        }
    }
}