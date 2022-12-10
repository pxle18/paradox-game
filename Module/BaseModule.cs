using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.Doors;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module
{
    public enum Key
    {
        E,
        I,
        L,
        K,
        B,
        F1,
        F2,
        O,
        J,
        B_AIMING,
        NUM0,
        NUM1,
        NUM2,
        NUM3,
        NUM4,
        NUM5,
        NUM6,
        NUM7,
        NUM8,
        NUM9
    }

    public enum ColShapeState
    {
        Enter,
        Exit
    }

    public abstract class BaseModule
    {
        private bool _loaded = false;
        private StringBuilder _currentLog;

        public void Log(string log) => _currentLog?.AppendLine(log);

        public virtual bool OnClientConnected(Player client) => true;

        public virtual void OnPlayerFirstSpawn(DbPlayer dbPlayer) { }

        public virtual void OnVehicleSpawn(SxVehicle sxvehicle) { }

        public virtual void OnServerBeforeRestart() { }

        public virtual void OnPlayerFirstSpawnAfterSync(DbPlayer dbPlayer) { }

        public virtual void OnPlayerSpawn(DbPlayer dbPlayer) { }

        public virtual void OnPlayerConnected(DbPlayer dbPlayer) { }
        public virtual void OnPlayerLoggedIn(DbPlayer dbPlayer) { }
        public virtual void OnPlayerDisconnected(DbPlayer dbPlayer, string reason) { }

        public virtual bool OnPlayerDeathBefore(DbPlayer dbPlayer, NetHandle killer, uint weapon) => false;

        public virtual bool HasDoorAccess(DbPlayer dbPlayer, Door door) => false;

        public virtual void OnPlayerDeath(DbPlayer dbPlayer, NetHandle killer, uint weapon) { }

        public virtual void OnVehicleDeleteTask(SxVehicle sxVehicle) { }
        public virtual void OnPlayerEnterVehicle(DbPlayer dbPlayer, Vehicle vehicle, sbyte seat) { }

        public virtual void OnPlayerExitVehicle(DbPlayer dbPlayer, Vehicle vehicle) { }

        public virtual void OnPlayerWeaponSwitch(DbPlayer dbPlayer, WeaponHash oldWeapon, WeaponHash newWeapon) { }
        public virtual bool OnKeyPressed(DbPlayer dbPlayer, Key key) => false;

        public virtual bool OnChatCommand(DbPlayer dbPlayer, string command, string[] args) => false;
        public virtual bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState) => false;

        protected virtual bool OnLoad() => true;
        public virtual void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader) { }
        public virtual void OnMinuteUpdate() { }
        public virtual async Task OnMinuteUpdateAsync() => await Task.Delay(0);
        public virtual void OnTwoMinutesUpdate() { }
        public virtual void OnFiveMinuteUpdate() { }
        public virtual void OnFifteenMinuteUpdate() { }
        public virtual void OnPlayerMinuteUpdate(DbPlayer dbPlayer) { }
        public virtual void OnTenSecUpdate() { }
        public virtual async Task OnTenSecUpdateAsync() => await Task.Delay(0);
        public virtual void OnFiveSecUpdate() { }

        public virtual int GetOrder() => 0;
        public virtual void OnDailyReset() { }

        protected bool UpdateSetting(string key, string value)
        {
            Settings.Setting setting = Settings.SettingsModule.Instance.GetAll().ToList().Where(s => s.Value.Key.ToLower() == key.ToLower()).FirstOrDefault().Value;
            if (setting == null) return false;

            setting.Value = value;

            Settings.SettingsModule.Instance.SaveSetting(setting);
            return false;
        }

        public virtual bool Load(bool reload = false)
        {
            Stopwatch stopwatch = null;

            try
            {
                if (_loaded && !reload) return true;

                stopwatch = new Stopwatch();

                stopwatch.Start();

                var requiredModules = RequiredModules();
                if (requiredModules != null)
                {
                    foreach (var requiredModule in requiredModules)
                    {
                        Modules.Instance.Load(requiredModule, reload);
                    }
                }

                _currentLog = new StringBuilder();
                _loaded = OnLoad();


                stopwatch.Stop();

            }
            catch (Exception e)
            {
                Logging.Logger.Crash(e);
                Logging.Logger.Print("!!!! CRITICAL ERROR IN Module " + this.ToString() + " !!!!");
            }
            finally
            {
                Logger.Print($"Loaded Module {GetType().Name} in {stopwatch?.ElapsedMilliseconds}ms succesfully");
            }

            return _loaded;
        }

        public virtual Type[] RequiredModules() => null;
    }
}