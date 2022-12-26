using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Telefon.App;

namespace VMP_CNR.Module.NSA.Menu
{
    public class NSAListCallsMenuBuilder : MenuBuilder
    {
        public NSAListCallsMenuBuilder() : base(PlayerMenu.NSACallListMenu)
        {

        }

        public override Module.Menu.NativeMenu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.NativeMenu(Menu, "NSA Offene Anrufe");
            l_Menu.Add($"Schließen");

            List<uint> tmpList = new List<uint>();
            foreach (DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers())
            {
                if (!xPlayer.HasData("current_caller")) continue;
                if (tmpList.Contains(xPlayer.handy[0]) || tmpList.Contains((uint)xPlayer.GetData("current_caller"))) continue;

                DbPlayer ConPlayer = TelefonInputApp.GetPlayerByPhoneNumber(xPlayer.GetData("current_caller"));
                if (ConPlayer == null || !ConPlayer.IsValid()) continue;

                if (NSAObservationModule.ObservationList.Where(o => o.Value.PlayerId == ConPlayer.Id || o.Value.PlayerId == xPlayer.Id).Count() == 0) continue;

                tmpList.Add(xPlayer.handy[0]);
                tmpList.Add((uint)xPlayer.GetData("current_caller"));

                l_Menu.Add($"{xPlayer.GetName()} ({xPlayer.handy[0]}) == {ConPlayer.GetName()} ({ConPlayer.handy[0]})");
            }

            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {

                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }

                int idx = 1;

                List<uint> tmpList = new List<uint>();
                foreach (DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers())
                {
                    if (!xPlayer.HasData("current_caller")) continue;
                    if (tmpList.Contains(xPlayer.handy[0]) || tmpList.Contains((uint)xPlayer.GetData("current_caller"))) continue;

                    DbPlayer ConPlayer = TelefonInputApp.GetPlayerByPhoneNumber(xPlayer.GetData("current_caller"));
                    if (ConPlayer == null || !ConPlayer.IsValid()) continue;

                    if (NSAObservationModule.ObservationList.Where(o => o.Value.PlayerId == ConPlayer.Id || o.Value.PlayerId == xPlayer.Id).Count() == 0) continue;

                    tmpList.Add(xPlayer.handy[0]);
                    tmpList.Add((uint)xPlayer.GetData("current_caller"));

                    if(idx == index)
                    {
                        // Enable this if list with obersvations is active
                        if (!xPlayer.HasData("current_caller")) return false;
                        if (xPlayer.IsInAdminDuty()) return false;

                        if (ConPlayer == null || !ConPlayer.IsValid()) return false;
                        if (ConPlayer.IsInAdminDuty()) return false;

                        string voiceHashPush = xPlayer.VoiceHash + "~3~0~0~2;" + ConPlayer.VoiceHash;
                        dbPlayer.Player.TriggerEvent("setCallingPlayer", voiceHashPush);

                        dbPlayer.SetData("nsa_activePhone", xPlayer.handy[0]);

                        dbPlayer.SendNewNotification("Mithören gestartet " + xPlayer.handy[0]);
                        NSAModule.Instance.SendMessageToNSALead($"{dbPlayer.GetName()} hört nun das Telefonat von {xPlayer.GetName()} mit.");
                        return true;
                    }
                    idx++;
                }


                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
