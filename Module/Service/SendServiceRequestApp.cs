using GTANetworkAPI;
using System.Linq;
using System.Text.RegularExpressions;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Computer.Apps.MarketplaceApp;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Service
{
    public class SendServiceRequestApp : SimpleApp
    {
        public SendServiceRequestApp() : base("ServiceSendRequestApp") { }

        [RemoteEvent]
        public void addServiceRequest(Player client, string department, string message, bool number, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var requestSuccess = false;

            if (dbPlayer.HasData("service") && dbPlayer.GetData("service") > 0)
            {
                dbPlayer.SendNewNotification("Sie haben bereits einen Notruf/Service offen!");
                return;
            }

            var telnr = "0";
            if (number)
            {
                telnr = dbPlayer.handy[0].ToString();
            }

            message = replaceContent(message);

            switch (department)
            {
                case "police":
                    
                    if (dbPlayer.IsACop() && dbPlayer.IsInDuty() && dbPlayer.TeamId != (int)TeamTypes.TEAM_GOV)
                    {
                        return;
                    }
                    
                    if (TeamModule.Instance[(int)TeamTypes.TEAM_POLICE].GetTeamMembers().Where(c => c.Duty).Count() 
                        - TeamModule.Instance[(int)TeamTypes.TEAM_POLICE].GetTeamMembers().Where(c => c.IsInAdminDuty() || c.IsInGuideDuty() || c.IsInGameDesignDuty()).Count() 
                        > 0)
                    {
                        TeamModule.Instance[(int)TeamTypes.TEAM_POLICE].SendNotification($"Ein Notruf von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (TeamModule.Instance[(int)TeamTypes.TEAM_FIB].GetTeamMembers().Where(c => c.Duty).Count()
                        - TeamModule.Instance[(int)TeamTypes.TEAM_FIB].GetTeamMembers().Where(c => c.IsInAdminDuty() || c.IsInGuideDuty() || c.IsInGameDesignDuty()).Count()
                        > 0)
                    {
                        TeamModule.Instance[(int)TeamTypes.TEAM_FIB].SendNotification($"Ein Notruf von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)TeamTypes.TEAM_POLICE, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)TeamTypes.TEAM_POLICE, service);

                        dbPlayer.SetData("service", 1);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben einen Notruf zur Polizei abgesendet!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Die Leitstelle ist derzeit nicht besetzt!");
                    }

                    break;
                case "medic":
                    
                    if(ServiceModule.Instance.IsServiceInRangeOfTeam((uint)TeamTypes.TEAM_MEDIC, dbPlayer.Player.Position))
                    {
                        dbPlayer.SendNewNotification("In der Nähe wurde bereits ein Notruf an das LSMC abgesendet!");
                        return;
                    }

                    if (TeamModule.Instance[(int)TeamTypes.TEAM_MEDIC].GetTeamMembers().Where(c => c.Duty).Count()
                        - TeamModule.Instance[(int)TeamTypes.TEAM_MEDIC].GetTeamMembers().Where(c => c.IsInAdminDuty() || c.IsInGuideDuty() || c.IsInGameDesignDuty()).Count()
                        > 0)
                    {
                        TeamModule.Instance[(int)TeamTypes.TEAM_MEDIC].SendNotification($"Ein Notruf von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)TeamTypes.TEAM_MEDIC, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)TeamTypes.TEAM_MEDIC, service);

                        dbPlayer.SetData("service", 7);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben einen Notruf zur Rettungswache abgesendet!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Die Leitstelle ist derzeit nicht besetzt!");
                    }

                    break;
                case "fahrlehrer":
                    if (dbPlayer.Player.Position.DistanceTo(new Vector3(-810.6085, -1347.864, 5.166561)) >= 20.0f)
                    {
                        dbPlayer.SendNewNotification("Sie muessen an der Fahrschule sein um einen Fahrlehrer zu rufen!");
                        return;
                    }

                    if (TeamModule.Instance[(int)TeamTypes.TEAM_DRIVINGSCHOOL].GetTeamMembers().Where(c => c.Duty).Count()
                        - TeamModule.Instance[(int)TeamTypes.TEAM_DRIVINGSCHOOL].GetTeamMembers().Where(c => c.IsInAdminDuty() || c.IsInGuideDuty() || c.IsInGameDesignDuty()).Count()
                        > 0)
                    {
                        TeamModule.Instance[(int)TeamTypes.TEAM_DRIVINGSCHOOL].SendNotification($"Eine Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)TeamTypes.TEAM_DRIVINGSCHOOL, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)TeamTypes.TEAM_DRIVINGSCHOOL, service);

                        dbPlayer.SetData("service", 3);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben einen Fahrlehrer gerufen!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Derzeit ist kein Fahrlehrer im Dienst!");
                    }

                    break;
                case "tow":
                    if (TeamModule.Instance[(int)TeamTypes.TEAM_DPOS].GetTeamMembers().Where(c => c.Duty).Count()
                        - TeamModule.Instance[(int)TeamTypes.TEAM_DPOS].GetTeamMembers().Where(c => c.IsInAdminDuty() || c.IsInGuideDuty() || c.IsInGameDesignDuty()).Count()
                        > 0)
                    {
                        TeamModule.Instance[(int)TeamTypes.TEAM_DPOS].SendNotification($"Eine Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)TeamTypes.TEAM_DPOS, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)TeamTypes.TEAM_DPOS, service);

                        dbPlayer.SetData("service", 16);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben einen Abschleppdienst gerufen!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Derzeit ist der Abschleppdienst nicht verfuegbar!");
                    }

                    break;
                case "news":
                    if (TeamModule.Instance[(int)TeamTypes.TEAM_NEWS].GetTeamMembers().Where(c => c.AccountStatus == AccountStatus.LoggedIn).Count()
                        - TeamModule.Instance[(int)TeamTypes.TEAM_NEWS].GetTeamMembers().Where(c => c.IsInAdminDuty() || c.IsInGuideDuty() || c.IsInGameDesignDuty()).Count()
                        > 0)
                    {
                        TeamModule.Instance[(int)TeamTypes.TEAM_NEWS].SendNotification($"Eine Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)TeamTypes.TEAM_NEWS, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)TeamTypes.TEAM_NEWS, service);

                        dbPlayer.SetData("service", 4);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben die News kontaktiert!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Derzeit ist kein Weazel News Mitarbeiter im Dienst!");
                    }

                    break;
                case "lsc":
                    TeamModule.Instance[(int)TeamTypes.TEAM_LSC].SendNotification($"Eine Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                    requestSuccess = true;

                    // Trzdm Prüfung weil man sonst für jeden case andere Variablen-Namen drin lassen muss. #Much
                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)TeamTypes.TEAM_LSC, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)TeamTypes.TEAM_LSC, service);

                        dbPlayer.SetData("service", 26);

                        if (status)
                            dbPlayer.SendNewNotification("Sie haben den Los Santos Customs informiert!");
                    }

                    break;
                case "government":
                    if (TeamModule.Instance[(int) TeamTypes.TEAM_GOV].GetTeamMembers().Where(c => c.Duty).Count()
                        - TeamModule.Instance[(int)TeamTypes.TEAM_GOV].GetTeamMembers().Where(c => c.IsInAdminDuty() || c.IsInGuideDuty() || c.IsInGameDesignDuty()).Count()
                        > 0)
                    {
                        TeamModule.Instance[(int) TeamTypes.TEAM_GOV].SendNotification($"Eine Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint) TeamTypes.TEAM_GOV, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint) TeamTypes.TEAM_GOV, service);

                        dbPlayer.SetData("service", 14);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben die Regierung informiert!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Derzeit ist die Regierung nicht verfuegbar!");
                    }

                    break;
                case "army":

                    if (dbPlayer.TeamId == (int)TeamTypes.TEAM_ARMY) return;

                    if (TeamModule.Instance[(int)TeamTypes.TEAM_ARMY].GetTeamMembers().Where(c => c.Duty).Count()
                        - TeamModule.Instance[(int)TeamTypes.TEAM_ARMY].GetTeamMembers().Where(c => c.IsInAdminDuty() || c.IsInGuideDuty() || c.IsInGameDesignDuty()).Count()
                        > 0)
                    {
                        TeamModule.Instance[(int)TeamTypes.TEAM_ARMY].SendNotification($"Eine Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)TeamTypes.TEAM_ARMY, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)TeamTypes.TEAM_ARMY, service);

                        dbPlayer.SetData("service", 13);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben die US Army informiert!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Derzeit ist die US Army nicht verfuegbar!");
                    }

                    break;
                case "auction":
                    if (TeamModule.Instance[(int)TeamTypes.TEAM_AUCTION].GetTeamMembers().Where(c => c.Duty).Count() > 0)
                    {
                        TeamModule.Instance[(int)TeamTypes.TEAM_AUCTION].SendNotification($"Eine Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)TeamTypes.TEAM_AUCTION, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)TeamTypes.TEAM_AUCTION, service);

                        dbPlayer.SetData("service", 40);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben ein Auktionsgebot gesendet!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Derzeit ist keine Auktion verfügbar!");
                    }

                    break;
            }
        }
        private string replaceContent(string input)
        {
            input = input.Replace("\"", "");
            input = input.Replace("'", "");
            input = input.Replace("`", "");
            input = input.Replace("´", "");
            //return Regex.Replace(input, @"^[a-zA-Z0-9\s]+$", "");
            return Regex.Replace(input, @"[^a-zA-Z0-9\s]", "");
        }
    }
}