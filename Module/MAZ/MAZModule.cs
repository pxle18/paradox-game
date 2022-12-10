using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;
using static VMP_CNR.Module.Chat.Chats;

namespace VMP_CNR.Module.MAZ
{
    public class MAZModule : SqlModule<MAZModule, MAZ, uint>
    {
        public DateTime LastActive = DateTime.Now;
        public int loadedAmount = 0;
        public static int militaryChestsToLoadMin = 15;
        public static int militaryChestsToLoadMax = 20;
        public bool MAZIsSomeoneOpening = false;

        public static uint MilitaryChestId = 1140;
        public static uint WeaponChestId = 303;

        protected override string GetQuery()
        {
            return "SELECT * FROM `maz_positions`;";
        }

        protected override void OnItemLoad(MAZ u)
        {
            NAPI.World.RemoveIpl(u.DlcName);
            
            base.OnItemLoad(u);
        }

        public bool IsMAZActive()
        {
            return GetAll().Where(m => m.Value.IsActive).Count() > 0;
        }

        public bool CanMAZLoaded()
        {
            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            if (Configurations.Configuration.Instance.DevMode) return true;

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 20)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    return false;
            }

            return true;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandstartmaz(Player player, string commandParams)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!Configurations.Configuration.Instance.DevMode) return;


            if (!UInt32.TryParse(commandParams, out uint mazId))
            {
                return;
            }

            MAZ mAZ = MAZModule.Instance.Get(mazId);
            if (mAZ == null) return;

            mAZ.SetActive();
            dbPlayer.SendNewNotification($"MAZ {mazId} wurde geladen!");

            return;
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandstartrandommaz(Player player, string commandParams)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!dbPlayer.IsValid() || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPermissions());
                return;
            }

            if (!UInt32.TryParse(commandParams, out uint mazId))
            {
                return;
            }

            int index = new Random().Next(GetAll().Count);

            MAZ maz = GetAll().Values.ToList()[index];

            if (maz != null)
            {
                maz.SetActive();
                dbPlayer.SendNewNotification($"MAZ {maz.Id} wurde geladen!");
            }


            return;
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandgomaz(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!Configurations.Configuration.Instance.DevMode) return;

            MAZ mAZ = MAZModule.Instance.GetAll().Values.Where(m => m.IsActive).FirstOrDefault();
            if (mAZ == null) return;

            dbPlayer.Player.SetPosition(mAZ.Position);
            dbPlayer.SendNewNotification($"Zum MAZ teleportiert!");
            return;
        }

        public override void OnMinuteUpdate()
        {
            // Schon 1x in Wende gewesen
            if (loadedAmount >= 1 || !CanMAZLoaded()) return;

            // Human Labs wird gemacht
            if (Robbery.WeaponFactoryRobberyModule.Instance.IsActive) return;

            if (loadedAmount > 0 && LastActive.AddHours(3) > DateTime.Now) return; // wenn schonma, dann mind 3h pause

            // Unter 20 Soldaten im Dienst
            if (TeamModule.Instance.Get((uint)TeamTypes.TEAM_ARMY).GetTeamMembers().Where(t => t.Duty).Count() < 25) return;

            bool ChanceRequireSuccess = false;
            Random random = new Random();

            if (loadedAmount == 0 && random.Next(1, 600) < 4)
            {
                ChanceRequireSuccess = true;
                Logging.Logger.Debug("MAZ chance triggered");
            }
            else if (random.Next(1, 1000) < 3)
            {
                ChanceRequireSuccess = true;
            }

            if (ChanceRequireSuccess)
            {
                int index = random.Next(GetAll().Count);
                if(GetAll().Values.ToList()[index] != null)
                {
                    GetAll().Values.ToList()[index].SetActive();
                }
            }
        }

        protected override void OnLoaded()
        {
            LastActive = DateTime.Now;
            MAZIsSomeoneOpening = false;
            loadedAmount = 0;
        }

        public void UnpackMilitaryChest(Container container)
        {
            int contValue = 100;
            Random random = new Random();

            while(contValue > 0)
            {
                int rcheck = random.Next(1, 100);

                if(rcheck <= 5) // marksman
                {
                    container.AddItem(87, 2);
                    contValue -= 50;
                }
                else if (rcheck <= 10) // goldbarren
                {
                    container.AddItem(487, 10);
                    contValue -= 30;
                }
                else if (rcheck <= 20) // marksmanmag
                {
                    container.AddItem(226, 10);
                    contValue -= 30;
                }
                else if (rcheck <= 30) // gusenberg
                {
                    container.AddItem(77, 4);
                    contValue -= 20;
                }
                else if (rcheck <= 45) // gusenberg mag
                {
                    container.AddItem(216, 10);
                    contValue -= 25;
                }
                else if (rcheck <= 70) // advancedrifle
                {
                    container.AddItem(81, 6);
                    contValue -= 17;
                }
                else if (rcheck <= 100) // advancedrifle mags
                {
                    container.AddItem(220, 10);
                    contValue -= 20;
                }
            }
        }
    }

    public static class MAZExtension
    {
        public static void SetActive(this MAZ maz)
        {
            Task.Run(async () =>
            {
                MAZ mAZ2 = MAZModule.Instance.GetAll().Values.Where(m => m.IsActive).FirstOrDefault();
                if (mAZ2 != null)
                {
                    mAZ2.IsActive = false;
                }

                foreach (MAZ xMaz in MAZModule.Instance.GetAll().Values.ToList())
                {
                    NAPI.Task.Run(() =>
                    {
                        NAPI.World.RemoveIpl(xMaz.DlcName);
                    });
                }

                MAZModule.Instance.loadedAmount++;

                maz.IsActive = true;

                NAPI.Task.Run(() => { NAPI.World.RequestIpl(maz.DlcName); });

                StaticContainer staticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.MAZ);

                // Set to position
                if (staticContainer != null)
                {
                    staticContainer.Position = maz.Position;
                    staticContainer.Locked = true;

                    if (staticContainer.Container != null)
                    {
                        Random random = new Random();
                        staticContainer.Container.ClearInventory(true);
                        staticContainer.Container.AddItem(MAZModule.MilitaryChestId, random.Next(MAZModule.militaryChestsToLoadMin, MAZModule.militaryChestsToLoadMax));
                    }
                }

                Logging.Logger.Debug($"MAZ {maz.Id} loaded");

                await Chats.SendGlobalMessage($"Zentrales Flugabwehrsystem: Es wurde der Absturz einer feindlichen Militärmaschine im Hoheitsgebiet Los Santos gemeldet!", COLOR.LIGHTBLUE, ICON.GOV);
            });
        }
    }

    public class MAZ : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }

        public string DlcName { get; set; }

        public bool IsActive { get; set; }
        public bool Breaked { get; set; }


        public MAZ(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            DlcName = reader.GetString("dlcname");
            IsActive = false;
            Breaked = false;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
