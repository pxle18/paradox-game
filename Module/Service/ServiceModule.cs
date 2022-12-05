using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Computer.Apps.ExportApp.Apps;
using VMP_CNR.Module.Computer.Apps.StreifenApp;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Zone;

namespace VMP_CNR.Module.Service
{
    public sealed class ServiceModule : Module<ServiceModule>
    {
        public Dictionary<uint, List<Service>> serviceList;

        public Dictionary<uint, List<ServiceEvaluation>> evaluations;

        public override bool Load(bool reload = false)
        {
            serviceList = new Dictionary<uint, List<Service>>();
            evaluations = new Dictionary<uint, List<ServiceEvaluation>>();
            return true;
        }

        public void AddForEvaluation(DbPlayer dbPlayer)
        {
            try
            {
                if (evaluations.ContainsKey(dbPlayer.TeamId))
                {
                    List<ServiceEvaluation> evals = evaluations[dbPlayer.TeamId].ToList();

                    ServiceEvaluation eval = evals.Where(e => e.id == dbPlayer.Id).FirstOrDefault();
                    if (eval != null)
                    {
                        eval.amount++;
                        eval.timestr = DateTime.Now;
                    }
                    else
                    {
                        evals.Add(new ServiceEvaluation(dbPlayer.Id, 1));
                    }
                    evaluations[dbPlayer.TeamId] = evals;
                }
                else
                {
                    evaluations.Add(dbPlayer.TeamId, new List<ServiceEvaluation>() { new ServiceEvaluation(dbPlayer.Id, 1) });
                }
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
            }
        }

        public bool IsServiceInRangeOfTeam(uint teamid, Vector3 Position)
        {

            List<Service> teamServices = GetServicesForTeam(teamid);
            
            if(teamServices != null && teamServices.Count > 0 && teamServices.Where(s => s.Position.DistanceTo(Position) < 10.0f).Count() > 0)
            {
                return true;
            }

            return false;
        }

        public List<Service> GetServicesForTeam(uint teamId)
        {
            // CopSpecials
            if (teamId == (int)teams.TEAM_FIB) teamId = (int)teams.TEAM_POLICE;

            List<Service> teamServices;

            if (!serviceList.ContainsKey(teamId))
            {
                teamServices = new List<Service>();
                serviceList.Add(teamId, teamServices);
            }
            else
            {
                teamServices = serviceList[teamId];
            }

            return teamServices;
        }

        public List<Service> GetAvailableServices(DbPlayer dbPlayer)
        {
            return GetServicesForTeam(dbPlayer.TeamId).Where(service => service.Accepted.Count() == 0).ToList();
        }

        public List<Service> GetCreatedServices(DbPlayer dbPlayer)
        {
            try
            {
                List<Service> returnServices = new List<Service>();

                foreach(KeyValuePair<uint, List<Service>> kvp in serviceList)
                {
                    if(kvp.Value != null && kvp.Value.Count > 0)
                    {
                        foreach(Service service in kvp.Value.ToList().Where(s => s.Player.Id == dbPlayer.Id))
                        {
                            returnServices.Add(service);
                        }
                    }
                }

                return returnServices;
            }
            catch(Exception e) {
                Logging.Logger.Crash(e);
                return new List<Service>();
            }
        }

        public List<Service> GetAcceptedTeamServices(DbPlayer dbPlayer)
        {
            return GetServicesForTeam(dbPlayer.TeamId).Where(service => service.Accepted.Count() > 0).ToList();
        }

        public List<Service> GetAcceptedServices(DbPlayer dbPlayer)
        {
            return GetServicesForTeam(dbPlayer.TeamId).Where(service => service.Accepted.Contains(dbPlayer.GetName())).ToList();
        }

        public override void OnMinuteUpdate()
        {
            if (!serviceList.ContainsKey((int)teams.TEAM_MEDIC)) return;
            foreach(Service service in serviceList[(int)teams.TEAM_MEDIC].Where(s => s.Player != null && s.Player.IsValid() && s.Player.IsInjured()))
            {
                string optional = "";

                // fix bug with service and self die... (wird nicht mehr überschrieben, nur noch bei notrufen sicherheitsfreigabe o. lsmc selbst)
                if (service.Player.GovLevel.ToLower() == "a" || service.Player.GovLevel.ToLower() == "b" || service.Player.GovLevel.ToLower() == "c")
                {
                    optional = "[PRIORISIERT]";
                    service.Message = $"{optional} Verletzung: {service.Player.Injury.Name} - {service.Player.Injury.TimeToDeath - service.Player.deadtime[0]} Min";
                }
                if (service.Player.TeamId == (int)teams.TEAM_MEDIC)
                {
                    optional = "[LSMC]"; 
                    service.Message = $"{optional} Verletzung: {service.Player.Injury.Name} - {service.Player.Injury.TimeToDeath - service.Player.deadtime[0]} Min";
                }
            }
        }

        /*
        public void RemoveInjuredPlayerService(DbPlayer dbPlayer)
        {
            if (!serviceList.ContainsKey((int)teams.TEAM_MEDIC)) return;
            Instance.CancelOwnService(dbPlayer, (uint)teams.TEAM_MEDIC);
            dbPlayer.ResetData("service");
        }*/

        public bool Add(DbPlayer dbPlayer, uint teamId, Service service)
        {
            var teamServices = GetServicesForTeam(teamId);
            foreach (var itr in teamServices)
            {
                if (itr.Player == null)
                    continue;

                if (itr.Player.Id == dbPlayer.Id)
                    return false;
            }

            teamServices.Add(service);
            return true;
        }

        public string GetSpecialDescriptionForPlayer(DbPlayer dbPlayer, Service service)
        {
            string desc = "[" + Convert.ToInt32(service.Position.DistanceTo(dbPlayer.Player.Position)) + "m - gesendet: " + service.Created.ToString("HH:mm:ss") + " ";
            if (service.TeamId == (int)teams.TEAM_MEDIC)
            {
                desc += ZoneModule.Instance.IsInNorthZone(service.Position) ? " | Norden]" : "]";

            }
            else desc += "]";

            desc += service.Message;

            return desc;
        }

        public bool Accept(DbPlayer dbPlayer, DbPlayer destinationPlayer)
        {
            var createdService = GetCreatedServices(destinationPlayer);
            if (createdService == null || createdService.Count() <= 0) return false;

            uint playerTeam = (dbPlayer.TeamId != (uint)teams.TEAM_FIB) ? (uint)dbPlayer.TeamId : (uint)teams.TEAM_POLICE;

            if (playerTeam != createdService[0].TeamId) return false;
            if (createdService[0].Accepted.Contains(dbPlayer.GetName())) return false;
            
            bool status = createdService[0].Accepted.Add(dbPlayer.GetName());

            // add evaluation app data
            AddForEvaluation(dbPlayer);

            // Add Officer from same streife
            if (StreifenAppModule.Instance.TeamStreifen.ContainsKey(dbPlayer.TeamId))
            {
                Streife streife = StreifenAppModule.Instance.TeamStreifen[dbPlayer.TeamId].ToList().Where(s => s.Officers.Where(so => so != null && so.PlayerId == dbPlayer.Id).Count() > 0).FirstOrDefault();

                if(streife != null)
                {
                    foreach(StreifeOfficer officer in streife.Officers.ToList())
                    {
                        if (officer == null) continue;
                        if (officer.PlayerId == dbPlayer.Id) continue;

                        DbPlayer offPlayer = Players.Players.Instance.GetByDbId(officer.PlayerId);
                        if(offPlayer != null && offPlayer.IsValid())
                        {
                            AddForEvaluation(offPlayer);
                        }
                    }
                }
            }


            return status;
        }

        public bool CancelOwnService(DbPlayer dbPlayer, uint teamId)
        {
            var teamServices = GetServicesForTeam(teamId);
            if (teamServices.Count == 0) return false;

            var createdService = GetCreatedServices(dbPlayer);
            if (createdService.Count <= 0) return false;

            if (!teamServices.Contains(createdService[0])) return false;
            bool status = teamServices.Remove(createdService[0]);
            return status;
        }

        public bool Cancel(DbPlayer dbPlayer, DbPlayer player, uint teamId)
        {
            var teamServices = GetServicesForTeam(teamId);
            if (teamServices.Count == 0) return false;

            var createdService = GetCreatedServices(player);
            if (createdService == null || createdService.Count() <= 0) return false;

            bool status = teamServices.Remove(createdService[0]);
            return status;
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if(dbPlayer.IsInjured())
            {
                CancelOwnService(dbPlayer, (int)teams.TEAM_MEDIC);
            }
        }
    }
}
