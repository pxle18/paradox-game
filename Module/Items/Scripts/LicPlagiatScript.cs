using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Blitzer;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool LicPlagiatCar(DbPlayer dbPlayer)
        {
            uint itemId = 1450;
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (itemModel == null) return false;

            if (dbPlayer.Lic_Car[0] == 1 || dbPlayer.Lic_Car[0] == 2)
            {
                dbPlayer.SendNewNotification("Du hast bereits eine eingetragene Lizenz!");
                return false;
            }

            dbPlayer.Lic_Car[0] = 2;

            dbPlayer.SendNewNotification($"Du hast {itemModel.Name} erfolgreich benutz!");
            return true;
        }
        public static bool LicPlagiatPlaneA(DbPlayer dbPlayer)
        {
            uint itemId = 1451;
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (itemModel == null) return false;

            if (dbPlayer.Lic_PlaneA[0] == 1 || dbPlayer.Lic_PlaneA[0] == 2)
            {
                dbPlayer.SendNewNotification("Du hast bereits eine eingetragene Lizenz!");
                return false;
            }

            dbPlayer.Lic_PlaneA[0] = 2;

            dbPlayer.SendNewNotification($"Du hast {itemModel.Name} erfolgreich benutz!");
            return true;
        }
        public static bool LicPlagiatPlaneB(DbPlayer dbPlayer)
        {
            uint itemId = 1453;
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (itemModel == null) return false;

            if (dbPlayer.Lic_PlaneB[0] == 1 || dbPlayer.Lic_PlaneB[0] == 2)
            {
                dbPlayer.SendNewNotification("Du hast bereits eine eingetragene Lizenz!");
                return false;
            }

            dbPlayer.Lic_PlaneB[0] = 2;

            dbPlayer.SendNewNotification($"Du hast {itemModel.Name} erfolgreich benutz!");
            return true;
        }
        public static bool LicPlagiatLKW(DbPlayer dbPlayer)
        {
            uint itemId = 1452;
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (itemModel == null) return false;

            if (dbPlayer.Lic_LKW[0] == 1 || dbPlayer.Lic_LKW[0] == 2)
            {
                dbPlayer.SendNewNotification("Du hast bereits eine eingetragene Lizenz!");
                return false;
            }

            dbPlayer.Lic_LKW[0] = 2;

            dbPlayer.SendNewNotification($"Du hast {itemModel.Name} erfolgreich benutz!");
            return true;
        }
        public static bool LicPlagiatBoot(DbPlayer dbPlayer)
        {
            uint itemId = 1454;
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (itemModel == null) return false;

            if (dbPlayer.Lic_Boot[0] == 1 || dbPlayer.Lic_Boot[0] == 2)
            {
                dbPlayer.SendNewNotification("Du hast bereits eine eingetragene Lizenz!");
                return false;
            }

            dbPlayer.Lic_Boot[0] = 2;

            dbPlayer.SendNewNotification($"Du hast {itemModel.Name} erfolgreich benutz!");
            return true;
        }
        public static bool LicPlagiatBike(DbPlayer dbPlayer)
        {
            uint itemId = 1455;
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (itemModel == null) return false;

            if (dbPlayer.Lic_Bike[0] == 1 || dbPlayer.Lic_Bike[0] == 2)
            {
                dbPlayer.SendNewNotification("Du hast bereits eine eingetragene Lizenz!");
                return false;
            }

            dbPlayer.Lic_Bike[0] = 2;

            dbPlayer.SendNewNotification($"Du hast {itemModel.Name} erfolgreich benutz!");
            return true;
        }
        public static bool LicPlagiatPBS(DbPlayer dbPlayer)
        {
            uint itemId = 1456;
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (itemModel == null) return false;

            if (dbPlayer.Lic_Transfer[0] == 1 || dbPlayer.Lic_Transfer[0] == 2)
            {
                dbPlayer.SendNewNotification("Du hast bereits eine eingetragene Lizenz!");
                return false;
            }

            dbPlayer.Lic_Transfer[0] = 2;

            dbPlayer.SendNewNotification($"Du hast {itemModel.Name} erfolgreich benutz!");
            return true;
        }
        public static bool LicPlagiatBiz(DbPlayer dbPlayer)
        {
            uint itemId = 1460;
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (itemModel == null) return false;

            if (dbPlayer.Lic_Biz[0] == 1 || dbPlayer.Lic_Biz[0] == 2)
            {
                dbPlayer.SendNewNotification("Du hast bereits eine eingetragene Lizenz!");
                return false;
            }

            dbPlayer.Lic_Biz[0] = 2;

            dbPlayer.SendNewNotification($"Du hast {itemModel.Name} erfolgreich benutz!");
            return true;
        }
        public static bool LicPlagiatGun(DbPlayer dbPlayer)
        {
            uint itemId = 1461;
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (itemModel == null) return false;

            if (dbPlayer.Lic_Gun[0] == 1 || dbPlayer.Lic_Gun[0] == 2)
            {
                dbPlayer.SendNewNotification("Du hast bereits eine eingetragene Lizenz!");
                return false;
            }

            dbPlayer.Lic_Gun[0] = 2;

            dbPlayer.SendNewNotification($"Du hast {itemModel.Name} erfolgreich benutz!");
            return true;
        }
        public static bool LicPlagiatHunting(DbPlayer dbPlayer)
        {
            uint itemId = 1462;
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (itemModel == null) return false;

            if (dbPlayer.Lic_Hunting[0] == 1 || dbPlayer.Lic_Hunting[0] == 2)
            {
                dbPlayer.SendNewNotification("Du hast bereits eine eingetragene Lizenz!");
                return false;
            }

            dbPlayer.Lic_Hunting[0] = 2;

            dbPlayer.SendNewNotification($"Du hast {itemModel.Name} erfolgreich benutz!");
            return true;
        }
    }
}