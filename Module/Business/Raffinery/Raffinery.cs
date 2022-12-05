using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR.Module.Business.Raffinery
{
    public class RaffineryLogObject
    {
        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }
    }
    public class Raffinery : Loadable<uint>
    {
        public uint Id { get; }
        public Vector3 Position { get; }
        public RaffineryAusbaustufe AusbauStufe { get; set; }
        public Container Container { get; set; }
        public int FörderMengeMin { get; set; }
        public ColShape ColShape { get; set; }
        public int BuyPrice { get; set; }
        public Business OwnerBusiness { get; set; }

        public Raffinery(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            BuyPrice = reader.GetInt32("buy_price");
            Position = new Vector3(reader.GetFloat("pos_x"),
                reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            AusbauStufe = RaffineryAusbaustufeModule.Instance.Get(reader.GetUInt32("ausbaustufe"));
            ColShape = ColShapes.Create(Position, 2.0f);
            ColShape.SetData("raffineryId", Id);
            OwnerBusiness = BusinessModule.Instance.GetAll().Values.Where(b => b.BusinessBranch.RaffinerieId == Id).FirstOrDefault();


            Container = ContainerManager.LoadContainer(Id, ContainerTypes.RAFFINERY);

            Random random = new Random();
            FörderMengeMin = random.Next(AusbauStufe.MinGenerate, AusbauStufe.MaxGenerate);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public bool IsOwnedByBusines()
        {
            return OwnerBusiness != null;
        }

        public Business GetOwnedBusiness()
        {
            return OwnerBusiness;
        }

        public void ReloadData()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    await conn.OpenAsync();
                    cmd.CommandText = $"SELECT * FROM `business_raffinery` where id = {Id};";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                AusbauStufe = RaffineryAusbaustufeModule.Instance.Get(reader.GetUInt32("ausbaustufe"));
                                BuyPrice = reader.GetInt32("buy_price");
                            }
                        }
                    }
                    await conn.CloseAsync();
                }
            }));
        }

        public List<RaffineryLogObject> GetLogRaffinery()
        {
            List<RaffineryLogObject> Log = new List<RaffineryLogObject>();
            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = $"SELECT log_raffinery.*,player.name FROM log_raffinery,player WHERE log_raffinery.raffinery_id={Id} AND player.id=log_raffinery.player_id ORDER BY log_raffinery.date DESC LIMIT 10;";
                cmd.Prepare();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            RaffineryLogObject result = new RaffineryLogObject
                            {
                                Name = reader.GetString("Name"),
                                Amount = reader.GetInt32("amount"),
                                Date = reader.GetDateTime("date"),
                            };
                            Log.Add(result);
                        }

                    }
                }

            }
            return Log;
        }
    }
}
