using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Dealer.Menu;
using VMP_CNR.Module.Gangwar;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Dealer
{
    public class DealerModule : SqlModule<DealerModule, Dealer, uint>
    {
        public uint HeroinAmpulleItemId = 1442;
        public uint DiamondItemId       = 21;
        public uint GoldBarrenItemId    = 487;
        public int MaxDealerCount       = 4;
        public int DiamondCap           = 15;
        public int GoldCap              = 15;
        public int WeaponsCap           = 5;
        public static Random random = new Random();

        public static int Dealer5MinSellCap = 500000;

        public uint BigDealer = 0;
        public uint VehicleClawAmount = 0;
        public uint Maulwurf = 0;
        public int MaxMaulwuerfe = 0;
        public int MaxVehicleClawAmount = 1;
        public List<int> MaulwurfSpawnChances = new List<int> { 8, 28, 48, 68, 78, 86, 92, 97, 100 };
        
        public float MaulwurfAlarmChance = 1.0f;

        protected override string GetQuery()
        {
            return $"SELECT * FROM `dealer` ORDER BY RAND() LIMIT {MaxDealerCount};";
        }

        protected override bool OnLoad()
        {
            int rnd = Utils.RandomNumber(1, 100);
            MaulwurfSpawnChances.ForEach(item =>
            {
                if (rnd > item)
                    MaxMaulwuerfe++;
            });
            MenuManager.Instance.AddBuilder(new DealerSellMenu());
            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;
            if (!dbPlayer.Team.IsGangsters() || dbPlayer.Team.IsBadOrga()) return false;
            if (key != Key.E) return false;

            Dealer dealer = DealerModule.Instance.GetAll().Values.Where(d => d.Position.DistanceTo(dbPlayer.Player.Position) < 2.0f).FirstOrDefault();

            if(dealer != null)
            {
                if(!CanDealersUsed())
                {
                    dbPlayer.SendNewNotification("Hey ich muss erst noch umladen, gib mir etwas Zeit...");
                    return true;
                }

                dbPlayer.SetData("current_dealer", dealer.Id);
                MenuManager.Instance.Build(PlayerMenu.DealerSellMenu, dbPlayer).Show(dbPlayer);
                return true;
            }

            return false;
        }

        public bool CanDealersUsed()
        {
            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 20)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 20)
                    {
                        return false;
                    }

                    break;
            }
            return true;
        }

        public override void OnFiveMinuteUpdate()
        {
            foreach (var l_Dealer in this.GetAll().Values)
            {
                if(l_Dealer.DealerSoldAmount > 0)
                {
                    l_Dealer.DealerSoldAmount = 0; // resett dealer 5 min cap
                }

                foreach (var l_Resource in l_Dealer.DealerResources)
                {
                    int l_ResetTime = Configurations.Configuration.Instance.DevMode ? 10 : 60;

                    if (l_Resource.TimeSinceFull.AddMinutes(l_ResetTime) <= DateTime.Now && l_Resource.IsFull())
                    {
                        l_Resource.Sold = 0;
                        l_Resource.TimeSinceFull = DateTime.Now;
                    }
                }

                l_Dealer.Alert = false;
            }
        }
        
        public Dealer GetRandomDealer()
        {
            return GetAll().ElementAt(random.Next(0, MaxDealerCount - 1)).Value;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandsavedealer(Player player, string comment)
        {
            DbPlayer dbPlayer = player.GetPlayer();


            if (!Configurations.Configuration.Instance.DevMode)
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string heading = player.Rotation.Z.ToString().Replace(",", ".");
            
            Main.ServerNpcs.Add(new Npc(PedHash.Abigail, player.Position, player.Heading, 0));

            MySQLHandler.ExecuteAsync($"INSERT INTO dealer (pos_x, pos_y, pos_z, heading, note) VALUES('{MySqlHelper.EscapeString(x)}', '{MySqlHelper.EscapeString(y)}', '{MySqlHelper.EscapeString(z)}', '{MySqlHelper.EscapeString(heading)}', '{MySqlHelper.EscapeString(comment)}')");
            dbPlayer.SendNewNotification(string.Format("Dealer saved as: {0}", comment), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandshowdealer(Player player, string arg)
        {
            DbPlayer dbPlayer = player.GetPlayer();

            if (!Configurations.Configuration.Instance.DevMode)
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!int.TryParse(arg, out int id)) return;
            var dealer = GetAll().ElementAt(id).Value;
            player.SendWayPoint(dealer.Position.X, dealer.Position.Y);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandrandomdealer(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();

            if (!Configurations.Configuration.Instance.DevMode)
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dealer = GetRandomDealer();
            var position = Utils.GenerateRandomPosition(dealer.Position);
            player.SendWayPoint(position.X, position.Y);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandgotodealer(Player player, string arg)
        {
            DbPlayer dbPlayer = player.GetPlayer();

            if (!Configurations.Configuration.Instance.DevMode)
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!int.TryParse(arg, out int id)) return;
            var dealer = GetAll().ElementAt(id).Value;
            player.SetPosition(dealer.Position);
        }
    }
}
