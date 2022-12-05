using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Doors;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Staatsgefaengnis
{
    public class SGVoltagesModule : SqlModule<SGVoltagesModule, SGVoltage, uint>
    {
        public static int ManipulateToCrashElectircal = 6;

        public static Vector3 Hauptverteiler = new Vector3(1605.83, 2621.01, 45.5649);

        public DateTime lastBreaked = DateTime.Now;

        protected override string GetQuery()
        {
            return "SELECT * FROM `sg_voltages`;";
        }

        protected override void OnLoaded()
        {
            lastBreaked = DateTime.Now.AddMinutes(-30);
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer != null && dbPlayer.IsValid() && key == Key.E && dbPlayer.CanInteract())
            {
                if (!IsAbleToManipulate()) return false;

                if(dbPlayer.Player.Position.DistanceTo(Hauptverteiler) < 1.0f)
                {
                    if(GetManipulatedAmount() < ManipulateToCrashElectircal)
                    {
                        dbPlayer.SendNewNotification($"Es müssen mindestens {ManipulateToCrashElectircal} Stromkästen manipuliert sein um den Hauptverteiler auszuschalten!");
                        return true;
                    }

                    Task.Run(async () =>
                    {
                        dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);

                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_hammering@male@base", "base");

                        Chat.Chats.sendProgressBar(dbPlayer, 10000);
                        await NAPI.Task.WaitForMainThread(10000);

                        if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured()) return;

                        dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");

                        dbPlayer.StopAnimation();

                        lastBreaked = DateTime.Now;

                        foreach (Door door in DoorModule.Instance.GetAll().Values.Where(d => d.Group == (int)DoorGroups.MainJail))
                        {
                            door.Locked = false;
                            door.LastBreak = DateTime.Now;
                        }

                        foreach (SGVoltage sGVoltage in GetAll().Values)
                        {
                            sGVoltage.Breaked = false;
                        }

                        TeamModule.Instance.SendChatMessageToDepartments($"Es wurde ein Stromausfall am Staatsgefängnis gemeldet! Cunningham-Cooperation Secure System - Staatsgefängnis!");

                        for (int i = 0; i < 4; i++)
                        {
                            foreach (DbPlayer dbPlayer1 in Players.Players.Instance.GetPlayersListInRange(StaatsgefaengnisModule.sgBellPosition, 300.0f))
                            {
                                dbPlayer1.SendNewNotification($"1337Allahuakbar$sgalarm", duration: 31000);
                            }
                            await NAPI.Task.WaitForMainThread(30000);
                        }
                    });

                }


                SGVoltage voltage = GetAll().Values.Where(s => s.Position.DistanceTo(dbPlayer.Player.Position) < 1.0f).FirstOrDefault();

                if(voltage != null)
                {
                    if(dbPlayer.TeamId == (int)teams.TEAM_ARMY)
                    {
                        // repair as army
                        Task.Run(async () =>
                        {
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.SetData("userCannotInterrupt", true);

                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                            Chat.Chats.sendProgressBar(dbPlayer, 10000);
                            await NAPI.Task.WaitForMainThread(10000);

                            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured()) return;

                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.ResetData("userCannotInterrupt");

                            dbPlayer.StopAnimation();

                            if(voltage.Breaked) voltage.Breaked = false;
                            dbPlayer.SendNewNotification("Stromkasten auf Manipulation geprüft!");
                        });
                        return true;
                    }
                    else
                    {

                        if(lastBreaked.AddMinutes(45) > DateTime.Now)
                        {
                            dbPlayer.SendNewNotification("Das Stromnetz wurde vor kurzem erst manipuliert!");
                            return true;
                        }

                        if (voltage.Breaked)
                        {
                            dbPlayer.SendNewNotification("Dieser Stromkasten ist bereits manipuliert!");
                            return true;
                        }

                            // break
                        Task.Run(async () =>
                        {
                            dbPlayer.Player.TriggerNewClient("freezePlayer", true);
                            dbPlayer.SetData("userCannotInterrupt", true);

                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_hammering@male@base", "base");

                            Chat.Chats.sendProgressBar(dbPlayer, 35000);
                            await NAPI.Task.WaitForMainThread(35000);

                            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured()) return;

                            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                            dbPlayer.ResetData("userCannotInterrupt");

                            dbPlayer.StopAnimation();

                            voltage.Breaked = true;
                            dbPlayer.SendNewNotification("Stromkasten manipuliert!");
                            dbPlayer.SendNewNotification($"Manipuliere mindestens {ManipulateToCrashElectircal} Stromkästen um am Hauptverteiler den Strom auszustellen! (Derzeit {GetManipulatedAmount()} manipuliert)", PlayerNotification.NotificationType.STANDARD, "", 8000);
                        });
                        return true;
                    }
                }
            }
            return false;
        }

        public int GetManipulatedAmount()
        {
            return GetAll().Where(s => s.Value.Breaked).Count();
        }

        public bool IsAbleToManipulate()
        {
            if (Configurations.Configuration.Instance.DevMode) return true;

            // Unter 10 Soldaten im Dienst
            if (TeamModule.Instance.Get((uint)teams.TEAM_ARMY).GetTeamMembers().Where(t => t.Duty).Count() < 10) return false;

            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;


            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 30)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 15)
                    {
                        return false;
                    }

                    break;
            }


            return true;
        }
    }

    public class SGVoltage : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }

        public float Heading { get; set; }

        public bool Breaked { get; set; }

        public SGVoltage(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            Breaked = false;

            //if (Configurations.Configuration.Instance.DevMode) Spawners.Markers.Create(1, Position, new Vector3(), new Vector3(), 0.7f, 255, 0, 0, 255);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
