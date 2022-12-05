using VMP_CNR.Module.Armory;
using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Staatskasse;

namespace VMP_CNR
{
    public class ArmoryItemMenuBuilder : MenuBuilder
    {
        public ArmoryItemMenuBuilder() : base(PlayerMenu.ArmoryItems)
        {
        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu(Menu, "Armory Items");

            menu.Add(MSG.General.Close(), "");
            menu.Add("Zurueck", "");

            if (!dbPlayer.HasData("ArmoryId")) return null;
            var ArmoryId = dbPlayer.GetData("ArmoryId");
            Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
            if (Armory == null) return null;
            foreach (var ArmoryItem in Armory.ArmoryItems)
            {
                menu.Add((ArmoryItem.Price > 0 ? ("$" + ArmoryItem.Price + " ") : "") + ArmoryItem.Item.Name,
                    "");
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
                if (!dbPlayer.HasData("ArmoryId")) return false;
                var ArmoryId = dbPlayer.GetData("ArmoryId");
                Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
                if (Armory == null) return false;

                switch (index)
                {
                    case 0:
                        MenuManager.DismissMenu(dbPlayer.Player, (int) PlayerMenu.ArmoryItems);
                        return false;
                    case 1:
                        MenuManager.DismissMenu(dbPlayer.Player, (int) PlayerMenu.Armory);
                        return false;
                    default:
                        var actualIndex = 0;
                        foreach (var ArmoryItem in Armory.ArmoryItems)
                        {
                            if (actualIndex == index - 2)
                            {
                                // Rang check
                                if (dbPlayer.TeamRank < ArmoryItem.RestrictedRang)
                                {
                                    dbPlayer.SendNewNotification(
                                        "Sie haben nicht den benötigten Rang fuer diese Waffe!");
                                    return false;
                                }

                                if (!dbPlayer.IsInDuty() && !dbPlayer.IsNSADuty)
                                {
                                    dbPlayer.SendNewNotification(
                                        "Sie muessen dafuer im Dienst sein!");
                                    return false;
                                }

                                // Check Armory
                                if (Armory.GetPackets() < ArmoryItem.Packets)
                                {
                                    dbPlayer.SendNewNotification(
                                        $"Die Waffenkammer hat nicht mehr genuegend Materialien! (Benötigt: {ArmoryItem.Packets} )");
                                    return false;
                                }

                                // Check inventory
                                if (!dbPlayer.Container.CanInventoryItemAdded(ArmoryItem.Item, 1))
                                {
                                    dbPlayer.SendNewNotification(
                                        $"Sie können das nicht mehr tragen, Ihr Inventar ist voll!");
                                    return false;
                                }
                                if (ArmoryItem.Price > 0 && !dbPlayer.TakeBankMoney(ArmoryItem.Price))
                                {
                                    dbPlayer.SendNewNotification(
                                        $"Dieses Item kostet {ArmoryItem.Price}$ (Bank)!");
                                    return false;
                                }

                                // Found
                                dbPlayer.Container.AddItem(ArmoryItem.Item, 1);
                                Armory.RemovePackets(ArmoryItem.Packets);

                                if (ArmoryItem.Price > 0)
                                {
                                    dbPlayer.SendNewNotification($"{ArmoryItem.Item.Name} für ${ArmoryItem.Price} ausgerüstet!");
                                    KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, +ArmoryItem.Price);
                                }
                                return false;
                            }

                            actualIndex++;
                        }

                        break;
                }

                return false;
            }
        }
    }
}