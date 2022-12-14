using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.NSA;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.ReversePhone;
using VMP_CNR.Module.Space;
using VMP_CNR.Module.Telefon.App;
using VMP_CNR.Module.Voice;

namespace VMP_CNR.Module.Players.Phone
{
    public static class PhoneCall
    {
        public static string PHONECALL_TYPE = "phone_calling";
        public static string PHONENUMBER = "phone_number";

        public static bool IsPlayerInCall(Player player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;
            // is the requested player in phone a call
            if (!dbPlayer.HasData(PHONECALL_TYPE)) return false;

            return dbPlayer.GetData(PHONECALL_TYPE) == "waiting" ||
                   dbPlayer.GetData(PHONECALL_TYPE) == "incoming" ||
                   dbPlayer.GetData(PHONECALL_TYPE) == "active" ||
                   dbPlayer.HasData("current_caller");
        }

        public static void ClosePhone(this DbPlayer dbPlayer)
        {
            dbPlayer.Player.TriggerNewClient("hatNudeln", false);
        }

        public static async Task CancelPhoneCall(this DbPlayer dbPlayer)
        {
            Player player = dbPlayer.Player;
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract()) return;

            if (dbPlayer.HasData("current_caller"))
            {
                int callNumber = dbPlayer.GetData("current_caller");
                if (callNumber == 0) return;

                DbPlayer dbCalledPlayer = await CallManageApp.GetPlayerByPhoneNumber(callNumber);
                if (dbCalledPlayer != null && dbCalledPlayer.IsValid() && dbCalledPlayer.HasData("current_caller"))
                {
                    if (dbCalledPlayer.GetData("current_caller") == dbPlayer.handy[0])
                    {
                        dbCalledPlayer.SetData("current_caller", 0);
                        dbCalledPlayer.ResetData("current_caller");
                        dbCalledPlayer.Player.TriggerNewClient("cancelCall", 0);
                        dbCalledPlayer.Player.TriggerNewClient("setCallingPlayer", "");

                        ReversePhoneModule.Instance.AddPhoneHistory(dbPlayer, (int)dbCalledPlayer.handy[0], 0);
                        ReversePhoneModule.Instance.AddPhoneHistory(dbCalledPlayer, (int)dbPlayer.handy[0], 0);

                        NSAObservationModule.CancelPhoneHearing((int)dbCalledPlayer.handy[0]);

                        NAPI.Task.Run(() =>
                        {
                            if (!NAPI.Player.IsPlayerInAnyVehicle(dbCalledPlayer.Player) && dbCalledPlayer.CanInteract())
                                dbCalledPlayer.StopAnimation();
                        });
                    }
                    dbCalledPlayer.PlayerWhoHearRingtone = new List<DbPlayer>();

                }
            }

            NSAObservationModule.CancelPhoneHearing((int)dbPlayer.handy[0]);
            dbPlayer.ResetData("current_caller");
            dbPlayer.Player.TriggerNewClient("cancelCall", 0);
            dbPlayer.Player.TriggerNewClient("setCallingPlayer", "");

            NAPI.Task.Run(() =>
            {
                if (!NAPI.Player.IsPlayerInAnyVehicle(dbPlayer.Player))
                    dbPlayer.StopAnimation();
            });

            NSAObservationModule.CancelPhoneHearing((int)dbPlayer.handy[0]);
            dbPlayer.ResetData("current_caller");
            dbPlayer.Player.TriggerNewClient("cancelCall", 0);
            dbPlayer.Player.TriggerNewClient("setCallingPlayer", "");

            NAPI.Task.Run(() =>
            {
                if (!NAPI.Player.IsPlayerInAnyVehicle(dbPlayer.Player))
                    dbPlayer.StopAnimation();
            });
        }

        public static bool CanUserstartCall(DbPlayer dbPlayer)
        {
            // can player have a call
            if (!CanPlayerHaveCall(dbPlayer))
            {
                dbPlayer.SendNewNotification(
                    
                    "Fuer diese Aktion benötigst du ein verfuegbares " +
                    ItemModelModule.Instance.Get(174).Name);
                return false;
            }

            // is player already in call
            if (IsPlayerInCall(dbPlayer.Player))
            {
                dbPlayer.SendNewNotification(
                     "Du befindest dich bereits in einem Gespraech.");
                return false;
            }

            if (dbPlayer.IsOnMars())
            {
                dbPlayer.SendNewNotification("Auf dem Mars existieren keine Telefon-Masten!");
                return false;
            }

            // player can have a phone call
            return true;
        }

        public static bool CanPlayerHaveCall(DbPlayer dbPlayer)
        {
            // verify is not cuffed or tied
            if (dbPlayer.IsCuffed || dbPlayer.IsTied) return false;

            // verify has item smartphone
            return dbPlayer.Container.GetItemAmount(174) >= 1;
        }
    }
}
