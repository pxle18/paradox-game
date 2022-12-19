using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Procedures.Enumerations;
using VMP_CNR.Module.Procedures.Interfaces;

namespace VMP_CNR.Module.Procedures
{
    public class ProcedureClientEvents : Script
    {
        [RemoteEvent]
        public void ProcessProcedure(Player player, string returnString, string remoteEventKey)
        {
            if (!player.CheckRemoteEventKey(remoteEventKey)) return;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            ProcedureModule.Instance.ProcessProcedure(dbPlayer, returnString);
        }
    }

    public sealed class ProcedureModule : Module<ProcedureModule>
    {
        public void CreateProcedure(DbPlayer player, Procedure procedure)
        {
            procedure.Procedures.RemoveAt(0);

            player.Procedure = procedure;

            var firstProcedure = player.Procedure.Procedures.First();
            firstProcedure.Invoke(player);
        }

        public void ShowProcedureStep(DbPlayer player, ProcedureCallbackTypes procedureCallback, string procedureKey, string procedureTitle, string procedureMessage)
        {
            if (player.Procedure == null) return;

            player.CurrentProcedureKey = procedureKey;

            switch (procedureCallback)
            {
                case ProcedureCallbackTypes.Text:
                    ComponentManager.Get<TextInputBoxWindow>().Show()(player,
                        new TextInputBoxWindowObject() { Title = procedureTitle, Callback = "ProcessProcedure", Message = procedureMessage });

                    break;
            }

        }

        public void ProcessProcedure(DbPlayer player, string returnString)
        {
            if (player.Procedure == null) return;

            player.ProcedureData[player.CurrentProcedureKey] = returnString;

            var firstProcedure = player.Procedure.Procedures.First();
            if (firstProcedure == null)
            {
                player.Procedure.FinishProcedure(player, player.ProcedureData);
                return;
            }

            firstProcedure.Invoke(player);

            player.Procedure.Procedures.RemoveAt(0);
        }
    }
}
