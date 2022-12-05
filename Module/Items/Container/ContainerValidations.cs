using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.RemoteEvents;

namespace VMP_CNR.Module.Items
{
    public static class ContainerValidations
    {
        public static bool CanUseAction(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsValid()) return false;

            // Check Cuff Die Death
            if (!dbPlayer.CanInteract()) return false;

            if (dbPlayer.HasData("disableinv") && dbPlayer.GetData("disableinv")) return false; // Show Inventory
                        
            return true;
        }
    }
}
