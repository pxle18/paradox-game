using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Business;
using VMP_CNR.Module.Clothes;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Tattoo
{
    public static class TattooShopFunctions
    {
        private static void SaveBusinessId(this TattooShop tattooShop)
        {
            MySQLHandler.ExecuteAsync($"UPDATE tattoo_shops SET business_id = '{tattooShop.BusinessId}' WHERE id = '{tattooShop.Id}'");
        }
        
        public static void SetBusiness(this TattooShop tattooShop, int businessId)
        {
            tattooShop.BusinessId = businessId;
            tattooShop.SaveBusinessId();
        }

        public static void SaveLicenses(this TattooShop tattooShop)
        {
            MySQLHandler.ExecuteAsync($"UPDATE tattoo_shops SET tattoo_licenses = '{JsonConvert.SerializeObject(tattooShop.tattooLicenses)}' WHERE id = '{tattooShop.Id}'");
        }
        
        public static void AddBank(this TattooShop tattooShop, int value)
        {
            tattooShop.SetBank(tattooShop.Bank + value);
        }
        
        public static void MinusBank(this TattooShop tattooShop, int value)
        {
            tattooShop.SetBank(tattooShop.Bank - value);
        }

        public static void SetBank(this TattooShop tattooShop, int bank)
        {
            tattooShop.Bank = bank;
            tattooShop.SaveBank();
        }
        
        public static void SaveBank(this TattooShop tattooShop)
        {
            MySQLHandler.ExecuteAsync($"UPDATE tattoo_shops SET bank = '{tattooShop.Bank}' WHERE id = '{tattooShop.Id}'");
        }

        public static TattooShop GetTattooShop(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.HasTattooShop()) 
                return null;

            int bizId = (int)dbPlayer.GetActiveBusinessMember().BusinessId;

            return TattooShopModule.Instance.GetAll().First(t => t.Value.BusinessId == bizId).Value;
        }
        
        public static bool HasTattooShop(this DbPlayer dbPlayer)
        {
            if (dbPlayer.GetActiveBusinessMember() == null)
                return false;
            
            if (!dbPlayer.GetActiveBusinessMember().Owner)
                return false;

            int lBusinessId = (int) dbPlayer.GetActiveBusinessMember().BusinessId;
            if (TattooShopModule.Instance.GetAll().Count(x => x.Value.BusinessId == lBusinessId) == 0)
                return false;

            return true;
        }

        public static bool HasTattooManager(this DbPlayer dbPlayer)
        {
            return (dbPlayer.GetActiveBusinessMember().Manage);
        }

        public static void AddLicense(this TattooShop tattooShop, TattooLicense tattooLicense)
        {
            tattooShop.tattooLicenses.Add(new TattooAddedItem() { AssetsTattooId = tattooLicense.AssetsTattooId, Price = tattooLicense.Price, TattooLicenseId = (int)tattooLicense.Id });

            tattooShop.SaveLicenses();
        }

        public static void LaserTattoo(this DbPlayer dbPlayer, uint tattooId)
        {
            dbPlayer.RemoveTattoo(tattooId);
        }

        public static void SetTattooClothes(this DbPlayer dbPlayer)
        {
            if (dbPlayer.Customization.Gender == 0)
            {
                // Oberkörper frei
                dbPlayer.SetClothes(11, 15, 0);
                // Unterhemd frei
                dbPlayer.SetClothes(8, 57, 0);
                // Torso frei
                dbPlayer.SetClothes(3, 15, 0);
                dbPlayer.SetClothes(4, 21, 0);
            }
            else
            {
                // Naked (.)(.)
                dbPlayer.SetClothes(3, 15, 0);
                dbPlayer.SetClothes(4, 15, 0);
                dbPlayer.SetClothes(8, 0, 99);
                dbPlayer.SetClothes(11, 15, 0);
            }
        }
    }
}
