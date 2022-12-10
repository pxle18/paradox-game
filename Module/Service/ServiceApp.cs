using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Service
{
    public class ServiceApp : SimpleApp
    {
        public ServiceApp() : base("ServiceRequestApp") { }

        [RemoteEvent]
        public void cancelServiceRequest(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.HasData("service") && dbPlayer.GetData("service") > 0)
            {
                bool status = ServiceModule.Instance.CancelOwnService(dbPlayer, (uint)dbPlayer.GetData("service"));

                if (status)
                {
                    switch (dbPlayer.GetData("service"))
                    {
                        case 1:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)TeamTypes.TEAM_POLICE].SendNotification($"Der Notruf von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben den Notruf abgebrochen!");

                            break;
                        case 7:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)TeamTypes.TEAM_MEDIC].SendNotification($"Der Notruf von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben den Notruf abgebrochen!");

                            break;
                        case 3:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)TeamTypes.TEAM_DRIVINGSCHOOL].SendNotification($"Die Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben die Anfrage abgebrochen!");

                            break;
                        case 16:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)TeamTypes.TEAM_DPOS].SendNotification($"Die Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben die Anfrage abgebrochen!");

                            break;
                        case 4:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)TeamTypes.TEAM_NEWS].SendNotification($"Die Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben die Anfrage abgebrochen!");

                            break;
                        case 26:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)TeamTypes.TEAM_LSC].SendNotification($"Die Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben die Anfrage abgebrochen!");
                            break;
                        case 13:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)TeamTypes.TEAM_ARMY].SendNotification($"Die Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben die Anfrage abgebrochen!");
                            break;
                        case 14:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int) TeamTypes.TEAM_GOV].SendNotification($"Die Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben die Anfrage abgebrochen!");
                            break;
                        case 40:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)TeamTypes.TEAM_GOV].SendNotification($"Die Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben die Anfrage abgebrochen!");
                            break;
                    }
                }
            }
        }
    }
}