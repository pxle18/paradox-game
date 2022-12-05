using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR
{
    public class PoliceObject
    {
        public int Id { get; set; }
        public int modelID { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public string Owner { get; set; }
        public ColShape Shape { get; set; }
        public Object Entity { get; set; }
        public ItemModel Item { get; set; }
    }

    public class PoliceObjectModule : Module<PoliceObjectModule>
    {
        private Dictionary<int, PoliceObject> _objects = new Dictionary<int, PoliceObject>();
        private int _currentUnique;

        private const int MaxPoliceCounts = 30;

        protected override bool OnLoad()
        {
            _objects = new Dictionary<int, PoliceObject>();
            return base.OnLoad();
        }

        /*public override void OnMinuteUpdate()
        {
            foreach (var pair in objects)
            {
                Refresh(pair.Value);
            }
        }*/

        public bool IsMaxReached()
        {
            return MaxPoliceCounts < _objects.Count;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if(colShape != null && dbPlayer != null && dbPlayer.IsValid() && colShapeState == ColShapeState.Enter)
            {
                if(colShape.HasData("nail") && dbPlayer.RageExtension.IsInVehicle)
                {
                    if(dbPlayer != null && dbPlayer.IsValid())
                        dbPlayer.Player.TriggerNewClient("nagelband");
                    return true;
                }
            }
            return false;
        }

        public PoliceObject Add(int model, Player player, ItemModel item, bool nail = false)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return null;

            var polO = new PoliceObject
            {
                Id = _currentUnique++,
                Position = player.Position,
                Owner = dbPlayer.GetName(),
                Item = item,
                modelID = model
            };

            var pos = new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 1f);
            var rot = player.Rotation;

            if (nail)
            {
                pos = new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 0.97f);
                rot = new Vector3(player.Rotation.X, player.Rotation.Y, player.Rotation.Z + 90.0f);
                polO.Shape = ColShapes.Create(pos, 4.0f, 0);
                polO.Shape.SetData<int>("nail", 1);
            }

            polO.Entity = ObjectSpawn.Create(model, pos, rot);
            polO.Rotation = rot;

            _objects.Add(polO.Id, polO);
            return polO;
        }

        public Dictionary<int, PoliceObject> GetAll()
        {
            return _objects;
        }

        public PoliceObject GetNearest(Vector3 position)
        {
            foreach (var kvp in _objects)
            {
                var l_Entity = kvp.Value.Entity;
                if (l_Entity == null)
                    continue;

                if (l_Entity.Position.DistanceTo(position) <= 2.0f)
                    return kvp.Value;
            }

            return null;
        }

        public void Delete(PoliceObject obj)
        {
            obj.Shape?.Delete();
            obj.Entity.Delete();
            _objects.Remove(obj.Id);
        }

        public void Refresh(PoliceObject obj)
        {
            NAPI.Task.Run(() =>
            {
                obj.Entity.Delete();
                obj.Entity = ObjectSpawn.Create(obj.modelID, obj.Position, obj.Rotation);
            });
        }
    }
}