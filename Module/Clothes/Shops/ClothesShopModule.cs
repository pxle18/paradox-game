using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Clothes.Props;
using VMP_CNR.Module.Clothes.Slots;
using VMP_CNR.Module.Clothes.Windows;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Events;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR.Module.Clothes.Shops
{
    public class ClothesShopModule : Module<ClothesShopModule>
    {
        public const int MaxSlots = 12;

        public Dictionary<int, ClothesSlot> clothesSlots;

        public Dictionary<int, PropsSlot> propsSlots;

        private Dictionary<uint, ClothesShop> shops;

        public override Type[] RequiredModules()
        {
            return new[] {typeof(ClothModule), typeof(PropModule), typeof(EventModule)};
        }

        protected override bool OnLoad()
        {
            try
            {
                clothesSlots = LoadClothesSlots();

                propsSlots = LoadPropsSlots();

                shops = LoadShops();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return true;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer == null || dbPlayer.RageExtension.IsInVehicle || !dbPlayer.IsValid()) return false;

            try
            {
                if (!dbPlayer.HasData("clothShopId")) return false;

                uint shopId = dbPlayer.GetData("clothShopId");

                ClothesShop currentShop = Instance.GetShopById(shopId);

                if (currentShop == null) return false;

                if (currentShop.EventID > 0 && !EventModule.Instance.IsEventActive(currentShop.EventID)) return false;

                if (currentShop.CWSId > 0)
                {
                    CWS cws = CWSModule.Instance.Get(currentShop.CWSId);

                    if (cws != null)
                    {
                        dbPlayer.SendNewNotification($"An diesem Geschäft zahlen Sie mit {cws.Name}-Punkten!");
                    }
                }

                ComponentManager.Get<ClothesShopWindow>().Show()(
                    dbPlayer,
                    new[]
                        {
                            currentShop.GetClothesSlotsForPlayer(dbPlayer).ToDictionary(
                                keyValuePair => keyValuePair.Key.ToString(),
                                keyValuePair => keyValuePair.Value.Name
                            ),
                            currentShop.GetPropsSlotsForPlayer(dbPlayer).ToDictionary(
                                keyValuePair => $"p-{keyValuePair.Key}",
                                keyValuePair => keyValuePair.Value.Name
                            )
                        }
                        .SelectMany(dict => dict)
                        .ToDictionary(pair => pair.Key, pair => pair.Value)
                        .Select(kvp => new Slot(kvp.Key, kvp.Value))
                        .ToList(),
                    currentShop.Name
                );
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return true;
        }

        public Dictionary<uint, ClothesShop> LoadShops()
        {
            var shopList = new Dictionary<uint, ClothesShop>();

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = "SELECT * FROM clothes_shops WHERE !(pos_x = 0) AND activated = 1;";
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return shopList;

                    while (reader.Read())
                    {
                        var shopId = reader.GetUInt32(0);

                        var shopSlotClothes = new Dictionary<int, List<Cloth>>();
                        var shopSlotProps = new Dictionary<int, List<Prop>>();

                        var shopPropsSlots = new Dictionary<int, PropsSlot>();
                        var shopClothesSlots = new Dictionary<int, ClothesSlot>();

                        var shopClothes = ClothModule.Instance.GetClothesForShop(shopId);
                        var shopProps = PropModule.Instance.GetPropsForShop(shopId);

                        // Handle Clothes.
                        foreach (Cloth cloth in shopClothes)
                        {
                            if (!shopClothesSlots.ContainsKey(cloth.Slot))
                            {
                                if (clothesSlots.ContainsKey(cloth.Slot))
                                {
                                    shopClothesSlots.Add(cloth.Slot, clothesSlots[cloth.Slot]);
                                }
                            }

                            if (shopSlotClothes.ContainsKey(cloth.Slot))
                            {
                                shopSlotClothes[cloth.Slot].Add(cloth);
                            }
                            else
                            {
                                shopSlotClothes.Add(cloth.Slot, new List<Cloth>());
                                shopSlotClothes[cloth.Slot].Add(cloth);
                            }
                        }

                        // Handle Props.
                        foreach (Prop prop in shopProps)
                        {
                            if (!shopPropsSlots.ContainsKey(prop.Slot))
                            {
                                if (propsSlots.ContainsKey(prop.Slot))
                                {
                                    shopPropsSlots.Add(prop.Slot, propsSlots[prop.Slot]);
                                }
                            }

                            if (shopSlotProps.ContainsKey(prop.Slot))
                            {
                                shopSlotProps[prop.Slot].Add(prop);
                            }
                            else
                            {
                                shopSlotProps.Add(prop.Slot, new List<Prop>());
                                shopSlotProps[prop.Slot].Add(prop);
                            }
                        }

                        var clothesShop = new ClothesShop(
                            reader,
                            shopSlotClothes,
                            shopSlotProps,
                            shopClothesSlots,
                            shopPropsSlots
                        );

                        shopList.Add(clothesShop.Id, clothesShop);
                        OnShopSpawn(clothesShop);
                    }
                }

                conn.Close();
            }

            return shopList;
        }

        public Dictionary<int, ClothesSlot> LoadClothesSlots()
        {
            var slotList = new Dictionary<int, ClothesSlot>();

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = "SELECT " +
                                  "cslots.id AS slotId, " +
                                  "cslots.name AS slotName, " +
                                  "IFNULL(csubcats.id, -1) AS subCatId, " +
                                  "IFNULL(csubcats.name, '') AS subCatName " +
                                  "FROM clothes_slots cslots " +
                                  "LEFT JOIN clothes_subcats csubcats ON cslots.id=csubcats.cat_id " +
                                  "ORDER BY cslots.order;";

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return slotList;

                    var slot = -1;
                    var list = new List<SlotCategory>();
                    var name = "";

                    while (reader.Read())
                    {
                        var currentSlot = reader.GetInt32("slotId");
                        var currentName = reader.GetString("slotName");

                        if (slot == -1)
                        {
                            slot = currentSlot;
                            name = currentName;
                        }

                        if (currentSlot != slot)
                        {
                            list.Add(new SlotCategory(-1, "Ohne Kategorie"));
                            slotList.Add(slot, new ClothesSlot(slot, name, list));

                            slot = currentSlot;
                            name = currentName;

                            list = new List<SlotCategory>();
                        }

                        var subCatId = reader.GetInt32("subCatId");
                        var subCatName = reader.GetString("subCatName");

                        if (string.IsNullOrEmpty(subCatName)) continue;

                        list.Add(new SlotCategory(
                            subCatId,
                            subCatName
                        ));
                    }
                }

                conn.Close();
            }

            return slotList;
        }

        public Dictionary<int, PropsSlot> LoadPropsSlots()
        {
            var slotList = new Dictionary<int, PropsSlot>();

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = "SELECT " +
                                  "pslots.id AS slotId, " +
                                  "pslots.name AS slotName, " +
                                  "IFNULL(psubcats.id, -1) AS subCatId, " +
                                  "IFNULL(psubcats.name, '') AS subCatName " +
                                  "FROM props_slots pslots " +
                                  "LEFT JOIN props_subcats psubcats ON pslots.id=psubcats.cat_id " +
                                  "ORDER BY pslots.order;";

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return slotList;

                    var slot = -1;
                    var list = new List<SlotCategory>();
                    var name = "";

                    while (reader.Read())
                    {
                        var currentSlot = reader.GetInt32("slotId");
                        var currentName = reader.GetString("slotName");

                        if (slot == -1)
                        {
                            slot = currentSlot;
                            name = currentName;
                        }

                        if (currentSlot != slot)
                        {
                            list.Add(new SlotCategory(-1, "Ohne Kategorie"));
                            slotList.Add(slot, new PropsSlot(slot, name, list));

                            slot = currentSlot;
                            name = currentName;

                            list = new List<SlotCategory>();
                        }

                        var subCatId = reader.GetInt32("subCatId");
                        var subCatName = reader.GetString("subCatName");

                        if (string.IsNullOrEmpty(subCatName)) continue;

                        list.Add(new SlotCategory(
                            subCatId,
                            subCatName
                        ));
                    }
                }

                conn.Close();
            }

            return slotList;
        }

        public Dictionary<int, ClothesSlot> GetClothesSlots()
        {
            return clothesSlots;
        }

        public Dictionary<int, PropsSlot> GetPropsSlots()
        {
            return propsSlots;
        }

        public ClothesShop GetShopById(uint shopId)
        {
            return shops.TryGetValue(shopId, out ClothesShop rob) ? rob : null;
        }

        public int GetActualClothesPrice(DbPlayer dbPlayer)
        {
            var maxprice = 0;
            var character = dbPlayer.Character;
            for (var i = 0; i < MaxSlots; i++)
            {
                if (!dbPlayer.HasData("clothesActualItem-" + i)) continue;

                if (!ClothModule.Instance.Contains((uint) dbPlayer.GetData("clothesActualItem-" + i))) continue;

                var cloth =
                    ClothModule.Instance[(uint) dbPlayer.GetData("clothesActualItem-" + i)];
                if (!character.Wardrobe.Contains(cloth.Id))
                {
                    maxprice += cloth.Price;
                }
            }

            for (var i = 0; i < MaxSlots; i++)
            {
                if (!dbPlayer.HasData("propsActualItem-" + i)) continue;

                if (!PropModule.Instance.Contains((uint) dbPlayer.GetData("propsActualItem-" + i))) continue;

                var prop =
                    PropModule.Instance[(uint) dbPlayer.GetData("propsActualItem-" + i)];
                if (!character.Props.Contains(prop.Id))
                {
                    maxprice += prop.Price;
                }
            }

            return maxprice;
        }

        public void Dress(DbPlayer dbPlayer, List<ClothesShopWindow.SimpleCloth> wearing)
        {
            if (wearing == null) return;

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            try
            {
                foreach (ClothesShopWindow.SimpleCloth simpleCloth in wearing)
                {
                    if (simpleCloth == null) continue;

                    if (!int.TryParse(simpleCloth.Slot, out var slot)) continue; // why return??

                    Character.Character character = dbPlayer.Character;

                    if (simpleCloth.IsProp)
                    {
                        if (!character.EquipedProps.ContainsKey(slot))
                        {
                            character.EquipedProps.Add(slot, simpleCloth.Id);
                        }
                        else
                        {
                            character.EquipedProps[slot] = simpleCloth.Id;
                        }

                        continue;
                    }

                    character.Clothes[slot] = simpleCloth.Id;
                }

                ClothModule.SaveCharacter(dbPlayer);

                ClothModule.Instance.ApplyPlayerClothes(dbPlayer);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public void Buy(DbPlayer dbPlayer, List<ClothesShopWindow.SimpleCloth> cart)
        {
            if (cart == null) return;

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            try
            {
                Character.Character character = dbPlayer.Character;
                var wardrobe = character.Wardrobe;

                if (wardrobe == null || character == null || character.Props == null) return;

                foreach (ClothesShopWindow.SimpleCloth simpleCloth in cart)
                {
                    if (simpleCloth == null) continue;

                    if (!int.TryParse(simpleCloth.Slot, out var slot)) continue; // & hier nutzt du continue??

                    if (simpleCloth.IsProp)
                    {
                        if (!character.Props.Contains(simpleCloth.Id))
                        {
                            ClothModule.AddNewProp(dbPlayer, simpleCloth.Id);
                        }

                        continue;
                    }

                    if (!wardrobe.Contains(simpleCloth.Id))
                    {
                        ClothModule.AddNewCloth(dbPlayer, simpleCloth.Id);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public static void OnShopSpawn(ClothesShop shop)
        {
            if (shop.EventID > 0 && !EventModule.Instance.IsEventActive(shop.EventID))
                return;

            if (shop.Teamid == 0)
            {
                Blips.Create(shop.Position, "", 73, 1.0f);
            }
        }
    }
}