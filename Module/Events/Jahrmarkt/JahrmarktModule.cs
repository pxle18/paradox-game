using Google.Protobuf.WellKnownTypes;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.Events.Jahrmarkt.Scooter;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.PlayerDataCustom;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Events.Jahrmarkt
{
    public class JahrmarktModule : Module<JahrmarktModule>
    {
        public static bool isActive = false;

        public List<SxVehicle> Jetskies = new List<SxVehicle>();

        public static Vector3 RentMenuNpcPos = new Vector3(-1607.97, -1132.21, 2.14087);

        public static Vector3 JetskiSpawnPos = new Vector3(-1630.25, -1163.02, 0.101744);
        public static float JetskiSpawnRot = 140.165f;

        public static int MaxActiveJetskies = 10;

        public static Vector3 JetskiCOlShape120Position = new Vector3(-1707.18, -1303.67, 0.0418908);
        public static Vector3 JetskiCOlShape90Position = new Vector3(-1707.18, -1303.67, 0.0418908);

        public ColShape JetSkiColShape120 = Spawners.ColShapes.Create(JetskiCOlShape120Position, 240.0f, 0);
        public ColShape JetskiCOlShape90 = Spawners.ColShapes.Create(JetskiCOlShape90Position, 180.0f, 0);

        public static uint JetSkiVehicleDataId = 7;

        public static Vector3 LabyMenuPos = new Vector3(-1649.58, -993.79, 13.0174);
        public static float LabyMenuRot = 226.325f;

        public static Vector3 LabyStart = new Vector3(-1582.34, - 1051.19, 13.0177);
        public static float LabyStartRot = 319.981f;

        public static Vector3 LabyEnd = new Vector3(-1580.43, -999.077, 13.0178);

        public static uint LabyDimension = 20;

        public List<Vector3> entenAngelPositions = new List<Vector3>();

        public static Vector3 entenSellPosition = new Vector3(-1714.85, -1125.26, 13.1553);

        public static int MaxEnten = 100;

        public static int WahrsagerPrice = 5000;

        public static Vector3 WahrsagerPos = new Vector3(-1618.07, -1042.63, 5.79074);

        public Dictionary<uint, string> WahrsagerOutputs = new Dictionary<uint, string>();

        public List<PedHash> WahrSagerSkins = new List<PedHash>();

        protected override bool OnLoad()
        {
            if (!JahrmarktModule.isActive) base.OnLoad();

            WahrSagerSkins = new List<PedHash>();
            WahrSagerSkins.Add(PedHash.Rabbit);
            WahrSagerSkins.Add(PedHash.Rat);
            WahrSagerSkins.Add(PedHash.Cat);
            WahrSagerSkins.Add(PedHash.Coyote);
            WahrSagerSkins.Add(PedHash.Husky);
            WahrSagerSkins.Add(PedHash.Chop);
            WahrSagerSkins.Add(PedHash.Hen);
            WahrSagerSkins.Add(PedHash.Cow);
            WahrSagerSkins.Add(PedHash.Deer);


            WahrsagerOutputs = new Dictionary<uint, string>();

            WahrsagerOutputs.Add(1, "Es wird demnächst in deinem leben Blitzen, achtung das Ticket wird teuer!");
            WahrsagerOutputs.Add(2, "Du wirst eine große Enttäuschung erleben!");
            WahrsagerOutputs.Add(3, "Ich hätte das Geld besser für Cunningham Cracker ausgegeben.. als ob du hier deine Zukunft für 5.000$ bekommst..");
            WahrsagerOutputs.Add(4, "Wer den Kopf steckt in den Sand, wird am Hinterteil erkannt!");
            WahrsagerOutputs.Add(5, "Die Vorhersage für die heutige Nacht: Mit erhöhter Dunkelheit ist zu rechnen!");
            WahrsagerOutputs.Add(6, "Ich habe heute leider kein Foto für dich!");
            WahrsagerOutputs.Add(7, "Es gibt zwei Worte die dir im Leben viele Türen öffnen können. Ziehen und Drücken.");
            WahrsagerOutputs.Add(8, "Sorry Leute ich bin leider kein Dealer.");
            WahrsagerOutputs.Add(9, "Ich habe gesehen wie du kleine Igel in die Pfütze schupst!");
            WahrsagerOutputs.Add(10, "Hattest du nicht einen Hund dabei?");
            WahrsagerOutputs.Add(11, "Beim Einreiseamt hast du dich aber auch durchgemogelt, oder?");
            WahrsagerOutputs.Add(12, "Ich kann dir dein Wahres ich offenbaren, möchtest du diesen Trank nehmen?");

            entenAngelPositions = new List<Vector3>();
            entenAngelPositions.Add(new Vector3(-1709.64, -1117.72, 13.1531));
            entenAngelPositions.Add(new Vector3(-1710.82, -1118.88, 13.1536));
            entenAngelPositions.Add(new Vector3(-1712.01, -1120.02, 13.1539));
            entenAngelPositions.Add(new Vector3(-1713.13, -1121.07, 13.154));
            entenAngelPositions.Add(new Vector3(-1713.74, -1121.72, 13.1542));

            Jetskies = new List<SxVehicle>();
            JetSkiColShape120 = Spawners.ColShapes.Create(JetskiCOlShape120Position, 240.0f, 0);
            JetSkiColShape120.SetData("jahrmarktjetskie", true);

            JetskiCOlShape90 = Spawners.ColShapes.Create(JetskiCOlShape90Position, 180.0f, 0);
            JetskiCOlShape90.SetData("jahrmarktjetskiewarning", true);

            ColShape tmpColShape = Spawners.ColShapes.Create(LabyStart, 4.0f, LabyDimension);
            tmpColShape.SetData("labyan", true);

            tmpColShape = Spawners.ColShapes.Create(LabyEnd, 3.0f, LabyDimension);
            tmpColShape.SetData("labyen", true);

            // Jetski NPC
            new Npc(PedHash.Surfer01AMY, RentMenuNpcPos, 318.224f, 0);

            return base.OnLoad();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async void Commandrespawnscooter(Player player)
        {
            if (!JahrmarktModule.isActive) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanAccessMethod()) return;

            if (!ServerFeatures.IsActive("jahrmarkt-scooter")) return;

            foreach (KeyValuePair<uint, Scooter.Scooter> kvp in ScooterModule.Instance.Scooters.ToList())
            {
                if (kvp.Value == null) continue;
                if (kvp.Value.sxVehicle != null && kvp.Value.sxVehicle.IsValid())
                {
                    VehicleHandler.Instance.DeleteVehicle(kvp.Value.sxVehicle, false);
                }
                ScooterModule.Instance.Scooters[kvp.Key].sxVehicle = VehicleHandler.Instance.CreateServerVehicle(866, false, ScooterModule.Instance.Scooters[kvp.Key].SpawnPos, ScooterModule.Instance.Scooters[kvp.Key].SpawnRot, -1, -1, 0, false, false, true);

                while (ScooterModule.Instance.Scooters[kvp.Key].sxVehicle.entity == null)
                {
                    await NAPI.Task.WaitForMainThread(100);
                }

                ScooterModule.Instance.Scooters[kvp.Key].sxVehicle.DynamicMotorMultiplier = 35;
            }
            return;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!JahrmarktModule.isActive) return false;
            if (dbPlayer != null && dbPlayer.IsValid())
            {
                if(colShape.HasData("jahrmarktjetskie") && colShapeState == ColShapeState.Exit)
                {
                    if (!ServerFeatures.IsActive("jahrmarkt-jetski")) return false;
                    RemovePlayerJetskie(dbPlayer);
                }
                if (colShape.HasData("jahrmarktjetskiewarning") && colShapeState == ColShapeState.Exit)
                {
                    if (!ServerFeatures.IsActive("jahrmarkt-jetski")) return false;
                    SxVehicle jetskie = Jetskies.ToList().Where(j => j.ownerId == dbPlayer.Id).FirstOrDefault();
                    if (jetskie != null)
                    {
                        dbPlayer.SendNewNotification("Achtung: Sie verlassen den Jetski Fahrbereich! (Bei weiterer Entfernung wird ihr Jetski eingeparkt!)");
                    }
                }

                // Labyrinth
                if(colShape.HasData("labyan") && colShapeState == ColShapeState.Enter)
                {
                    dbPlayer.SetData("inLaby", true);
                    dbPlayer.SendNewNotification($"Willkommen im Labyrinth, finde den Ausgang!");
                }
                if(colShape.HasData("labyen") && dbPlayer.HasData("inLaby") && colShapeState == ColShapeState.Enter)
                {
                    int points = new Random().Next(18, 26);
                    dbPlayer.SendNewNotification($"Du hast das Labyrinth erfolgreich abgeschlossen und erhaltest {points} Jahrmarkt-Punkte!");

                    if(!dbPlayer.GiveCWS(CWSTypes.Jahrmarkt, points))
                    {
                        dbPlayer.SendNewNotification("Sie haben bereits das heutige Limit für Jahrmarktpunkte erreicht!");
                    }


                    dbPlayer.Player.TriggerNewClient("setBlackout", false);

                    dbPlayer.ResetData("inLaby");

                    dbPlayer.Player.SetPosition(LabyMenuPos);
                    dbPlayer.Player.SetRotation(LabyMenuRot);

                    dbPlayer.SetDimension(0);
                    dbPlayer.Dimension[0] = 0;
                }
            }

            return false;
        }

        public void RemovePlayerJetskie(DbPlayer dbPlayer)
        {
            SxVehicle jetskie = Jetskies.ToList().Where(j => j.ownerId == dbPlayer.Id).FirstOrDefault();
            if(jetskie != null)
            {
                Jetskies.Remove(jetskie);
                VehicleHandler.Instance.DeleteVehicle(jetskie);
            }
        }

        public void SpawnPlayerJetskie(DbPlayer dbPlayer)
        {
            if (!ServerFeatures.IsActive("jahrmarkt-jetski")) return;

            NAPI.Task.Run(async () =>
            {
                var sxVehicle = VehicleHandler.Instance.CreateServerVehicle(JetSkiVehicleDataId, false,
                        JetskiSpawnPos, JetskiSpawnRot, Main.rndColor(),
                        Main.rndColor(), 0, true, true, false, 0, dbPlayer.GetName(), 0, 999, dbPlayer.Id);
                while (sxVehicle.entity == null)
                {
                    await NAPI.Task.WaitForMainThread(100);
                }
                Jetskies.Add(sxVehicle);
            });
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (!JahrmarktModule.isActive) return false;
            if (dbPlayer != null && dbPlayer.IsValid() && key == Key.E)
            {
                if(dbPlayer.Player.Position.DistanceTo(RentMenuNpcPos) < 1.0f)
                {
                    if (!ServerFeatures.IsActive("jahrmarkt-jetski")) return false;

                    if (Crime.CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes) > 0)
                    {
                        dbPlayer.SendNewNotification("Gesucht können Sie keinen Jetski mieten!");
                        return true;
                    }
                    ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"Jahrmarkt Jetski Vermietung", $"Wollen Sie einen Jetski für 500$ mieten?", "JetskiRentConfirm", "", ""));
                    return true;
                }
                else if (dbPlayer.Player.Position.DistanceTo(LabyMenuPos) < 1.0f)
                {
                    if (Crime.CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes) > 0)
                    {
                        dbPlayer.SendNewNotification("Gesucht können Sie hier nicht teilnehmen!");
                        return true;
                    }
                    ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"Das GlasLabyrinth", $"Wollen Sie wirklich hier rein? Der Einzige Weg raus ist der Ausgang, kosten: 500$", "labyConfirm", "", ""));
                    return true;
                }

                if(entenAngelPositions.Where(eap => eap.DistanceTo(dbPlayer.Player.Position) < 1.5f).Count() > 0)
                {
                    if(!dbPlayer.Container.CanInventoryItemAdded(1127))
                    {
                        dbPlayer.SendNewNotification("Ihre Tasche hat nicht genügend platz!");
                        return true;
                    }

                    if (!dbPlayer.CanInteract()) return true;


                    try
                    { 
                        string CustomKey = "enten";
                        if (!dbPlayer.PlayerDataCustom.ContainsKey(CustomKey))
                        {
                            dbPlayer.PlayerDataCustom.Add(CustomKey, new DbPlayerDataCustom(0,dbPlayer.Id,"enten","0",DateTime.Now));
                            dbPlayer.PlayerDataCustom[CustomKey].CreateKey();
                        }

                        if(dbPlayer.PlayerDataCustom.TryGetValue(CustomKey, out var value))
                        {
                            if(value.ParseInt() >= MaxEnten)
                            {
                                dbPlayer.SendNewNotification("Genug Enten für heute");
                                return true;
                            }
                            else
                            {
                                value.UpdateValue(value.ParseInt() + 1);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }

                    
                    NAPI.Task.Run(async () =>
                    {
                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_stand_fishing@idle_a", "idle_a");
                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetCannotInteract(true);
                        Chats.sendProgressBar(dbPlayer, 30000);
                        await NAPI.Task.WaitForMainThread(30000);

                        dbPlayer.SetCannotInteract(false);
                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);


                        uint itemId = Convert.ToUInt32(new Random().Next(1124, 1128));
                        Attachments.AttachmentModule.Instance.ClearAllAttachments(dbPlayer);
                        dbPlayer.StopAnimation();
                        dbPlayer.Container.AddItem(itemId, 1);
                        dbPlayer.SendNewNotification($"Du hast eine {ItemModelModule.Instance.Get(itemId).Name} erhalten!");

                    });
                    return true;
                }

                if(dbPlayer.Player.Position.DistanceTo(WahrsagerPos) < 1.5f)
                {
                    ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"Wahrsager", $"Willst du wirklich deine Weisheit hören? Manche vertragen sie nicht..., kosten: 5000$", "wahrsager1", "", ""));
                    return true;
                }

                if (dbPlayer.Player.Position.DistanceTo(entenSellPosition) < 1.5f)
                {
                    uint itemId = 1125;
                    int multiplier = 16;
                    if(dbPlayer.Container.GetItemAmount(itemId) > 0)
                    {
                        int redamount = dbPlayer.Container.GetItemAmount(itemId);
                        dbPlayer.Container.RemoveItemAll(itemId);

                        dbPlayer.SendNewNotification($"Du hast für {redamount} {ItemModelModule.Instance.Get(itemId).Name} {(redamount * multiplier)} Jahrmarkt Punkte erhalten!");
                        if (!dbPlayer.GiveCWS(CWSTypes.Jahrmarkt, (redamount * multiplier)))
                        {
                            dbPlayer.SendNewNotification("Sie haben bereits das heutige Limit für Jahrmarktpunkte erreicht!");
                        }
                    }

                    itemId = 1126;
                    multiplier = 4;
                    if (dbPlayer.Container.GetItemAmount(itemId) > 0)
                    {
                        int redamount = dbPlayer.Container.GetItemAmount(itemId);
                        dbPlayer.Container.RemoveItemAll(itemId);

                        dbPlayer.SendNewNotification($"Du hast für {redamount} {ItemModelModule.Instance.Get(itemId).Name} {(redamount * multiplier)} Jahrmarkt Punkte erhalten!");
                        if (!dbPlayer.GiveCWS(CWSTypes.Jahrmarkt, (redamount * multiplier)))
                        {
                            dbPlayer.SendNewNotification("Sie haben bereits das heutige Limit für Jahrmarktpunkte erreicht!");
                        }
                    }

                    itemId = 1127;
                    multiplier = 8;
                    if (dbPlayer.Container.GetItemAmount(itemId) > 0)
                    {
                        int redamount = dbPlayer.Container.GetItemAmount(itemId);
                        dbPlayer.Container.RemoveItemAll(itemId);

                        dbPlayer.SendNewNotification($"Du hast für {redamount} {ItemModelModule.Instance.Get(itemId).Name} {(redamount * multiplier)} Jahrmarkt Punkte erhalten!");
                        if (!dbPlayer.GiveCWS(CWSTypes.Jahrmarkt, (redamount * multiplier)))
                        {
                            dbPlayer.SendNewNotification("Sie haben bereits das heutige Limit für Jahrmarktpunkte erreicht!");
                        }
                    }

                    itemId = 1124;
                    multiplier = 20;
                    if (dbPlayer.Container.GetItemAmount(itemId) > 0)
                    {
                        int redamount = dbPlayer.Container.GetItemAmount(itemId);
                        dbPlayer.Container.RemoveItemAll(itemId);

                        dbPlayer.SendNewNotification($"Du hast für {redamount} {ItemModelModule.Instance.Get(itemId).Name} {(redamount * multiplier)} Jahrmarkt Punkte erhalten!");
                        if (!dbPlayer.GiveCWS(CWSTypes.Jahrmarkt, (redamount * multiplier)))
                        {
                            dbPlayer.SendNewNotification("Sie haben bereits das heutige Limit für Jahrmarktpunkte erreicht!");
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public override void OnTenSecUpdate()
        {
            if (!JahrmarktModule.isActive) return;
            if (!ServerFeatures.IsActive("jahrmarkt-jetski")) return;

            foreach (SxVehicle sxVehicle in Jetskies.ToList())
            {
                if (sxVehicle != null && sxVehicle.entity != null)
                {
                    foreach (DbPlayer dbPlayer in sxVehicle.GetOccupants().GetLegacyDictionary().Values)
                    {
                        if (dbPlayer.Player.VehicleSeat == 0 && sxVehicle.GetSpeed() > 10)
                        {
                            dbPlayer.GiveCWS(CWSTypes.Jahrmarkt, 4);
                        }
                    }
                }
            }
        }

        public override void OnMinuteUpdate()
        {
            if (!JahrmarktModule.isActive) return;
            if (!ServerFeatures.IsActive("jahrmarkt-jetski")) return;

            foreach (SxVehicle sxVehicle in Jetskies.ToList())
            {
                if (sxVehicle != null && sxVehicle.entity != null)
                {
                    if(sxVehicle.entity.IsSeatFree(0))
                    {
                        if (sxVehicle.entity.HasData("racingLeaveCheck"))
                        {
                            Jetskies.Remove(sxVehicle);
                            VehicleHandler.Instance.DeleteVehicle(sxVehicle, false);
                        }
                        else sxVehicle.entity.SetData("racingLeaveCheck", 1);
                    }
                    else
                    {
                        if (sxVehicle.entity.HasData("racingLeaveCheck"))
                        {
                            sxVehicle.entity.ResetData("racingLeaveCheck");
                        }
                    }
                }
            }
        }
    }

    public class JahrmarktEventResult : Script
    {
        [RemoteEvent]
        public void JetskiRentConfirm(Player p_Player, string pb_map, string none, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            if (!JahrmarktModule.isActive) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();


            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            SxVehicle jetskie = JahrmarktModule.Instance.Jetskies.ToList().Where(j => j.ownerId == dbPlayer.Id).FirstOrDefault();
            if (jetskie != null)
            {
                dbPlayer.SendNewNotification("Sie haben bereits ein Jetski gemietet!");
                return;
            }

            if(JahrmarktModule.Instance.Jetskies.Count() >= JahrmarktModule.MaxActiveJetskies)
            {
                dbPlayer.SendNewNotification("Ich hab kein Jetski mehr da, warte bis jemand eins zurückbringt!");
                return;
            }

            if (!dbPlayer.TakeMoney(500))
            {
                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(1000));
                return;
            }

            JahrmarktModule.Instance.SpawnPlayerJetskie(dbPlayer);
            dbPlayer.SendNewNotification("Ihr Jetski steht nun für Sie bereit!");
        }

        [RemoteEvent]
        public void labyConfirm(Player p_Player, string pb_map, string none, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            if (!JahrmarktModule.isActive) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();


            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (!dbPlayer.TakeMoney(500))
            {
                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(500));
                return;
            }

            dbPlayer.SetDimension(JahrmarktModule.LabyDimension);
            dbPlayer.Dimension[0] = JahrmarktModule.LabyDimension;

            dbPlayer.Player.SetPosition(JahrmarktModule.LabyStart);
            dbPlayer.Player.SetRotation(JahrmarktModule.LabyStartRot);

            dbPlayer.Player.TriggerNewClient("setBlackout", true);
        }

        [RemoteEvent]
        public void wahrsager1(Player p_Player, string pb_map, string none, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            if (!JahrmarktModule.isActive) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();


            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            if (!dbPlayer.CanInteract()) return;

            if (!dbPlayer.TakeMoney(5000))
            {
                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(5000));
                return;
            }

            Random rand = new Random();

            var output = JahrmarktModule.Instance.WahrsagerOutputs.ElementAt(rand.Next(0, JahrmarktModule.Instance.WahrsagerOutputs.Count));

            if(output.Key == 12)
            {
                ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"Wahrsager", $"{output.Value}", "wahrsager2", "", ""));
            }
            else
            {
                ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"Wahrsager", $"{output.Value}", "nothing", "", ""));
            }
        }

        [RemoteEvent]
        public void wahrsager2(Player p_Player, string pb_map, string none, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            if (!JahrmarktModule.isActive) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();


            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            Random rand = new Random();

            int r = rand.Next(JahrmarktModule.Instance.WahrSagerSkins.Count);

            PedHash skin = JahrmarktModule.Instance.WahrSagerSkins[r];

            Task.Run(async () =>
            {
                dbPlayer.SetCannotInteract(true);
                dbPlayer.SetSkin(skin);
                await NAPI.Task.WaitForMainThread(60000);
                dbPlayer.ApplyCharacter();
                dbPlayer.SetCannotInteract(false);
            });
        }
    }
}
