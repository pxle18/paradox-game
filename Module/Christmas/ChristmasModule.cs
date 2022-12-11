using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Events;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Christmas
{
    public sealed class ChristmasModule : Module<ChristmasModule>
    {
        public Vector3 PresentLocation = new Vector3(-416.655, 1160.048, 325.858);

        enum Geschenke
        {
            RotesGeschenk       = 1197,
            GrünesGeschenk      = 1196,
            HellblauesGeschenk  = 1195,
            PinkesGeschenk      = 1194,
            GelbesGeschenk      = 1193,
            GrauesGeschenk      = 1192,
            SchwarzesGeschenk   = 1191,
            GrossesGeschenk     = 1198
        }

        enum Events
        {
            Weihnachten2020     = 1
        }

        public override Type[] RequiredModules()
        {
            return new[]
            {
                typeof(EventModule)
            };
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.RageExtension.IsInVehicle || !dbPlayer.CanInteract()) return false;

            if (!EventModule.Instance.IsEventActive((uint)Events.Weihnachten2020)) return false;

            if (dbPlayer.Player.Position.DistanceTo(PresentLocation) <= 10)
            {
                //Wenn Spieler in normaler World
                if (dbPlayer.Dimension[0] == 0)
                {
                    DateTime actualDate = System.DateTime.Now;
                    if ((actualDate.Month == 12 && actualDate.Day >= 1 && actualDate.Day <= 24) || Configurations.Configuration.Instance.DevMode)
                    {
                        //Wenn letzte Abholung nicht am selben Tag sondern davor war
                        if (dbPlayer.xmasLast.Day < actualDate.Day || (dbPlayer.xmasLast.Day == 30 && dbPlayer.xmasLast.Month == 11))
                        {
                            //Gucken ob genug Platz im Inventar ist... #ICH HAB KEIN PAKET BEKOMMEN WEIL ICH KEIN PLATZ HATTE UND EIN KEK BIN
                            if (!dbPlayer.Container.CanInventoryItemAdded((uint)Geschenke.GrünesGeschenk))
                            {
                                dbPlayer.SendNewNotification("Du hast nicht genuegend Platz um das Paket entgegen zu nehmen", PlayerNotification.NotificationType.ERROR, "XMAS");
                                return false;
                            }

                            if (actualDate.Day != 24)
                            {
                                // Ich grüße meine Mama,
                                // Meinen Papa...
                                // Meine Geschwister
                                // Und Espenhain, Felix, Evelyn, und mich (Euka)
                                if (dbPlayer.Level >= 60)
                                    dbPlayer.Container.AddItem((uint)Geschenke.SchwarzesGeschenk);
                                else if (dbPlayer.Level >= 50)
                                    dbPlayer.Container.AddItem((uint)Geschenke.GrauesGeschenk);
                                else if (dbPlayer.Level >= 40)
                                    dbPlayer.Container.AddItem((uint)Geschenke.GelbesGeschenk);
                                else if (dbPlayer.Level >= 30)
                                    dbPlayer.Container.AddItem((uint)Geschenke.PinkesGeschenk);
                                else if (dbPlayer.Level >= 20)
                                    dbPlayer.Container.AddItem((uint)Geschenke.HellblauesGeschenk);
                                else if (dbPlayer.Level >= 10)
                                    dbPlayer.Container.AddItem((uint)Geschenke.GrünesGeschenk);
                                else
                                    dbPlayer.Container.AddItem((uint)Geschenke.RotesGeschenk);
                            }
                            else
                                dbPlayer.Container.AddItem((uint)Geschenke.GrossesGeschenk);

                            dbPlayer.SendNewNotification("Hier ist dein Geschenk! Packst du es jetzt schon aus?", PlayerNotification.NotificationType.SUCCESS, "XMAS");
                            dbPlayer.SendNewNotification("Oder wartest du bis Heiligabend?", PlayerNotification.NotificationType.INFO, "XMAS");

                            // Für ordentliche Tests alla
                            if (!Configuration.Instance.DevMode)
                            {
                                dbPlayer.xmasLast = actualDate;
                                dbPlayer.SaveChristmasState();
                            }
                        }
                        //Abholung war am selben Tag bereits...
                        else
                        {
                            dbPlayer.SendNewNotification("Du hast dein Geschenk fuer heute bereits abgeholt.", PlayerNotification.NotificationType.ERROR, "XMAS");
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        protected override bool OnLoad()
        {
            if (EventModule.Instance.IsEventActive((uint)Events.Weihnachten2020))
                PlayerNotifications.Instance.Add(PresentLocation, "XMAS", "Was haben wir denn hier...");

            return true;
        }
    }
}
