using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Animal
{
    public class Animal : Loadable<uint>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public uint PedHashUInt { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public bool Spawned { get; set; }

        bool IsPlayingAnim { get; set; }
        public DbPlayer isFollowing { get; set; }

        public bool IsInVehicle { get; set; }
        public Ped Ped { get; set; }

        public Animal(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            PedHashUInt = reader.GetUInt32("ped_hash");
            Level = reader.GetInt32("level");
            Experience = reader.GetInt32("exp");
            Spawned = false;
            isFollowing = null;
            IsPlayingAnim = false;
            IsInVehicle = false;
        }
        public override uint GetIdentifier()
        {
            return Id;
        }

        public void Spawn(DbPlayer controller, Vector3 pos, float heading, uint dimension)
        {
            if(Spawned)
            {
                if(Ped != null)
                {
                    NAPI.Entity.DeleteEntity(Ped);
                }
                Spawned = false;
            }

            NAPI.Task.Run(async () =>
            {
                Ped = NAPI.Ped.CreatePed(PedHashUInt, pos, heading, true, false, false, true, dimension);

                while(Ped == null)
                {
                    await NAPI.Task.WaitForMainThread(50);
                }

                Ped.Controller = controller.Player;
                Spawned = true;

                if(Level >= 10)
                {
                    await NAPI.Task.WaitForMainThread(100);
                    SetArmour(100);
                }
            });
        }

        public void AnimalAnimSit()
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(50);

                if (IsPlayingAnim)
                {
                    ClearTasks();
                    IsPlayingAnim = false;
                }
                else IsPlayingAnim = true;

                if(PedHashUInt == (uint)PedHash.Shepherd)
                    Controller.Player.TriggerNewClient("animal_playanim", Ped, "creatures@retriever@amb@world_dog_sitting@base", "base", -1, 1);
                else if(PedHashUInt == (uint)PedHash.Chop)
                    Controller.Player.TriggerNewClient("animal_playanim", Ped, "creatures@rottweiler@amb@world_dog_sitting@base", "base", -1, 1);
                else if(PedHashUInt == (uint)PedHash.MountainLion)
                {
                    Controller.Player.TriggerNewClient("animal_playanim", Ped, "creatures@cougar@melee@streamed_core@", "ground_attack_0", -1, 1);
                    await NAPI.Task.WaitForMainThread(2000);
                    ClearTasks();
                }

            });
        }

        public void ClearTasks()
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {
                if (IsPlayingAnim) IsPlayingAnim = false;
                await NAPI.Task.WaitForMainThread(0);
                Controller.Player.TriggerNewClient("animal_cleartasks", Ped);

            });
        }

        public void AnimalAnimLayDown()
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {

                if (IsPlayingAnim)
                {
                    ClearTasks();
                }
                else IsPlayingAnim = true;
                await NAPI.Task.WaitForMainThread(50);


                if (PedHashUInt == (uint)PedHash.MountainLion)
                {
                    Controller.Player.TriggerNewClient("animal_playanim", Ped, "creatures@cougar@amb@world_cougar_rest@enter", "enter", -1, 1);
                    await NAPI.Task.WaitForMainThread(4000);
                    Controller.Player.TriggerNewClient("animal_playanim", Ped, "creatures@cougar@amb@world_cougar_rest@enter", "base", -1, 1);
                    await NAPI.Task.WaitForMainThread(1000);
                    Controller.Player.TriggerNewClient("animal_playanim", Ped, "creatures@cougar@amb@world_cougar_rest@enter", "base", -1, 1);
                }
                else if (PedHashUInt == (uint)PedHash.Chop)
                    Controller.Player.TriggerNewClient("animal_playanim", Ped, "creatures@rottweiler@tricks@", "beg_loop", -1, 1);
                else if (PedHashUInt == (uint)PedHash.Shepherd)
                {
                    Controller.Player.TriggerNewClient("animal_playanim", Ped, "creatures@retriever@melee@streamed_core@", "ground_attack_0", -1, 1);
                    await NAPI.Task.WaitForMainThread(2800);
                    ClearTasks();
                }

            });
        }


        public void FindBall()
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {

                if (IsPlayingAnim)
                {
                    ClearTasks();
                }
                else IsPlayingAnim = true;
                await NAPI.Task.WaitForMainThread(50);

                Controller.Player.TriggerNewClient("animal_findball", Ped);
            });
        }

        public void AnimalAnimBellen()
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {

                if (IsPlayingAnim)
                {
                    ClearTasks();
                }
                else IsPlayingAnim = true;
                await NAPI.Task.WaitForMainThread(50);


                if (PedHashUInt == (uint)PedHash.Shepherd)
                    Controller.Player.TriggerNewClient("animal_playanim", Ped, "creatures@retriever@amb@world_dog_barking@base", "base", -1, 1);
                else if (PedHashUInt == (uint)PedHash.Chop)
                    Controller.Player.TriggerNewClient("animal_playanim", Ped, "creatures@rottweiler@amb@world_dog_barking@base", "base", -1, 1);
                else if (PedHashUInt == (uint)PedHash.MountainLion)
                    Controller.Player.TriggerNewClient("animal_playanim", Ped, "creatures@cougar@melee@", "growling", -1, 1);

            });
        }

        public void StopFollow()
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {
                ClearTasks();
                await NAPI.Task.WaitForMainThread(50);

                isFollowing = null;
                Controller.Player.TriggerNewClient("animal_stopFollow");
            });
        }


        public void SetClothes(int arg1, int arg2, int arg3)
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(50);
                Controller.Player.TriggerNewClient("animal_cloth", Ped, arg1, arg2, arg3);
            });
        }

        public void GiveWeapon(uint weaponid, int ammo, bool equipnow)
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(50);
                Controller.Player.TriggerNewClient("animal_giveweapon", Ped, weaponid, ammo, equipnow);
            });
        }

        public void SetArmour(int armour)
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(50);
                Controller.Player.TriggerNewClient("animal_setarmour", Ped, armour);
            });
        }
        public void SetHealth(int health)
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {
                await NAPI.Task.WaitForMainThread(50);
                Controller.Player.TriggerNewClient("animal_sethealth", Ped, health);
            });
        }

        public void Follow(float speed = 3.0f)
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {
                ClearTasks();
                await NAPI.Task.WaitForMainThread(50);

                isFollowing = Controller;
                Controller.Player.TriggerNewClient("animal_setFollow", Ped, speed);
            });
        }

        public void GotoCoords(Vector3 coords, float speed = 3.0f)
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {
                ClearTasks();
                await NAPI.Task.WaitForMainThread(50);

                Controller.Player.TriggerNewClient("animal_gotoCoord", Ped, coords.X, coords.Y, coords.Z, speed);
            });
        }
        public void PlayAnimation(string animdic, string animname, int flag, int duration = -1)
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {
                Controller.Player.TriggerNewClient("animal_playanim", Ped, animdic, animname, duration, flag);
            });
        }

        public void Attack(Player entity)
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {
                ClearTasks();
                await NAPI.Task.WaitForMainThread(50);

                if(isFollowing != null)
                {
                    StopFollow();
                }
                await NAPI.Task.WaitForMainThread(100);

                Controller.Player.TriggerNewClient("animal_attack", Ped, entity);
            });
        }
        public void Attack(Entity entity)
        {
            if (Ped == null || Ped.Handle == null || Ped.Controller == null) return;

            DbPlayer Controller = Ped.Controller.GetPlayer();

            if (Controller == null || !Controller.IsValid()) return;

            NAPI.Task.Run(async () =>
            {
                ClearTasks();
                await NAPI.Task.WaitForMainThread(50);

                Controller.Player.TriggerNewClient("animal_attack", Ped, entity);
            });
        }
    }
}
