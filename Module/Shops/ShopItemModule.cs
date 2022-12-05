using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR.Module.Shops
{
    public class ShopItemModule : SqlModule<ShopItemModule, ShopItem, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemModelModule),  typeof(ShopModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `shops_items`;";
        }

        protected override void OnItemLoad(ShopItem u)
        {
            Shop shop = ShopsModule.Instance.GetShop((int)u.ShopId);
            if (shop != null) shop.ShopItems.Add(u);
        }
    }
}
