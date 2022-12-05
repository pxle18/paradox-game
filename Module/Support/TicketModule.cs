using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Support
{
    public sealed class TicketModule : Module<TicketModule>
    {
        public Dictionary<string, Ticket> tickets;

        public override bool Load(bool reload = false)
        {
            tickets = new Dictionary<string, Ticket>();
            return true;
        }

        public bool Add(DbPlayer dbPlayer, Ticket ticket)
        {
            if (tickets.ContainsKey(dbPlayer.GetName())) return false;

            tickets.Add(dbPlayer.GetName(), ticket);
            return true;
        }

        public bool Accept(DbPlayer dbPlayer, DbPlayer destinationPlayer)
        {
            var createdTicket = GetTicketByOwner(destinationPlayer);
            if (createdTicket == null) return false;
            if (createdTicket.Accepted.Count > 0) return false;

            bool status = createdTicket.Accepted.Add(dbPlayer.GetName());
            return status;
        }

        public Dictionary<string, Ticket> GetAll()
        {
            return tickets;
        }

        public List<Ticket> GetAcceptedTickets(DbPlayer dbPlayer)
        {
            return (from kvp in tickets where kvp.Value.Accepted.Contains(dbPlayer.GetName()) select kvp.Value).ToList();
        }

        public List<Ticket> GetOpenTickets()
        {
            List<String> toBeRemoved = new List<String>();
            foreach (var ticket in tickets)
            {
                if (DateTime.Compare(ticket.Value.Created_at.AddMinutes(15), DateTime.Now) < 0 || !Players.Players.Instance.FindPlayer(ticket.Key).IsValid()) toBeRemoved.Add(ticket.Key);
            }

            foreach (var ticket in toBeRemoved)
            {
                if (tickets.ContainsKey(ticket)) tickets.Remove(ticket);
            }

            return (from kvp in tickets orderby kvp.Value.Created_at ascending where kvp.Value.Accepted.Count() == 0 select kvp.Value).ToList();
        }

        public Ticket GetTicketByOwner(DbPlayer dbPlayer)
        {
            return tickets.ContainsKey(dbPlayer.GetName()) ? tickets[dbPlayer.GetName()] : null;
        }

        public bool DeleteTicketByOwner(DbPlayer dbPlayer)
        {
            var createdTicket = GetTicketByOwner(dbPlayer);
            if (createdTicket == null) return false;
            bool status = tickets.Remove(dbPlayer.GetName());
            return status;
        }

        public bool ChangeChatStatus(DbPlayer dbPlayer, bool status)
        {
            var createdTicket = GetTicketByOwner(dbPlayer);
            if (createdTicket == null) return false;

            createdTicket.ChatStatus = status;
            return true;
        }

        public bool getCurrentChatStatus(DbPlayer dbPlayer)
        {
            var createdTicket = GetTicketByOwner(dbPlayer);
            if (createdTicket == null) return false;

            bool status = createdTicket.ChatStatus;
            return status;
        }

        public string getCurrentTicketSupporter(DbPlayer dbPlayer)
        {
            var createdTicket = GetTicketByOwner(dbPlayer);
            if (createdTicket == null) return null;

            string user = string.Join(',', createdTicket.Accepted);
            return user;
        }
    }
}
