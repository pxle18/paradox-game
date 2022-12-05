using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Support;

namespace VMP_CNR.Module.Email
{
    public static class EmailTemplates
    {
        public static string GetTicketTemplate(int ticketSum, string ticketDesc)
        {

            return $"<small>Los Santos Police Department - {DateTime.Now.ToString("d")}</small> <br><br>" +
                $"Ihnen wurde ein Ticket in Höhe von <b>${ticketSum}</b> ausgestellt.<br><br>" +
                $"<b>Grund:</b><br>" +
                $"{ticketDesc}";

        }

        public static string GetTicketRemoveTemplate(string ticketDesc)
        {

            return $"<small>Los Santos Police Department - {DateTime.Now.ToString("d")}</small> <br><br>" +
                $"Ihnen wurde ein Ticket erlassen.<br><br>" +
                $"<b>Ticket Grund:</b><br>" +
                $"{ticketDesc}";
        }

        public static string GetTicketRemoveListTemplate(List<CrimePlayerReason> crimes)
        {
            string returns = $"<small>Los Santos Police Department - {DateTime.Now.ToString("d")}</small> <br><br>" +
                $"Ihnen wurden folgende Tickets erlassen.<br><br>" +
                $"<b>Tickets:</b><br>";

            foreach (CrimePlayerReason crime in crimes)
            {
                returns += crime.Name + "<br>";
            }
            return returns;
        }

        public static string GetArrestTemplate(List<CrimePlayerReason> crimes, int jailCosts, int jailTime)
        {
            string returns = $"<small>Los Santos Police Department - {DateTime.Now.ToString("d")}</small> <br><br>" +
                $"Sie wurden ins Gefängnis eingewiesen. Der Staat hat eine Haftzeit von {jailTime} Hafteinheiten und einen Haftbetrag von ${jailCosts} festgelegt!<br><br>" +
                $"<b>Begangene Straftaten:</b><br>";

            foreach(CrimePlayerReason crime in crimes)
            {
                returns += crime.Name + "<br>";
            }
            return returns;
        }
    }
}
