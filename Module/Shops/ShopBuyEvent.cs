using System.Collections.Generic;

namespace VMP_CNR.Module.Shops
{
    public class ShopBuyEvent
    {
        public class BasketItem
        {
            public int itemId;
            public int count;
        }

        public int shopId;
        public List<BasketItem> basket;
    }
}