using GTANetworkAPI;
using System;
using System.Linq;
using VMP_CNR.Handler;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles.Data;

namespace VMP_CNR.Module.Animal
{
    public class AnimalModule : SqlModule<AnimalModule, Animal, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(AnimalVariationModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `animals`;";
        }

        protected override void OnLoaded()
        {
            MenuManager.Instance.AddBuilder(new AnimalAssignMenuBuilder()); // liegt im House->Menu
        }

        public override void OnMinuteUpdate()
        {
            return;

            foreach(Animal animal in Instance.GetAll().Values.ToList().Where(a => a.Spawned))
            {
                if(animal.Ped == null || !animal.Ped.Exists)
                {
                    animal.Spawned = false;
                }
                else
                {

                    if(animal.Ped.Controller == null)
                    {
                        animal.Ped.Delete();
                        animal.Spawned = false;
                    }
                    else
                    {
                        DbPlayer controllerPlayer = animal.Ped.Controller.GetPlayer();
                        if(controllerPlayer == null || !controllerPlayer.IsValid())
                        {
                            animal.Ped.Delete();
                            animal.Spawned = false;
                        }
                        else
                        {
                            Vector3 coords = animal.Ped.Position;
                            if (coords.DistanceTo(controllerPlayer.Player.Position) > 300)
                            {
                                animal.Ped.Delete();
                                animal.Spawned = false;
                            }
                        }
                    }
                }
            }
        }

        public override void OnTenSecUpdate()
        {
            return;

            NAPI.Task.Run(async () =>
            {
                foreach(Animal animal in Instance.GetAll().Values.ToList().Where(a => a.Spawned))
                {
                    if(animal.Ped != null && animal.Ped.Controller != null)
                    {
                        DbPlayer Controller = animal.Ped.Controller.GetPlayer();

                        if (Controller != null && Controller.IsValid())
                        {
                            await NAPI.Task.WaitForMainThread(0);
                            if (animal.Ped.Position.DistanceTo(animal.Ped.Controller.Position) < 200.0f) continue;
                        }
                        animal.Ped.Controller = null;
                        animal.Ped.Delete();
                    }
                }
            });
        }

        public override bool OnKeyPressed(DbPlayer l_DbPlayer, Key key)
        {
            if(l_DbPlayer.IsAnimalActiv())
            {
                if(key == Key.NUM0)// follow
                {
                    if (l_DbPlayer.PlayerPed.isFollowing != null)
                    {
                        l_DbPlayer.PlayerPed.isFollowing = null;
                        NAPI.Ped.ClearPedTasks(l_DbPlayer.PlayerPed.Ped);
                        l_DbPlayer.PlayerPed.StopFollow();
                        l_DbPlayer.SendNewNotification(l_DbPlayer.PlayerPed.Name + " folgt dir nicht mehr.");
                    }
                    else
                    {
                        l_DbPlayer.PlayerPed.isFollowing = l_DbPlayer;
                        l_DbPlayer.PlayerPed.Follow();
                        l_DbPlayer.SendNewNotification(l_DbPlayer.PlayerPed.Name + " folgt dir nun.");

                        NAPI.Task.Run(async () =>
                        {
                            l_DbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody), "rcmnigel1c", "hailing_whistle_waive_a");
                            l_DbPlayer.SetCannotInteract(true);
                            await NAPI.Task.WaitForMainThread(1800);
                            l_DbPlayer.SetCannotInteract(false);
                            l_DbPlayer.StopAnimation();
                        });
                    }
                    return true;
                }
                else if(key == Key.NUM1)
                {
                    l_DbPlayer.PlayerPed.AnimalAnimSit();
                    return true;
                }
                else if (key == Key.NUM2)
                {
                    l_DbPlayer.PlayerPed.AnimalAnimBellen();
                    return true;
                }
                else if (key == Key.NUM3)
                {
                    l_DbPlayer.PlayerPed.AnimalAnimLayDown();
                    return true;
                }
                else if (key == Key.NUM4)
                {
                    l_DbPlayer.PlayerPed.FindBall();
                    return true;
                }
                else if (key == Key.NUM5)
                {
                    l_DbPlayer.SetAnimalIntoVehicleMode(l_DbPlayer.RageExtension.IsInVehicle);
                    return true;
                }
            }

            return false;
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandclothpet(Player player, string texture)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!Int32.TryParse(texture, out int textureId)) return;

            iPlayer.PlayerPed.SetClothes(0, 0, textureId);

            if (AnimalVariationModule.Instance.VariationAnimals.ContainsKey(iPlayer.PlayerPed.Ped))
            {
                AnimalVariationModule.Instance.VariationAnimals[iPlayer.PlayerPed.Ped] = textureId;
            }
            else AnimalVariationModule.Instance.VariationAnimals.Add(iPlayer.PlayerPed.Ped, textureId);
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandassignpet(Player p_Player, string pedIdObj)
        {
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!UInt32.TryParse(pedIdObj, out uint pedId)) return;

            dbPlayer.LoadAnimal(pedId, dbPlayer.Player.Position, dbPlayer.Player.Dimension);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void Commandtestpetanim(Player player, string commandParams)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length <= 0) return;

            if (!int.TryParse(args[0], out int flag)) return;

            player.SendNotification($"{flag} {args[1]} {args[2]}");

            iPlayer.PlayerPed.PlayAnimation(args[1], args[2], flag, -1);
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void Commandgivepetweapon(Player player, string weaponHash)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (iPlayer.PlayerPed == null) return;

            if (Enum.TryParse(weaponHash, true, out WeaponHash weapon))
            {
                iPlayer.PlayerPed.GiveWeapon((uint)weapon, 600, true);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commanddriveme(Player p_Player, string pedhash)
        {
            DbPlayer l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Enum.TryParse(pedhash, true, out PedHash skin)) return;

            NAPI.Task.Run(async () =>
            {

                SxVehicle myveh = VehicleHandler.Instance.CreateServerVehicle(
                                VehicleDataModule.Instance.GetDataByName("neon").Id, true,
                                l_DbPlayer.Player.Position, l_DbPlayer.Player.Rotation.Z, 1, 131, l_DbPlayer.Player.Dimension,
                                true, false, false, 0,
                                l_DbPlayer.GetName(), 0, 999, (uint)l_DbPlayer.Id, 100, 1000, "", "", 0, null, null, true);

                while (myveh.entity == null)
                {
                    await NAPI.Task.WaitForMainThread(100);
                }

                l_DbPlayer.Player.TriggerNewClient("testcop", l_DbPlayer.PlayerPed.Ped, myveh.entity, l_DbPlayer.Player.Position.X, l_DbPlayer.Player.Position.Y, l_DbPlayer.Player.Position.Z);
            });
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if(dbPlayer.IsAnimalActiv())
            {
                dbPlayer.DeleteAnimal();
            }
        }
    }

    public static class AnimalPlayerExtension
    {
        public static bool IsAnimalActiv(this DbPlayer dbPlayer)
        {
            return (dbPlayer.PlayerPed != null && dbPlayer.PlayerPed.Spawned != false && dbPlayer.PlayerPed.Ped != null && dbPlayer.PlayerPed.Ped.Exists);
        }

        public static void DeleteAnimal(this DbPlayer dbPlayer)
        {
            if (dbPlayer.PlayerPed == null || !dbPlayer.PlayerPed.Spawned) return;

            NAPI.Task.Run(async () =>
            {

                if (dbPlayer.PlayerPed.isFollowing != null)
                {
                    dbPlayer.PlayerPed.StopFollow();

                    await NAPI.Task.WaitForMainThread(500);
                }

                if (dbPlayer.PlayerPed.Ped != null && dbPlayer.PlayerPed.Ped.Exists)
                {
                    dbPlayer.PlayerPed.Ped.Delete();
                    dbPlayer.PlayerPed.Ped = null;
                }

                dbPlayer.PlayerPed.Spawned = false;
                dbPlayer.PlayerPed = null;
            });
        }

        public static void LoadAnimal(this DbPlayer dbPlayer, uint animalId, Vector3 pos, uint dimension)
        {
            NAPI.Task.Run(async () =>
            {
                if (!AnimalModule.Instance.Contains(animalId)) return;

                if (dbPlayer.IsAnimalActiv())
                {
                    dbPlayer.DeleteAnimal();

                    await NAPI.Task.WaitForMainThread(500);
                }

                Animal animal = AnimalModule.Instance.Get(animalId);

                // not found or already spawned...
                if (animal == null || animal.Spawned || animal.Ped != null) return;

                animal.Spawn(dbPlayer, pos, dbPlayer.Player.Heading, dimension);
                dbPlayer.PlayerPed = animal;
            });
        }


        public static void SetAnimalIntoVehicleMode(this DbPlayer dbPlayer, bool inVehicle)
        {
            if (dbPlayer.PlayerPed == null) return;

            // not found or already spawned...
            if (dbPlayer.PlayerPed == null || dbPlayer.PlayerPed.Ped == null) return;

            if (inVehicle)
            {
                if (dbPlayer.PlayerPed.IsInVehicle) return;
                if (!dbPlayer.RageExtension.IsInVehicle) return;

                dbPlayer.PlayerPed.Ped.Delete();
                dbPlayer.PlayerPed.Spawned = false;
                dbPlayer.PlayerPed.IsInVehicle = true;
            }
            else
            {
                if (!dbPlayer.PlayerPed.IsInVehicle) return;
                if (dbPlayer.RageExtension.IsInVehicle) return;
                dbPlayer.PlayerPed.Spawn(dbPlayer, dbPlayer.Player.Position, dbPlayer.Player.Heading, dbPlayer.Player.Dimension);
                dbPlayer.PlayerPed.IsInVehicle = false;
            }
        }
    }
}
