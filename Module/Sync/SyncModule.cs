using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
/*
namespace VMP_CNR.Module.Sync
{
    public class SyncModule : SqlModule<SyncModule, SyncShape, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `syncshapes`;";
        }

        protected override void OnItemLoaded(SyncShape loadable)
        {
            Spawners.ColShapes.Create(loadable.Position, loadable.Range, loadable.Dimension).SetData("syncShape", true);
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            if (!colShape.HasData("syncShape")) return false;
            if (ServerFeatures.IsActive("customsyncshape"))
            {
                if (dbPlayer.LastShapeSynced == null || dbPlayer.LastShapeSynced.AddSeconds(10) >= DateTime.Now) return false;

                Task.Run(async () =>
                {
                    uint oldDimension = dbPlayer.Player.Dimension;

                    dbPlayer.LastShapeSynced = DateTime.Now;

                    dbPlayer.SetDimension(99);
                    await NAPI.Task.WaitForMainThread(200);
                    dbPlayer.SetDimension(oldDimension);
                });
            }
            return false;
            return false;
        }
    }

    public class SyncShape : Loadable<uint>
    {
        public uint Id { get; set; }

        public Vector3 Position { get; set; }

        public float Range { get; set; }

        public uint Dimension { get; set; }

        public SyncShape(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Range = reader.GetFloat("radius");
            Dimension = reader.GetUInt32("dimension");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
*/