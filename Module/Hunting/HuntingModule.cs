using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Animal;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Hunting
{
    public class HuntingAreaSpawn
    {
        public uint Id { get; set; }

        public uint HuntingZoneId { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }

        public HuntingAreaSpawn(MySqlDataReader reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            HuntingZoneId = reader.GetUInt32("huntingzone_id");
        }
    }

    public class HuntingAnimal
    {
        public Vector3 spawnPos { get; set; }
        public bool IsInAnim { get; set; }

        public bool IsItemGet { get; set; }

        public HuntingAnimal(Vector3 SpawnPos)
        {
            spawnPos = SpawnPos;
            IsInAnim = false;
            IsItemGet = false;
        }
    }

    public class HuntingArea : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
        public List<uint> Peds { get; set; }
        public int MaxAnimals { get; set; }
        public List<HuntingAreaSpawn> spawnPos { get; set; }

        public Dictionary<Ped, HuntingAnimal> spawnedPeds { get; set; }

        public HuntingArea(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Radius = reader.GetFloat("radius");
            MaxAnimals = reader.GetInt32("max_animals");
            spawnPos = new List<HuntingAreaSpawn>();

            if (Configuration.Instance.DevMode)
            {
                Spawners.Blips.Create(Position, "", 141, 1.0f);
            }

            ColShape jagdShape = Spawners.ColShapes.Create(Position, Radius, 0);
            jagdShape.SetData<uint>("huntingzone", Id);

            spawnedPeds = new Dictionary<Ped, HuntingAnimal>();
            Peds = new List<uint>();

            var pedsString = reader.GetString("peds");

            if (!string.IsNullOrEmpty(pedsString))
            {
                var splittedPeds = pedsString.Split(',');
                foreach (var pedIdStr in splittedPeds)
                {
                    if (!uint.TryParse(pedIdStr, out var pedId) || pedId == 0) continue;
                    Peds.Add(pedId);
                }
            }

        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class HuntingModule : SqlModule<HuntingModule, HuntingArea, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `huntingzone`;";
        }

        protected override void OnLoaded()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM huntingzone_spawns;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            HuntingArea huntingArea = Instance.Get(reader.GetUInt32("huntingzone_id"));
                            if(huntingArea != null)
                            {
                                huntingArea.spawnPos.Add(new HuntingAreaSpawn(reader));
                            }
                        }
                    }
                }
                conn.Close();
            }
        }

        public override void OnTenSecUpdate()
        {
            if (!ServerFeatures.IsActive("hunting"))
                return;

            try
            {
                Random rnd = new Random();
                NAPI.Task.Run(async () =>
                {
                    foreach (HuntingArea huntingArea in Instance.GetAll().Values)
                    {
                        foreach (KeyValuePair<Ped, HuntingAnimal> kvp in huntingArea.spawnedPeds.ToList())
                        {
                            await NAPI.Task.WaitForMainThread(0);
                            if (NAPI.Entity.DoesEntityExist(kvp.Key) && kvp.Key != null)
                            {

                                Animal.AnimalData animalData = Animal.AnimalDataModule.Instance.GetAll().Values.Where(a => a.UIntHash == kvp.Key.Model).FirstOrDefault();
                                if (animalData == null) continue;

                                Vector3 entityPos = NAPI.Entity.GetEntityPosition(kvp.Key);
                                DbPlayer fallbackController = Players.Players.Instance.GetPlayersInRange(entityPos, 80).FirstOrDefault();

                                if (kvp.Key.Controller == null || kvp.Key.Controller.Position.DistanceTo(entityPos) > huntingArea.Radius)
                                {
                                    if (fallbackController != null && fallbackController.IsValid())
                                    {
                                        kvp.Key.Controller = fallbackController.Player;
                                        await NAPI.Task.WaitForMainThread(1000);
                                    }
                                    else continue;
                                }

                                DbPlayer controller = kvp.Key.Controller.GetPlayer();
                                if (controller == null || !controller.IsValid()) continue;

                                Vector3 runTo = kvp.Value.spawnPos.Add(new Vector3(rnd.Next(-(int)huntingArea.Radius / 2, (int)huntingArea.Radius) / 2, rnd.Next(-(int)huntingArea.Radius / 2, (int)huntingArea.Radius) / 2, 0));

                            // handle attacks
                            if (animalData.CanAttack) // mountainlion
                            {
                                    DbPlayer target = Players.Players.Instance.GetPlayersInRange(entityPos, 15).ToList().Where(p => p != null && p.IsValid() && !p.isInjured()).FirstOrDefault();
                                    if (target != null && target.IsValid() && !target.isInjured())
                                    {
                                        kvp.Key.Controller.TriggerNewClient("animal_attack", kvp.Key, target.Player);
                                        continue;
                                    }
                                }

                            // handle random anims
                            if (rnd.Next(1, 10) <= 3)
                                {
                                    switch (kvp.Key.Model)
                                    {
                                        case (uint)PedHash.Boar: // wildschwein
                                        if (kvp.Value.IsInAnim) continue;
                                            kvp.Key.Controller.TriggerNewClient("animal_playanim", kvp.Key, "creatures@boar@amb@world_boar_grazing@idle_a", "idle_c", -1, 1);
                                            kvp.Value.IsInAnim = true;
                                            continue;
                                        case (uint)PedHash.Coyote: // coyote
                                        if (kvp.Value.IsInAnim) continue;
                                            kvp.Key.Controller.TriggerNewClient("animal_playanim", kvp.Key, "creatures@coyote@amb@world_coyote_rest@base", "base", -1, 1);
                                            kvp.Value.IsInAnim = true;
                                            continue;
                                        case (uint)PedHash.Deer: // deer
                                        if (kvp.Value.IsInAnim) continue;
                                            kvp.Key.Controller.TriggerNewClient("animal_playanim", kvp.Key, "amb@lo_res_idles@", "creatures_world_deer_grazing_lo_res_base", -1, 1);
                                            kvp.Value.IsInAnim = true;
                                            continue;
                                    }
                                }

                                if (kvp.Value.IsInAnim) kvp.Value.IsInAnim = false;
                                kvp.Key.Controller.TriggerNewClient("animal_gotoCoord", kvp.Key, runTo.X, runTo.Y, runTo.Z, 2);
                            }
                        }
                    }

                });
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E) return false;

            try
            {
                NAPI.Task.Run(async () =>
                {
                    foreach (HuntingArea huntingArea in Instance.GetAll().Values)
                    {
                        foreach (KeyValuePair<Ped, HuntingAnimal> kvp in huntingArea.spawnedPeds.ToList())
                        {
                            await NAPI.Task.WaitForMainThread(0);
                            if (dbPlayer.Player.Position.DistanceTo(NAPI.Entity.GetEntityPosition(kvp.Key)) < 2.0)
                            {
                                // Checks if the animal is dead or dying.
                                dbPlayer.Player.TriggerNewClient("animal_checkDeath", kvp.Key, "huntingAnimalState");
                                return;
                            }
                        }
                    }
                });
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
            }
            return false;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!ServerFeatures.IsActive("hunting"))
                return false;

            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.isInjured()) return false;

            if(colShapeState == ColShapeState.Enter)
            {
                if(colShape.HasData("huntingzone"))
                {
                    HuntingArea huntingArea = Instance.Get(colShape.GetData<uint>("huntingzone"));
                    if (huntingArea == null) return false;

                    if (huntingArea.spawnedPeds.Count >= huntingArea.MaxAnimals) return false;

                    Random rnd = new Random();

                    int chance = rnd.Next(0, 100);
                    if (chance >= 60) return false;

                    int r = rnd.Next(huntingArea.spawnPos.Count);
                    HuntingAreaSpawn huntingAreaSpawn = huntingArea.spawnPos[r];
                    if (huntingAreaSpawn == null) return false;

                    Vector3 spawnPos = huntingAreaSpawn.Position;
                    Vector3 runTo = huntingArea.Position.Add(new Vector3(rnd.Next(-(int)huntingArea.Radius, (int)huntingArea.Radius), rnd.Next(-(int)huntingArea.Radius, (int)huntingArea.Radius), 0));


                    int rpedidx = rnd.Next(huntingArea.Peds.Count);
                    uint PedSpawnId = huntingArea.Peds[rpedidx];

                    // Animal Textures?
                    Animal.AnimalData animalData = Animal.AnimalDataModule.Instance.GetAll().Values.Where(a => a.UIntHash == PedSpawnId).FirstOrDefault();
                    if (animalData == null) return false;

                    NAPI.Task.Run(async () =>
                    {
                        Ped Ped = NAPI.Ped.CreatePed(PedSpawnId, spawnPos, 0.0f, true, false, false, false, 0);

                        while (Ped == null)
                        {
                            await NAPI.Task.WaitForMainThread(50);
                        }


                        if (!huntingArea.spawnedPeds.ContainsKey(Ped))
                        {
                            huntingArea.spawnedPeds.Add(Ped, new HuntingAnimal(spawnPos));
                        }

                        Ped.Controller = dbPlayer.Player;

                        
                        if(animalData != null && animalData.Textures > 0)
                        {
                            int customTexture = rnd.Next(0, animalData.Textures + 1);

                            foreach (DbPlayer xPlayer in Players.Players.Instance.GetPlayersInRange(spawnPos, 350))
                            {
                                if (xPlayer == null || !xPlayer.IsValid()) continue;
                                xPlayer.Player.TriggerNewClient("animal_cloth", Ped, 0, 0, customTexture);
                            }

                            if (Animal.AnimalVariationModule.Instance.VariationAnimals.ContainsKey(Ped))
                            {
                                Animal.AnimalVariationModule.Instance.VariationAnimals[Ped] = customTexture;
                            }
                            else Animal.AnimalVariationModule.Instance.VariationAnimals.Add(Ped, customTexture);

                        }

                        await NAPI.Task.WaitForMainThread(500);

                        if (animalData.CanAttack)
                        {
                            Ped.Controller.TriggerNewClient("animal_setarmour", Ped, 100);
                            await NAPI.Task.WaitForMainThread(500);
                        }

                        if (Ped.Controller != null)
                        {
                            DbPlayer dbController = Ped.Controller.GetPlayer();
                            if (dbController != null && dbController.IsValid())
                            {
                                Ped.Controller.TriggerNewClient("animal_gotoCoord", Ped, runTo.X, runTo.Y, runTo.Z, 2);
                            }
                        }
                    });

                }
            }

            return false;
        }
    }

    public class HuntingModuleEvents : Script
    {
        [RemoteEvent]
        public async void huntingAnimalState(Player player, bool state, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            Ped currentAnimal = null;
            HuntingAnimal currHuntingAnimal = null;
            HuntingArea currentArea = null;

            try
            {
                foreach (HuntingArea huntingArea in HuntingModule.Instance.GetAll().Values)
                {
                    foreach (KeyValuePair<Ped, HuntingAnimal> kvp in huntingArea.spawnedPeds.ToList())
                    {
                        await NAPI.Task.WaitForMainThread(0);
                        if (dbPlayer.Player.Position.DistanceTo(NAPI.Entity.GetEntityPosition(kvp.Key)) < 2.0)
                        {
                            currentArea = huntingArea;
                            currentAnimal = kvp.Key;
                            currHuntingAnimal = kvp.Value;
                            break;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
            }

            if (currentAnimal != null && currHuntingAnimal != null && !currHuntingAnimal.IsItemGet && currentAnimal.Exists && state)
            {
                // Deaktiviert bis Freigabe
                return; 

                Animal.AnimalData animalData = AnimalDataModule.Instance.GetAll().Values.FirstOrDefault(a => a.UIntHash == currentAnimal.Model);
                if (animalData == null) return;

                if (dbPlayer.Player.CurrentWeapon != WeaponHash.Knife && 
                    dbPlayer.Player.CurrentWeapon != WeaponHash.Dagger && 
                    dbPlayer.Player.CurrentWeapon != WeaponHash.Switchblade &&
                    dbPlayer.Player.CurrentWeapon != WeaponHash.Machete)
                {
                    dbPlayer.SendNewNotification("Du benötigst hierfür ein Messer!");
                    return;
                }

                currHuntingAnimal.IsItemGet = true;

                if (animalData.HuntingItemId > 0)
                {

                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl),
                                Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1], 8, true);
                    dbPlayer.Player.TriggerNewClient("freezePlayer", true);

                    Chats.sendProgressBar(dbPlayer, 5000);
                    await NAPI.Task.WaitForMainThread(5000);

                    // Stop liege Animation
                    dbPlayer.StopAnimation();

                    int amount = new Random().Next(1, 5);
                    ItemModel model = ItemModelModule.Instance.Get(animalData.HuntingItemId);
                    dbPlayer.Container.AddItem(model.Id, amount);
                    dbPlayer.SendNewNotification($"Du hast {model.Name} bekommen!");
                    dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                }
                if (currentArea.spawnedPeds.ContainsKey(currentAnimal))
                {
                    currentArea.spawnedPeds.Remove(currentAnimal);
                }
                if (currentAnimal != null && currentAnimal.Exists)
                {
                    currentAnimal.Delete();
                }
            }

        }

    }
}
