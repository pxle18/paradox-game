using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.JumpPoints;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Bunker
{
    public class Bunker : Loadable<uint>
    {
        public uint Id { get; set; }
        public JumpPoint jpEnter { get; set; }

        public int jpEnterId { get; set; }
        public JumpPoint jpExit { get; set; }
        public int jpExitId { get; set; }
        public string IPL { get; set; }

        public uint OwnerTeamId { get; set; }

        public bool IsOwnerSetted { get; set; }

        public StaticContainer BunkerBlackMoneyContainer { get; set; }

        public StaticContainer BunkerRessourceOrderContainer { get; set; }

        public int BlackMoneyAmount { get; set; }

        public Bunker(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            IPL = reader.GetString("iplname");

            jpEnterId = reader.GetInt32("jp_enter_id");
            jpExitId = reader.GetInt32("jp_exit_id");

            BlackMoneyAmount = reader.GetInt32("blackmoneyamount");

            Logging.Logger.Debug($"Bunker {Id}({IPL} loaded!");
            OwnerTeamId = 0;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public bool IsControlled()
        {
            return OwnerTeamId != 0;
        }

        public bool IsControlledByTeam(uint teamid)
        {
            return OwnerTeamId == teamid;
        }

        public void SaveBlackMoney()
        {
            var query = string.Format($"UPDATE `bunker` SET blackmoneyamount = '{BlackMoneyAmount}' WHERE `id` = '{Id}';");
            MySQLHandler.ExecuteAsync(query);
        }

        public void CheckControllers()
        {
            if (IsOwnerSetted) return;

            Dictionary<uint, int> TeamCountsInBunker = new Dictionary<uint, int>();
            foreach(DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers().Where(p => p.Player.Dimension == BunkerModule.BunkerDimension && p.Player.Position.DistanceTo(BunkerModule.BunkerCheckPosition) < BunkerModule.BunkerCheckRange))
            {
                if(dbPlayer.TeamId != 0 && dbPlayer.IsAGangster())
                {
                    if (!TeamCountsInBunker.ContainsKey(dbPlayer.TeamId)) TeamCountsInBunker.Add(dbPlayer.TeamId, 1);
                    else TeamCountsInBunker[dbPlayer.TeamId]++;
                }
            }

            if(TeamCountsInBunker.Count == 1 && TeamCountsInBunker.First().Value >= 5)
            {
                SetOwnerTeam(TeamCountsInBunker.First().Key); // Bunker eingenommen
            }
            else
            {
                if (BunkerBlackMoneyContainer != null) BunkerBlackMoneyContainer.Locked = true;
                if (BunkerRessourceOrderContainer != null) BunkerRessourceOrderContainer.Locked = true;
            }
        }

        public void SetOwnerTeam(uint TeamId)
        {
            if(OwnerTeamId != TeamId)
            {
                // Set New Ones
                OwnerTeamId = TeamId;

                if (BunkerBlackMoneyContainer != null) BunkerBlackMoneyContainer.Locked = false;
                if (BunkerRessourceOrderContainer != null) BunkerRessourceOrderContainer.Locked = false;

                TeamModule.Instance.GetById((int)TeamId).SendNotification("Dein Team hat den Bunker eingenommen!");
            }
        }
    }

    public class BunkerOrder
    {
        public uint ItemId { get; set; }
        public DateTime OrderDate { get; set; }
        public int Amount { get; set; }

        public BunkerOrder(uint itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
            OrderDate = DateTime.Now;
        }
    }
}
