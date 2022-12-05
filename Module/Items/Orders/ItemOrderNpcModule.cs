using System;
using System.Linq;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items
{
    public class ItemOrderNpcModule : SqlModule<ItemOrderNpcModule, ItemOrderNpc, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemOrderNpcItemModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `itemorder_npc`;";
        }

        public ItemOrderNpc GetByPlayerPosition(DbPlayer dbPlayer)
        {
            foreach (ItemOrderNpc itemOrderNpc in GetAll().Values)
            {
                if (dbPlayer.Player.Position.DistanceTo(itemOrderNpc.Position) < 3.0f)
                {
                    return itemOrderNpc;
                }
            }
            return null;
        }
    }
}
