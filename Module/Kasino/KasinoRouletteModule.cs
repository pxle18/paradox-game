using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Extensions;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Kasino.Windows;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.RemoteEvents;
using VMP_CNR.Module.Sync;

namespace VMP_CNR.Module.Kasino
{
    /**
     * This is part of the PARADOX Game-Rework.
     * Made by module@jabber.ru
     */

    public enum RouletteBetTypes
    {
        FIELD,

        FIRST,
        SECOND,
        THIRD,

        EVEN,
        ODD,

        RED,
        BLACK
    }

    public class KasinoRouletteBet
    {
        public bool IsFieldBet { get; set; }
        public int FieldBet { get; set; }

        public int Amount { get; set; }

        public RouletteBetTypes BetType { get; set; }
    }

    public sealed class KasinoRouletteModule : Module<KasinoRouletteModule>
    {
        private readonly Dictionary<uint, List<KasinoRouletteBet>> _rouletteBets;
        private readonly Random _random;

        public KasinoRouletteModule()
        {
            _rouletteBets = new Dictionary<uint, List<KasinoRouletteBet>>();
            _random = new Random();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandroulettewindow(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();

            ComponentManager.Get<RouletteWindow>().Show()(dbPlayer);
            return;
        }

        public override void OnMinuteUpdate()
        {
            if (_rouletteBets.Count <= 0) return;

            int betFieldNumber = _random.Next(0, 36);

            _rouletteBets.ForEach(
                rouletteBets =>
                {
                    var player = Players.Players.Instance.FindPlayerById(rouletteBets.Key);
                    if (player == null) return;

                    player.SendNewNotification($"Die gezogene Nummer lautet: {betFieldNumber}");

                    ComponentManager.Get<RouletteWindow>().ResponseResultEndGame(player, betFieldNumber);
                }
            );
        }

        [RemoteEvent]
        private void GetNextRoundTime(Player player, string remoteEventKey)
        {
            if (!player.CheckRemoteEventKey(remoteEventKey)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var currentTime = DateTime.Now;

            var nextFullMinute = SyncThread.LastSyncMinuteCheck.AddMinutes(1);
            var deltaFullMinute = (nextFullMinute - currentTime).TotalSeconds;

            ComponentManager.Get<RouletteWindow>().ResponseResultNextRound(dbPlayer, (int)deltaFullMinute);

            /**
             * Trigger Client: resultNextRound
             */
        }

        [RemoteEvent("placeFieldBet")]
        private void PlaceFieldBet(Player player, int fieldNumber, int amount, string remoteEventKey)
        {
            if (!player.CheckRemoteEventKey(remoteEventKey)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!dbPlayer.TakeMoney(amount))
            {
                dbPlayer.SendNewNotification($"Du hast nicht genügend Geld.");
                return;
            }

            if (!fieldNumber.IsBetween(0, 36)) return;

            List<KasinoRouletteBet> playerRouletteBets = new List<KasinoRouletteBet>();

            if (_rouletteBets[dbPlayer.Id] != null)
                playerRouletteBets = _rouletteBets[dbPlayer.Id];

            playerRouletteBets.Add(new KasinoRouletteBet()
            {
                BetType = RouletteBetTypes.FIELD,

                FieldBet = fieldNumber,
                IsFieldBet = true
            });

            _rouletteBets[dbPlayer.Id] = playerRouletteBets;
        }
    }
}
