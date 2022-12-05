using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Voice;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Business.NightClubs;
using System.Threading.Tasks;
using VMP_CNR.Module.Schwarzgeld;
using VMP_CNR.Module.NSA;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Events.CWS;
using VMP_CNR.Module.Teams.Shelter;

namespace VMP_CNR.Module.Injury.Windows
{
    
    public class InsuranceWindow : Window<Func<DbPlayer, bool>>
    {
        private class ShowEvent : Event
        {
            public ShowEvent(DbPlayer dbPlayer) : base(dbPlayer)
            {
            }
        }

        public InsuranceWindow() : base("Insurance")
        {
        }

        public override Func<DbPlayer, bool> Show()
        {
            return (player) => OnShow(new ShowEvent(player));
        }

        [RemoteEvent]
        public void setInsurance(Player client, int insuranceType, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (insuranceType < 0 || insuranceType > 2) return;

            if(dbPlayer.InsuranceType == insuranceType)
            {
                dbPlayer.SendNewNotification("Du hast diese Art von Krankenversicherung bereits aktiv!");
                return;
            }

            switch(insuranceType)
            {
                case 0:
                    dbPlayer.SendNewNotification("Du hast dich für keine Krankenversicherung entschieden, alle Kosten trägst du nun selbst!");
                    dbPlayer.InsuranceType = insuranceType;
                    break;
                case 1:
                    dbPlayer.SendNewNotification("Du hast dich für eine Krankenversicherung entschieden, es werden 50% der Behandlungs und Komakosten übernommen!");
                    dbPlayer.InsuranceType = insuranceType;
                    break;
                case 2:
                    dbPlayer.SendNewNotification("Du hast dich für eine private Krankenversicherung entschieden, es werden 100% der Behandlungs und Komakosten übernommen!");
                    dbPlayer.InsuranceType = insuranceType;
                    break;
            }


            string insurance = "keine";
            if (dbPlayer.InsuranceType == 1)
            {
                insurance = "vorhanden";
            }
            else if (dbPlayer.InsuranceType == 2)
            {
                insurance = "privat";
            }


            dbPlayer.Player.TriggerNewClient("setInsurance", insurance);
            dbPlayer.SaveInsurance();
        }
    }
}
