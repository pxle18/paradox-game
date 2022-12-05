using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Armory;
using VMP_CNR.Module.Banks;
using VMP_CNR.Module.Events;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Vehicles.Garages;

namespace VMP_CNR.Module.AsyncEventTasks
{
    public static partial class AsyncEventTasks
    {
        public static void EnterColShapeTask(ColShape shape, Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (shape.HasData("notificationId"))
            {
                var notification =
                    PlayerNotifications.Instance.GetById(shape.GetData<int>("notificationId"));
                dbPlayer.SendNotification(notification);
            }

            if (Modules.Instance.OnColShapeEvent(dbPlayer, shape, ColShapeState.Enter)) return;

            if (shape.HasData("clothShopId"))
            {
                if (shape.HasData("eventId"))
                {
                    if (!EventModule.Instance.IsEventActive(shape.GetData<uint>("eventId")))
                        return;
                }

                dbPlayer.SetData("clothShopId", shape.GetData<uint>("clothShopId"));
                dbPlayer.SendNewNotification("Benutze E um Kleidung zu kaufen!", title: "Kleidungsstore");
                return;
            }

            if (shape.HasData("teamWardrobe"))
            {
                HashSet<int> teamsHashset = shape.GetData<HashSet<int>>("teamWardrobe");
                uint teamId = dbPlayer.TeamId;
                if (teamsHashset.Contains((int)teams.TEAM_IAA) && dbPlayer.IsNSADuty)
                {
                    teamId = (uint)teams.TEAM_IAA;
                }

                if (!teamsHashset.Contains((int)teamId)) return;
                dbPlayer.SetData("teamWardrobe", teamsHashset);
                dbPlayer.SendNewNotification("Benutze E um Kleidung zu kaufen!", title: "Fraktionskleiderschrank");
                return;
            }

            if (shape.HasData("ammunationId"))
            {
                dbPlayer.SendNewNotification("Benutze E um Waffen zu kaufen!", title: "Ammunation");

                int ammunationId = shape.GetData<int>("ammunationId");
                dbPlayer.SetData("ammunationId", ammunationId);
                return;
            }

            if (shape.HasData("garageId"))
            {
                try
                {
                    uint garageId = shape.GetData<uint>("garageId");
                    if (!GarageModule.Instance.Contains(garageId)) return;
                    var garage = GarageModule.Instance[garageId];
                    if (garage == null) return;
                    var labelText = "";

                    if (garage.PublicTeamRestriction > 0 && dbPlayer.TeamId != garage.PublicTeamRestriction)
                    {
                        return;
                    }

                    if (!garage.DisableInfos)
                    {

                        if (garage.IsTeamGarage())
                        {
                            labelText = labelText + " (Fraktionsgarage)";
                        }
                        else if (garage.Type == GarageType.VehicleCollection)
                        {
                            labelText = "Fahrzeug fuer 2500$ Kaution freikaufen";
                        }
                        else if (garage.Type == GarageType.VehicleAdminGarage)
                        {
                            labelText = "Fahrzeug fuer 25000$ Kaution freikaufen";
                        }
                        else
                        {
                            labelText = "Benutze E um die Garage zu öffnen!";
                        }
                    }
                    dbPlayer.SetData("garageId", garageId);
                    return;
                }
                catch(Exception e) {
                    Logger.Crash(e);
                }
            }
            
            if (shape.HasData("bankId"))
            {
                var bankId = shape.GetData<uint>("bankId");
                if (bankId == 0)
                {
                    return;
                }

                var parseBankId = uint.TryParse(bankId.ToString(), out uint bankIdNew);
                if (!parseBankId)
                {
                    return;
                }

                var bank = BankModule.Instance.Get(bankIdNew);
                dbPlayer.SendNewNotification("Benutze E um auf dein Konto zuzugreifen!", title: bank.Name);
                dbPlayer.SetData("bankId", bankId);
                return;
            }

            if (shape.HasData("ArmoryId"))
            {
                int ArmoryId = shape.GetData<int>("ArmoryId");
                var Armory = ArmoryModule.Instance.Get(ArmoryId);
                if (Armory == null) return;
                dbPlayer.SendNewNotification("Benutze E zum interagieren!", title: "Waffenkammer");
                dbPlayer.SetData("ArmoryId", ArmoryId);
                return;
            }
        }
    }
}
