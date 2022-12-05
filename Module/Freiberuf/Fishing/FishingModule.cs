using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Freiberuf.Fishing
{
    public class FishingSpot : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
       
        public int RareSpotFishId { get; set; }

        public int Range { get; set; }

        public FishingSpot(MySqlDataReader reader) : base(reader)
        {

            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Range = reader.GetInt32("range");
            RareSpotFishId = 0;
        }
        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class FishingModule : SqlModule<FishingModule, FishingSpot, uint>
    {
        public int CountLoadedRar = 0;

        public static uint FishingRoItemId = 524;
        public static uint KoederItemId = 1283;

        public static List<int> NormalFishes = new List<int>() { 165, 163, 162, 161, 160 };

        public List<DbPlayer> FishingPlayers = new List<DbPlayer>();

        private object lockObj = new object();

        protected override string GetQuery()
        {
            return "SELECT * FROM `fishing_spots` ORDER BY RAND();";
        }

        public void AddToFishing(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            lock (lockObj)
            {
                if (!FishingPlayers.Contains(dbPlayer))
                    FishingPlayers.Add(dbPlayer);
            }
        }

        public void RemoveFromFishing(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            lock (lockObj)
            {
                if (FishingPlayers.Contains(dbPlayer))
                    FishingPlayers.Remove(dbPlayer);
            }
        }

        public bool ContainsPlayerFishing(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid())
                return false;

            bool exists = false;
            lock (lockObj)
            {
                exists = FishingPlayers.Contains(dbPlayer);
            }

            return exists;
        }

        public List<DbPlayer> GetFishingPlayers()
        {
            List<DbPlayer> players = new List<DbPlayer>();
            lock (lockObj)
            {
                players = FishingPlayers.ToList();
            }

            return players;
        }

        public override void OnTenSecUpdate()
        {
            foreach (DbPlayer dbPlayer1 in GetFishingPlayers())
            {
                SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer1.Player.Position, 10.0f);
                if (sxVehicle == null || !sxVehicle.IsValid() || sxVehicle.Data.ClassificationId != 3 || !ServerFeatures.IsActive("fishing"))
                {
                    Attachments.AttachmentModule.Instance.ClearAllAttachments(dbPlayer1);
                    if (ContainsPlayerFishing(dbPlayer1)) RemoveFromFishing(dbPlayer1);

                    dbPlayer1.StopFishing();
                    continue;
                }

                FishingSpot fishingSpot = FishingModule.Instance.GetAll().Values.ToList().Where(fs => fs.Position.DistanceTo(dbPlayer1.Player.Position) < fs.Range).FirstOrDefault();
                if (fishingSpot == null)
                {
                    Attachments.AttachmentModule.Instance.ClearAllAttachments(dbPlayer1);
                    dbPlayer1.ResetData("fishing_koeder");
                    dbPlayer1.SendNewNotification("Du bist nicht mehr im Fischfang Bereich... hier gibt es nichts!");
                    dbPlayer1.Player.TriggerNewClient("setFishState", false);

                    if (ContainsPlayerFishing(dbPlayer1)) RemoveFromFishing(dbPlayer1);
                    continue;
                }

                if (new Random().Next(1, 20) <= 2)
                {
                    dbPlayer1.SetData("fishing_fish", true);
                    dbPlayer1.Player.TriggerNewClient("setFishState", true);
                }
            }
        }

        public override void OnPlayerEnterVehicle(DbPlayer dbPlayer, Vehicle vehicle, sbyte seat)
        {
            if (ContainsPlayerFishing(dbPlayer))
            {
                RemoveFromFishing(dbPlayer);
                dbPlayer.Player.TriggerNewClient("setFishState", false);
                Attachments.AttachmentModule.Instance.ClearAllAttachments(dbPlayer);
                dbPlayer.ResetData("fishing_koeder");
                dbPlayer.ResetData("fishing_fish");
            }
            return;
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if(ContainsPlayerFishing(dbPlayer))
            {
                RemoveFromFishing(dbPlayer);
            }
        }

        protected override void OnLoaded()
        {
            FishingPlayers = new List<DbPlayer>();
            base.OnLoaded();
        }

        protected override void OnItemLoaded(FishingSpot loadable)
        {
            if (CountLoadedRar == 0)
            {
                loadable.RareSpotFishId = 168; // Hammerhai
                CountLoadedRar++;
            }
            else if (CountLoadedRar == 1)
            {
                loadable.RareSpotFishId = 167; // Marlin
                CountLoadedRar++;
            }
            else if (CountLoadedRar == 2)
            {
                loadable.RareSpotFishId = 166; // Arowana
                CountLoadedRar++;
            }
            else if (CountLoadedRar == 3)
            {
                loadable.RareSpotFishId = 164; // Hummer
                CountLoadedRar++;
            }
            else loadable.RareSpotFishId = 0;

            base.OnItemLoaded(loadable);
        }
    }

    public static class FishingPlayerExtension
    {
        public static void StartFishing(this DbPlayer dbPlayer)
        {
            if (!ServerFeatures.IsActive("fishing"))
            {
                dbPlayer.SendNewNotification("Das Angeln ist temporär deaktiviert. Wir arbeiten an der Re-Aktivierung des Features!");
                return;
            }

            if (!dbPlayer.HasData("fishingmarkers"))
            {
                List<CustomMarkerClientObject> clientSendData = new List<CustomMarkerClientObject>();

                foreach (FishingSpot fishingSpot in FishingModule.Instance.GetAll().Values)
                {
                    clientSendData.Add(new CustomMarkerClientObject() { Position = fishingSpot.Position, Color = 4, MarkerId = 317, Name = "Fischfang" });
                }

                dbPlayer.Player.TriggerNewClient("setcustommarks", CustomMarkersKeys.FishingJob, true, NAPI.Util.ToJson(clientSendData));
                dbPlayer.SetData("fishingmarkers", true);

                // Activate Angel Menü
                dbPlayer.Player.TriggerNewClient("showfishing", true);
            }
        }

        public static void StopFishing(this DbPlayer dbPlayer)
        {
            if (dbPlayer.HasData("fishingmarkers"))
            {
                dbPlayer.ResetData("fishing_koeder");
                dbPlayer.ResetData("fishingmarkers");

                dbPlayer.Player.TriggerNewClient("showfishing", false);
                dbPlayer.Player.TriggerNewClient("setAngelState", false);

                dbPlayer.Player.TriggerNewClient("clearcustommarks", CustomMarkersKeys.FishingJob);
            }
        }
    }
}
