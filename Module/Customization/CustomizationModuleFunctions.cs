using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Assets.Tattoo;
using VMP_CNR.Module.Attachments;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Clothes.Character;
using VMP_CNR.Module.Einreiseamt;
using VMP_CNR.Module.Events.Halloween;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Customization
{
    public static class CustomizationModuleFunctions
    {
        public static readonly Vector3 creatorCharPos = new Vector3(402.8664, -996.4108, -99.00027);
        public static readonly Vector3 creatorPos = new Vector3(402.8664, -997.5515, -98.5);
        public static readonly Vector3 cameraLookAtPos = new Vector3(402.8664, -996.4108, -98.5);
        public const float FacingAngle = -185.0f;

        public static void ApplyCharacter(this DbPlayer dbPlayer, bool loadArmorHealthFromDBValue = false, bool ignoreOutfit = false)
        {
            if (!dbPlayer.IsFirstSpawn && !loadArmorHealthFromDBValue) dbPlayer.UpdatePlayerHealthAndArmor();

            if (dbPlayer.HasData("clonePerson"))
            {
                DbPlayer target = Players.Players.Instance.GetByDbId(dbPlayer.GetData("clonePerson"));
                if (target != null && target.IsValid())
                {
                    ApplyCharacterFromOther(dbPlayer, target);
                    return;
                }
            }

            if (HalloweenModule.isActive && dbPlayer.IsZombie())
            {
                dbPlayer.SetSkin((PedHash)HalloweenModule.Instance.GetZombieSkinRandom());
                return;
            }

            if (dbPlayer.Customization == null) dbPlayer.Customization = new CharacterCustomization();

            if ((PedHash)dbPlayer.Player.Model != PedHash.FreemodeFemale01 && (PedHash)dbPlayer.Player.Model != PedHash.FreemodeMale01) dbPlayer.SetSkin(dbPlayer.Character.Skin);

            var headBlend = new HeadBlend
            {
                ShapeFirst = dbPlayer.Customization.Parents.MotherShape,
                ShapeSecond = dbPlayer.Customization.Parents.FatherShape,
                ShapeThird = 0,
                SkinFirst = dbPlayer.Customization.Parents.MotherSkin,
                SkinSecond = dbPlayer.Customization.Parents.FatherSkin,
                SkinThird = 0,
                ShapeMix = dbPlayer.Customization.Parents.Similarity,
                SkinMix = dbPlayer.Customization.Parents.SkinSimilarity,
                ThirdMix = 0
            };

            var headOverlays = new Dictionary<int, HeadOverlay>(dbPlayer.Customization.Appearance.Length);

            for (int i = 0, length = dbPlayer.Customization.Appearance.Length; i < length; i++)
            {
                headOverlays[i] = new HeadOverlay
                {
                    Index = dbPlayer.Customization.Appearance[i].Value,
                    Opacity = dbPlayer.Customization.Appearance[i].Opacity,
                    Color = (byte)GetHeadOverlayColor(dbPlayer.Customization, i)
                };
            }

            List<Decoration> decorations = new List<Decoration>();
            foreach (uint assetsTattooId in dbPlayer.Customization.Tattoos)
            {
                if (!AssetsTattooModule.Instance.GetAll().ContainsKey(assetsTattooId)) continue;
                AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get(assetsTattooId);
                Decoration decoration = new Decoration();
                decoration.Collection = NAPI.Util.GetHashKey(assetsTattoo.Collection);
                decoration.Overlay = dbPlayer.Customization.Gender == 0 ? NAPI.Util.GetHashKey(assetsTattoo.HashMale) : NAPI.Util.GetHashKey(assetsTattoo.HashFemale);

                decorations.Add(decoration);
            }

            NAPI.Task.Run(() =>
            {
                NAPI.Player.SetPlayerCustomization(dbPlayer.Player, dbPlayer.Customization.Gender == 0, headBlend, dbPlayer.Customization.EyeColor,
                dbPlayer.Customization.Hair.Color, dbPlayer.Customization.Hair.HighlightColor,
                dbPlayer.Customization.Features, headOverlays, decorations.ToArray());

                for (var key = 0; key < dbPlayer.Customization.Features.Length; key++)
                {
                    NAPI.Player.SetPlayerFaceFeature(dbPlayer.Player, key, dbPlayer.Customization.Features[key]);
                }
            });

            // Set Hair
            dbPlayer.SetClothes(2, dbPlayer.Customization.Hair.Hair, 0);

            NAPI.Task.Run(() =>
            {
                NAPI.Player.SetPlayerHairColor(dbPlayer.Player, dbPlayer.Customization.Hair.Color, dbPlayer.Customization.Hair.HighlightColor);
            });

            ClothModule.Instance.RefreshPlayerClothes(dbPlayer, ignoreOutfit);

            // Remove Mask
            dbPlayer.SetClothes(1, 0, 0);

            //Resync Weapons
            dbPlayer.LoadPlayerWeapons();

            // Set to fist
            dbPlayer.GiveServerWeapon(WeaponHash.Unarmed, 1);

            dbPlayer.ApplyPlayerHealth();

            //Fix not visible Bug?
            NAPI.Task.Run(() => { dbPlayer.Player.Transparency = 255; });

            AttachmentModule.Instance.RemoveAllAttachments(dbPlayer);
            dbPlayer.SyncAttachmentOnlyItems();
        }

        public static void ApplyCharacterFromOther(this DbPlayer dbPlayer, DbPlayer destinationPlayer)
        {
            if (dbPlayer.Customization == null) dbPlayer.Customization = new CharacterCustomization();

            var armor = dbPlayer.Player.Armor;
            var health = dbPlayer.Player.Health;
            if ((PedHash)dbPlayer.Player.Model != PedHash.FreemodeFemale01 && (PedHash)dbPlayer.Player.Model != PedHash.FreemodeMale01) dbPlayer.Player.SetSkin(dbPlayer.Character.Skin);
            dbPlayer.SetArmorPlayer(armor);
            dbPlayer.SetHealth(health);

            var headBlend = new HeadBlend
            {
                ShapeFirst = destinationPlayer.Customization.Parents.MotherShape,
                ShapeSecond = destinationPlayer.Customization.Parents.FatherShape,
                ShapeThird = 0,
                SkinFirst = destinationPlayer.Customization.Parents.MotherSkin,
                SkinSecond = destinationPlayer.Customization.Parents.FatherSkin,
                SkinThird = 0,
                ShapeMix = destinationPlayer.Customization.Parents.Similarity,
                SkinMix = destinationPlayer.Customization.Parents.SkinSimilarity,
                ThirdMix = 0
            };

            var headOverlays = new Dictionary<int, HeadOverlay>(destinationPlayer.Customization.Appearance.Length);

            for (int i = 0, length = destinationPlayer.Customization.Appearance.Length; i < length; i++)
            {
                headOverlays[i] = new HeadOverlay
                {
                    Index = destinationPlayer.Customization.Appearance[i].Value,
                    Opacity = destinationPlayer.Customization.Appearance[i].Opacity,
                    Color = (byte)GetHeadOverlayColor(destinationPlayer.Customization, i)
                };
            }

            List<Decoration> decorations = new List<Decoration>();
            foreach (uint assetsTattooId in destinationPlayer.Customization.Tattoos)
            {
                if (!AssetsTattooModule.Instance.GetAll().ContainsKey(assetsTattooId)) continue;
                AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get(assetsTattooId);
                Decoration decoration = new Decoration();
                decoration.Collection = NAPI.Util.GetHashKey(assetsTattoo.Collection);
                decoration.Overlay = destinationPlayer.Customization.Gender == 0 ? NAPI.Util.GetHashKey(assetsTattoo.HashMale) : NAPI.Util.GetHashKey(assetsTattoo.HashFemale);

                decorations.Add(decoration);
            }

            NAPI.Task.Run(() =>
            {
                NAPI.Player.SetPlayerCustomization(dbPlayer.Player, dbPlayer.Customization.Gender == 0, headBlend, dbPlayer.Customization.EyeColor,
                    dbPlayer.Customization.Hair.Color, dbPlayer.Customization.Hair.HighlightColor,
                    dbPlayer.Customization.Features, headOverlays, decorations.ToArray());

                for (var key = 0; key < dbPlayer.Customization.Features.Length; key++)
                {
                    NAPI.Player.SetPlayerFaceFeature(dbPlayer.Player, key, dbPlayer.Customization.Features[key]);
                }
            });

            // Set Hair
            dbPlayer.SetClothes(2, destinationPlayer.Customization.Hair.Hair, 0);

            NAPI.Task.Run(() =>
            {
                NAPI.Player.SetPlayerHairColor(dbPlayer.Player, destinationPlayer.Customization.Hair.Color, destinationPlayer.Customization.Hair.HighlightColor);
            });

            ClothModule.Instance.RefreshPlayerClothes(dbPlayer);

            // Remove Mask
            dbPlayer.SetClothes(1, 0, 0);

            //Resync Weapons
            dbPlayer.LoadPlayerWeapons();

            // Set to fist
            dbPlayer.GiveServerWeapon(WeaponHash.Unarmed, 1);

            //Fix not visible Bug?
            dbPlayer.Player.Transparency = 255;
        }

        private static int GetHeadOverlayColor(CharacterCustomization customization, int overlayId)
        {
            switch (overlayId)
            {
                case 1:
                    return customization.BeardColor;
                case 2:
                    return customization.EyebrowColor;
                case 5:
                    return customization.BlushColor;
                case 8:
                    return customization.LipstickColor;
                case 10:
                    return customization.ChestHairColor;
                default:
                    return 0;
            }
        }

        public static void ClearDecorations(this DbPlayer dbPlayer)
        {
            dbPlayer.Player.TriggerNewClient("clearPlayerDecorations");
        }

        public static void ApplyDecorations(this DbPlayer dbPlayer)
        {
            dbPlayer.ClearDecorations();

            List<Decoration> decorations = new List<Decoration>();
            foreach (uint assetsTattooId in dbPlayer.Customization.Tattoos)
            {
                if (!AssetsTattooModule.Instance.GetAll().ContainsKey(assetsTattooId)) continue;
                AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get(assetsTattooId);
                Decoration decoration = new Decoration();
                decoration.Collection = NAPI.Util.GetHashKey(assetsTattoo.Collection);
                decoration.Overlay = dbPlayer.Customization.Gender == 0 ? NAPI.Util.GetHashKey(assetsTattoo.HashMale) : NAPI.Util.GetHashKey(assetsTattoo.HashFemale);

                NAPI.Player.SetPlayerDecoration(dbPlayer.Player, decoration);
            }
        }

        public static void SaveCustomization(this DbPlayer dbPlayer)
        {
            try
            {
                var customizationString = JsonConvert.SerializeObject(dbPlayer.Customization) ?? "";
                var query =
                    $"UPDATE `player` SET customization = '{customizationString}' WHERE id = '{dbPlayer.Id}';";
                query +=
                    $"UPDATE `player_character` SET skin = '{Enum.GetName(typeof(PedHash), dbPlayer.Character.Skin)}' WHERE player_id = '{dbPlayer.Id}';";
                MySQLHandler.ExecuteAsync(query);
            }
            catch (Exception exception)
            {
                Logger.Crash(exception);
            }
        }

        public static void AddTattoo(this DbPlayer dbPlayer, uint tattooId)
        {
            if (!dbPlayer.Customization.Tattoos.Contains(tattooId)) dbPlayer.Customization.Tattoos.Add(tattooId);
            dbPlayer.SaveCustomization();
            dbPlayer.ApplyDecorations();
        }

        public static void RemoveTattoo(this DbPlayer dbPlayer, uint tattooId)
        {
            if (dbPlayer.Customization.Tattoos.Contains(tattooId)) dbPlayer.Customization.Tattoos.Remove(tattooId);
            dbPlayer.SaveCustomization();
            dbPlayer.ApplyDecorations();
        }

        public static void StartCustomization(this DbPlayer dbPlayer)
        {
            var player = dbPlayer.Player;

            dbPlayer.SetData("lastPositionSpectate", dbPlayer.Player.Position);
            dbPlayer.SetData("lastDimension", player.Dimension);
            dbPlayer.SetData("lastArmor", player.Armor);

            dbPlayer.SetDimension(dbPlayer.Id);
            player.Transparency = 255;
            player.Rotation = new Vector3(0f, 0f, FacingAngle);
            player.SetPosition(creatorCharPos);

            var character = dbPlayer.Character;

            if (dbPlayer.Customization == null)
            {
                dbPlayer.Customization = new CharacterCustomization();
                dbPlayer.SetData("firstCharacter", true);
                SetCreatorClothes(dbPlayer);
            }

            ComponentManager.Get<CustomizationWindow>().Show()(dbPlayer, dbPlayer.Customization);
        }

        public static void SetCreatorClothes(this DbPlayer dbPlayer)
        {
            if (dbPlayer.Customization == null) return;

            // clothes
            for (var i = 0; i < 10; i++) dbPlayer.Player.ClearAccessory(i);

            dbPlayer.SetClothes(1, 0, 0);

            if (dbPlayer.Customization.Gender == 0)
            {
                // Oberkörper frei
                dbPlayer.SetClothes(11, 15, 0);
                // Unterhemd frei
                dbPlayer.SetClothes(8, 57, 0);
                // Torso frei
                dbPlayer.SetClothes(3, 15, 0);
            }
            else
            {
                // Naked (.)(.)
                dbPlayer.SetClothes(3, 15, 0);
                dbPlayer.SetClothes(4, 15, 0);
                dbPlayer.SetClothes(8, 0, 99);
                dbPlayer.SetClothes(11, 0, 99);
            }

            dbPlayer.SetClothes(2, dbPlayer.Customization.Hair.Hair, 0);
        }

        public static async Task StopCustomization(this DbPlayer dbPlayer)
        {
            var player = dbPlayer.Player;

            if (dbPlayer.HasData("lastPositionSpectate"))
            {
                Logger.Print("Try");
                try
                {
                    player.SetPosition((Vector3)dbPlayer.GetData("lastPositionSpectate"));
                }
                catch (Exception e)
                {
                    Logger.Print(e.ToString() + " 374");
                }

                if (dbPlayer.HasData("lastDimension"))
                {
                    dbPlayer.SetDimension(dbPlayer.GetData("lastDimension"));
                }

                dbPlayer.ResetData("lastPositionSpectate");
                dbPlayer.ResetData("lastDimension");
                dbPlayer.SetMedicCuffed(false);
                dbPlayer.SetCuffed(false);
                dbPlayer.Freeze(false);
            }
            else
            {
                dbPlayer.Player.SetPosition(new Vector3(298.7902, -584.4927, 43.26085));
                dbPlayer.SetDimension(0);
            }

            // revert back to last save data
            if (dbPlayer.HasData("ChangedGender"))
            {
                //character.customization =TODO: old customization 
                dbPlayer.ResetData("ChangedGender");
            }

            dbPlayer.ApplyCharacter();
            ClothModule.Instance.RefreshPlayerClothes(dbPlayer);

            if (dbPlayer.HasData("lastArmor"))
            {
                dbPlayer.SetArmorPlayer(dbPlayer.GetData("lastArmor"));
            }

            if (dbPlayer.IsNewbie())
            {
                dbPlayer.SetDimension(dbPlayer.Id);

                await Task.Delay(100);
                dbPlayer.Player.TriggerNewClient("startWelcomeCutscene", dbPlayer.Customization.Gender, dbPlayer.GetName());
            }
        }
    }

    public class CustomizationEvents : Script
    {
        [RemoteEvent]
        public async Task cutsceneEnded(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.Player.TriggerNewClient("moveSkyCamera", dbPlayer.Player, "up", 1, false);

            dbPlayer.ApplyCharacter(true);
            dbPlayer.ApplyPlayerHealth();
            dbPlayer.Freeze(false);
            dbPlayer.SetDimension(0);

            dbPlayer.Player.SetPosition(new GTANetworkAPI.Vector3(-1144.26, -2792.27, 27.708));
            dbPlayer.Player.SetRotation(237.428f);


            await Task.Delay(3000);
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            dbPlayer.Player.TriggerNewClient("moveSkyCamera", dbPlayer.Player, "down", 1, false);

        }
    }
}
