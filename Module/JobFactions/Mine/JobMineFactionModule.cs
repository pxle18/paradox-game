using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.JobFactions.Mine
{
    public class JobMineFactionModule : Module<JobMineFactionModule>
    {
        public Dictionary<uint, int> PlayerSourceStorage = new Dictionary<uint, int>();

        public Vector3 AbbauPosition = new Vector3(2953.53, 2787.5, 40.7504);
        public Vector3 StorageLoadPosition = new Vector3(2681.83, 2796.3, 40.698);
        
        public Vector3 ContainerAluPosition = new Vector3(2592.48, 2832.17, 31.6245);
        public float ContainerAliRange = 12.0f;

        public Vector3 ContainerBroncePositon = new Vector3(2629.21, 2868.81, 33.8326);
        public float ContainerBronceRange = 14.0f;

        public Vector3 ContainerZinkPositon = new Vector3(2724.88, 2869.89, 35.2724);
        public float ContainerZinkRange = 17.0f;

        public Vector3 ContainerIronPosition = new Vector3(2720.81, 2894.9, 35.9592);
        public float ContainerIronRange = 18.0f;

        public Vector3 ContainerSchmelzofen = new Vector3(1087.48, -2001.56, 30.8806);
        public Vector3 ContainerSchmelzcoal = new Vector3(1087.42, -2004.98, 31.3806);
        public Vector3 ContainerSchmelzofenOutput = new Vector3(1062.61, -1977.51, 31.2495);

        public Vector3 VehicleLoadSchmelzPosition = new Vector3(1062.61, -1977.51, 31.2495);
        public Vector3 VehicleUnloadIntoSchmelzePosition = new Vector3(1079.88, -1968.68, 30.4669);
        public Vector3 VehicleUnloadMinePosition = new Vector3(2697.36, 2774.2, 38.1126);

        public Vector3 MineStoragePosition = new Vector3(2676.81, 2760.38, 37.9589);

        public uint AluErz = 463;
        public uint AluBarren = 462;

        public uint IronErz = 299;
        public uint IronBarren = 300;

        public uint Copper = 961;
        public uint BronceBarren = 464;

        public uint ZinkKohle = 298;
        public uint Batterien = 15;

        public uint Coal = 963;

        public List<uint> FertigeProdukte;
        public List<uint> Rohprodukte;

        protected override bool OnLoad()
        {
            // Loading Storages

            PlayerSourceStorage.Clear();

            var query = $"SELECT * FROM `jobfaction_mine_storage`";
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            PlayerSourceStorage.Add(reader.GetUInt32("player_id"), reader.GetInt32("storage"));
                        }
                    }
                }
            }

            this.FertigeProdukte = new List<uint> { AluBarren, IronBarren, BronceBarren, Batterien };
            this.Rohprodukte = new List<uint> { AluErz, IronErz, Copper, ZinkKohle };

            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || (dbPlayer.TeamId != (int)TeamTypes.TEAM_MINE1 && dbPlayer.TeamId != (int)TeamTypes.TEAM_MINE2) || !dbPlayer.RageExtension.IsInVehicle) return false;

            SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();
            if (sxVehicle == null || !sxVehicle.IsValid()) return false;

            if (sxVehicle.teamid == (uint)dbPlayer.TeamId && dbPlayer.Player.Position.DistanceTo(VehicleLoadSchmelzPosition) < 8.0f)
            {
                if (dbPlayer.CanInteract() && sxVehicle.CanInteract)
                {
                    Task.Run(async () =>
                    {

                        Chats.sendProgressBar(dbPlayer, (120000));

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);
                        sxVehicle.CanInteract = false;
                        sxVehicle.SyncExtension.SetEngineStatus(false);

                        await Task.Delay(120000);

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        sxVehicle.CanInteract = true;
                        sxVehicle.SyncExtension.SetEngineStatus(true);

                        Container TeamMineSchmelzeOutputContainer = TeamModule.Instance.Get(dbPlayer.TeamId).MineContainerSchmelzeOutput;
                        foreach (Item item in TeamMineSchmelzeOutputContainer.Slots.Values.ToList())
                        {
                            if (!this.FertigeProdukte.Contains(item.Id)) continue;

                            if (sxVehicle.Container.CanInventoryItemAdded(item.Model, item.Amount))
                            {
                                sxVehicle.Container.AddItem(item.Model, item.Amount, item.Data, -1, true);
                                TeamMineSchmelzeOutputContainer.RemoveItem(item.Model, item.Amount, true);
                            }
                            else if(sxVehicle.Container.CanInventoryItemAdded(item.Model, 1))
                            {
                                for(int counter = 1; counter <= item.Amount; counter++)
                                {
                                    if(!sxVehicle.Container.CanInventoryItemAdded(item.Model, counter))
                                    {
                                        if (!sxVehicle.Container.CanInventoryItemAdded(item.Model, counter - 1)) break;

                                        sxVehicle.Container.AddItem(item.Model, counter-1, item.Data, -1, true);
                                        TeamMineSchmelzeOutputContainer.RemoveItem(item.Model, counter-1, true);

                                        break;
                                    }
                                }
                            }
                        }
                        sxVehicle.Container.SaveAll();
                        TeamMineSchmelzeOutputContainer.SaveAll();
                        return;
                    });
                }
            }
            else if (sxVehicle.teamid == (uint)dbPlayer.TeamId && dbPlayer.Player.Position.DistanceTo(VehicleUnloadMinePosition) < 8.0f)
            {
                if (dbPlayer.CanInteract() && sxVehicle.CanInteract)
                {
                    Task.Run(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, (120000));

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);
                        sxVehicle.CanInteract = false;
                        sxVehicle.SyncExtension.SetEngineStatus(false);

                        await Task.Delay(120000);

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        sxVehicle.CanInteract = true;
                        sxVehicle.SyncExtension.SetEngineStatus(true);

                        Container TeamMineStorageContainer = TeamModule.Instance.Get(dbPlayer.TeamId).MineContainerStorage;
                        foreach (Item item in sxVehicle.Container.Slots.Values.ToList())
                        {
                            if (TeamMineStorageContainer.CanInventoryItemAdded(item.Model, item.Amount))
                            {
                                TeamMineStorageContainer.AddItem(item.Model, item.Amount, item.Data, -1, true);
                                sxVehicle.Container.RemoveItem(item.Model, item.Amount, true);
                            }
                        }
                        sxVehicle.Container.SaveAll();
                        TeamMineStorageContainer.SaveAll();
                        return;
                    });
                }
            }
            else if (sxVehicle.teamid == (uint)dbPlayer.TeamId && dbPlayer.Player.Position.DistanceTo(VehicleUnloadIntoSchmelzePosition) < 8.0f)
            {
                if (dbPlayer.CanInteract() && sxVehicle.CanInteract)
                {
                    Task.Run(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, (120000));

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);
                        sxVehicle.CanInteract = false;
                        sxVehicle.SyncExtension.SetEngineStatus(false);

                        await Task.Delay(120000);

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        sxVehicle.CanInteract = true;
                        sxVehicle.SyncExtension.SetEngineStatus(true);

                        Container TeamMineSchmelzContainer = TeamModule.Instance.Get(dbPlayer.TeamId).MineContainerSchmelze;
                        int weight = 0;

                        foreach (Item item in sxVehicle.Container.Slots.Values.ToList())
                        {
                            if (this.Rohprodukte.Contains(item.Id) && TeamMineSchmelzContainer.CanInventoryItemAdded(item.Model, item.Amount))
                            {
                                TeamMineSchmelzContainer.AddItem(item.Model, item.Amount, item.Data, -1, true);
                                sxVehicle.Container.RemoveItem(item.Model, item.Amount, true);

                                weight += item.Model.Weight * item.Amount;
                            }
                        }

                        int pay = weight / 1000 * 5; //50$ Pro 100 KG
                        dbPlayer.GiveBankMoney(pay);
                        dbPlayer.AddPlayerBankHistory(pay, "Mine - Schmelze");

                        dbPlayer.LogVermoegen();
                        TeamMineSchmelzContainer.SaveAll();
                        sxVehicle.Container.SaveAll();
                        return;
                    });
                }
            }

            if (sxVehicle.Data.Hash == (uint)VehicleHash.Bulldozer && dbPlayer.Player.Position.DistanceTo(AbbauPosition) < 20.0f)
            {
                if(sxVehicle.HasData("mine1_loadage"))
                {
                    dbPlayer.SendNewNotification("Die Schaufel ist bereits vollgeladen!");
                    return false;
                }
                else if(dbPlayer.CanInteract() && sxVehicle.CanInteract)
                {
                    Task.Run(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, (12000));

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);
                        sxVehicle.CanInteract = false;
                        sxVehicle.SyncExtension.SetEngineStatus(false);

                        await Task.Delay(12000);

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        sxVehicle.CanInteract = true;
                        sxVehicle.SyncExtension.SetEngineStatus(true);

                        Random rnd = new Random();
                        int amount = rnd.Next(45, 85);
                        sxVehicle.SetData("mine1_loadage", amount);

                        dbPlayer.SendNewNotification($">> {amount} kg Steingemisch geladen.");
                        return;
                    });
                }
            }
            else if (sxVehicle.Data.Hash == (uint)VehicleHash.Bulldozer && dbPlayer.Player.Position.DistanceTo(StorageLoadPosition) < 5.0f)
            {
                if (sxVehicle.HasData("mine1_loadage") && dbPlayer.CanInteract() && sxVehicle.CanInteract)
                {
                    Task.Run(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, (12000));

                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);
                        sxVehicle.CanInteract = false;
                        sxVehicle.SyncExtension.SetEngineStatus(false);

                        await Task.Delay(12000);

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        sxVehicle.CanInteract = true;
                        sxVehicle.SyncExtension.SetEngineStatus(true);

                        if(sxVehicle.GetData("mine1_loadage") == null)
                        {
                            return;
                        }

                        string amountString = sxVehicle.GetData("mine1_loadage").ToString();
                        if (!int.TryParse(amountString, out int amount))
                            return;

                        sxVehicle.ResetData("mine1_loadage");

                        AddPlayerMineStorageAmount(dbPlayer, amount);

                        int pay = amount * 15;
                        dbPlayer.GiveBankMoney(pay);
                        dbPlayer.AddPlayerBankHistory(pay, "Mine - Abladen");

                        dbPlayer.LogVermoegen();

                        dbPlayer.SendNewNotification($">> {amount} kg Steingemisch entladen. (Bestand: {GetPlayerStorageAmount(dbPlayer)} kg)");
                        return;
                    });
                }
                return false;
            }

            return false;
        }

        private void CreateStorage(DbPlayer dbPlayer)
        {
            if (PlayerSourceStorage.ContainsKey(dbPlayer.Id)) return;

            MySQLHandler.ExecuteAsync($"INSERT IGNORE INTO jobfaction_mine_storage (`player_id`,`storage`) VALUES ('{dbPlayer.Id}', '0');");
            PlayerSourceStorage.Add(dbPlayer.Id, 0);
        }

        public int GetPlayerStorageAmount(DbPlayer dbPlayer)
        {
            if (PlayerSourceStorage.ContainsKey(dbPlayer.Id)) {
                return PlayerSourceStorage[dbPlayer.Id];
            }
            else
            {
                CreateStorage(dbPlayer);
                return PlayerSourceStorage[dbPlayer.Id];
            }
        }

        public void AddPlayerMineStorageAmount(DbPlayer dbPlayer, int amount)
        {
            if (!PlayerSourceStorage.ContainsKey(dbPlayer.Id))
            {
                CreateStorage(dbPlayer);
            }
            PlayerSourceStorage[dbPlayer.Id] += amount;
            SaveMineStorage(dbPlayer);
        }

        public void SaveMineStorage(DbPlayer dbPlayer)
        {
            if (!PlayerSourceStorage.ContainsKey(dbPlayer.Id))
            {
                CreateStorage(dbPlayer);
                return;
            }
            else MySQLHandler.ExecuteAsync($"UPDATE jobfaction_mine_storage SET `storage` = '{PlayerSourceStorage[dbPlayer.Id]}' WHERE `player_id` = '{dbPlayer.Id}'");
        }

        public override void OnFiveMinuteUpdate()
        {
            foreach (DbPlayer member in TeamModule.Instance.Get((uint)TeamTypes.TEAM_MINE1).Members.Values.ToList().Concat(TeamModule.Instance.Get((uint)TeamTypes.TEAM_MINE2).Members.Values.ToList()))
            {
                if (PlayerSourceStorage.ContainsKey(member.Id) && PlayerSourceStorage[member.Id] >= 50)
                {
                    PlayerSourceStorage[member.Id] -= 50;
                    SaveMineStorage(member);

                    if (member.Team.MineAluContainer != null) member.Team.MineAluContainer.AddItem(AluErz, 100); // Aluerz -> Alu
                    if (member.Team.MineIronContainer != null) member.Team.MineIronContainer.AddItem(IronErz, 100); // Eisenerz -> Eisen
                    if (member.Team.MineBronceContainer != null) member.Team.MineBronceContainer.AddItem(Copper, 100); // Kupfer -> Bronce
                    if (member.Team.MineZinkContainer != null) member.Team.MineZinkContainer.AddItem(ZinkKohle, 400); // Zinkkohle -> Zink (Batterien)
                }
            }

            // Check mine 1 Team
            Team Mine1 = TeamModule.Instance.Get((uint)TeamTypes.TEAM_MINE1);
            if(Mine1 != null)
            {
                if (Mine1.MineContainerSchmelze != null && Mine1.MineContainerSchmelzCoal != null)
                {
                    if (Mine1.MineContainerSchmelzCoal.GetItemAmount(Coal) >= 15)
                    {
                        int playerCountOnline = TeamModule.Instance.Get((uint)TeamTypes.TEAM_MINE1).GetTeamMembers().Where(m => m.AccountStatus == AccountStatus.LoggedIn).Count();
                        Container MineSchmelzeOutPutContainer = TeamModule.Instance.Get(Mine1.Id).MineContainerSchmelzeOutput;

                        int amountToConvert = Mine1.MineContainerSchmelze.GetItemAmount(IronErz);
                        if (amountToConvert > (200 * playerCountOnline)) amountToConvert = (200 * playerCountOnline);
                        // Eisen
                        if (MineSchmelzeOutPutContainer.CanInventoryItemAdded(IronBarren, amountToConvert/5))
                        {
                            Mine1.MineContainerSchmelze.RemoveItem(IronErz, amountToConvert, true);
                            MineSchmelzeOutPutContainer.AddItem(IronBarren, amountToConvert/5, new Dictionary<string, dynamic>(), -1, true);
                        }

                        amountToConvert = Mine1.MineContainerSchmelze.GetItemAmount(Copper);
                        if (amountToConvert > (200 * playerCountOnline)) amountToConvert = (200 * playerCountOnline);
                        // Bronce
                        if (MineSchmelzeOutPutContainer.CanInventoryItemAdded(BronceBarren, amountToConvert/5))
                        {
                            Mine1.MineContainerSchmelze.RemoveItem(Copper, amountToConvert, true);
                            MineSchmelzeOutPutContainer.AddItem(BronceBarren, amountToConvert/5, new Dictionary<string, dynamic>(), -1, true);
                        }

                        amountToConvert = Mine1.MineContainerSchmelze.GetItemAmount(AluErz);
                        if (amountToConvert > (200 * playerCountOnline)) amountToConvert = (200 * playerCountOnline);
                        // Alu
                        if (MineSchmelzeOutPutContainer.CanInventoryItemAdded(AluBarren, amountToConvert/5))
                        {
                            Mine1.MineContainerSchmelze.RemoveItem(AluErz, amountToConvert, true);
                            MineSchmelzeOutPutContainer.AddItem(AluBarren, amountToConvert/5, new Dictionary<string, dynamic>(), -1, true);
                        }


                        amountToConvert = Mine1.MineContainerSchmelze.GetItemAmount(ZinkKohle);
                        if (amountToConvert > (200 * playerCountOnline)) amountToConvert = (200 * playerCountOnline);
                        // Zink -> Batterien
                        if (MineSchmelzeOutPutContainer.CanInventoryItemAdded(Batterien, amountToConvert/5))
                        {
                            Mine1.MineContainerSchmelze.RemoveItem(ZinkKohle, amountToConvert, true);
                            MineSchmelzeOutPutContainer.AddItem(Batterien, amountToConvert/5, new Dictionary<string, dynamic>(), -1, true);
                        }

                        Mine1.MineContainerSchmelze.SaveAll();
                        MineSchmelzeOutPutContainer.SaveAll();
                        Mine1.MineContainerSchmelzCoal.RemoveItem(Coal, 15);
                    }
                }
            }

            // Check mine 2 Team (just use old var, faulheit)
            Mine1 = TeamModule.Instance.Get((uint)TeamTypes.TEAM_MINE2);
            if (Mine1 != null)
            {
                if (Mine1.MineContainerSchmelze != null && Mine1.MineContainerSchmelzCoal != null)
                {
                    if (Mine1.MineContainerSchmelzCoal.GetItemAmount(Coal) >= 15)
                    {
                        int playerCountOnline = TeamModule.Instance.Get((uint)TeamTypes.TEAM_MINE2).GetTeamMembers().Where(m => m.AccountStatus == AccountStatus.LoggedIn).Count();

                        int amountToConvert = Mine1.MineContainerSchmelze.GetItemAmount(IronErz);
                        if (amountToConvert > (200 * playerCountOnline)) amountToConvert = (200 * playerCountOnline);
                        // Eisen
                        if (Mine1.MineContainerSchmelze.CanInventoryItemAdded(IronBarren, amountToConvert / 40))
                        {
                            Mine1.MineContainerSchmelze.RemoveItem(IronErz, amountToConvert, true);
                            Mine1.MineContainerSchmelze.AddItem(IronBarren, amountToConvert / 10, new Dictionary<string, dynamic>(), -1, true);
                        }

                        amountToConvert = Mine1.MineContainerSchmelze.GetItemAmount(Copper);
                        if (amountToConvert > (200 * playerCountOnline)) amountToConvert = (200 * playerCountOnline);
                        // Bronce
                        if (Mine1.MineContainerSchmelze.CanInventoryItemAdded(BronceBarren, amountToConvert / 40))
                        {
                            Mine1.MineContainerSchmelze.RemoveItem(Copper, amountToConvert, true);
                            Mine1.MineContainerSchmelze.AddItem(BronceBarren, amountToConvert / 10, new Dictionary<string, dynamic>(), -1, true);
                        }

                        amountToConvert = Mine1.MineContainerSchmelze.GetItemAmount(AluErz);
                        if (amountToConvert > (200 * playerCountOnline)) amountToConvert = (200 * playerCountOnline);
                        // Alu
                        if (Mine1.MineContainerSchmelze.CanInventoryItemAdded(AluBarren, amountToConvert / 40))
                        {
                            Mine1.MineContainerSchmelze.RemoveItem(AluErz, amountToConvert, true);
                            Mine1.MineContainerSchmelze.AddItem(AluBarren, amountToConvert / 10, new Dictionary<string, dynamic>(), -1, true);
                        }

                        amountToConvert = Mine1.MineContainerSchmelze.GetItemAmount(ZinkKohle);
                        if (amountToConvert > (100 * playerCountOnline)) amountToConvert = (200 * playerCountOnline);
                        // Zink -> Batterien
                        if (Mine1.MineContainerSchmelze.CanInventoryItemAdded(Batterien, amountToConvert / 5))
                        {
                            Mine1.MineContainerSchmelze.RemoveItem(ZinkKohle, amountToConvert, true);
                            Mine1.MineContainerSchmelze.AddItem(Batterien, amountToConvert / 5, new Dictionary<string, dynamic>(), -1, true);
                        }

                        Mine1.MineContainerSchmelze.SaveAll();
                        Mine1.MineContainerSchmelzCoal.RemoveItem(Coal, 15);
                    }
                }
            }
        }
    }
}
