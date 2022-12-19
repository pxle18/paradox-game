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
using VMP_CNR.Module.Logging;
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

            var christmasCodes = ChristmasPresentModule.Instance.GetAll().Values.ToList().FindAll(christmasPresent =>
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

            try
            {
                Logger.Print("Size: " + GetAll().Values.ToList().Count());

                var christmasCodes = GetAll().Values.ToList().FindAll(christmasPresent => christmasPresent.Code == "" && christmasPresent.PlayerId == dbPlayer.Id);

                Logger.Print("Size Filtered: " + christmasCodes.Count());

                if (christmasCodes == null || christmasCodes.Count() <= 0)
                {
                    Logger.Print("Null lol");
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Adventskalender Login-Code einlösen", Callback = "RedeemChristmasCode", Message = "Gib deinen Login-Code ein, den du auf xmas.prdx.to erhalten hast." });
                    return true;
                }

                Logger.Print("Proccessing");
                ProcessChristmasCodes(dbPlayer, christmasCodes);
            }
            catch (Exception e) { Logger.Print(e.ToString()); }


            return true;
        }

        public void ProcessChristmasCodes(DbPlayer player, List<ChristmasPresentModel> christmasPresents)
        {
            player.SendNewNotification("Hey, wir haben deine Geschenke gefunden! Überprüfe dein Inventar.", PlayerNotification.NotificationType.SUCCESS, "XMAS.PRDX.TO");

            player.xmasLast = DateTime.Now;
            player.SaveChristmasState();

            christmasPresents.ForEach(code =>
            {
                player.Container.AddItem(code.Item, code.Amount);
                code.Delete();

                Instance.Remove(code.Id);
            });
        }
    }
}
