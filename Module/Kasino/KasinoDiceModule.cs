using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;

namespace VMP_CNR.Module.Kasino
{
    public class KasinoDiceModule : SqlModule<KasinoDiceModule, KasinoDice, uint>
    {
        public string DiceString = "Würfelspiel";

        protected override string GetQuery()
        {
            return "SELECT * FROM `kasino_dice`";
        }

        public override void OnTenSecUpdate()
        {
            foreach (var kasinoDice in GetAll().Values.ToList())
            {
                if (!kasinoDice.IsInGame || kasinoDice.StartTime == DateTime.MinValue || kasinoDice.Price == 0)continue;

                if (DateTime.Now >= kasinoDice.StartTime.AddSeconds(5) && !kasinoDice.FinishGame)
                {
                    kasinoDice.FinishGame = true;
                    if (kasinoDice.Participant.Count < 2)
                    {
                        if (kasinoDice.Participant.Count > 0)
                        {
                            kasinoDice.Participant.First().GiveMoney(kasinoDice.Price);
                            kasinoDice.Participant.First().SendNewNotification("Es hat keiner die Anfrage angenommen.", PlayerNotification.NotificationType.CASINO, DiceString);
                        }

                        kasinoDice.StartTime = DateTime.MinValue;
                        kasinoDice.IsInGame = false;
                        kasinoDice.Price = 0;
                        kasinoDice.Participant = new List<DbPlayer>();
                        kasinoDice.FinishGame = false;
                        return;
                    }

                    DbPlayer dbPlayerHighestNumber = null;
                    int highestNumber = 0;

                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        foreach (DbPlayer dbPlayer in kasinoDice.Participant.ToList())
                        {
                            var trig = Main.Random.Next(1, 201);
                            if (highestNumber <= trig)
                            {
                                highestNumber = trig;
                                dbPlayerHighestNumber = dbPlayer;
                            }
                            // Self start anim
                            dbPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop), "mp_player_int_upperwank", "mp_player_int_wank_01");

                            int counter = kasinoDice.Participant.Count;
                            foreach (DbPlayer dbPlayer2 in kasinoDice.Participant.ToList())
                            {
                                    dbPlayer2.SendNewNotification($"1337Allahuakbar$dice", duration: 2000);
                                
                                    await NAPI.Task.WaitForMainThread(1800);
                                    dbPlayer2.SendNewNotification($"{dbPlayer.GetName()} - {trig}", PlayerNotification.NotificationType.CASINO, DiceString, duration: counter * 2000 + 2000);
                                    counter--;
                                
                            }
                            
                            await NAPI.Task.WaitForMainThread(2000);
                            dbPlayer.StopAnimation();
                           
                        }

                        //finaly someone wins
                        dbPlayerHighestNumber.SendNewNotification($"Du hast {(int) (kasinoDice.Price * kasinoDice.Participant.Count * 0.95)} gewonnen!", PlayerNotification.NotificationType.CASINO, DiceString);
                        dbPlayerHighestNumber.GiveMoney((int)(kasinoDice.Price * kasinoDice.Participant.Count * 0.95));
                        
                        dbPlayerHighestNumber.PlayAnimation((int) (AnimationFlags.AllowPlayerControl | AnimationFlags.Loop), "missfbi3_sniping", "male_unarmed_a");
                        await NAPI.Task.WaitForMainThread(2000);
                        dbPlayerHighestNumber.StopAnimation();
                        

                        Logger.AddDiceGameToDbLog(dbPlayerHighestNumber.Id, kasinoDice.Price, kasinoDice.Participant.Count, kasinoDice.Participant.Count * kasinoDice.Price);
                        kasinoDice.StartTime = DateTime.MinValue;
                        kasinoDice.IsInGame = false;
                        kasinoDice.Price = 0;
                        kasinoDice.Participant = new List<DbPlayer>();
                        kasinoDice.FinishGame = false;
                    }));
                }
            }
        }


        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E) return false;

            KasinoDice kasinoDice = GetClosest(dbPlayer);
            if (kasinoDice == null) return false;

            if (!dbPlayer.Rank.CanAccessFeature("casino") && !KasinoModule.Instance.CasinoGuests.Contains(dbPlayer)) return true;

            if (!kasinoDice.IsInGame)
            {
                dbPlayer.SetData("casino_dice", kasinoDice);
                StartKasinoDice(dbPlayer, kasinoDice);

            }
            else
            {
                dbPlayer.SendNewNotification("Die Runde läuft noch!", PlayerNotification.NotificationType.CASINO, DiceString);
                return false;
            }




            return true;
        }

        public void StartKasinoDice(DbPlayer dbPlayer, KasinoDice kasinoDice)
        {
            GTANetworkAPI.NAPI.Task.Run(() => ComponentManager.Get<TextInputBoxWindow>().Show()(
                dbPlayer, new TextInputBoxWindowObject() { Title = $"Würfelrunde ({kasinoDice.MinPrice} $ - {kasinoDice.MaxPrice} $)", Callback = "StartDiceGame", Message = "Gib einen Einsatz ein, den jeder Teilnehmer setzen muss." }));
        }



        public KasinoDice GetClosest(DbPlayer dbPlayer)
        {
            return GetAll().FirstOrDefault(kasinoDice => kasinoDice.Value.Position.DistanceTo(dbPlayer.Player.Position) < kasinoDice.Value.Radius).Value;
        }
    }
}
