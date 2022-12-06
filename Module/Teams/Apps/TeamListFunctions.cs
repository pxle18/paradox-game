using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Module.NSA;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using static VMP_CNR.Module.Teams.Apps.TeamListApp;

namespace VMP_CNR.Module.Teams.Apps
{
    public static class TeamListFunctions
    {
        public static List<TeamMember> GetTeamMembersForList(DbPlayer dbPlayer)
        {
            var members = new List<TeamMember>();
            foreach (var member in dbPlayer.Team.Members.ToList())
            {
                var currentDbPlayer = member.Value;
                if (currentDbPlayer == null || !currentDbPlayer.IsValid()) continue;

                if (currentDbPlayer.IsInAdminDuty() || currentDbPlayer.IsInGuideDuty() || currentDbPlayer.IsInGameDesignDuty()) continue;

                var currentTeamRankPermission = currentDbPlayer.TeamRankPermission;

                string l_Name = currentDbPlayer.GetName();

                if (!dbPlayer.Team.IsGangsters())
                {
                    if (currentDbPlayer.IsInDuty())
                        l_Name = currentDbPlayer.GetName() + " (ID)";
                    else
                        l_Name = currentDbPlayer.GetName() + " (NiD)";
                }

                uint handyNumber = currentDbPlayer.handy[0];

                if (currentDbPlayer.HasData("nsaChangedNumber"))
                {
                    handyNumber = (uint)currentDbPlayer.GetData("nsaChangedNumber");
                }

                members.Add(new TeamMember(currentDbPlayer.Id, l_Name,
                    currentDbPlayer.TeamRank, currentTeamRankPermission.Inventory, currentTeamRankPermission.Bank,
                    currentTeamRankPermission.Manage, handyNumber));
            }
            members = members.OrderByDescending(t => t.Rank).ToList();

            // FIB
            if (dbPlayer.TeamId == (uint)TeamTypes.TEAM_FIB)
            {
                var tmp = new List<TeamMember>();

                foreach (var l_Member in TeamModule.Instance.GetById((int)TeamTypes.TEAM_GOV).Members)
                {
                    var l_FIBPlayer = l_Member.Value;
                    if (l_FIBPlayer == null || !l_FIBPlayer.IsValid())
                        continue;

                    if (l_FIBPlayer.IsInAdminDuty()) continue;
                    if (l_FIBPlayer.IsInGameDesignDuty()) continue;

                    var l_Permission = l_FIBPlayer.TeamRankPermission;

                    string l_Name = l_FIBPlayer.GetName();
                    if (l_FIBPlayer.IsInDuty())
                    {
                        l_Name = $"[GOV] {l_FIBPlayer.GetName()} (ID)";
                    }
                    else
                    {
                        l_Name = $"[GOV] {l_FIBPlayer.GetName()} (NiD)";
                    }


                    uint handyNumber = l_FIBPlayer.handy[0];

                    if (l_FIBPlayer.HasData("nsaChangedNumber"))
                    {
                        handyNumber = (uint)l_FIBPlayer.GetData("nsaChangedNumber");
                    }

                    tmp.Add(new TeamMember(l_FIBPlayer.Id, l_Name,
                        l_FIBPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                        l_Permission.Manage, handyNumber));

                }
                tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                members = members.Concat(tmp).ToList();
                tmp.Clear();
            }

            // NSA
            if (dbPlayer.IsNSADuty && dbPlayer.IsNSAState >= (int)NSARangs.NORMAL)
            {
                var tmp = new List<TeamMember>();

                foreach (var l_Member in TeamModule.Instance.GetById((int)TeamTypes.TEAM_ARMY).Members.ToList())
                {
                    var l_FIBPlayer = l_Member.Value;
                    if (l_FIBPlayer == null || !l_FIBPlayer.IsValid())
                        continue;

                    if (l_FIBPlayer.IsInAdminDuty()) continue;
                    if (l_FIBPlayer.IsInGameDesignDuty()) continue;

                    var l_Permission = l_FIBPlayer.TeamRankPermission;

                    string l_Name = l_FIBPlayer.GetName();
                    if (l_FIBPlayer.IsInDuty())
                    {
                        l_Name = $"[ARMY] {l_FIBPlayer.GetName()} (ID)";
                    }
                    else
                    {
                        l_Name = $"[ARMY] {l_FIBPlayer.GetName()} (NiD)";
                    }


                    uint handyNumber = l_FIBPlayer.handy[0];

                    if (l_FIBPlayer.HasData("nsaChangedNumber"))
                    {
                        handyNumber = (uint)l_FIBPlayer.GetData("nsaChangedNumber");
                    }

                    tmp.Add(new TeamMember(l_FIBPlayer.Id, l_Name,
                        l_FIBPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                        l_Permission.Manage, handyNumber));

                }
                tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                members = members.Concat(tmp).ToList();
                tmp.Clear();

                foreach (var l_Member in TeamModule.Instance.GetById((int)TeamTypes.TEAM_POLICE).Members.ToList())
                {
                    var l_PDPlayer = l_Member.Value;
                    if (l_PDPlayer == null || !l_PDPlayer.IsValid())
                        continue;
                    if (l_PDPlayer.IsInAdminDuty()) continue;
                    if (l_PDPlayer.IsInGameDesignDuty()) continue;

                    var l_Permission = l_PDPlayer.TeamRankPermission;

                    string l_Name = l_PDPlayer.GetName();
                    if (l_PDPlayer.IsInDuty())
                    {
                        l_Name = $"[LSPD] {l_PDPlayer.GetName()} (ID)";
                    }
                    else
                    {
                        l_Name = $"[LSPD] {l_PDPlayer.GetName()} (NiD)";
                    }


                    uint handyNumber = l_PDPlayer.handy[0];

                    if (l_PDPlayer.HasData("nsaChangedNumber"))
                    {
                        handyNumber = (uint)l_PDPlayer.GetData("nsaChangedNumber");
                    }

                    tmp.Add(new TeamMember(l_PDPlayer.Id, l_Name,
                        l_PDPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                        l_Permission.Manage, handyNumber));
                }
                tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                members = members.Concat(tmp).ToList();
                tmp.Clear();

                foreach (var l_Member in TeamModule.Instance.GetById((int)TeamTypes.TEAM_SWAT).Members.ToList())
                {
                    var l_PDPlayer = l_Member.Value;
                    if (l_PDPlayer == null || !l_PDPlayer.IsValid())
                        continue;
                    if (l_PDPlayer.IsInAdminDuty()) continue;
                    if (l_PDPlayer.IsInGameDesignDuty()) continue;

                    var l_Permission = l_PDPlayer.TeamRankPermission;

                    string l_Name = l_PDPlayer.GetName();
                    if (l_PDPlayer.IsInDuty())
                    {
                        l_Name = $"[SWAT] {l_PDPlayer.GetName()} (ID)";
                    }
                    else
                    {
                        l_Name = $"[SWAT] {l_PDPlayer.GetName()} (NiD)";
                    }


                    uint handyNumber = l_PDPlayer.handy[0];

                    if (l_PDPlayer.HasData("nsaChangedNumber"))
                    {
                        handyNumber = (uint)l_PDPlayer.GetData("nsaChangedNumber");
                    }

                    tmp.Add(new TeamMember(l_PDPlayer.Id, l_Name,
                        l_PDPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                        l_Permission.Manage, handyNumber));
                }
                tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                members = members.Concat(tmp).ToList();
                tmp.Clear();

                foreach (var l_Member in TeamModule.Instance.GetById((int)TeamTypes.TEAM_FIB).Members.ToList())
                {
                    var l_PDPlayer = l_Member.Value;
                    if (l_PDPlayer == null || !l_PDPlayer.IsValid())
                        continue;
                    if (l_PDPlayer.IsInAdminDuty()) continue;
                    if (l_PDPlayer.IsInGameDesignDuty()) continue;

                    var l_Permission = l_PDPlayer.TeamRankPermission;

                    string l_Name = l_PDPlayer.GetName();
                    if (l_PDPlayer.IsInDuty())
                    {
                        l_Name = $"[FIB] {l_PDPlayer.GetName()} (ID)";
                    }
                    else
                    {
                        l_Name = $"[FIB] {l_PDPlayer.GetName()} (NiD)";
                    }

                    uint handyNumber = l_PDPlayer.handy[0];

                    if (l_PDPlayer.HasData("nsaChangedNumber"))
                    {
                        handyNumber = (uint)l_PDPlayer.GetData("nsaChangedNumber");
                    }

                    tmp.Add(new TeamMember(l_PDPlayer.Id, l_Name,
                        l_PDPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                        l_Permission.Manage, handyNumber));
                }
                tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                members = members.Concat(tmp).ToList();
                tmp.Clear();

                foreach (var l_Member in TeamModule.Instance.GetById((int)TeamTypes.TEAM_MEDIC).Members.ToList())
                {
                    var l_PDPlayer = l_Member.Value;
                    if (l_PDPlayer == null || !l_PDPlayer.IsValid())
                        continue;
                    if (l_PDPlayer.IsInAdminDuty()) continue;
                    if (l_PDPlayer.IsInGameDesignDuty()) continue;

                    var l_Permission = l_PDPlayer.TeamRankPermission;

                    string l_Name = l_PDPlayer.GetName();
                    if (l_PDPlayer.IsInDuty())
                    {
                        l_Name = $"[LSMC] {l_PDPlayer.GetName()} (ID)";
                    }
                    else
                    {
                        l_Name = $"[LSMC] {l_PDPlayer.GetName()} (NiD)";
                    }

                    uint handyNumber = l_PDPlayer.handy[0];

                    if (l_PDPlayer.HasData("nsaChangedNumber"))
                    {
                        handyNumber = (uint)l_PDPlayer.GetData("nsaChangedNumber");
                    }

                    tmp.Add(new TeamMember(l_PDPlayer.Id, l_Name,
                        l_PDPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                        l_Permission.Manage, handyNumber));
                }
                tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                members = members.Concat(tmp).ToList();
                tmp.Clear();

                foreach (var l_Member in TeamModule.Instance.GetById((int)TeamTypes.TEAM_DPOS).Members.ToList())
                {
                    var l_PDPlayer = l_Member.Value;
                    if (l_PDPlayer == null || !l_PDPlayer.IsValid())
                        continue;
                    if (l_PDPlayer.IsInAdminDuty()) continue;
                    if (l_PDPlayer.IsInGameDesignDuty()) continue;

                    var l_Permission = l_PDPlayer.TeamRankPermission;

                    string l_Name = l_PDPlayer.GetName();
                    if (l_PDPlayer.IsInDuty())
                    {
                        l_Name = $"[DPOS] {l_PDPlayer.GetName()} (ID)";
                    }
                    else
                    {
                        l_Name = $"[DPOS] {l_PDPlayer.GetName()} (NiD)";
                    }


                    uint handyNumber = l_PDPlayer.handy[0];

                    if (l_PDPlayer.HasData("nsaChangedNumber"))
                    {
                        handyNumber = (uint)l_PDPlayer.GetData("nsaChangedNumber");
                    }

                    tmp.Add(new TeamMember(l_PDPlayer.Id, l_Name,
                        l_PDPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                        l_Permission.Manage, handyNumber));
                }
                tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                members = members.Concat(tmp).ToList();
            }

            return members;
        }
    }
}
