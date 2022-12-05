

using GTANetworkAPI;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players
{
    public static class PlayerDelete
    {
        public static void DeleteEntity(this Player client)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null) return;
            var character = dbPlayer.Character;
            if (character != null)
            {
                client.RemoveEntityDataWhenExists("CustomCharacter");
                client.RemoveEntityDataWhenExists("dataParentsMother");
                client.RemoveEntityDataWhenExists("dataParentsFather");
                client.RemoveEntityDataWhenExists("dataParentsSimilarity");
                client.RemoveEntityDataWhenExists("dataParentsSkinSimilarity");
                client.RemoveEntityDataWhenExists("dataHairColor");
                client.RemoveEntityDataWhenExists("dataHairHighlightColor");
                client.RemoveEntityDataWhenExists("dataBeardColor");
                client.RemoveEntityDataWhenExists("dataEyebrowColor");
                client.RemoveEntityDataWhenExists("dataBlushColor");
                client.RemoveEntityDataWhenExists("dataLipstickColor");
                client.RemoveEntityDataWhenExists("dataChestHairColor");
                client.RemoveEntityDataWhenExists("dataEyeColor");
                client.RemoveEntityDataWhenExists("dataFeaturesLength");
                client.RemoveEntityDataWhenExists("dataAppearanceLength");
                if (dbPlayer.Customization != null)
                {
                    for (int i = 0, length = dbPlayer.Customization.Features.Length; i < length; i++)
                    {
                        client.RemoveEntityDataWhenExists("dataFeatures-" + i);
                    }

                    for (int i = 0, length = dbPlayer.Customization.Appearance.Length; i < length; i++)
                    {
                        client.RemoveEntityDataWhenExists("dataAppearance-" + i);
                        client.RemoveEntityDataWhenExists("dataAppearanceOpacity-" + i);
                    }
                }
            }

            client.RemoveEntityDataWhenExists("phone_calling");
            client.RemoveEntityDataWhenExists("phone_number");
            client.RemoveEntityDataWhenExists("isInjured");
            client.RemoveEntityDataWhenExists("VOICE_RANGE");
            client.RemoveEntityDataWhenExists("death");
        }
    }
}