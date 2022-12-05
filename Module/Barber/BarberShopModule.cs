using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Module.Assets.Beard;
using VMP_CNR.Module.Assets.Chest;
using VMP_CNR.Module.Assets.Hair;
using VMP_CNR.Module.Assets.HairColor;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Barber
{
    public class BarberShopModule : SqlModule<BarberShopModule, BarberShop, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(AssetsBeardModule), typeof(AssetsHairModule), typeof(AssetsHairColorModule), typeof(AssetsChestModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `barber_shops`;";
        }

        public Dictionary<uint, ListJsonBarberObject> MaleListJsonBarberObject = new Dictionary<uint, ListJsonBarberObject>();
        public Dictionary<uint, ListJsonBarberObject> FemaleListJsonBarberObject = new Dictionary<uint, ListJsonBarberObject>();

        public static uint blip = 71;
        public static int color = 4;


        protected override bool OnLoad()
        {
            MaleListJsonBarberObject = new Dictionary<uint, ListJsonBarberObject>();
            FemaleListJsonBarberObject = new Dictionary<uint, ListJsonBarberObject>();

            return base.OnLoad();
        }

        protected override void OnItemLoaded(BarberShop barber)
        {
            Main.ServerBlips.Add(Spawners.Blips.Create(barber.Position, "", blip, 1.0f, color: color));

            MaleListJsonBarberObject.Add(barber.Id, GetListJsonBarberObject(barber.Id, 0));
            FemaleListJsonBarberObject.Add(barber.Id, GetListJsonBarberObject(barber.Id, 1));
        }

        public ListJsonBarberObject GetListJsonBarberObject(uint BarberShopId, int gender)
        {
            IEnumerable<KeyValuePair<uint, AssetsHair>> hairs = AssetsHairModule.Instance.GetAll().Where(x => x.Value != null && x.Value.BarberShopId == BarberShopId && x.Value.Gender == gender);
            IEnumerable<KeyValuePair<uint, AssetsBeard>> beards = AssetsBeardModule.Instance.GetAll().Where(x => x.Value != null && x.Value.BarberShopId == BarberShopId);
            IEnumerable<KeyValuePair<uint, AssetsChest>> chests = AssetsChestModule.Instance.GetAll().Where(x => x.Value != null && x.Value.BarberShopId == BarberShopId);

            Dictionary<uint, AssetsHairColor> colors = AssetsHairColorModule.Instance.GetAll();
            ListJsonBarberObject objectToClient = new ListJsonBarberObject();

            List<JsonBarberObject> temp = new List<JsonBarberObject>();


            foreach (KeyValuePair<uint, AssetsHair> hair in hairs)
            {
                var value = hair.Value;
                var jsonBarberObject = new JsonBarberObject
                {
                    Id = value.Id,
                    CustomizationId = (uint)value.CustomisationId,
                    Name = value.Name,
                    Price = (uint) value.Price
                };

                temp.Add(jsonBarberObject);
            }
            
            objectToClient.Hairs = temp;
            temp = new List<JsonBarberObject>();

            foreach (KeyValuePair<uint, AssetsBeard> beard in beards)
            {
                var value = beard.Value;
                var jsonBarberObject = new JsonBarberObject
                {
                    Id = value.Id,
                    CustomizationId = (uint) value.CustomisationId,
                    Name = value.Name,
                    Price = (uint) value.Price
                };

                temp.Add(jsonBarberObject);
            }
            objectToClient.Beards = temp;
            temp = new List<JsonBarberObject>();
            foreach (KeyValuePair<uint, AssetsChest> chest in chests)
            {
                var value = chest.Value;
                var jsonBarberObject = new JsonBarberObject
                {
                    Id = value.Id,
                    CustomizationId = (uint) value.CustomisationId,
                    Name = value.Name,
                    Price = (uint) value.Price
                };

                temp.Add(jsonBarberObject);
            }
            objectToClient.Chests = temp;
            temp = new List<JsonBarberObject>();
            foreach (KeyValuePair<uint, AssetsHairColor> color in colors)
            {
                var value = color.Value;
                var jsonBarberObject = new JsonBarberObject
                {
                    Id = value.Id,
                    CustomizationId = (uint) value.CustomisationId,
                    Name = value.Name,
                    Price = (uint) value.Price
                };
                temp.Add(jsonBarberObject);
            }
            objectToClient.Colors = temp;
            return objectToClient;
        }
    }

    public class JsonBarberObject
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "price")]
        public uint Price { get; set; }

        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }

        [JsonProperty(PropertyName = "customid")]
        public uint CustomizationId { get; set; }
    }

    public class ListJsonBarberObject
    {
        [JsonProperty(PropertyName = "hairs")]
        public List<JsonBarberObject> Hairs { get; set; }
        [JsonProperty(PropertyName = "beards")]
        public List<JsonBarberObject> Beards { get; set; }
        [JsonProperty(PropertyName = "chests")]
        public List<JsonBarberObject> Chests { get; set; }

        [JsonProperty(PropertyName = "colors")]
        public List<JsonBarberObject> Colors { get; set; }

    }

}
