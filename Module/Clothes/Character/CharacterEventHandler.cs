using GTANetworkAPI;
using Newtonsoft.Json;
using System.Linq;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Einreiseamt;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Clothes.Character
{
    public class CharacterEventHandler : Script
    {
        

        public void SetGender(Player client, int gender)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var character = dbPlayer.Character;
            
            var Armor = dbPlayer.Player.Armor;
            character.Skin = gender == 0 ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01;
            client.SetSkin(character.Skin);
            dbPlayer.SetArmorPlayer(Armor);
            dbPlayer.SetData("ChangedGender", true);
            dbPlayer.SetCreatorClothes();
        }

        [RemoteEvent]
        public async void SaveCharacter(Player client, int gender, byte fatherShape, byte motherShape, byte fatherSkin, byte motherSkin, float similarity, float skinSimilarity,
            string featuredData, string appearenceData, string hairData, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            
            var character = dbPlayer.Character;
            var price = 500 * dbPlayer.Level;

            if (dbPlayer.Level <= 3 || dbPlayer.HasData("firstCharacter"))
            {
                price = 0;
            }

            if (dbPlayer.NeuEingereist())
            {
                foreach (DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers().Where(p => p.IsEinreiseAmt()))
                {
                    xPlayer.SendNewNotification("Spieler ist aus der Schoenheitsklinik: " + dbPlayer.GetName());
                }
            }
            
            if (price > 0 && !dbPlayer.TakeMoney(price))
            {
                dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(price), notificationType:PlayerNotification.NotificationType.ERROR);
                // revert back to last save data
                if (dbPlayer.HasData("ChangedGender"))
                {
                    //character.customization =TODO: old customization 
                    dbPlayer.ResetData("ChangedGender");
                }

                await dbPlayer.StopCustomization();
                return;
            }
            else
            {
                dbPlayer.SendNewNotification("Sie haben ihren Character fuer $" + price + " geaendert!", notificationType:PlayerNotification.NotificationType.SUCCESS);
            }
            
            if (character == null) return;

            // gender
            dbPlayer.Customization.Gender = gender;

            // parents
            dbPlayer.Customization.Parents.FatherShape = fatherShape;
            dbPlayer.Customization.Parents.MotherShape = motherShape;
            dbPlayer.Customization.Parents.FatherSkin = fatherSkin;
            dbPlayer.Customization.Parents.MotherSkin = motherSkin;
            dbPlayer.Customization.Parents.Similarity = similarity;
            dbPlayer.Customization.Parents.SkinSimilarity = skinSimilarity;

            // features
            var featureData = JsonConvert.DeserializeObject<float[]>(featuredData);
            dbPlayer.Customization.Features = featureData;
            
            // appearance
            var appearanceData =
                JsonConvert.DeserializeObject<AppearanceItem[]>(appearenceData);
            dbPlayer.Customization.Appearance = appearanceData;

            // hair & colors
            var hairAndColorData = JsonConvert.DeserializeObject<byte[]>(hairData);
            for (var i = 0; i < hairAndColorData.Length; i++)
            {
                switch (i)
                {
                    // Hair
                    case 0:
                        {
                            dbPlayer.Customization.Hair.Hair = hairAndColorData[i];
                            break;
                        }

                    // Hair Color
                    case 1:
                        {
                            dbPlayer.Customization.Hair.Color = hairAndColorData[i];
                            break;
                        }

                    // Hair Highlight Color
                    case 2:
                        {
                            dbPlayer.Customization.Hair.HighlightColor = hairAndColorData[i];
                            break;
                        }

                    // Eyebrow Color
                    case 3:
                        {
                            dbPlayer.Customization.EyebrowColor = hairAndColorData[i];
                            break;
                        }

                    // Beard Color
                    case 4:
                        {
                            dbPlayer.Customization.BeardColor = hairAndColorData[i];
                            break;
                        }

                    // Eye Color
                    case 5:
                        {
                            dbPlayer.Customization.EyeColor = hairAndColorData[i];
                            break;
                        }

                    // Blush Color
                    case 6:
                        {
                            dbPlayer.Customization.BlushColor = hairAndColorData[i];
                            break;
                        }

                    // Lipstick Color
                    case 7:
                        {
                            dbPlayer.Customization.LipstickColor = hairAndColorData[i];
                            break;
                        }

                    // Chest Hair Color
                    case 8:
                        {
                            dbPlayer.Customization.ChestHairColor = hairAndColorData[i];
                            break;
                        }
                }
            }

            dbPlayer.SaveCustomization();
            await dbPlayer.StopCustomization();
        }
        
        [RemoteEvent]
        public async void LeaveCreator(Player client, object[] args, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var character = dbPlayer.Character;

            await dbPlayer.StopCustomization();
        }
    }
}
