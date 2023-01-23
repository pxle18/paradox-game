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
    }
}
