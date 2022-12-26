using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR
{
    public class AccountLicenseMenuBuilder : MenuBuilder
    {
        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            var menu = new NativeMenu(Menu, "Lizenzen");
            switch (dbPlayer.Lic_Car[0])
            {
                case 1:
                    menu.Add("Fuehrerschein PKW", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Fuehrerschein  PKW", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (dbPlayer.Lic_Car[0] < 0)
                        menu.Add("Fuehrerschein  PKW", "Vorhanden, Sperre: " + dbPlayer.Lic_Car[0] + " Minuten");
                    break;
            }

            switch (dbPlayer.Lic_LKW[0])
            {
                case 1:
                    menu.Add("Fuehrerschein LKW", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Fuehrerschein LKW", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (dbPlayer.Lic_LKW[0] < 0)
                        menu.Add("Fuehrerschein LKW", "Vorhanden, Sperre: " + dbPlayer.Lic_LKW[0] + " Minuten");
                    break;
            }

            switch (dbPlayer.Lic_Bike[0])
            {
                case 1:
                    menu.Add("Motorradschein", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Motorradschein", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (dbPlayer.Lic_Bike[0] < 0)
                        menu.Add("Motorradschein", "Vorhanden, Sperre: " + dbPlayer.Lic_Bike[0] + " Minuten");
                    break;
            }

            switch (dbPlayer.Lic_PlaneA[0])
            {
                case 1:
                    menu.Add("Flugschein A", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Flugschein A", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (dbPlayer.Lic_PlaneA[0] < 0)
                        menu.Add("Flugschein A", "Vorhanden, Sperre: " + dbPlayer.Lic_PlaneA[0] + " Minuten");
                    break;
            }

            switch (dbPlayer.Lic_PlaneB[0])
            {
                case 1:
                    menu.Add("Flugschein B", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Flugschein B", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (dbPlayer.Lic_PlaneB[0] < 0)
                        menu.Add("Flugschein B", "Vorhanden, Sperre: " + dbPlayer.Lic_PlaneB[0] + " Minuten");
                    break;
            }

            switch (dbPlayer.Lic_Boot[0])
            {
                case 1:
                    menu.Add("Bootsschein", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Bootsschein", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (dbPlayer.Lic_Boot[0] < 0)
                        menu.Add("Bootsschein", "Vorhanden, Sperre: " + dbPlayer.Lic_Boot[0] + " Minuten");
                    break;
            }

            switch (dbPlayer.Lic_Gun[0])
            {
                case 1:
                    menu.Add("Waffenschein", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Waffenschein", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (dbPlayer.Lic_Gun[0] < 0)
                        menu.Add("Waffenschein", "Vorhanden, Sperre: " + dbPlayer.Lic_Gun[0] + " Minuten");
                    break;
            }

            switch (dbPlayer.Lic_Hunting[0]) {
                case 1:
                    menu.Add("Jagdschein", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Jagdschein", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (dbPlayer.Lic_Hunting[0] < 0)
                        menu.Add("Jagdschein", "Vorhanden, Sperre: " + dbPlayer.Lic_Hunting[0] + " Minuten");
                    break;
            }

            switch (dbPlayer.Lic_Biz[0])
            {
                case 1:
                    menu.Add("Gewerbeschein", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Gewerbeschein", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (dbPlayer.Lic_Biz[0] < 0)
                        menu.Add("Gewerbeschein", "Vorhanden, Sperre: " + dbPlayer.Lic_Biz[0] + " Minuten");
                    break;
            }

            switch (dbPlayer.Lic_Transfer[0])
            {
                case 1:
                    menu.Add("Personenbeförderungsschein", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Personenbeförderungsschein", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (dbPlayer.Lic_Transfer[0] < 0)
                        menu.Add("Personenbeförderungsschein",
                            "Vorhanden, Sperre: " + dbPlayer.Lic_Transfer[0] + " Minuten");
                    break;
            }

            switch (dbPlayer.Lic_Taxi[0])
            {
                case 1:
                    menu.Add("Taxilizenz", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Taxilizenz", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (dbPlayer.Lic_Taxi[0] < 0)
                        menu.Add("Taxilizenz", "Vorhanden, Sperre: " + dbPlayer.Lic_Taxi[0] + " Minuten");
                    break;
            }

            menu.Add(GlobalMessages.General.Close(), "");

            return menu;
        }

        public AccountLicenseMenuBuilder() : base(PlayerMenu.AccountLicense)
        {
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EvenntHandler();
        }

        private class EvenntHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                MenuManager.DismissMenu(dbPlayer.Player, (uint) PlayerMenu.AccountLicense);
                return false;
            }
        }
    }
}