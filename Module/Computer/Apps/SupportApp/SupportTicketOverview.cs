using GTANetworkAPI;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Support;

namespace VMP_CNR.Module.Computer.Apps.SupportApp
{
    public class SupportTicketOverview : SimpleApp
    {
        public SupportTicketOverview() : base("SupportTicketOverview") { }

        [RemoteEvent]
        public void supportTeleportToPlayer(Player client, string name, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, name)) return;
            if (string.IsNullOrEmpty(name)) return;
            if (!dbPlayer.CanAccessMethod()) return;


            if (dbPlayer.RankId == 0)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            NAPI.Task.Run(() =>
            {
                var destinationPlayer = Players.Players.Instance.FindPlayer(name);
                if (destinationPlayer == null) return;

                if (dbPlayer.RageExtension.IsInVehicle)
                {
                    dbPlayer.Player.Vehicle.Position = destinationPlayer.Player.Position;
                    dbPlayer.Player.Vehicle.Dimension = destinationPlayer.Player.Dimension;
                }
                else
                {
                    dbPlayer.Player.SetPosition(destinationPlayer.Player.Position);
                }

                dbPlayer.DimensionType[0] = destinationPlayer.DimensionType[0];
                dbPlayer.SetDimension(destinationPlayer.Player.Dimension);

                dbPlayer.SendNewNotification("Sie haben sich zu " + destinationPlayer.GetName() + " teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

                if (dbPlayer.Rank.CanAccessFeature("silentTeleport")) return;
                if (dbPlayer.IsInGuideDuty())
                {
                    destinationPlayer.SendNewNotification("Guide " + destinationPlayer.GetName() + " hat sich zu ihnen teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                }
                else if (dbPlayer.RankId < 6)
                {
                    destinationPlayer.SendNewNotification("Administrator " + dbPlayer.GetName() + " hat sich zu ihnen teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                }
            });
        }

        [RemoteEvent]
        public async void closeTicket(Player client, string name, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;

            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!MySQLHandler.IsValidNoSQLi(dbPlayer, name)) return;
            if (string.IsNullOrEmpty(name)) return;

            await NAPI.Task.WaitForMainThread(0);

            if (dbPlayer.RankId == 0)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            var findplayer = Players.Players.Instance.FindPlayer(name);
            if (findplayer == null) return;

            var ticket = TicketModule.Instance.GetTicketByOwner(findplayer);

            if (ticket != null)
            {
                Logger.AddSupportLog(ticket.Player.Id, dbPlayer.Id, ticket.Description, ticket.Created_at);
            }
            bool ticketResponse = TicketModule.Instance.DeleteTicketByOwner(findplayer);
            //bool konversationResponse = KonversationModule.Instance.Delete(findplayer);

            dbPlayer.SendNewNotification(ticketResponse ? $"Sie haben das Ticket von {findplayer.GetName()} geschlossen!" : $"Das Ticket von {findplayer.GetName()} konnte nicht geschlossen werden!");
            findplayer.SendNewNotification("Ihr Ticket wurde geschlossen!");
            
        }
    }
}
