using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Extensions;
using VMP_CNR.Module.Christmas.Models;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Kasino.Windows;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.RemoteEvents;
using VMP_CNR.Module.Sync;

namespace VMP_CNR.Module.Christmas
{
    /**
     * This is part of the PARADOX Game-Rework.
     * Made by module@jabber.ru
     */

    public sealed class ChristmasPresentEvents : Script
    {
        [RemoteEvent]
        public void RedeemChristmasCode(Player player, string christmasCode, string remoteKey)
        {
            if (player.CheckRemoteEventKey(remoteKey)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var christmasCodes = ChristmasPresentModule.Instance.GetAll().Values.Where(christmasPresent => 
                christmasPresent.PlayerId == 1 && christmasPresent.Code == christmasCode);

            if (christmasCodes.Count() <= 0)
            {
                dbPlayer.SendNewNotification("Wir konnten leider keine Geschenke unter deinem Code. Bist du sicher, dass du bereits Geschenke im Adventskalender geöffnet hast?", PlayerNotification.NotificationType.ERROR, "XMAS.PRDX.TO", 8000);
                return;
            }

            ChristmasPresentModule.Instance.ProcessChristmasCodes(dbPlayer, christmasCodes);
        }
    }

    public sealed class ChristmasPresentModule : SqlModule<ChristmasPresentModule, ChristmasPresentModel, uint>
    {
        private readonly Vector3 _presentLocation;

        public ChristmasPresentModule()
        {
            _presentLocation = new Vector3(-416.655, 1160.048, 325.858);
        }

        protected override string GetQuery() => "SELECT * FROM log_present_reward;";
        public override Type[] RequiredModules() => new[] { typeof(ItemModelModule) };

        protected override bool OnLoad()
        {
            PlayerNotifications.Instance.Add(
                _presentLocation,
                "PARADOX Roleplay", "Nehmt eure gesammelten Geschenke mit. I think it’s time, isn’t it?"
            );

            return true;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.RageExtension.IsInVehicle || !dbPlayer.CanInteract()) return false;
            if (dbPlayer.Player.Position.DistanceTo(_presentLocation) > 10) return false;

            var christmasCodes = GetAll().Values.Where(christmasPresent => 
                christmasPresent.PlayerId == dbPlayer.Id && christmasPresent.Code == string.Empty);

            if (christmasCodes.Count() <= 0)
            {
                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Aventskalender Login-Code einlösen", Callback = "RedeemChristmasCode", Message = "Gib deinen Login-Code ein, den du auf xmas.prdx.to erhalten hast." });
                return true;
            }

            ProcessChristmasCodes(dbPlayer, christmasCodes);

            return true;
        }

        public void ProcessChristmasCodes(DbPlayer player, IEnumerable<ChristmasPresentModel> christmasPresents)
        {
            player.SendNewNotification("Hey, wir haben deine Geschenke gefunden!", PlayerNotification.NotificationType.SUCCESS, "XMAS.PRDX.TO");
            
            christmasPresents.ForEach(code =>
            {
                player.Container.AddItem(code.Item, code.Amount);
                code.Delete();

                Instance.Remove(code.Id);
            });
        }
    }
}
