using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VMP_CNR.Module.Computer.Apps.VehicleImpoundApp
{
    public class VehicleImpoundOverview
    {
        [JsonProperty(PropertyName = "id")]
        public uint VehicleId { get; set; }

        [JsonProperty(PropertyName = "model")]
        public String Model { get; set; }

        [JsonProperty(PropertyName = "officer")]
        public String Officer { get; set; }

        [JsonProperty(PropertyName = "reason")]
        public String Reason { get; set; }

        [JsonProperty(PropertyName = "date")]
        public long Date { get; set; }

        [JsonProperty(PropertyName = "release")]
        public long Release { get; set; }

    }
}
