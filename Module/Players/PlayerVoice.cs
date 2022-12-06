using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players
{
    public static class PlayerVoice
    {
        public static bool CanUseMegaphone(this DbPlayer dbPlayer)
        {
            return dbPlayer.IsACop() || 
                   dbPlayer.TeamId == (int) TeamTypes.TEAM_MEDIC ||
                   dbPlayer.TeamId == (int) TeamTypes.TEAM_DPOS || 
                   dbPlayer.TeamId == (int) TeamTypes.TEAM_NEWS ||
                   dbPlayer.TeamId == (int) TeamTypes.TEAM_GOV ||
                   dbPlayer.TeamId == (int) TeamTypes.TEAM_DRIVINGSCHOOL;
        }

        //Todo: needs externalized player module
        public static void RadioToggle(this DbPlayer dbPlayer)
        {
            /*if (!dbPlayer.HasData("RADIO_LS"))
            {
                if (dbPlayer.player.VehicleSeat == -1)
                {
                    for (int index = 0; index < Players.Count; index++)
                    {
                        if (!validatePlayer(index)) continue;
                        if (API.isPlayerInAnyVehicle(Players[index].player) &&
                            dbPlayer.player.Vehicle == Players[index].player.Vehicle)
                        {
                            Players[index].SetData("RADIO_LS", 1);
                            VoiceList.setPlayerRadio(Players[index], 0);
                            API.sendNotificationToPlayer(Players[index].player,
                                "Sie hören ~y~Radio Los Santos");
                        }
                    }
                    return;
                }
                else
                {
                    dbPlayer.SendNewNotification(
                        msgServerInfo + "Für diese Aktion müssen Sie Fahrer des Fahrzeuges sein!");
                    return;
                }
            }
            else
            {
                if (API.getPlayerVehicleSeat(dbPlayer.player) == -1)
                {
                    for (int index = 0; index < Players.Count; index++)
                    {
                        if (!validatePlayer(index)) continue;
                        if (API.isPlayerInAnyVehicle(Players[index].player) &&
                            dbPlayer.player.Vehicle == Players[index].player.Vehicle)
                        {
                            API.resetEntityData(Players[index].player, "RADIO_LS");
                            VoiceList.setPlayerRadio(Players[index], 0);
                            API.sendNotificationToPlayer(Players[index].player,
                                "Radio ausgeschaltet");
                        }
                    }
                    return;
                }
                else
                {
                    dbPlayer.SendNewNotification(
                        msgServerInfo + "Für diese Aktion müssen Sie Fahrer des Fahrzeuges sein!");
                    return;
                }
            }*/
        }
    }
}