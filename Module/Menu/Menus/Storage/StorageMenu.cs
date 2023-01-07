using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Freiberuf;
using VMP_CNR.Module.Freiberuf.Mower;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.InteriorProp;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Storage;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR
{
    public class StorageMenuBuilder : MenuBuilder
    {
        public StorageMenuBuilder() : base(PlayerMenu.StorageMenu)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            StorageRoom storageRoom = StorageRoomModule.Instance.GetClosest(dbPlayer);
            if (storageRoom != null)
            {
                var menu = new NativeMenu(Menu, $"Lagerraum ({storageRoom.Id})");
                if (storageRoom.IsBuyable())
                {
                    menu.Add("Lagerraum kaufen $" + storageRoom.Price);
                }
                else
                {
                    menu.Add("Lagerraum betreten");
                    menu.Add("Lagerraum ausbauen");
                    menu.Add("als Hauptlager setzen");
                    if (!storageRoom.CocainLabor) menu.Add("Kokainlabor ausbauen");
                }
                menu.Add(GlobalMessages.General.Close());
                return menu;
            }
            return null;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                StorageRoom storageRoom = StorageRoomModule.Instance.GetClosest(dbPlayer);
                if(storageRoom != null)
                {
                    if(index == 0)
                    {
                        //Kaufen
                        if(storageRoom.IsBuyable())
                        {
                            if(dbPlayer.GetStoragesOwned().Count >= StorageModule.Instance.LimitPlayerStorages)
                            {
                                dbPlayer.SendNewNotification($"Sie haben die maximale Anzahl an Lager gekauft ({StorageModule.Instance.LimitPlayerStorages})!");
                                return true;
                            }
                            if(dbPlayer.TakeBankMoney(storageRoom.Price))
                            {
                                storageRoom.SetOwnerTo(dbPlayer);
                                dbPlayer.SendNewNotification("Lager für $" + storageRoom.Price + " gekauft!");
                                return true;
                            }
                        }
                        else // betreten
                        {
                            if (!storageRoom.Locked)
                            {
                                // Player Into StorageRoom 
                                dbPlayer.SetData("storageRoomId", storageRoom.Id);
                                dbPlayer.Player.SetPosition(StorageModule.Instance.InteriorPosition);
                                dbPlayer.Player.SetRotation(StorageModule.Instance.InteriorHeading);
                                dbPlayer.SetDimension(storageRoom.Id);

                                if(storageRoom.CocainLabor)
                                {
                                    InteriorPropModule.Instance.LoadInteriorForPlayer(dbPlayer, InteriorPropListsType.Kokainlabor);
                                }
                                else
                                {
                                    InteriorPropModule.Instance.LoadInteriorForPlayer(dbPlayer, InteriorPropListsType.Lagerraum);
                                }
                                return true;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification("Lager ist abgeschlossen!", title: "Lager", notificationType: PlayerNotification.NotificationType.ERROR);
                                return true;
                            }
                        }
                    }
                    else if(index == 1)
                    {
                        if (dbPlayer.Id != storageRoom.OwnerId) return true;
                        storageRoom.Upgrade(dbPlayer);
                        return true;
                    }
                    else if (index == 2)
                    {
                        if (dbPlayer.Id != storageRoom.OwnerId) return true;
                        storageRoom.SetMainFlagg(dbPlayer);
                        return true;
                    }
                    else if (index == 3)
                    {
                        if (dbPlayer.Id != storageRoom.OwnerId) return true;
                        if (storageRoom.CocainLabor) return true;
                        else
                            storageRoom.UpgradeCocain(dbPlayer);
                        return true;
                    }
                    else
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}