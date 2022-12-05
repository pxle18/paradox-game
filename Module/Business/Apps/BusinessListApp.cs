using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Business.Apps
{
    public class BusinessListApp : SimpleApp
    {
        public BusinessListApp() : base("BusinessListApp")
        {
        }

        [RemoteEvent]
        public void leaveBusiness(Player p_Player, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.IsValid() || !l_DbPlayer.IsMemberOfBusiness())
                return;
                
            l_DbPlayer.RemoveBusinessMembership(l_DbPlayer.GetActiveBusiness());
            l_DbPlayer.ActiveBusinessId = 0;
            l_DbPlayer.UpdateApps();
        }

        [RemoteEvent]
        public void saveBusinessMOTD(Player p_Player, string motd, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            DbPlayer l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.IsValid() || !l_DbPlayer.IsMemberOfBusiness())
                return;
            var l_edit_Member = l_DbPlayer.GetActiveBusiness().GetMember((uint)l_DbPlayer.Id);
            
            if (l_edit_Member == null || (!l_edit_Member.Owner && !l_edit_Member.Manage))
            {
                l_DbPlayer.SendNewNotification("Dazu bist du nicht berechtigt.", title:"Business", notificationType:PlayerNotification.NotificationType.BUSINESS);
                return;
            }

            motd = MySqlHelper.EscapeString(motd);

            l_DbPlayer.GetActiveBusiness().ChangeMotd(motd);
        }

        [RemoteEvent]
        public void editBusinessMember(Player p_Player, int p_MemberID, bool p_Bank, bool p_Manage, int p_Salary, bool raffinery, bool fuelstation, bool nightclub, bool tattoo, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            try { 
                DbPlayer l_DbPlayer = p_Player.GetPlayer();
                if (l_DbPlayer == null || !l_DbPlayer.IsValid() || !l_DbPlayer.IsMemberOfBusiness())
                    return;

                //user who gets edited
                var l_Member = l_DbPlayer.GetActiveBusiness().GetMember((uint)p_MemberID);
                if (l_Member == null)
                    return;

                //user who edits
                var l_edit_Member = l_DbPlayer.GetActiveBusiness().GetMember((uint)p_Player.GetPlayer().Id);
                if (l_edit_Member == null)
                    return;

                //If owner gets edited by someone else then himself OR edit player has no permission
                if ((l_Member.Owner && !l_edit_Member.Owner) || !l_edit_Member.Manage) {
                    l_DbPlayer.SendNewNotification("Dazu bist du nicht berechtigt.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                    return;
                }

                if (l_edit_Member.Owner || l_edit_Member.Manage)
                {
                    l_Member.Raffinery = raffinery;
                    l_Member.Fuelstation = fuelstation;
                    l_Member.NightClub = nightclub;
                    l_Member.Tattoo = tattoo;
                }
                if (l_edit_Member.Owner)
                {
                    l_Member.Money = p_Bank;
                    l_Member.Manage = p_Manage;
                }

                if (l_edit_Member.Owner || l_edit_Member.Money)
                {
                    if (p_Salary < 0) return;


                    l_Member.Salary = p_Salary;
                }

                l_DbPlayer.SaveBusinessMembership(l_Member);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEvent]
        public void kickBusinessMember(Player client, int playerId, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsMemberOfBusiness()) return;
                
                // which business we are talking about...
                Business business = dbPlayer.GetActiveBusiness();
                if (business == null) return;
                
                // player who kicks
                Business.Member editorMember = business.GetMember(dbPlayer.Id);
                if (editorMember == null) return;

                // if the player who edits is not a manager decline!
                if (!editorMember.Manage)
                {
                    dbPlayer.SendNewNotification(
                        "Dazu bist du nicht berechtigt!", 
                        title: "Business", 
                        notificationType: 
                        PlayerNotification.NotificationType.BUSINESS
                    );
                    
                    return;
                }
                
                // player who should be kicked
                Business.Member editedMember = business.GetMemberFromAllMembers((uint) playerId);
                if (editedMember == null) return;

                // if the player who should be kicked is the owner of the business decline!
                if (editedMember.Owner)
                {
                    dbPlayer.SendNewNotification(
                        "Dazu bist du nicht berechtigt!", 
                        title: "Business", 
                        notificationType: 
                        PlayerNotification.NotificationType.BUSINESS
                    );
                    
                    return;
                }
                
                DbPlayer editedPlayer = Players.Players.Instance.FindPlayer(playerId);
                
                // if the player who should be kicked is not online we just have to execute the sql query.
                if (editedPlayer == null || !editedPlayer.IsValid())
                {
                    business.RemoveBusinessMembership((uint) playerId, business);
                    
                    return;
                }
                
                editedPlayer.RemoveBusinessMembership(dbPlayer.GetActiveBusiness());
                editedPlayer.ActiveBusinessId = 0;
                editedPlayer.UpdateApps();
            }));
        }

        [RemoteEvent]
        public void addPlayerToBusiness(Player p_Player, string p_Name, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                try
                {
                    DbPlayer l_InvitingPlayer = p_Player.GetPlayer();
                    if (l_InvitingPlayer == null || !l_InvitingPlayer.IsValid())
                        return;

                    if (l_InvitingPlayer.GetActiveBusiness() == null)
                        return;
                    if (String.IsNullOrEmpty(p_Name)) return;
                    DbPlayer l_DbPlayer = Players.Players.Instance.FindPlayer(p_Name);
                    if (l_DbPlayer == null)
                    {
                        l_InvitingPlayer.SendNewNotification($"{p_Name} wurde nicht gefunden.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                        return;
                    }

                    if (l_DbPlayer.GetActiveBusiness() != null)
                    {
                        l_InvitingPlayer.SendNewNotification($"{p_Name} ist bereits in einem Business.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                        return;
                    }

                    ComponentManager.Get<ConfirmationWindow>().Show()(l_DbPlayer, new ConfirmationObject($"{l_InvitingPlayer.GetActiveBusiness().Name}", $"Möchtest du die Einladung von {l_InvitingPlayer.GetName()} annehmen?", "addBusinessMemberConfirm", l_InvitingPlayer.GetName(), l_InvitingPlayer.GetActiveBusiness().Name));
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        [RemoteEvent]
        public void addBusinessMemberConfirm(Player p_Player, string p_InvitingPersonName, string p_BusinessName, string key)
        {
            if (!p_Player.CheckRemoteEventKey(key)) return;
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                try
                {
                    var l_DbPlayer = p_Player.GetPlayer();
                    if (l_DbPlayer == null || !l_DbPlayer.IsValid())
                        return;

                    var l_InvitingPlayer = Players.Players.Instance.FindPlayer(p_InvitingPersonName).Player.GetPlayer();
                    if (l_InvitingPlayer == null || !l_InvitingPlayer.IsValid())
                        return;

                    var l_ManagePerm = l_InvitingPlayer.GetActiveBusinessMember();
                    if (l_ManagePerm == null)
                        return;

                    if (l_ManagePerm.Manage == false && l_ManagePerm.Owner == false)
                    {
                        l_DbPlayer.SendNewNotification($"{l_InvitingPlayer.GetName()} ist nicht berechtigt, Personen in das Business einzuladen.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                        l_InvitingPlayer.SendNewNotification("Du bist nicht berechtig, Personen in das Business einzuladen.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                        return;
                    }

                    if (l_DbPlayer.GetActiveBusiness() != null)
                    {
                        l_InvitingPlayer.SendNewNotification($"{l_DbPlayer.GetName()} ist bereits in einem Business.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                        l_DbPlayer.SendNewNotification("Du bist bereits in einem Business.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                        return;
                    }

                    l_DbPlayer.SendNewNotification($"Willkommen im Business " + l_InvitingPlayer.GetActiveBusiness().Name, title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                    l_DbPlayer.AddBusinessMembership(l_InvitingPlayer.GetActiveBusiness());
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }
    }
}