using System;
using System.Collections.Concurrent;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Boerse.Menu;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Boerse
{
    public sealed class PlayerDepotModule : Module<PlayerDepotModule>
    {
        public static uint DepotKosten                  = 50000;
        public static uint DepotDailyMaximum            = 50000;
        public static Vector3 DepotManagementPosition   = new Vector3(253.5603, 220.9893, 106.2865);

        public override Type[] RequiredModules()
        {
            return new[] { typeof(ConfigurationModule) };
        }

        /// <summary>
        /// Beim Verbinden soll das entsprechende Depot des Spielers geladen werden
        /// </summary>
        /// <param name="dbPlayer"></param>
        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            dbPlayer.LoadPlayerDepot();
        }
        
        protected override bool OnLoad()
        {
            MenuManager.Instance.AddBuilder(new ManageDepotMenu());
            return base.OnLoad();
        }
        
        /// <summary>
        /// Öffnet das Depot-Menü um ein neues Depot zu erstellen oder um über das aktuelle zu verfügen
        /// </summary>
        /// <param name="dbPlayer"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E) return false;
            if (dbPlayer.Player.Position.DistanceTo(DepotManagementPosition) > 1.5f) return false;
            
            // INTERNAL TEST RESTRICTION
            if (dbPlayer.RankId != (int)AdminLevelTypes.Gamedesigner &&
                dbPlayer.RankId != (int)AdminLevelTypes.SeniorGamedesigner &&
                dbPlayer.RankId != (int)AdminLevelTypes.SuperAdministrator &&
                dbPlayer.RankId != (int)AdminLevelTypes.Manager &&
                dbPlayer.RankId != (int)AdminLevelTypes.Projektleitung &&
                dbPlayer.RankId != (int)AdminLevelTypes.Entwicklungsleitung &&
                dbPlayer.RankId != (int)AdminLevelTypes.SeniorEntwickler)
                return false;

            MenuManager.Instance.Build(PlayerMenu.ManageDepotMenu, dbPlayer).Show(dbPlayer);
            return true;
        }
    }
}