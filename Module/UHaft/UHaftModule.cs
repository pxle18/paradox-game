using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.UHaft
{
    public sealed class UHaftmodule : Module<UHaftmodule>
    {
        public static uint UhaftFluchtCrimeId = 99;
        public static Vector3 JailPdPosition = new Vector3(478.469, -1005.87, 26.2731);
        public static Vector3 UhaftComputerPosition = new Vector3(473.544, -1014.98, 26.2733);

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (!dbPlayer.IsACop() || !dbPlayer.IsInDuty() || dbPlayer.Player.Position.DistanceTo(UhaftComputerPosition) > 2.0f || key != Key.E) return false;
            ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Untersuchungshaft", Callback = "SetUhaftPlayer", Message = "Geben Sie einen Namen ein" });
            return true;
        }
        
        public override void OnPlayerMinuteUpdate(DbPlayer dbPlayer)
        {
            // Wenn Spieler in UHaft
            if(dbPlayer.UHaftTime > 0)
            {
                // Nicht an den Zellen
                if(dbPlayer.Player.Position.DistanceTo(JailPdPosition) > 30.0f)
                {
                    // Cop in Range
                    if(Players.Players.Instance.GetValidPlayers().Where(p => p != dbPlayer && p.IsInDuty() && p.IsACop() && p.Player.Position.DistanceTo(dbPlayer.Player.Position) < 20.0f).Count() <= 0)
                    {
                        // Flucht
                        string wantedstring = $"Untersuchungshaft Flucht - { DateTime.Now.Hour}:{ DateTime.Now.Minute} { DateTime.Now.Day}/{ DateTime.Now.Month}/{ DateTime.Now.Year}";
                        dbPlayer.AddCrime("Leitstelle", CrimeReasonModule.Instance.Get((uint)UhaftFluchtCrimeId), wantedstring);

                        dbPlayer.UHaftTime = 0;
                    }
                }

                if (dbPlayer.UHaftTime > 0) dbPlayer.UHaftTime++;
            }
        }
    }
}
