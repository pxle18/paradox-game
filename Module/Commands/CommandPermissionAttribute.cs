using System;

namespace VMP_CNR.Module.Commands
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandPermissionAttribute : Attribute
    {
        public uint? TeamId { get; set; } = null;
        public bool PlayerRankPermission { get; set; } = false;
        public bool AllowedDeath { get; set; } = false;
        public bool AllowedOnCuff { get; set; } = true;
        public bool AllowedOnTied { get; set; } = true;
    }
}