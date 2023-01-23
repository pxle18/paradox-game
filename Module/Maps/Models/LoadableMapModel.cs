using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Maps.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadableMapModel
    {
        [JsonProperty("objects")]
        public List<LoadableMapObjectModel> Objects { get; set; } = new List<LoadableMapObjectModel>();

        private List<GTANetworkAPI.Object> LoadedObjects { get; set; } = new List<GTANetworkAPI.Object>();

        public void Load()
        {
            foreach (var mapObject in Objects)
            {
                var entity = NAPI.Object.CreateObject(
                    Convert.ToInt32(mapObject.Hash),
                    mapObject.Position,
                    mapObject.Rotation
                );

                LoadedObjects.Add(entity);
            }
        }

        public void Unload()
        {
            NAPI.Task.Run(() =>
            {
                foreach (var entity in LoadedObjects)
                {
                    LoadedObjects.Remove(entity);
                    
                    entity.Delete();
                }
            });
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class LoadableMapObjectModel
    {
        [JsonProperty("hash")]
        public long Hash { get; set; }


        [JsonProperty("posX")]
        public float PositionX { get; set; }

        [JsonProperty("posY")]
        public float PositionY { get; set; }

        [JsonProperty("posZ")]
        public float PositionZ { get; set; }


        [JsonProperty("rotX")]
        public float RotationX { get; set; }

        [JsonProperty("rotY")]
        public float RotationY { get; set; }

        [JsonProperty("rotZ")]
        public float RotationZ { get; set; }


        [JsonIgnore]
        public Vector3 Position => new Vector3(PositionX, PositionY, PositionZ);

        [JsonIgnore]
        public Vector3 Rotation => new Vector3(RotationX, RotationY, RotationZ);
    }
}
