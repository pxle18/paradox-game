using System;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.Players.PlayerTask
{
    public class PlayerTask
    {
        public uint Id { get; }
        public PlayerTaskType Type { get; }
        public string Data { get; }
        public DbPlayer Owner { get; }
        public DateTime Finish { get; }
        public bool TaskFinished { get; set; }

        public PlayerTask(uint id, uint taskTypeId, DbPlayer dbPlayer, string data, DateTime finish)
        {
            Id = id;
            Type = PlayerTaskTypeModule.Instance.Get(taskTypeId);
            Owner = dbPlayer;
            Data = data;
            Finish = finish;
            TaskFinished = false;
        }

        public void OnTaskFinish()
        {
            try
            {
                if (TaskFinished)
                {
                    Owner.RemoveTask(Id);
                    return;
                }
            }
            catch(Exception e)
            {
                Logger.Crash(e);
                return;
            }
            try
            {
                switch (Type.Id)
                {
                    case PlayerTaskTypeId.KellerAusbau:
                        var iHouse = HouseModule.Instance.GetByOwner(Owner.Id);
                        if (iHouse == null) return;
                        iHouse.Keller = 1;
                        iHouse.SaveKeller();
                        break;
                    case PlayerTaskTypeId.MoneyKellerAusbau:
                        iHouse = HouseModule.Instance.GetByOwner(Owner.Id);
                        if (iHouse == null) return;
                        iHouse.MoneyKeller = 1;
                        iHouse.SaveMoneyKeller();
                        break;
                    case PlayerTaskTypeId.LaborAusbau:
                        iHouse = HouseModule.Instance.GetByOwner(Owner.Id);
                        if (iHouse == null) return;
                        iHouse.Keller = 2;
                        iHouse.SaveKeller();
                        break;
                    case PlayerTaskTypeId.VehicleImport:
                        var crumbs = Owner.GetName().Split('_');

                        string firstLetter;
                        string secondLetter;

                        if (crumbs.Length == 2 && crumbs[0].Length > 0 && crumbs[1].Length > 0)
                        {
                            firstLetter = crumbs[0][0].ToString();
                            secondLetter = crumbs[1][0].ToString();
                        }
                        else
                        {
                            firstLetter = "";
                            secondLetter = "";
                        }

                        if (!uint.TryParse(Data, out var vehicleDataId)) return;

                        var xData = VehicleDataModule.Instance.GetDataById(vehicleDataId);
                        if (xData == null) return;
                        var query =
                            $"INSERT INTO `vehicles` (`owner`, `garage_id`, `inGarage`, `plate`, `model`, `vehiclehash`) VALUES ('{Owner.Id}', '525', '1', '{firstLetter + secondLetter + " " + Owner.ForumId}', '{xData.Id}', '{xData.Model}');";

                        MySQLHandler.ExecuteAsync(query);
                        break;
                }
                TaskFinished = true;
                Owner.SendNewNotification(
                    $"Ihr {Type.Name} wurde nach {Type.TaskTime} Minuten abgeschlossen!");
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
    }
}