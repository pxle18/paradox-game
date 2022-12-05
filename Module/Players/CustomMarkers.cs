using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.Players
{
    public class CustomMarkerClientObject
    {
        [JsonProperty(PropertyName = "pos")]
        public Vector3 Position { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "color")]
        public int Color { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int MarkerId { get; set; }
    }

    public static class CustomMarkersKeys
    {
        public static string GarbageJob = "garbage";
        public static string AirFlightControl = "airflight";
        public static string FishingJob = "fishing";
        public static string Leitstelle = "leitst";
    }
}
