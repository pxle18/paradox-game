using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Doors;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Staatsgefaengnis
{
    public class LaundryWashing
    {
        public DateTime DateTime { get; set; }
        public int LaundryAmount { get; set; }
    }

    public class LaundryModule : SqlModule<LaundryModule, LaundryPosition, uint>
    {
        public static int MaxTakeAbleLaundryPerBasket = 10;

        public static int WashingMinuites = 10;

        public static uint LaundryBagItemId = 1137;
        public static uint LaundryKeyItemId = 1138;

        public static Vector3 WaschmaschineInput = new Vector3(1766.04, 2583.43, 45.9177);
        public static Vector3 WaschmaschineOutput = new Vector3(1770.16, 2583.57, 45.9177);

        public static Vector3 LaundryToRopePoint = new Vector3(1770.13, 2581.21, 45.9177);

        public static Vector3 LaundryBagPosition = new Vector3(1772.05, 2583.1, 45.9177);

        public Dictionary<uint, LaundryWashing> WashingPlayers = new Dictionary<uint, LaundryWashing>();

        protected override string GetQuery()
        {
            return "SELECT * FROM `laundrypositions`;";
        }

        protected override void OnLoaded()
        {

            WashingPlayers = new Dictionary<uint, LaundryWashing>();

            NAPI.Marker.CreateMarker(25, (WaschmaschineInput - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, 0);
            NAPI.Marker.CreateMarker(25, (WaschmaschineOutput - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, 0);

            NAPI.Marker.CreateMarker(25, (LaundryToRopePoint - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, 0);

            NAPI.Marker.CreateMarker(25, (LaundryBagPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, 0);

        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if(dbPlayer != null && dbPlayer.IsValid() && key == Key.E)
            {
                if (dbPlayer.IsSGJobActive(SGJobs.WASHING))
                {
                    if (dbPlayer.Player.Position.DistanceTo(LaundryToRopePoint) < 1.0f)
                    {
                        if (dbPlayer.Container.GetItemAmount(LaundryBagItemId) <= 0)
                        {
                            dbPlayer.SendNewNotification("Du hast keinen Wäschekorb mit dreckiger Wäsche!");
                            return true;
                        }

                        Item item = dbPlayer.Container.GetItemById((int)LaundryBagItemId);

                        if (item.Data == null || !item.Data.ContainsKey("laundry"))
                        {
                            dbPlayer.SendNewNotification("Du hast keinen Wäschekorb mit Wäsche!");
                            return true;
                        }
                        else
                        {
                            int amount = item.Data["laundry"];

                            if (amount < 2)
                            {
                                dbPlayer.SendNewNotification($"Du brauchst mindestens 2 Bettlaken um ein Seil herzustellen!");
                                return true;
                            }

                            if(!dbPlayer.Container.CanInventoryItemAdded(17))
                            {
                                dbPlayer.SendNewNotification($"Ich kann so viel nicht tragen!");
                                return true;
                            }

                            if (WashingPlayers.ContainsKey(dbPlayer.Id))
                            {
                                return true;
                            }
                            else
                            {
                                ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"Seil Herstellung", $"Wollen Sie 2 Bettlaken zu einem Seil verarbeiten? (Lass dich nicht erwischen!)", "LaundryToRopeConfirm", "", ""));
                                return true;
                            }
                        }
                    }


                    if (dbPlayer.Player.Position.DistanceTo(WaschmaschineInput) < 1.0f)
                    {
                        if (dbPlayer.Container.GetItemAmount(LaundryBagItemId) <= 0)
                        {
                            dbPlayer.SendNewNotification("Du hast keinen Wäschekorb mit dreckiger Wäsche!");
                            return true;
                        }

                        Item item = dbPlayer.Container.GetItemById((int)LaundryBagItemId);

                        if (item.Data == null || item.Data.ContainsKey("washed") || !item.Data.ContainsKey("laundry"))
                        {
                            dbPlayer.SendNewNotification("Du hast keinen Wäschekorb mit dreckiger Wäsche!");
                            return true;
                        }
                        else
                        {
                            int amount = item.Data["laundry"];

                            if (amount < 1)
                            {
                                dbPlayer.SendNewNotification($"Der Wäschekorb ist leer!");
                                return true;
                            }

                            if(WashingPlayers.ContainsKey(dbPlayer.Id))
                            {
                                dbPlayer.SendNewNotification("Du hast bereits eine Waschmaschine am laufen!");
                                return true;
                            }
                            else
                            {
                                dbPlayer.Container.RemoveItemAll(item.Id);
                                dbPlayer.SendNewNotification($"Waschvorgang gestartet! (Dauer ~ {WashingMinuites} Minuten!");

                                WashingPlayers.Add(dbPlayer.Id, new LaundryWashing() { DateTime = DateTime.Now, LaundryAmount = amount });
                                return true;
                            }
                        }
                    }

                    if (dbPlayer.Player.Position.DistanceTo(WaschmaschineOutput) < 1.0f)
                    {
                        if(!WashingPlayers.ContainsKey(dbPlayer.Id))
                        {
                            dbPlayer.SendNewNotification("Du hast keine Waschmaschine am laufen!");
                            return true;
                        }
                        else
                        {
                            if(WashingPlayers[dbPlayer.Id].DateTime.AddMinutes(WashingMinuites) > DateTime.Now)
                            {
                                dbPlayer.SendNewNotification("Deine Waschmaschine ist noch am waschen!");
                                return true;
                            }
                            else
                            {
                                Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();

                                data.Add("laundry", WashingPlayers[dbPlayer.Id].LaundryAmount);
                                data.Add("washed", true);

                                dbPlayer.Container.AddItem(LaundryBagItemId, 1, data);

                                WashingPlayers.Remove(dbPlayer.Id);

                                dbPlayer.SendNewNotification("Wäschekorb mit sauberer Wäsche entnommen!");
                                return true;
                            }
                        }
                    }

                    if (dbPlayer.Player.Position.DistanceTo(LaundryBagPosition) < 1.0f)
                    {
                        if (dbPlayer.Container.GetItemAmount(LaundryBagItemId) <= 0)
                        {
                            if (WashingPlayers.ContainsKey(dbPlayer.Id))
                            {
                                dbPlayer.SendNewNotification("Du hast bereits eine Waschmaschine am laufen!");
                                return true;
                            }

                            dbPlayer.Container.AddItem(LaundryBagItemId);
                            dbPlayer.SendNewNotification("Du hast einen Wäschekorb aufgenommen und kannst die Bettlaken in den Zellen einsammeln!");
                        }
                        else
                        {
                            Item item = dbPlayer.Container.GetItemById((int)LaundryBagItemId);

                            if (item.Data != null && item.Data.ContainsKey("washed") && item.Data.ContainsKey("laundry"))
                            {
                                // Haftzeitminderung & Geld TODO
                                int amount = item.Data["laundry"];

                                int moneymultiplier = 100;

                                int money = moneymultiplier * amount;
                                int haftzeitminderung = amount;
                                int fbhaftzeitminderung = 0;

                                if (dbPlayer.JailTime[0] > 5)
                                {
                                    if(dbPlayer.JailTime[0]- haftzeitminderung < 5)
                                    {
                                        fbhaftzeitminderung = dbPlayer.JailTime[0]-5;

                                        dbPlayer.JailTime[0] = 5;
                                        dbPlayer.jailtimeReducing[0] = 0;
                                    }
                                    else
                                    {
                                        if (dbPlayer.jailtimeReducing[0] > haftzeitminderung)
                                        {
                                            fbhaftzeitminderung = haftzeitminderung;
                                            dbPlayer.JailTime[0] -= haftzeitminderung;
                                            dbPlayer.jailtimeReducing[0] -= haftzeitminderung;
                                        }
                                        else
                                        {
                                            fbhaftzeitminderung = dbPlayer.jailtimeReducing[0];

                                            dbPlayer.JailTime[0] -= dbPlayer.jailtimeReducing[0];
                                            dbPlayer.jailtimeReducing[0] -= 0;
                                        }
                                    }
                                }


                                dbPlayer.GiveMoney(money);
                                dbPlayer.SendNewNotification($"Für die Wäsche haben Sie ${money} sowie eine Haftzeitminderung von {fbhaftzeitminderung} bekommen.");

                                dbPlayer.Container.RemoveItemAll(item.Id);
                            }
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool HasDoorAccess(DbPlayer dbPlayer, Door door)
        {
            // Key Item, all in SG Zellentüren
            if (door.Model == 631614199 && door.Group == 1 && dbPlayer.Container.GetItemAmount(LaundryKeyItemId) > 0) return true;

            return false;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandsgjob(Player player, string commandParams)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!Configurations.Configuration.Instance.DevMode) return;

            if (!Int32.TryParse(commandParams, out int group))
            {
                return;
            }

            if(StaatsgefaengnisModule.Instance.SGJobPlayers.ContainsKey(dbPlayer))
            {
                StaatsgefaengnisModule.Instance.SGJobPlayers[dbPlayer] = (SGJobs)group;
            }
            StaatsgefaengnisModule.Instance.SGJobPlayers.Add(dbPlayer, (SGJobs)group);

            return;
        }
    }

    public class LaundryModuleEvents : Script
    {
        [RemoteEvent]
        public async void LaundryToRopeConfirm(Player p_Player, string pb_map, string none, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract()) return;

            if (dbPlayer.Player.Position.DistanceTo(LaundryModule.LaundryToRopePoint) < 1.0f)
            {
                if (dbPlayer.Container.GetItemAmount(LaundryModule.LaundryBagItemId) <= 0)
                {
                    dbPlayer.SendNewNotification("Du hast keinen Wäschekorb mit dreckiger Wäsche!");
                    return;
                }

                Item item = dbPlayer.Container.GetItemById((int)LaundryModule.LaundryBagItemId);

                if (item.Data == null || !item.Data.ContainsKey("laundry"))
                {
                    dbPlayer.SendNewNotification("Du hast keinen Wäschekorb mit Wäsche!");
                    return;
                }
                else
                {
                    int amount = item.Data["laundry"];

                    if (amount < 2)
                    {
                        dbPlayer.SendNewNotification($"Du brauchst mindestens 2 Bettlaken um ein Seil herzustellen!");
                        return;
                    }

                    if (!dbPlayer.Container.CanInventoryItemAdded(17))
                    {
                        dbPlayer.SendNewNotification($"Ich kann so viel nicht tragen!");
                        return;
                    }

                    if (LaundryModule.Instance.WashingPlayers.ContainsKey(dbPlayer.Id))
                    {
                        return;
                    }
                    else
                    {
                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);

                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                        Chat.Chats.sendProgressBar(dbPlayer, 10000);
                        await Task.Delay(10000);

                        item.Data["laundry"] -= 2;

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");

                        dbPlayer.StopAnimation();

                        dbPlayer.SendNewNotification($"Sie haben 2 Bettlaken zu einem Seil verarbeitet!");
                        dbPlayer.Container.AddItem(17);

                        return;
                    }
                }
            }
        }
    }

    public class LaundryPosition : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }

        public float Heading { get; set; }

        public DateTime LastInteracted { get; set; }

        public LaundryPosition(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            LastInteracted = DateTime.Now;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}