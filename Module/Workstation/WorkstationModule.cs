using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Heist.Planning;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Workstation.Windows;

namespace VMP_CNR.Module.Workstation
{
    public class WorkstationModule : SqlModule<WorkstationModule, Workstation, uint>
    {

        protected override string GetQuery()
        {
            return "SELECT * FROM `workstations`;";
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.WorkstationId = reader.GetUInt32("workstation_id");   

            // Load Containers/Create
            dbPlayer.WorkstationEndContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.WORKSTATIONOUTPUT);
            dbPlayer.WorkstationFuelContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.WORKSTATIONFUEL);
            dbPlayer.WorkstationSourceContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.WORKSTATIONINPUT);
        }

        public override void OnFiveMinuteUpdate()
        {
            foreach(DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers().Where(p => p.HasWorkstation()))
            {
                if (dbPlayer == null || !dbPlayer.IsValid()
                    || dbPlayer.WorkstationEndContainer == null || dbPlayer.WorkstationFuelContainer == null || dbPlayer.WorkstationSourceContainer == null) continue;

                // Wenn Source leer
                if (dbPlayer.WorkstationSourceContainer.IsEmpty()) continue;

                Workstation workstation = dbPlayer.GetWorkstation();
                if (workstation == null || workstation.Interval15) continue;

                ProgressWorkstation(dbPlayer, workstation);
            }
        }

        public override void OnFifteenMinuteUpdate()
        {
            foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers().Where(p => p.HasWorkstation()))
            {
                if (dbPlayer == null || !dbPlayer.IsValid()
                    || dbPlayer.WorkstationEndContainer == null || dbPlayer.WorkstationFuelContainer == null || dbPlayer.WorkstationSourceContainer == null) continue;

                // Wenn Source leer
                if (dbPlayer.WorkstationSourceContainer.IsEmpty()) continue;

                Workstation workstation = dbPlayer.GetWorkstation();
                if (workstation == null || !workstation.Interval15) continue;

                ProgressWorkstation(dbPlayer, workstation);
            }
        }

        public void ProgressWorkstation(DbPlayer dbPlayer, Workstation workstation)
        {
            // Verarbeiten...

            bool containerFailure = false;
            // Prüfe ob Player die Source Items hat...
            foreach (KeyValuePair<uint, int> kvp in workstation.SourceConvertItems)
            {
                if (dbPlayer.WorkstationSourceContainer.GetItemAmount(kvp.Key) < kvp.Value) containerFailure = true;
            }

            if (containerFailure) return;

            // Kann End item geadded werden?
            if (!dbPlayer.WorkstationEndContainer.CanInventoryItemAdded(workstation.EndItemId, workstation.End5MinAmount)) return;
            // Fuel vorhanden?
            if (workstation.FuelItemId != 0 && dbPlayer.WorkstationFuelContainer.GetItemAmount(workstation.FuelItemId) < workstation.Fuel5MinAmount) return;

            // Remove
            dbPlayer.WorkstationFuelContainer.RemoveItem(workstation.FuelItemId, workstation.Fuel5MinAmount);

            foreach (KeyValuePair<uint, int> kvp in workstation.SourceConvertItems)
            {
                dbPlayer.WorkstationSourceContainer.RemoveItem(kvp.Key, kvp.Value);
            }
            dbPlayer.WorkstationEndContainer.AddItem(workstation.EndItemId, workstation.End5MinAmount);
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if(colShapeState == ColShapeState.Enter && colShape.HasData("workstation"))
            {
                Workstation workstation = Get(colShape.GetData<uint>("workstation"));
                dbPlayer.SendNewNotification($"Workstation {workstation.Name}, drücke E um diese für $2500 zu mieten!");
                if(workstation.FuelItemId == 0) dbPlayer.SendNewNotification($"Du kannst hier {ItemModelModule.Instance.Get(workstation.EndItemId).Name} herstellen!");
                else dbPlayer.SendNewNotification($"Du kannst hier {ItemModelModule.Instance.Get(workstation.EndItemId).Name} herstellen!");
                return true;
            }
            return false;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.RageExtension.IsInVehicle) return false;

            Workstation workstation = WorkstationModule.Instance.GetAll().Where(w => w.Value.NpcPosition.DistanceTo(dbPlayer.Player.Position) < 1.5f).FirstOrDefault().Value;
            if (workstation != null)
            {
                if(!workstation.LimitTeams.Contains(dbPlayer.TeamId) && !workstation.LimitTeams.Contains(99))
                {
                    dbPlayer.SendNewNotification($"Du scheinst mir zu unseriös zu sein... Arbeitest du schon etwas anderes?");
                    return true;
                }
                if(dbPlayer.WorkstationId == workstation.Id)
                {
                    dbPlayer.SendNewNotification($"Sie sind hier bereits eingemietet!");
                    return true;
                }

                // Planning room weaponstuff
                if(workstation.SpecialType == WorkstationSpecialType.PlanningRoomStahlpatronen)
                {
                    PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id);
                    if(room == null || room.BasementWeaponsLevel < 1)
                    {
                        dbPlayer.SendNewNotification($"Sie müssen zuerst die Waffenfunktion ausbauen!");
                        return true;
                    }
                }

                ComponentManager.Get<WorkstationWindow>().Show()(dbPlayer, workstation);
                return true;

            }
            return false;
        }
    }
    public class WorkstationConfirm : Script
    {
        [RemoteEvent]
        public void N_Workstation_Confirm(Player p_Player, string _id, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = p_Player.GetPlayer();
            dbPlayer.WorkstationEndContainer.ClearInventory();
            dbPlayer.WorkstationFuelContainer.ClearInventory();
            dbPlayer.WorkstationSourceContainer.ClearInventory();

            dbPlayer.SendNewNotification($"Sie haben sich erfolgreich eingemietet und können diese nun benutzen!");
            dbPlayer.WorkstationId = Convert.ToUInt32(_id);
            dbPlayer.SaveWorkstation();
        }
    }

    public static class WorkstationPlayerExtension
    {
        public static void SaveWorkstation(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE player SET workstation_id = '{dbPlayer.WorkstationId}' WHERE id = '{dbPlayer.Id}'");
        }

        public static Workstation GetWorkstation(this DbPlayer dbPlayer)
        {
            if (WorkstationModule.Instance.Contains(dbPlayer.WorkstationId)) 
            {
                return WorkstationModule.Instance.Get(dbPlayer.WorkstationId);
            }
            return null;
        }

        public static bool HasWorkstation(this DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            return dbPlayer.WorkstationId != 0;
        }
    }
}
