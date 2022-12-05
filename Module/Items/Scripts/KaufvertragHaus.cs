using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> SellHouseScript(DbPlayer dbPlayer)
        {
            if (dbPlayer.RageExtension.IsInVehicle) return false;

            if (dbPlayer.job[0] != (int)jobs.JOB_Makler) return false;
            if (!ServerFeatures.IsActive("makler-haus"))
            {
                dbPlayer.SendNewNotification("Diese Funktion ist derzeit deaktiviert. Weitere Informationen findest du im Forum.");
                return false;
            }

            await NAPI.Task.WaitForMainThread(1);
            NAPI.Task.Run(() => ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() 
            { Title = "Haus verkaufen", Callback = "maklerHouseApplyObjectId", Message = "Nummer der Immobilie:" }));


            return false;
        }
    }
}