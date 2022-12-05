using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VMP_CNR.Handler;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Vehicles;
using static VMP_CNR.Module.Laboratories.Methlaboratory;

namespace VMP_CNR.Module.Laboratories
{
    public class MethlaboratoryEvents : Script
    {
        [RemoteEvent]
        public void ChangeMethlaboratoryTemperatur(Player player, string returnString, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            ChangeMethlaboratoryParameter(player, returnString, "Temperatur");
        }
        [RemoteEvent]
        public void ChangeMethlaboratoryDruck(Player player, string returnString, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            ChangeMethlaboratoryParameter(player, returnString, "Druck");
        }
        [RemoteEvent]
        public void ChangeMethlaboratoryRuehrgeschwindigkeit(Player player, string returnString, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            ChangeMethlaboratoryParameter(player, returnString, "Ruehrgeschwindigkeit");
        }
        [RemoteEvent]
        public void ChangeMethlaboratoryMenge(Player player, string returnString, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            ChangeMethlaboratoryParameter(player, returnString, "Menge");
        }

        [RemoteEvent]
        public void updateMethlaboratoryParameter(Player client, string returnString, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

        }

        public void ChangeMethlaboratoryParameter(Player player, string returnString, string parameterName)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (!Configurations.Configuration.Instance.MethLabEnabled) return;

            if (!Regex.IsMatch(returnString, @"^[0-9]*$"))
            {
                dbPlayer.SendNewNotification("Nur Zahlen sind erlaubt");
                return;
            }
            if (!float.TryParse(returnString, out float value)) return;

            if (dbPlayer.DimensionType[0] != DimensionType.Methlaboratory) return;
            Methlaboratory methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (methlaboratory == null) return;
            Parameter parameter = methlaboratory.Parameters.Find(delegate (Parameter para) { return para.Name == parameterName; });
            if (parameter.MinValue <= value && value <= parameter.MaxValue)
            {
                parameter.ActValue = value;
                MySQLHandler.ExecuteAsync(
                    $"UPDATE `team_methlaboratories` SET `{parameter.Name.ToLower()}` = '{value}' WHERE `team_methlaboratories`.`teamid` = '{methlaboratory.TeamId}';");
                dbPlayer.SendNewNotification($"{parameter.Name} wurde auf {value} {parameter.Einheit} eingestellt.");
            }
            else
            {
                dbPlayer.SendNewNotification("Das ist kein gültiger Einstellparameter. Minimal- und Maximalwert beachten!");
            }
        }
    }
}
