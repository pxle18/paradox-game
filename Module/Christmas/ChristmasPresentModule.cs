using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR;
using VMP_CNR.Extensions;
using VMP_CNR.Module.Christmas.Models;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Kasino.Windows;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.RemoteEvents;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Sync;

namespace VMP_CNR.Module.Christmas
{
    /**
     * This is part of the Void Game-Rework.
     * Made by module@jabber.ru
     */

    public sealed class ChristmasPresentEvents : Script
    {
        [RemoteEvent]
        public void RedeemChristmasCode(Player player, string christmasCode, string remoteKey)
        {
            if (!player.CheckRemoteEventKey(remoteKey)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var christmasCodes = ChristmasPresentModule.Instance.Presents.Where(christmasPresent => christmasPresent.Code.ToLower() == christmasCode.ToLower());

            if (christmasCodes.Count() <= 0)
            {
                dbPlayer.SendNewNotification("Wir konnten leider keine Geschenke unter deinem Code. Bist du sicher, dass du bereits Geschenke im Adventskalender geöffnet hast?", PlayerNotification.NotificationType.ERROR, "XMAS.void.to", 8000);
                Logger.Print(ChristmasPresentModule.Instance.Presents.Count().ToString());

                return;
            }

            ChristmasPresentModule.Instance.ProcessChristmasCodes(dbPlayer, christmasCodes);
        }
    }

    public class ChristmasPresentModule : Module<ChristmasPresentModule>
    {
        public readonly Vector3 PresentLocation = new Vector3(184.08896, -958.8136, 29.953785);
        public readonly Vector3 TreeLocation = new Vector3(186.80109, -952.09033, 28.83992);

        public List<ChristmasPresentModel> Presents = new List<ChristmasPresentModel>();

        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemModelModule) };
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.ChristmasContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.CHRISTMAS);
        }

        protected override bool OnLoad()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT * FROM log_present_reward";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            try
                            {
                                var u = new ChristmasPresentModel(reader);
                                Instance.Presents.Add(u);
                            }
                            catch (Exception e) { Logger.Print(e.ToString()); }
                        }
                }
            }

            new Npc(PedHash.Abigail, PresentLocation, 156, 0);
            ObjectSpawn.Create(118627012, TreeLocation, new Vector3(0, 0, 0));

            PlayerNotifications.Instance.Add(
                PresentLocation,
                "Void Roleplay", "Nehmt eure gesammelten Geschenke mit. I think it’s time, isn’t it?"
            );

            return true;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.RageExtension.IsInVehicle) return false;

            if (dbPlayer.Player.Position.DistanceTo(PresentLocation) < 10.0f)
            {
                /*
                DateTime actualDate = System.DateTime.Now;
                //Wenn letzte Abholung nicht am selben Tag sondern davor war
                if (dbPlayer.xmasLast.Day < actualDate.Day || (dbPlayer.xmasLast.Day == 30 && dbPlayer.xmasLast.Month == 11))
                {
                    try
                    {
                        var christmasCodes = Instance.Presents.Where(christmasPresent => christmasPresent.PlayerId == dbPlayer.Id);
                        if (christmasCodes == null || christmasCodes.Count() <= 0)
                        {
                            ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Adventskalender Login-Code einlösen", Callback = "RedeemChristmasCode", Message = "Gib deinen Login-Code ein, den du auf xmas.void.to erhalten hast." });
                            return true;
                        }

                        ProcessChristmasCodes(dbPlayer, christmasCodes);
                    }
                    catch (Exception e) { Logger.Print("XMAS " + e.ToString()); }
                }
                else
                {
                    dbPlayer.SendNewNotification("Du hast dein Geschenk fuer heute bereits abgeholt. Wenn nicht - Sorry. Fix ich morgen", PlayerNotification.NotificationType.ERROR, "XMAS");
                    return false;
                }
                return true;*/
            }
            return false;
        }

        public void ProcessChristmasCodes(DbPlayer player, IEnumerable<ChristmasPresentModel> christmasPresents)
        {
            player.SendNewNotification("Hey, wir haben deine Geschenke gefunden! Bitte öffne an diesem Punkt dein Inventar.", PlayerNotification.NotificationType.SUCCESS, "XMAS.void.to");

            foreach (var code in christmasPresents.ToList())
            {
                player.ChristmasContainer.AddItem(code.Item, code.Amount);
                code.Delete();

                Instance.Presents.RemoveAll(christmasPresent => christmasPresent.Id == christmasPresent.Id);
            }

            player.xmasLast = DateTime.Now;
            player.SaveChristmasState();
        }
    }
}
