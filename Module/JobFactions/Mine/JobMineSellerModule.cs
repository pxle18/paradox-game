using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.JobFactions.Mine
{
    public class JobMineSeller : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }

        public uint TeamId { get; set; }

        public float Heading { get; set; }
        public int BatteryPrice { get; set; }
        public int BatteryLimit { get; set; }

        public Dictionary<uint, int> TodayBuyed { get; set; }

        public JobMineSeller(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            TeamId = reader.GetUInt32("team_id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            BatteryPrice = reader.GetInt32("battery_price");
            BatteryLimit = reader.GetInt32("battery_limit");
            TodayBuyed = new Dictionary<uint, int>();

            if(reader.GetString("today_buyed").Length > 1)
            {
                TodayBuyed = NAPI.Util.FromJson<Dictionary<uint, int>>(reader.GetString("today_buyed"));
            }

            new Npc(PedHash.Dockwork01SMM, Position, Heading, 0);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class JobMineSellerModule : SqlModule<JobMineSellerModule, JobMineSeller, uint>
    {

        int hour = DateTime.Now.Hour;
        int min = DateTime.Now.Minute;

        public static uint BatteryItemId = 15;


        protected override void OnItemLoaded(JobMineSeller loadable)
        {
            // wegen falls server länger braucht....
            if(hour == 0 && min < 30)
            {
                loadable.TodayBuyed = new Dictionary<uint, int>();
                SavePlayerBuyed(loadable);
            }
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM jobs_mining_sellers";
        }

        public void SavePlayerBuyed(JobMineSeller jobMineSeller)
        {
            MySQLHandler.ExecuteAsync($"UPDATE jobs_mining_sellers SET `today_buyed` = '{NAPI.Util.ToJson(jobMineSeller.TodayBuyed)}' WHERE id = {jobMineSeller.Id};");
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if(dbPlayer != null && dbPlayer.IsValid() && key == Key.E && !dbPlayer.RageExtension.IsInVehicle)
            {
                JobMineSeller jobMineSeller = GetAll().Values.Where(jbms => jbms.Position.DistanceTo(dbPlayer.Player.Position) < 2.0f).FirstOrDefault();

                if(jobMineSeller != null)
                {
                    if (jobMineSeller.TodayBuyed.ContainsKey(dbPlayer.Id))
                    {
                        if(jobMineSeller.TodayBuyed[dbPlayer.Id] >= jobMineSeller.BatteryLimit)
                        {
                            dbPlayer.SendNewNotification("Du hast die maximale Anzahl an Batterien für heute erreicht!");
                            return true;
                        }
                    }

                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Warenausgabe", Callback = "MiningBatterieOrder", Message = $"Wie viele Batterien möchtest du kaufen? ({jobMineSeller.BatteryPrice}$/stk)"});

                    return true;
                }
            }
            return false;
        }
    }

    public class JobMineSellerEvents : Script
    {
        [RemoteEvent]
        public void MiningBatterieOrder(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            JobMineSeller jobMineSeller = JobMineSellerModule.Instance.GetAll().Values.Where(jbms => jbms.Position.DistanceTo(dbPlayer.Player.Position) < 2.0f).FirstOrDefault();

            if (jobMineSeller != null)
            {
                if (dbPlayer.RageExtension.IsInVehicle) return;

                if (!Int32.TryParse(returnstring, out int amount))
                {
                    return;
                }

                int limit = jobMineSeller.BatteryLimit; // zb 100

                if (jobMineSeller.TodayBuyed.ContainsKey(dbPlayer.Id))
                {
                    limit -= jobMineSeller.TodayBuyed[dbPlayer.Id];
                }

                if (amount > limit)
                {
                    dbPlayer.SendNewNotification($"Du kannst heute nur noch {limit} Batterien kaufen!");
                    return;
                }

                int price = 0;
                price = amount * jobMineSeller.BatteryPrice;

                // Remove from Container
                Container TeamMineStorageContainer = TeamModule.Instance.Get(jobMineSeller.TeamId).MineContainerStorage;

                if (TeamMineStorageContainer == null) return;

                if (TeamMineStorageContainer.GetItemAmount(JobMineSellerModule.BatteryItemId) < amount)
                {
                    dbPlayer.SendNewNotification("Liefermenge nicht verfügbar. Versuchen Sie es später erneut.");
                    return;
                }

                // gebe
                if (!dbPlayer.Container.CanInventoryItemAdded(JobMineSellerModule.BatteryItemId, amount))
                {
                    dbPlayer.SendNewNotification($"Du kannst so viel nicht mehr tragen!");
                    return;
                }

                Task.Run(async () =>
                {
                    Payment.PaymentStatus pStatus = await Payment.PaymentModule.Instance.AskForPayment(dbPlayer, price);

                    if (pStatus == Payment.PaymentStatus.Wallet)
                    {
                        if (!dbPlayer.TakeMoney(price))
                        {
                            dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(price));
                            return;
                        }
                    }
                    else if (pStatus == Payment.PaymentStatus.Bank)
                    {
                        if (!dbPlayer.TakeBankMoney(price, "Ihre Zahlung Mine-Batterienverkauf ($" + price + ")"))
                        {
                            dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(price));
                            return;
                        }
                    }
                    else return;

                    // add player, remove mining
                    dbPlayer.Container.AddItem(JobMineSellerModule.BatteryItemId, amount);
                    TeamMineStorageContainer.RemoveItem(JobMineSellerModule.BatteryItemId, amount);

                    if (jobMineSeller.TodayBuyed.ContainsKey(dbPlayer.Id))
                    {
                        jobMineSeller.TodayBuyed[dbPlayer.Id] += amount;
                    }
                    else
                    {
                        jobMineSeller.TodayBuyed.Add(dbPlayer.Id, amount);
                    }
                    JobMineSellerModule.Instance.SavePlayerBuyed(jobMineSeller);

                    dbPlayer.SendNewNotification($"Sie haben {amount} Batterien für ${price} gekauft!");
                });

            }
            return;
        }
    }
}
