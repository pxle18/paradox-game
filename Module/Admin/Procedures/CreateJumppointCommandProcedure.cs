using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Procedures;
using VMP_CNR.Module.Procedures.Enumerations;
using VMP_CNR.Module.Procedures.Interfaces;

namespace VMP_CNR.Module.Admin.Procedures
{
    class CreateJumppointCommandProcedure : Procedure
    {
        public override List<Action<DbPlayer>> Procedures { get; set; } = new List<Action<DbPlayer>>() {
                StartCreateJumppoint,
        };

        public override Task FinishProcedure(DbPlayer player, Dictionary<string, string> procedureData)
        {
            if (!procedureData.TryGetValue("Name", out string jumpPointName))
            {
                player.SendNewNotification("Name not found.", PlayerNotification.NotificationType.ADMIN, "Procedure");
                return Task.CompletedTask;
            }

            player.SendNewNotification(jumpPointName, PlayerNotification.NotificationType.ADMIN, "Procedure");

            return Task.CompletedTask;
        }

        public static void StartCreateJumppoint(DbPlayer player) =>
            ProcedureModule.Instance.ShowProcedureStep(player, ProcedureCallbackTypes.Text, "Name", "Name", "Geben Sie den Namen des Jumppoints an.");
    }
}
