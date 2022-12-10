using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams.AmmoArmory;

namespace VMP_CNR.Module.Teams.AmmoPackageOrder
{
    class AmmoPackageEvents : Script
    {
        [RemoteEvent]
        public void AddAmmoPackageOrder(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.HasData("orderedTeam")) return;

                DbTeam dbTeam = TeamModule.Instance.Get(dbPlayer.GetData("orderedTeam"));
                if (dbTeam == null || !dbTeam.IsGangsters()) return;
                if (dbPlayer.Team.Id != (int)TeamTypes.TEAM_HUSTLER && dbPlayer.Team.Id != (int)TeamTypes.TEAM_ICA) return;

                if (int.TryParse(returnstring, out int amount))
                {
                    if ((dbTeam.TeamMetaData.OrderedPackets + (amount)) >= AmmoArmoryModule.PacketsMax)
                    {
                        dbPlayer.SendNewNotification("Maximale Anzahl an Bestellungen für diese Fraktion erreicht!");
                        return;
                    }
                    else
                    {
                        if (dbTeam.TeamMetaData.Container.CanInventoryItemAdded(AmmoArmoryModule.AmmoChestItem, amount))
                        {
                            if (!dbPlayer.TakeBlackMoney(AmmoPackageOrderModule.AmmoOrderSourcePrice * amount))
                            {
                                dbPlayer.SendNewNotification("Die Bestellungskosten betragen ($" + AmmoPackageOrderModule.AmmoOrderSourcePrice * amount + " Schwarzgeld)");
                                return;
                            }

                            dbTeam.TeamMetaData.OrderedPackets += (amount);
                            dbTeam.TeamMetaData.SaveOrderedPackets();
                            dbTeam.TeamMetaData.Container.AddItem(AmmoArmoryModule.AmmoChestItem, amount);
                            dbPlayer.SendNewNotification("Sie haben " + amount + " Waffenkisten für $" + AmmoPackageOrderModule.AmmoOrderSourcePrice * amount + " bestellt");
                        }
                        else
                        {
                            dbPlayer.SendNewNotification("So viel passt nicht in den Lieferungscontainer!");
                            return;
                        }
                    }
                    return;
                }
                else
                    dbPlayer.SendNewNotification("Falsche Anzahl!");
            }));
        }
    }
}
