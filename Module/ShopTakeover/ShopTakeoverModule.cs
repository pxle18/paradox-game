using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.ShopTakeover.Models;

namespace VMP_CNR.Module.ShopTakeover
{
    /**
     * This is part of the PARADOX Game-Rework.
     * Made by module@jabber.ru
     */
    public sealed class ShopTakeoverModule : SqlModule<ShopTakeoverModule, ShopTakeoverModel, uint>
    {
        protected override string GetQuery() => "SELECT * FROM shop_takeovers";
    }
}
