using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Events;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Spawners;

namespace VMP_CNR.Module.Shops
{
    public class ShopModule : SqlModule<ShopModule, Shop, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(EventModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `shops` WHERE pos_x != 0 AND pos_y != 0 AND deactivated = 0;";
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
        }

        protected override void OnItemLoaded(Shop u)
        {
            //if (u.Marker) //Main.ServerBlips.Add(Blips.Create(u.Position, "", blip, 1.0f, color: color));
            if (u.Position.X != 0 && u.Position.Y != 0 && u.Ped != PedHash.Michael)
            {
                if (u.EventId > 0 && EventModule.Instance.IsEventActive(u.EventId))
                    new Npc(u.Ped, u.Position, u.Heading, 0);
                else if (u.EventId == 0)
                    new Npc(u.Ped, u.Position, u.Heading, 0);
            }
        }
    }
}
