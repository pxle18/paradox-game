using System.Collections.Generic;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using Newtonsoft.Json;
using System;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players.Phone;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Players.Db;
using System.Linq;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.LeitstellenPhone;

namespace VMP_CNR.Module.Telefon.App
{
    public class TelefonInputApp : SimpleApp
    {
        public TelefonInputApp() : base("TelefonInput") { }

        public static DbPlayer GetPlayerByPhoneNumber(int p_PhoneNumber)
        {
            try
            {
                foreach (var l_Player in Players.Players.Instance.GetValidPlayers())
                {
                    if ((int)l_Player.handy[0] != p_PhoneNumber)
                        continue;

                    return l_Player;
                }
                return null;
            }
            catch (Exception e)
            {
                Logging.Logger.Crash(e);
                return null;
            } 
        }

        [RemoteEvent]
        public void callUserPhone(Player p_Player, int p_CallingNumber, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer l_Caller = p_Player.GetPlayer();
            if (l_Caller == null || !l_Caller.IsValid())
                return;

            int selfnumber = (int)l_Caller.handy[0];
            bool LeitstellenCallIncoming = false;

            // Wenn Player Leitstellentelefon hat
            TeamLeitstellenObject teamLeitstellenObject = LeitstellenPhoneModule.Instance.GetByAcceptor(l_Caller);
            if (teamLeitstellenObject != null)
            {
                selfnumber = teamLeitstellenObject.Number;
            }

            if (l_Caller.Container.GetItemAmount(174) < 1)
            {
                l_Caller.SendNewNotification("Sie besitzen kein Telefon.");
                return;
            }

            if (selfnumber == p_CallingNumber)
            {
                l_Caller.SendNewNotification("Du kannst dich nicht selber anrufen.", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (l_Caller.phoneSetting.flugmodus)
            {
                //Flugmodus aktiviert... kein Anruf möglich
                l_Caller.SendNewNotification("Der Flugmodus ist aktiviert... Kein Empfang", title: "NO SIGNAL", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }

            // player can start a phone call
            if (!PhoneCall.CanUserstartCall(l_Caller) || l_Caller.HasData("current_caller")) return;

            DbPlayer l_CalledPlayer = null;
            int l_CalledNumber = 0;



            if (LeitstellenPhoneModule.TeamNumberPhones.ContainsKey(p_CallingNumber))
            {
                teamLeitstellenObject = LeitstellenPhoneModule.Instance.GetLeitstelleByNumber(p_CallingNumber);
                
                if(teamLeitstellenObject == null || teamLeitstellenObject.Acceptor == null || !teamLeitstellenObject.Acceptor.IsValid())
                {
                    l_Caller.SendNewNotification("Die angegebene Rufnummer ist derzeit nicht verfuegbar.", notificationType: PlayerNotification.NotificationType.ERROR);
                    return;
                }

                uint acceptorTeam = teamLeitstellenObject.Acceptor.TeamId; 
                if (teamLeitstellenObject.TeamId == (uint)teams.TEAM_IAA && teamLeitstellenObject.Acceptor.IsNSADuty) acceptorTeam = (uint)teams.TEAM_IAA;

                if (acceptorTeam != teamLeitstellenObject.TeamId) {
                    l_Caller.SendNewNotification("Die angegebene Rufnummer ist derzeit nicht verfuegbar.", notificationType: PlayerNotification.NotificationType.ERROR);
                    return;
                }
                if(teamLeitstellenObject.StaatsFrakOnly && !l_Caller.IsACop() && l_Caller.TeamId != (int)teams.TEAM_NEWS &&
                    l_Caller.TeamId != (int)teams.TEAM_FIB && l_Caller.TeamId != (int)teams.TEAM_DPOS && l_Caller.TeamId != (int)teams.TEAM_MEDIC &&
                    l_Caller.TeamId != (int) teams.TEAM_DRIVINGSCHOOL)
                {
                    l_Caller.SendNewNotification("Diese Nummer ist für Sie nicht verfügbar!.", notificationType: PlayerNotification.NotificationType.ERROR);
                    return;
                }

                LeitstellenCallIncoming = true;

                l_CalledPlayer = teamLeitstellenObject.Acceptor;
                l_CalledNumber = teamLeitstellenObject.Number;
            }
            else
            {
                l_CalledPlayer = GetPlayerByPhoneNumber(p_CallingNumber);
            }


            if (l_CalledPlayer == null || l_CalledPlayer.Container.GetItemAmount(174) == 0 || l_CalledPlayer.phoneSetting.flugmodus || l_CalledPlayer.IsInAdminDuty() || l_CalledPlayer.IsInGuideDuty() || l_CalledPlayer.IsInGameDesignDuty())
            {
                l_Caller.SendNewNotification("Die angegebene Rufnummer ist derzeit nicht verfuegbar.", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }
            if (l_CalledPlayer.phoneSetting.blockCalls)
            {
                l_Caller.SendNewNotification("Die angegebene Rufnummer hat eingehende Anrufe blockiert.", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            CallUser l_CallerData = new CallUser()
            {
                method = "outgoing",
                caller = l_CalledNumber == 0 ? (int)l_CalledPlayer.handy[0] : l_CalledNumber,
                name = l_Caller.PhoneContacts.TryGetPhoneContactNameByNumber((uint)l_CalledNumber)
            };

            string IncomingCallerName = l_CalledPlayer.PhoneContacts.TryGetPhoneContactNameByNumber((uint)selfnumber);
            if (LeitstellenCallIncoming)
            {
                IncomingCallerName = (uint)selfnumber + " Leitstelle";
            }

            CallUser l_CallingData = new CallUser()
            {
                method = "incoming",
                caller = (int)selfnumber,
                name = IncomingCallerName
            };
            
            l_Caller.ResetData("current_caller");

            if (l_CalledPlayer.HasData("current_caller"))
            {
                l_Caller.SendNewNotification("Die angegebene Rufnummer ist derzeit im Gespraech.", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }
            l_Caller.SetData("current_caller", (int)l_CalledPlayer.handy[0]);
            l_CalledPlayer.SetData("current_caller", (int)l_Caller.handy[0]);

            l_Caller.Player.TriggerNewClient("setPhoneCallData", NAPI.Util.ToJson(l_CallerData));
            l_CalledPlayer.Player.TriggerNewClient("setPhoneCallData", NAPI.Util.ToJson(l_CallingData));
        }
    }

    public class CallUser
    {
        [JsonProperty(PropertyName = "method")]
        public string method { get; set; }
        [JsonProperty(PropertyName = "caller")]
        public int caller { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }
    }
}
