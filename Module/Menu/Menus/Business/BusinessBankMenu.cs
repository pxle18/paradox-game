using VMP_CNR.Module.Business;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.RemoteEvents;

namespace VMP_CNR
{
    public class BusinessBankMenuBuilder : MenuBuilder
    {
        public BusinessBankMenuBuilder() : base(PlayerMenu.BusinessBank)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Business Verwaltung");

            menu.Add(GlobalMessages.General.Close(), "");

            if (!dbPlayer.IsMemberOfBusiness())
            {
                menu.Description = $"Ein Unternehmen Gruenden!";
                menu.Add("Business kaufen - 250.000$", "");
            }
            else
            {
                menu.Add("Business Namen ändern - 50.000$", "");
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
                switch (index)
                {
                    case 1: // Kaufen
                        
                        if (!dbPlayer.IsMemberOfBusiness())
                        {
                            if (!dbPlayer.IsHomeless())
                            {
                                int createBusinessPrice = 250000;
                                
                                if (!dbPlayer.CheckForSpam(DbPlayer.OperationType.BusinessCreate)) return false;

                                if (!dbPlayer.TakeBankMoney(createBusinessPrice))
                                {
                                    dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(createBusinessPrice));
                                    break;
                                }

                                BusinessModule.Instance.CreatePlayerBusiness(dbPlayer);
                                dbPlayer.SendNewNotification("Business erfolgreich fuer $" +createBusinessPrice+ " erworben!");
                                
                                // Bankverlauf beim Inhaber beim erwerb eines Business
                                dbPlayer.AddPlayerBankHistory(-createBusinessPrice, "Business erworben");
                            }
                            else
                            {
                                dbPlayer.SendNewNotification("Sie haben keinen Wohnsitz!");
                                return true;
                            }
                        }
                        else
                        {
                            // Namechange
                            if(!dbPlayer.GetActiveBusinessMember().Manage)
                            {
                                dbPlayer.SendNewNotification("Sie müssen Manager eines Businesses sein um den Namen zu ändern!");
                                return true;
                            }
                            else
                            {
                                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject()
                                {
                                    Title = $"Namensänderung beantragen | {dbPlayer.GetActiveBusiness().Name}",
                                    Callback = "NameChangeBiz",
                                    Message = "Bitte geben Sie den neuen Namen ein, nutzen Sie hierfür Buchstaben (A-Z) optional (_ -). Die Kosten betragen $50000"
                                });
                                return true;
                            }
                        }
                        break;
                    default:
                        break;
                }

                return true;
            }
        }
    }
}