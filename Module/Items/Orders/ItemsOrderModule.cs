using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Items
{
    public sealed class ItemsOrderModule : Module<ItemsOrderModule>
    {
        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if(key == Key.E)
            {
                if (!dbPlayer.HasData("Itemorderflood"))
                {
                    foreach (ItemOrderNpc itemOrderNpc in ItemOrderNpcModule.Instance.GetAll().Values)
                    {
                        if (dbPlayer.Player.Position.DistanceTo(itemOrderNpc.Position) < 3.0f)
                        {
                            // Open Menu
                            MenuManager.Instance.Build(PlayerMenu.ItemOrderMenu, dbPlayer).Show(dbPlayer);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
