using System.Linq;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Meth;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool HeroinCook(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.DimensionType[0] != DimensionTypes.Camper)
            {
                dbPlayer.SendNewNotification(
                     "Sie muessen in einem Wohnmobil sein!");
                return false;
            }

            if (!dbPlayer.HasData("cooking"))
            {
                if (dbPlayer.Container.GetItemAmount(HeroinModule.cookerId) >= 1)
                {
                    if (dbPlayer.Container.GetItemAmount(HeroinModule.morphinId) <= 0 ||
                        dbPlayer.Container.GetItemAmount(HeroinModule.essigsaeureId) <= 0 ||
                        dbPlayer.Container.GetItemAmount(HeroinModule.natriumcarbonatId) <= 0)
                    {
                        dbPlayer.SendNewNotification("Du hast nicht die nötigen Ressourcen zum Heroin kochen dabei!");
                        return false;
                    }

                    if(HeroinModule.CookingPlayers.ToList().Where(p => p != null && p.IsValid() && p.Player.Dimension == dbPlayer.Player.Dimension).Count() >= 8)
                    {
                        dbPlayer.SendNewNotification("Hier ist kein Platz mehr? Wo willst du deinen Kocher hinstellen, alla?");
                        return false;
                    }

                    dbPlayer.SendNewNotification( "Sie kochen nun Heroin");
                    dbPlayer.SetData("cooking", true);
                    if (!HeroinModule.CookingPlayers.Contains(dbPlayer)) HeroinModule.CookingPlayers.Add(dbPlayer);
                    return true;
                }
            }
            else
            {
                dbPlayer.ResetData("cooking");
                dbPlayer.SendNewNotification( "Heroin kochen beendet!");
                if (HeroinModule.CookingPlayers.Contains(dbPlayer)) HeroinModule.CookingPlayers.Remove(dbPlayer);
            }
            return false;
        }
    }
}