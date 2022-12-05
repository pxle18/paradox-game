using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Bunker
{
    public class BunkerModuleEvents : Script
    {
        [RemoteEvent]
        public void BlackmoneyWithdrawBunker(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (Int32.TryParse(returnstring, out int withdraw))
            {
                if (withdraw <= 0) return;

                if (dbPlayer.Dimension[0] == BunkerModule.BunkerDimension)
                {
                    Bunker bunker = BunkerModule.Instance.GetAll().FirstOrDefault().Value;
                    if (bunker != null && bunker.IsControlledByTeam(dbPlayer.TeamId))
                    {
                        if (dbPlayer.Player.Position.DistanceTo(BunkerModule.BunkerBlackMoneyWithdraw) < 1.5f)
                        {
                            if (bunker.BlackMoneyAmount < withdraw)
                            {
                                dbPlayer.SendNewNotification("So viel befindet sich nicht in der Gelddruckmaschine!");
                                return;
                            }
                            else
                            {
                                bunker.BlackMoneyAmount -= withdraw;
                                dbPlayer.GiveMoney(withdraw);
                                bunker.SaveBlackMoney();

                                dbPlayer.SendNewNotification($"Sie haben {withdraw}$ entnommen!");
                                return;
                            }
                        }
                    }
                }
                return;
            }
            dbPlayer.SendNewNotification("Ungültiger Betrag!");
            return;
        }
    }
}
