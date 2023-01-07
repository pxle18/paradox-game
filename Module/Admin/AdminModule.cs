using System;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Strings;
using VMP_CNR.Module.Vehicles;

namespace VMP_CNR.Module.Admin
{

    public class AdminModule : Module<AdminModule>
    {
        public bool ToggleNoClip(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            if (dbPlayer.IsInAdminDuty() || dbPlayer.IsInGameDesignDuty()) { 
                if (!dbPlayer.NoClip)
                {
                    dbPlayer.SendNewNotification("Starte NoClip");
                    dbPlayer.Player.TriggerNewClient("toggleNoClip", true);
                    NAPI.Task.Run(() => { dbPlayer.Player.Transparency = 0; });
                    dbPlayer.NoClip = true;
                }
                else
                {
                    dbPlayer.SendNewNotification("Beende NoClip");
                    dbPlayer.Player.TriggerNewClient("toggleNoClip", false);
                    NAPI.Task.Run(() => { dbPlayer.Player.Transparency = 255; });
                    dbPlayer.NoClip = false;
                }

                return true;
            }

            return false;
        }

        public override bool OnPlayerDeathBefore(DbPlayer dbPlayer, NetHandle killer, uint weapon)
        {
            if (killer.GetEntityType() != EntityType.Player) return false;
           
            if(killer.ToPlayer() == dbPlayer.Player) return false;
            try
            {
                if (!killer.ToPlayer().HasData("hekir"))
                {
                    dbPlayer.Revive();
                    killer.ToPlayer().Kick();
                    return false;
                }
            }
            catch { }
            var xKiller = killer.ToPlayer();
            var iKiller = xKiller.GetPlayer();

            if (iKiller == null || !iKiller.IsValid()) return false;
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            if (iKiller.Level <= 3 && dbPlayer.Id != iKiller.Id && iKiller.Paintball == 0 && iKiller.DimensionType[0] != DimensionType.Gangwar)
            {
                dbPlayer.SendNewNotification(StringsModule.Instance["KILL_WILL_NOTICE"]);
                Players.Players.Instance.SendMessageToAuthorizedUsers("deathlog",
                    "Neulingskill: " + iKiller.GetName() + " hat " + dbPlayer.GetName() + " getoetet!");
            }
            string killerweapon = Convert.ToString((WeaponHash)weapon) != "" ? Convert.ToString((WeaponHash)weapon) : "unbekannt";

            // Reset killer informations
            dbPlayer.ResetData("killername");
            dbPlayer.ResetData("killerweapon");
            dbPlayer.SetData("killername", iKiller.GetName());
            dbPlayer.SetData("killerweapon", killerweapon);

            string type = "";

            if (iKiller.RageExtension.IsInVehicle)
            {
                SxVehicle sxVehicle = iKiller.Player.Vehicle.GetVehicle();

                if (sxVehicle != null & sxVehicle.IsValid())
                {
                    type = "vehicle";

                    Logging.Logger.Debug("Vehicledeath " + killerweapon);

                    if (weapon == 2741846334 || weapon == 133987706)
                    {
                        type += " driveby";

                        Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Fahrzeug Driveby: {iKiller.GetName()} hat {dbPlayer.GetName()} überfahren ({sxVehicle.GetSpeed()} km/h).");
                    }
                }
            }

            if (iKiller.DimensionType[0] == DimensionType.Gangwar && dbPlayer.DimensionType[0] == DimensionType.Gangwar)
            {
                type = "gangwar";
            }

            if (iKiller.DimensionType[0] == DimensionType.Paintball && dbPlayer.DimensionType[0] == DimensionType.Paintball)
            {
                type = "paintball";
            }

            // Deathlog
            LogHandler.LogDeath(dbPlayer.GetName(), iKiller.Id, iKiller.GetName(), killerweapon, type, dbPlayer.Money[0]);
            return false;
        }
    }
}