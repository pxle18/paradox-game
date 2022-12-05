using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using Newtonsoft.Json;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Clothes.Props;
using VMP_CNR.Module.Clothes.Shops;
using VMP_CNR.Module.Clothes.Slots;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.BigDataSender;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Clothes.Windows
{
    public class
        WardrobeWindow : Window<Func<DbPlayer, List<Slot>, Dictionary<string, WardrobeWindow.JsonCloth>, bool>>
    {
        /// <summary>
        /// Max Wearables to send; 0 = no restriction.
        /// </summary>
        private const int MaxWearablesToSent = 0;

        /// <summary>
        /// JSON data which will be delivered on clothes request.
        /// </summary>
        public class JsonCloth
        {
            public uint Id { get; }

            public string Name { get; }

            public int Slot { get; }

            public bool IsProp { get; }

            public JsonCloth(uint id, string name, int slot, bool isProp = false)
            {
                Id = id;
                Name = name;
                Slot = slot;
                IsProp = isProp;
            }
        }

        /// <summary>
        /// JSON data which will be delivered if the player enters the wardrobe.
        /// </summary>
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "slots")] private List<Slot> Slots { get; }

            [JsonProperty(PropertyName = "wearing")]
            private Dictionary<string, JsonCloth> Wearing { get; }

            public ShowEvent(
                DbPlayer dbPlayer,
                List<Slot> slots,
                Dictionary<string, JsonCloth> wearing
            ) : base(dbPlayer)
            {
                Slots = slots;
                Wearing = wearing;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public WardrobeWindow() : base("Wardrobe")
        {
        }

        /// <summary>
        /// Event handler if the window will be shown.
        /// </summary>
        /// <returns></returns>
        public override Func<DbPlayer, List<Slot>, Dictionary<string, JsonCloth>, bool> Show()
        {
            return (player, slots, wearing) => OnShow(new ShowEvent(player, slots, wearing));
        }

        /// <summary>
        /// Get clothes or props within a given slot.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="slotId"></param>
        [RemoteEvent]
        public void wardrobeLoadClothes(Player client, string slotId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            try
            {
                if (!IsUsable(dbPlayer)) return;

                Character.Character character = dbPlayer.Character;
                var wardrobe = character.Wardrobe;

                if (wardrobe == null || character?.Props == null) return;

                var isProp = false;

                if (slotId.StartsWith("p-"))
                {
                    isProp = true;
                    slotId = slotId.Remove(0, 2);
                }

                if (!int.TryParse(slotId, out var slot)) return;

                if (isProp)
                {
                    var propsToSent = PropModule.Instance.GetWardrobeBySlot(dbPlayer, slot)
                        .ConvertAll(x => new JsonCloth(x.Id, x.Name, x.Slot, true)).ToList();

                    // If we have toooo much props, the shop can not display them. So we cut the list.
                    if (MaxWearablesToSent > 0 && propsToSent.Count > MaxWearablesToSent)
                    {
                        propsToSent.RemoveRange(MaxWearablesToSent, propsToSent.Count - MaxWearablesToSent);
                    }

                    dbPlayer.Player.TriggerNewClientBig(
                        "componentServerEvent",
                        "Wardrobe",
                        "responseWardrobeClothes",
                        JsonConvert.SerializeObject(propsToSent)
                    );

                    return;
                }

                var clothesToSent = ClothModule.Instance.GetWardrobeBySlot(dbPlayer, character, slot)
                    .ConvertAll(x => new JsonCloth(x.Id, x.Name, x.Slot)).ToList();

                // If we have toooo much clothes, the wardrobe can not display them. So we cut the list.
                if (MaxWearablesToSent > 0 && clothesToSent.Count > MaxWearablesToSent)
                {
                    clothesToSent.RemoveRange(MaxWearablesToSent, clothesToSent.Count - MaxWearablesToSent);
                }

                dbPlayer.Player.TriggerNewClientBig(
                    "componentServerEvent",
                    "Wardrobe",
                    "responseWardrobeClothes",
                    JsonConvert.SerializeObject(clothesToSent)
                );
            }
            catch (Exception e)
            {
                dbPlayer.SendNewNotification("Ein Fehler ist aufgetreten...");

                Logger.Crash(e);
            }
        }

        /// <summary>
        /// Dresses the selected garment.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="clothId"></param>
        /// <param name="slotId"></param>
        [RemoteEvent]
        public void wardrobeDress(Player client, string clothId, string slotId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            try
            {
                if (!IsUsable(dbPlayer)) return;

                Character.Character character = dbPlayer.Character;
                var wardrobe = character.Wardrobe;

                if (wardrobe == null || character?.Props == null) return;

                var isProp = false;

                if (!int.TryParse(clothId, out var clothIdParsed)) return;

                if (slotId.StartsWith("p-"))
                {
                    isProp = true;
                    slotId = slotId.Remove(0, 2);
                }

                if (!int.TryParse(slotId, out var slotIdParsed)) return;

                if (isProp)
                {
                    var props = PropModule.Instance.GetWardrobeBySlot(dbPlayer, slotIdParsed);

                    Prop prop = props?.Find(p => p != null && p.Id == clothIdParsed);

                    if (prop == null) return;

                    ClothModule.Instance.SetPlayerAccessories(dbPlayer, slotIdParsed, prop.Variation, prop.Texture);

                    if (!character.EquipedProps.ContainsKey(slotIdParsed))
                    {
                        character.EquipedProps.Add(slotIdParsed, prop.Id);
                    }
                    else
                    {
                        character.EquipedProps[slotIdParsed] = prop.Id;
                    }

                    ClothModule.SaveCharacter(dbPlayer);

                    return;
                }

                var clothes = ClothModule.Instance.GetWardrobeBySlot(dbPlayer, character, slotIdParsed);

                Cloth cloth = clothes?.Find(c => c != null && c.Id == clothIdParsed);

                if (cloth == null) return;

                dbPlayer.SetClothes(slotIdParsed, cloth.Variation, cloth.Texture);

                if (!character.Clothes.ContainsKey(slotIdParsed))
                {
                    character.Clothes.Add(slotIdParsed, cloth.Id);
                }
                else
                {
                    character.Clothes[slotIdParsed] = cloth.Id;
                }

                ClothModule.SaveCharacter(dbPlayer);
            }
            catch (Exception e)
            {
                dbPlayer.SendNewNotification("Ein Fehler ist aufgetreten...");

                Logger.Crash(e);
            }
        }

        /// <summary>
        /// Save current clothes and props to the database.
        /// </summary>
        /// <param name="client"></param>
        [RemoteEvent]
        public void wardrobeSave(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            try
            {
                if (!IsUsable(dbPlayer)) return;

                Character.Character character = dbPlayer.Character;
                var wardrobe = character.Wardrobe;

                if (wardrobe == null || character?.Props == null) return;

                ClothModule.SaveCharacter(dbPlayer);
            }
            catch (Exception e)
            {
                dbPlayer.SendNewNotification("Ein Fehler ist aufgetreten...");

                Logger.Crash(e);
            }
        }

        /// <summary>
        /// Show the alterkleider menu.
        /// </summary>
        /// <param name="client"></param>
        [RemoteEvent]
        public void wardrobeAltkleider(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            try
            {
                if (!IsUsable(dbPlayer)) return;

                if (dbPlayer.HasData("teamWardrobe")) return;
                MenuManager.Instance.Build(PlayerMenu.Altkleider, dbPlayer).Show(dbPlayer);
            }
            catch (Exception e)
            {
                dbPlayer.SendNewNotification("Ein Fehler ist aufgetreten...");

                Logger.Crash(e);
            }
        }

        /// <summary>
        /// Show the outfits menu.
        /// </summary>
        /// <param name="client"></param>
        [RemoteEvent]
        public void wardrobeOutfits(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            try
            {
                if (!IsUsable(dbPlayer)) return;

                MenuManager.Instance.Build(PlayerMenu.OutfitsMenu, dbPlayer).Show(dbPlayer);
            }
            catch (Exception e)
            {
                dbPlayer.SendNewNotification("Ein Fehler ist aufgetreten...");

                Logger.Crash(e);
            }
        }

        private static bool IsUsable(DbPlayer dbPlayer)
        {
            try
            {
                if (dbPlayer.Player.Position.DistanceTo(new Vector3(-79.7095, -811.279, 243.386)) < 3.0f) return true;

                if (dbPlayer.DimensionType[0] != DimensionType.House || !dbPlayer.HasData("inHouse")) return false;

                House iHouse;

                if ((iHouse = HouseModule.Instance.Get((uint)dbPlayer.GetData("inHouse"))) == null) return false;

                return !(iHouse.Interior.ClothesPosition.DistanceTo(dbPlayer.Player.Position) > 3.0f);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return false;
            }
        }
    }
}