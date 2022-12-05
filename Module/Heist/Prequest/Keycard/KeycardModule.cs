using System;
using System.Collections.Generic;
using System.IO;
using GTANetworkAPI;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Handler;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Heist.Prequest.Keycard
{
    public class KeycardModule : Module<KeycardModule>
    {
        public static Vector3 PedPosition = new Vector3(-219.065, 6571.35, 2.71107);

        protected override bool OnLoad()
        {
            new Npc(PedHash.WareMechMale01, PedPosition, 70, 0);
            return base.OnLoad();
        }


        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Dimension[0] == 0 && key == Key.E)
            {
                if (!dbPlayer.IsAGangster() && !dbPlayer.IsBadOrga()) return false;
                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured() || dbPlayer.Player.Position.DistanceTo(PedPosition) > 5.0f) return false;

                if (dbPlayer.Player.Position.DistanceTo(PedPosition) < 5.0f && !dbPlayer.RageExtension.IsInVehicle)
                {
                    SxVehicle keycar = null;
                    foreach (SxVehicle sxVehicle in VehicleHandler.Instance.GetClosestVehicles(PedPosition, 20.0f))
                    {
                        if  (IsSpecialVehicle(sxVehicle) && 
                            (sxVehicle.Team.Id == 13 || sxVehicle.Team.Id == 1  || sxVehicle.Team.Id == 5))
                        {   
                            keycar = sxVehicle;
                            break;
                        }
                    }

                    if (keycar != null && keycar.IsValid() && keycar.CanInteract)
                    {
                        dbPlayer.SendNewNotification("Okay, ich baue die KeyCard nun aus!");
                        GetKeycard(dbPlayer, keycar);
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Bruder, wo hast du hier ein geeignetes Auto? Geh weg.");
                    }

                    return true;
                }
            }
            return false;
        }

        public bool IsSpecialVehicle(SxVehicle vehicle) {   
            // 1100 = PolVacca | 1332 = GT63SAMG-Fraktion | 1282 = Schafter7-Fraktion | 1392 = E-Klasse-Fraktion
            return vehicle.Data.Id == 1100 || vehicle.Data.Id == 1332 || vehicle.Data.Id == 1282 || vehicle.Data.Id == 1392;
        }

        public void GetKeycard(DbPlayer dbPlayer, SxVehicle vehicle)
        {
            Main.m_AsyncThread.AddToAsyncThread (new Task( async () => {
                int time = Configurations.Configuration.Instance.DevMode ? 30000 : 180000;

                vehicle.CanInteract = false;

                Chats.sendProgressBar(dbPlayer, time);

                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                dbPlayer.SetCannotInteract(true);

            await NAPI.Task.WaitForMainThread(time);

            dbPlayer.SetCannotInteract(false);

            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured() || dbPlayer.Player.Position.DistanceTo(PedPosition) > 5.0f) return;

            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
            dbPlayer.StopAnimation();

                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured() || dbPlayer.Player.Position.DistanceTo(PedPosition) > 5.0f) return;
                if (vehicle.entity.Position.DistanceTo(PedPosition) > 20.0f) return;

                Dictionary<string, dynamic> ItemData = new Dictionary<string, dynamic>();
                ItemData.Add("created", (DateTime)DateTime.Now);
                dbPlayer.Container.AddItem(1105, 1, ItemData);

                dbPlayer.SendNewNotification("Da haste die KeyCard", notificationType: PlayerNotification.NotificationType.SUCCESS);

                vehicle.CanInteract = true;
                vehicle.SetTeamCarGarage(true);
            }));
        }

        public bool HasPlayerActiveKeycard(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            if (dbPlayer.Container == null) return false;

            Item item = dbPlayer.Container.GetItemById(1105);
            if(item != null && item.Data != null && item.Data.ContainsKey("created"))
            {
                DateTime dateTime = item.Data["created"];

                if (dateTime.AddHours(24) > DateTime.Now)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
