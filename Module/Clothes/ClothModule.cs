using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VMP_CNR.Module.Clothes.Props;
using VMP_CNR.Module.Clothes.Team;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Jails;
using VMP_CNR.Module.Outfits;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Clothes
{
    public class ClothModule : SqlModule<ClothModule, Cloth, uint>
    {
        protected override string GetQuery()
        {
            return
                "SELECT `clothes`.*, IFNULL(`clothes_subitem`.`subcat_id`, -1) subcat_id " +
                "FROM `clothes` " +
                "LEFT JOIN `clothes_subitem` ON `clothes`.`id` = `clothes_subitem`.`cloth_id` " +
                "ORDER BY `clothes`.`default` DESC, `clothes`.`slot` ASC , `clothes`.`variation` ASC , `clothes`.`texture` ASC;";
        }

        public override Type[] RequiredModules()
        {
            return new[] {typeof(PropModule), typeof(TeamSkinModule)};
        }

        public Character.Character LoadCharacter(DbPlayer dbPlayer)
        {
            Character.Character character = null;

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM `player_character` WHERE player_id = '{dbPlayer.Id}' LIMIT 1;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            character = new Character.Character
                            {
                                PlayerId = reader.GetUInt32("player_id"),
                                Clothes = new Dictionary<int, uint>(),
                                ActiveClothes = new Dictionary<int, bool>(),
                                ActiveProps = new Dictionary<int, bool>(),
                                Wardrobe = new List<uint>(),
                                Props = new List<uint>(),
                                EquipedProps = new Dictionary<int, uint>()
                            };

                            string equipedClothesString = reader.GetString("equiped_clothes");
                            string equipedPropsString = reader.GetString("equiped_props");

                            // Migrating to New System
                            var wardrobeString = reader.GetString("wardrobe");
                            if (!string.IsNullOrEmpty(wardrobeString))
                            {
                                foreach (var clothIdString in wardrobeString.Split(','))
                                {
                                    if (!uint.TryParse(clothIdString, out var clothId)) continue;
                                    if (clothId == 0) continue;
                                    character.Wardrobe.Add(clothId);
                                    MySQLHandler.ExecuteAsync(
                                        $"INSERT INTO player_ownedclothes (`player_id`, `clothes_id`) VALUES ('{dbPlayer.Id}','{clothId}')");
                                }
                            }

                            var propsString = reader.GetString("props");
                            if (!string.IsNullOrEmpty(propsString))
                            {
                                foreach (var propIdString in propsString.Split(','))
                                {
                                    if (!uint.TryParse(propIdString, out var propId)) continue;
                                    if (propId == 0) continue;
                                    // first load needed be old ones...
                                    character.Props.Add(propId);
                                    MySQLHandler.ExecuteAsync(
                                        $"INSERT INTO player_ownedprops (`player_id`, `prop_id`) VALUES ('{dbPlayer.Id}','{propId}')");
                                }
                            }

                            // After that clear that slots
                            MySQLHandler.ExecuteAsync(
                                $"UPDATE player_character SET wardrobe = '', props = '' WHERE player_id = '{dbPlayer.Id}'");


                            var skin = reader.GetString("skin");
                            if (!string.IsNullOrEmpty(skin) && Enum.TryParse(skin, out PedHash skinHash) && skin != "")
                            {
                                character.Skin = skinHash;
                            }
                            else
                            {
                                if (dbPlayer.Customization == null)
                                {
                                    dbPlayer.Customization = new Customization.CharacterCustomization();

                                    SaveNewCustomization(dbPlayer);
                                    character.Skin = dbPlayer.Customization.Gender == 0
                                        ? PedHash.FreemodeMale01
                                        : PedHash.FreemodeFemale01;
                                }
                                else
                                {
                                    character.Skin = dbPlayer.Customization.Gender == 0
                                        ? PedHash.FreemodeMale01
                                        : PedHash.FreemodeFemale01;
                                }
                            }


                            // Clothes
                            if (!string.IsNullOrEmpty(equipedClothesString))
                            {
                                var splittedClothes = equipedClothesString.Split(',');

                                foreach (var clothIdString in splittedClothes)
                                {
                                    if (!uint.TryParse(clothIdString, out uint clothId)) continue;

                                    if (character.Skin == PedHash.FreemodeMale01 ||
                                        character.Skin == PedHash.FreemodeFemale01)
                                    {
                                        Cloth cloth = this[clothId];
                                        if (cloth == null) continue;
                                        if (!character.Clothes.ContainsKey(cloth.Slot))
                                        {
                                            character.Clothes.Add(cloth.Slot, clothId);
                                        }
                                    }
                                }
                            }

                            // Props
                            if (!string.IsNullOrEmpty(equipedPropsString))
                            {
                                var splittedProps = equipedPropsString.Split(',');

                                foreach (var propsIdString in splittedProps)
                                {
                                    if (!uint.TryParse(propsIdString, out uint accessoryId)) continue;

                                    if (character.Skin == PedHash.FreemodeMale01 ||
                                        character.Skin == PedHash.FreemodeFemale01)
                                    {
                                        Prop prop = PropModule.Instance[accessoryId];
                                        if (prop == null) continue;
                                        if (!character.EquipedProps.ContainsKey(prop.Slot))
                                        {
                                            character.EquipedProps.Add(prop.Slot, prop.Id);
                                        }
                                    }
                                }
                            }

                            // Check abusing trough old equipped stuff
                            /*foreach(KeyValuePair<int, uint> kvp in character.EquipedProps.ToList())
                            {
                                Prop prop = null;

                                // Schauen wir mal ob er Spieler acces drauf hat...
                                if (PropModule.Instance.GetAll().ContainsKey(kvp.Value))
                                {
                                    prop = PropModule.Instance.Get(kvp.Value);
                                }

                                if(prop == null || (!character.Props.Contains(prop.Id) && prop.TeamId != dbPlayer.TeamId))
                                {
                                    Prop defaultProp = PropModule.Instance.GetAll().Values.Where(c => c.IsDefault && c.Slot == kvp.Key).FirstOrDefault();

                                    if (defaultProp == null) continue; // why ever this could happen, we interrupt ?

                                    // ich besitze das garnicht also darf ich es auch nicht haben ^-*.*-^
                                    character.EquipedProps[kvp.Key] = defaultProp.Id;
                                }
                            }

                            // Check abusing trough old equipped stuff
                            foreach (KeyValuePair<int, uint> kvp in character.Clothes.ToList())
                            {
                                Cloth cloth = null;

                                // Schauen wir mal ob er Spieler acces drauf hat...
                                if (ClothModule.Instance.GetAll().ContainsKey(kvp.Value))
                                {
                                    cloth = ClothModule.Instance.Get(kvp.Value);
                                }

                                if (cloth == null || (!character.Wardrobe.Contains(cloth.Id) && !cloth.Teams.Contains(dbPlayer.TeamId)))
                                {
                                    Cloth defaultCloth = ClothModule.Instance.GetAll().Values.Where(c => c.IsDefault && c.Slot == kvp.Key).FirstOrDefault();

                                    if (defaultCloth == null) continue; // why ever this could happen, we interrupt ?

                                    // ich besitze das garnicht also darf ich es auch nicht haben ^-*.*-^
                                    character.Clothes[kvp.Key] = defaultCloth.Id;
                                }
                            }*/
                        }
                    }
                }
            }

            if (character != null && character.Wardrobe != null)
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"SELECT * FROM `player_ownedclothes` WHERE player_id = '{dbPlayer.Id}';";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                character.Wardrobe.Add(reader.GetUInt32("clothes_id"));
                            }
                        }
                    }
                }
            }

            if (character != null && character.Props != null)
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"SELECT * FROM `player_ownedprops` WHERE player_id = '{dbPlayer.Id}';";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                character.Props.Add(reader.GetUInt32("prop_id"));
                            }
                        }
                    }
                }
            }

            if (character == null)
            {
                character = CreateCharacterForPlayer(dbPlayer);
            }
            else
            {
                if (character.Clothes.Count <= 0)
                    LoadCharacterClothes(dbPlayer, character);

                if (character.EquipedProps.Count <= 0)
                    LoadCharacterProps(dbPlayer, character);
            }

            return character;
        }

        public void LoadCharacterClothes(DbPlayer dbPlayer, Character.Character character)
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT cloth_id FROM `player_clothes` WHERE player_id = '{dbPlayer.Id}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return;
                    while (reader.Read())
                    {
                        var clothId = reader.GetUInt32("cloth_id");
                        if (character.Skin == PedHash.FreemodeMale01 || character.Skin == PedHash.FreemodeFemale01)
                        {
                            var cloth = this[clothId];
                            if (cloth == null) continue;
                            if (!character.Clothes.ContainsKey(cloth.Slot))
                            {
                                character.Clothes.Add(cloth.Slot, clothId);
                            }
                        }
                    }
                }
            }
        }

        public static void ActualizeCharacterProps(DbPlayer dbPlayer)
        {
            var l_EquippedProps = dbPlayer.Character.EquipedProps;
            if (l_EquippedProps != null && l_EquippedProps.Count > 0)
            {
                foreach (var kvp in l_EquippedProps)
                {
                    if (dbPlayer.IsFreeMode())
                    {
                        var prop = PropModule.Instance[kvp.Value];
                        if (prop == null) continue;
                        dbPlayer.Player.SetAccessories(kvp.Key, prop.Variation, prop.Texture);
                    }
                }
            }
        }

        public static void LoadCharacterProps(DbPlayer dbPlayer, Character.Character character)
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT prop_id FROM `player_props` WHERE player_id = '{dbPlayer.Id}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return;
                    while (reader.Read())
                    {
                        var accessoryId = reader.GetUInt32("prop_id");
                        if (character.Skin == PedHash.FreemodeMale01 || character.Skin == PedHash.FreemodeFemale01)
                        {
                            var prop = PropModule.Instance[accessoryId];
                            if (prop == null) continue;
                            if (!character.EquipedProps.ContainsKey(prop.Slot))
                            {
                                character.EquipedProps.Add(prop.Slot, prop.Id);
                            }
                        }
                    }
                }
            }
        }

        public static void AddNewCloth(DbPlayer dbPlayer, uint ClothId)
        {
            dbPlayer.Character.Wardrobe.Add(ClothId);
            MySQLHandler.ExecuteAsync(
                $"INSERT IGNORE INTO player_ownedclothes (`player_id`, `clothes_id`) VALUES ('{dbPlayer.Id}','{ClothId}')");
        }

        public static void AddNewProp(DbPlayer dbPlayer, uint PropId)
        {
            dbPlayer.Character.Props.Add(PropId);
            MySQLHandler.ExecuteAsync(
                $"INSERT IGNORE INTO `player_ownedprops` (`player_id`, `prop_id`) VALUES ('{dbPlayer.Id}', '{PropId}')");
        }

        public static void SaveCharacter(DbPlayer dbPlayer)
        {
            var character = dbPlayer.Character;
            if (character == null) return;

            var query = $"UPDATE `player_character` SET skin = '{Enum.GetName(typeof(PedHash), character.Skin)}', " +
                        $"`equiped_clothes` = '{string.Join(",", character.Clothes.Values)}', " +
                        $"`equiped_props` = '{string.Join(",", character.EquipedProps.Values)}'WHERE player_id = '{dbPlayer.Id}';";
            MySQLHandler.ExecuteAsync(query);
        }

        public static Character.Character CreateCharacterForPlayer(DbPlayer dbPlayer)
        {
            dbPlayer.SetSkin(PedHash.FreemodeMale01);
            var character = new Character.Character
            {
                PlayerId = dbPlayer.Id,
                Clothes = new Dictionary<int, uint>(),
                Wardrobe = new List<uint>(),
                Props = new List<uint>(),
                EquipedProps = new Dictionary<int, uint>(),
                Skin = PedHash.FreemodeMale01
            };

            if (dbPlayer.Customization == null) dbPlayer.Customization = new Customization.CharacterCustomization();

            MySQLHandler.ExecuteAsync(
                $"INSERT INTO player_character (`player_id`, customization) VALUES ('{dbPlayer.Id}', '{JsonConvert.SerializeObject(dbPlayer.Customization)}')");

            dbPlayer.SendNewNotification(
                "Sie besitzen noch keinen Character, nutzen Sie die Schoenheitsklinik! (GPS)", title: "Info",
                notificationType: PlayerNotification.NotificationType.INFO);

            return character;
        }

        public static void SaveNewCustomization(DbPlayer dbPlayer)
        {
            if (dbPlayer.Customization == null) dbPlayer.Customization = new Customization.CharacterCustomization();

            MySQLHandler.ExecuteAsync(
                $"INSERT INTO player_character (`player_id`, customization) VALUES ('{dbPlayer.Id}', '{JsonConvert.SerializeObject(dbPlayer.Customization)}')");
        }

        public void TogglePlayerMask(DbPlayer dbPlayer)
        {
            if (!dbPlayer.Character.Clothes.ContainsKey(1)) return;
            //var active = player.GetClothesDrawable(1) != 0;
            var maskId = dbPlayer.Character.Clothes[1];
            var mask = Instance[maskId];
            if (mask == null) return;
            var player = dbPlayer.Player;

            if (dbPlayer.HasData("maskJNope"))
            {
                dbPlayer.SetClothes(1, 0, 0);
                dbPlayer.ResetData("maskJNope");
            }
            else
            {
                dbPlayer.SetClothes(1, mask.Variation, mask.Texture);
                if (!dbPlayer.HasData("maskJNope")) dbPlayer.SetData("maskJNope", true);
            }
        }

        public List<Cloth> FilterByTeam(List<Cloth> clothes, int team)
        {
            return clothes.Where<Cloth>(cloth => cloth.Teams.Contains((uint) team)).ToList();
        }

        public List<Cloth> GetBySlot(int slot)
        {
            return GetAll().Values.Where<Cloth>(cloth => cloth.Slot == slot).ToList();
        }

        public List<Cloth> GetTeamWarerobe(DbPlayer dbPlayer, int slot)
        {
            List<Cloth> wardrobeClothes = new List<Cloth>();
            var character = dbPlayer.Character;
            if (character == null) return wardrobeClothes;
            if (dbPlayer.Customization == null) return wardrobeClothes;

            if (slot == 3)
            {
                foreach (var cloth in GetAll().Values)
                {
                    if ((cloth.Teams.Contains((int) teams.TEAM_CIVILIAN) || cloth.Teams.Contains(dbPlayer.TeamId)) &&
                        cloth.Slot == 3 &&
                        cloth.Gender == dbPlayer.Customization.Gender)
                    {
                        wardrobeClothes.Add(cloth);
                    }
                }

                return wardrobeClothes;
            }

            foreach (var cloth in GetAll().Values)
            {
                if (cloth.Slot != slot) continue;
                if (!cloth.IsDefault && !cloth.Teams.Contains(dbPlayer.TeamId)) continue;
                if (cloth.Gender != 3 && cloth.Gender != dbPlayer.Customization.Gender) continue;
                wardrobeClothes.Add(cloth);
            }

            if (character.Wardrobe != null && character.Wardrobe.Count > 0)
            {
                foreach (var clothId in character.Wardrobe.ToList())
                {
                    var cloth = this[clothId];
                    if (cloth?.Slot != slot ||
                        !cloth.Teams.Contains((int) teams.TEAM_CIVILIAN) && !cloth.Teams.Contains(dbPlayer.TeamId) ||
                        cloth.Gender != 3 && cloth.Gender != dbPlayer.Customization.Gender) continue;
                    if (!wardrobeClothes.Contains(cloth))
                    {
                        wardrobeClothes.Add(cloth);
                    }
                }
            }

            return wardrobeClothes;
        }

        public List<Cloth> GetWardrobeBySlot(DbPlayer dbPlayer, Character.Character character, int slot)
        {
            var wardrobeClothes = new List<Cloth>();

            if (dbPlayer.Customization == null) return wardrobeClothes;

            if (slot == 3)
            {
                foreach (var cloth in GetAll().Values)
                {
                    if ((cloth.Teams.Contains((int) teams.TEAM_CIVILIAN) || cloth.Teams.Contains(dbPlayer.TeamId)) &&
                        cloth.Slot == 3 &&
                        cloth.Gender == dbPlayer.Customization.Gender)
                    {
                        wardrobeClothes.Add(cloth);
                    }
                }

                return wardrobeClothes;
            }

            foreach (var cloth in GetAll().Values)
            {
                if (cloth.IsDefault && cloth.Slot == slot &&
                    (cloth.Gender == 3 || cloth.Gender == dbPlayer.Customization.Gender))
                {
                    wardrobeClothes.Add(cloth);
                }
            }

            foreach (var clothId in character.Wardrobe.ToList())
            {
                var cloth = this[clothId];
                if (cloth?.Slot != slot ||
                    !cloth.Teams.Contains((int) teams.TEAM_CIVILIAN) && !cloth.Teams.Contains(dbPlayer.TeamId) ||
                    cloth.Gender != 3 && cloth.Gender != dbPlayer.Customization.Gender) continue;
                if (!wardrobeClothes.Contains(cloth))
                {
                    wardrobeClothes.Add(cloth);
                }
            }

            return wardrobeClothes;
        }

        public void ResetClothes(DbPlayer dbPlayer)
        {
            if (dbPlayer.HasData("clothShopId")) ApplyPlayerClothes(dbPlayer);
        }

        public void ApplyPlayerClothes(DbPlayer dbPlayer)
        {
            NAPI.Task.Run(() =>
            {
                dbPlayer.Player.ClearAccessory(0);
                dbPlayer.Player.ClearAccessory(1);
                dbPlayer.Player.ClearAccessory(2);
                dbPlayer.Player.ClearAccessory(6);
                dbPlayer.Player.ClearAccessory(7);

                var character = dbPlayer.Character;
                if (character == null)
                {
                    dbPlayer.SetSkin(PedHash.FreemodeMale01);
                    return;
                }

                var clotheslist = character.Clothes.ToList();
                var equipedAccessoires = character.EquipedProps;

                if (clotheslist != null && clotheslist.Count > 0)
                {
                    foreach (var kvp in clotheslist)
                    {
                        if (dbPlayer.IsFreeMode())
                        {
                            if (!Contains(kvp.Value)) continue;
                            var cloth = this[kvp.Value];
                            if (cloth == null) continue;
                            if (cloth.Slot == 1)
                                dbPlayer.SetClothes(1, 0, 0);
                            else
                                dbPlayer.SetClothes(kvp.Key, cloth.Variation, cloth.Texture);
                        }
                        else
                        {
                            var cloth = TeamSkinModule.Instance.GetTeamCloth(kvp.Value);
                            if (cloth == null) continue;
                            if (cloth.Slot == 1)
                                dbPlayer.SetClothes(1, 0, 0);
                            else
                                dbPlayer.SetClothes(kvp.Key, cloth.Variation, cloth.Texture);
                        }
                    }
                }

                if (equipedAccessoires != null && equipedAccessoires.Count > 0)
                {
                    foreach (var kvp in equipedAccessoires)
                    {
                        if (dbPlayer.IsFreeMode())
                        {
                            var prop = PropModule.Instance[kvp.Value];
                            if (prop == null) continue;
                            SetPlayerAccessories(dbPlayer, kvp.Key, prop.Variation, prop.Texture);
                        }
                        else
                        {
                            var prop = TeamSkinModule.Instance.GetTeamProp(kvp.Value);
                            if (prop == null) continue;
                            SetPlayerAccessories(dbPlayer, kvp.Key, prop.Variation, prop.Texture);
                        }
                    }
                }
                else
                {
                    dbPlayer.Player.ClearAccessory(0);
                    dbPlayer.Player.ClearAccessory(1);
                    dbPlayer.Player.ClearAccessory(2);
                    dbPlayer.Player.ClearAccessory(6);
                    dbPlayer.Player.ClearAccessory(7);
                }

                if (dbPlayer.IsFreeMode())
                {
                    dbPlayer.ApplyArmorVisibility();

                    if (dbPlayer.JailTime[0] > 0)
                    {
                        dbPlayer.SetPlayerJailClothes();
                    }
                }
            });
        }

        // TODO: Umbauen und NAPI.Task verkleinern
        public void RefreshPlayerClothes(DbPlayer dbPlayer, bool ignoreOutfit = false)
        {
            if (dbPlayer.HasData("clonePerson"))
            {
                DbPlayer target = Players.Players.Instance.GetByDbId(dbPlayer.GetData("clonePerson"));
                if (target != null && target.IsValid())
                {
                    RefreshPlayerClothesFromOther(dbPlayer, target);
                    return;
                }
            }

            if (!ignoreOutfit && dbPlayer.HasData("outfitactive"))
            {
                Player player = dbPlayer.Player;
                int outfitId = dbPlayer.GetData("outfitactive");
                bool overwrite = true;

                // find outfit by id and actual gender
                Outfit outfit = OutfitModule.Instance.GetAll().Values.FirstOrDefault(o =>
                    ((PedHash)player.Model == PedHash.FreemodeMale01 ? o.DataId : o.DataId - 1) == outfitId &&
                    o.Male == ((PedHash)player.Model == PedHash.FreemodeMale01)
                        ? true
                        : false);
                if (outfit == null) return;

                if (overwrite) dbPlayer.Character.Clothes.Clear();

                foreach (OutfitComponent kvp in outfit.Components)
                {
                    dbPlayer.SetClothes(kvp.Slot, kvp.Component, kvp.Texture);
                    if (overwrite) dbPlayer.Character.Clothes.Add(kvp.Slot, OutfitsModule.Instance.GetPropValue(kvp.Id));
                }

                // clear all
                NAPI.Task.Run(() =>
                {
                    player.ClearAccessory(0);
                    player.ClearAccessory(1);
                    player.ClearAccessory(2);
                    player.ClearAccessory(6);
                    player.ClearAccessory(7);

                    if (overwrite) dbPlayer.Character.EquipedProps.Clear();

                    foreach (OutfitProp kvp in outfit.Props)
                    {
                        player.ClearAccessory(kvp.Slot);
                        player.SetAccessories(kvp.Slot, kvp.Component, kvp.Texture);
                        if (overwrite)
                            dbPlayer.Character.EquipedProps.Add(kvp.Slot, OutfitsModule.Instance.GetPropValue(kvp.Id, true));
                    }
                    
                    // Fix Hair
                    // Set Hair
                    dbPlayer.SetClothes(2, dbPlayer.Customization.Hair.Hair, 0);
                    NAPI.Player.SetPlayerHairColor(player, dbPlayer.Customization.Hair.Color,
                        dbPlayer.Customization.Hair.HighlightColor);
                });
                
                return;
            }

            var character = dbPlayer.Character;
            if (character == null) return;
            var clotheslist = character.Clothes.ToList();
            var equipedAccessoires = character.EquipedProps.ToList();

            var Armor = dbPlayer.Player.Armor;
            dbPlayer.SetArmorPlayer(Armor);

            if (clotheslist != null && clotheslist.Count > 0)
            {
                foreach (var kvp in clotheslist)
                {
                    if (dbPlayer.IsFreeMode())
                    {
                        var cloth = this[kvp.Value];
                        if (cloth == null) continue;
                        if (cloth.Slot == 1)
                            dbPlayer.SetClothes(1, 0, 0);
                        else
                            dbPlayer.SetClothes(kvp.Key, cloth.Variation, cloth.Texture);
                    }
                    else
                    {
                        var cloth = TeamSkinModule.Instance.GetTeamCloth(kvp.Value);
                        if (cloth == null) continue;
                        if (cloth.Slot == 1)
                            dbPlayer.SetClothes(1, 0, 0);
                        else
                            dbPlayer.SetClothes(kvp.Key, cloth.Variation, cloth.Texture);
                    }
                }
            }

            NAPI.Task.Run(() =>
            {
                // TODO CLEAR ACCESOIRES
                dbPlayer.Player.ClearAccessory(0);
                dbPlayer.Player.ClearAccessory(1);
                dbPlayer.Player.ClearAccessory(2);
                dbPlayer.Player.ClearAccessory(6);
                dbPlayer.Player.ClearAccessory(7);
            });

            if (equipedAccessoires != null && equipedAccessoires.Count > 0)
            {
                foreach (var kvp in equipedAccessoires)
                {
                    if (dbPlayer.IsFreeMode())
                    {
                        var prop = PropModule.Instance[kvp.Value];
                        if (prop == null) continue;
                        SetPlayerAccessories(dbPlayer, kvp.Key, prop.Variation, prop.Texture);
                    }
                    else
                    {
                        var prop = TeamSkinModule.Instance.GetTeamProp(kvp.Value);
                        if (prop == null) continue;
                        SetPlayerAccessories(dbPlayer, kvp.Key, prop.Variation, prop.Texture);
                    }
                }
            }
            else
            {
                NAPI.Task.Run(() =>
                {
                    dbPlayer.Player.ClearAccessory(0);
                    dbPlayer.Player.ClearAccessory(1);
                    dbPlayer.Player.ClearAccessory(2);
                    dbPlayer.Player.ClearAccessory(6);
                    dbPlayer.Player.ClearAccessory(7);
                });
            }

            if (dbPlayer.IsFreeMode())
            {
                RefreshPlayerSpecials(dbPlayer, character);
            }
        }

        public void RefreshPlayerClothesFromOther(DbPlayer dbPlayer, DbPlayer target)
        {
            var character = target.Character;
            if (character == null) return;
            var clotheslist = character.Clothes.ToList();
            var equipedAccessoires = character.EquipedProps.ToList();

            var Armor = dbPlayer.Player.Armor;
            dbPlayer.SetArmorPlayer(Armor);

            if (clotheslist != null && clotheslist.Count > 0)
            {
                foreach (var kvp in clotheslist)
                {
                    if (target.IsFreeMode())
                    {
                        var cloth = this[kvp.Value];
                        if (cloth == null) continue;
                        if (cloth.Slot == 1)
                            dbPlayer.SetClothes(1, 0, 0);
                        else
                            dbPlayer.SetClothes(kvp.Key, cloth.Variation, cloth.Texture);
                    }
                    else
                    {
                        var cloth = TeamSkinModule.Instance.GetTeamCloth(kvp.Value);
                        if (cloth == null) continue;
                        if (cloth.Slot == 1)
                            dbPlayer.SetClothes(1, 0, 0);
                        else
                            dbPlayer.SetClothes(kvp.Key, cloth.Variation, cloth.Texture);
                    }
                }
            }

            // TODO CLEAR ACCESOIRES

            if (equipedAccessoires != null && equipedAccessoires.Count > 0)
            {
                foreach (var kvp in equipedAccessoires)
                {
                    if (target.IsFreeMode())
                    {
                        var prop = PropModule.Instance[kvp.Value];
                        if (prop == null) continue;
                        SetPlayerAccessories(dbPlayer, kvp.Key, prop.Variation, prop.Texture);
                    }
                    else
                    {
                        var prop = TeamSkinModule.Instance.GetTeamProp(kvp.Value);
                        if (prop == null) continue;
                        SetPlayerAccessories(dbPlayer, kvp.Key, prop.Variation, prop.Texture);
                    }
                }
            }
            else
            {
                NAPI.Task.Run(() =>
                {
                    dbPlayer.Player.ClearAccessory(0);
                    dbPlayer.Player.ClearAccessory(1);
                    dbPlayer.Player.ClearAccessory(2);
                    dbPlayer.Player.ClearAccessory(6);
                    dbPlayer.Player.ClearAccessory(7);
                });
            }

            if (dbPlayer.IsFreeMode())
            {
                RefreshPlayerSpecials(dbPlayer, character);
            }
        }

        public void SetPlayerAccessories(DbPlayer dbPlayer, int prop, int variation, int texture)
        {
            NAPI.Task.Run(() =>
            {
                dbPlayer.Player.SetAccessories(prop, variation, texture);
            });
        }

        public void RefreshPlayerSpecials(DbPlayer dbPlayer, Character.Character character)
        {
            dbPlayer.ApplyArmorVisibility();
            dbPlayer.RefreshNacked();

            // Jail
            if (dbPlayer.JailTime[0] > 0 && dbPlayer.Player.Position.DistanceTo(JailModule.PrisonZone) < JailModule.Range)
            {
                dbPlayer.SetPlayerJailClothes();
            }
        }

        public List<Cloth> GetClothesForShop(uint shopId)
        {
            return (from cloth in GetAll()
                where cloth.Value != null && (cloth.Value.StoreId == shopId || cloth.Value.StoreId == -1 || cloth.Value.IsDefault)
                select cloth.Value).ToList();
        }
    }

    public static class ClothesExtensions
    {
        public static void SetClothes(this DbPlayer dbPlayer, int slot, int drawable, int texture)
        {
            if (drawable > 255)
            {
                Players.Players.Instance.TriggerNewClientInRange(dbPlayer, "setCloth", dbPlayer.Player, slot, drawable,
                    texture);
            }
            else
            {
                NAPI.Task.Run(() =>
                {
                    dbPlayer.Player.SetClothes(slot, drawable, texture);
                });
            }

            var currentClothes = dbPlayer.HasData("clothes")
                ? dbPlayer.GetData("clothes")
                : new Dictionary<int, List<int>>();
            var clothValues = new List<int>() {drawable, texture};

            if (currentClothes.ContainsKey(slot))
            {
                currentClothes[slot] = clothValues;
            }
            else
            {
                currentClothes.Add(slot, clothValues);
            }

            dbPlayer.SetData("clothes", currentClothes);
        }
    }
}