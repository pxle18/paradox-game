using VMP_CNR.Module.Business;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Warrants;

namespace VMP_CNR.Module.Menu.Menus.Business
{
    public class BusinessEnterMenuBuilder : MenuBuilder
    {
        public BusinessEnterMenuBuilder() : base(PlayerMenu.BusinessEnter)
        {
        }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Business Tower");

            menu.Add(GlobalMessages.General.Close(), "");

            if (dbPlayer.IsMemberOfBusiness())
            {
                if (dbPlayer.GetActiveBusiness() != null)
                {
                    menu.Add("[EIGENES] " + dbPlayer.GetActiveBusiness().Name, "");
                }
            }
            else
            {
                menu.Add("Kein eigenes vorhanden.");
            }

            foreach (var biz in BusinessModule.Instance.GetOpenBusinesses())
            {
                if (biz.Locked) menu.Add(biz.Name, "");
                else menu.Add(biz.Name, "");
            }
            
            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (dbPlayer.IsMemberOfBusiness() && index == 1 && dbPlayer.GetActiveBusiness() != null)
                {
                    dbPlayer.StopAnimation();
                    var biz = dbPlayer.GetActiveBusiness();
                    dbPlayer.DimensionType[0] = DimensionTypes.Business;
                    dbPlayer.Player.SetPosition(BusinessModule.BusinessPosition);
                    dbPlayer.SetDimension(biz.Id);
                    biz.Visitors.Add(dbPlayer);
                    return true;
                }

                var point = 2;
                foreach (var biz in BusinessModule.Instance.GetOpenBusinesses())
                {
                    if (index == point)
                    {
                        dbPlayer.StopAnimation();
                        dbPlayer.DimensionType[0] = DimensionTypes.Business;
                        dbPlayer.Player.SetPosition(BusinessModule.BusinessPosition);
                        dbPlayer.SetDimension(biz.Id);
                        biz.Visitors.Add(dbPlayer);
                        return true;
                    }

                    point++;
                }
                
                return true;
            }
        }
    }
}