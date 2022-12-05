using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Computer.Apps.ServiceApp;

namespace VMP_CNR.Module.Calling
{
    public enum CallStatus
    {
        Ringing = 1,
        Accepted = 2,
        Declined = 3,
    }
    public class Call
    {
        public List<CallingMember> CallingMembers { get; set; }

        public void RefreshToParticipants()
        {

        }
    }

    public class CallingMember
    {
        public uint PlayerId { get; set; }
        public bool Muted { get; set; }

        public CallStatus CallStatus { get; set; }
    }
}
