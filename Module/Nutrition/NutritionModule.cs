using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Players.Db;
using GTANetworkAPI;
using System.Linq;
using VMP_CNR.Module.Commands;
using VMP_CNR.Module.Players;
using System.Threading.Tasks;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Logging;
using Newtonsoft.Json;
using VMP_CNR.Module.Kasino;

namespace VMP_CNR.Module.Nutrition
{
    /*
     * 
     * 
     * Healthy 100
     * Struggle 40
     * Bad 15
     * 
     * 
     * 
     */
     public class NutritionModule : Module<NutritionModule>
    {
        public int StandardFood = 100;
        public int StandardDrink = 100;

        public int Underfed = 30;
        public int Overfed = 170;

        public int Overfed_death = 200;
        public int Underfed_death = 15;

        public override void OnPlayerFirstSpawnAfterSync(DbPlayer dbPlayer)
        {

            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            //SHOW NUTRITION
            PushNutritionToPlayer(dbPlayer);
        }

        public override void OnFifteenMinuteUpdate()
        {
            foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
            {
                try
                {
                    if (dbPlayer == null || !dbPlayer.IsValid()) return;
                    if (dbPlayer.RankDuty>0) return;

                    dbPlayer.food[0] -= 3;
                    dbPlayer.drink[0] -= 3;

                    //PUSH NUTRITION
                    PushNutritionToPlayer(dbPlayer);
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }
        }

        public void setHealthy(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            dbPlayer.food[0] = this.StandardFood;
            dbPlayer.drink[0] = this.StandardDrink;
            PushNutritionToPlayer(dbPlayer);
        }

        public void TakeAShit(DbPlayer dbPlayer)
        {
            //Reduce Nutrition by shit
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.food[0] > StandardFood) { dbPlayer.food[0] = StandardFood; }
            if (dbPlayer.drink[0] > StandardDrink) { dbPlayer.drink[0] = StandardDrink; }

        }

        public void decreaseHealth(DbPlayer dbPlayer, int health)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (dbPlayer.IsInjured()) return;
            if (dbPlayer.Player.Health <= health)
            {
                dbPlayer.SetData("injured_by_nutrition", true);
            }

            dbPlayer.SetHealth(dbPlayer.Player.Health - health);
        }

        public void PushNutritionToPlayer(DbPlayer dbPlayer, bool effects = true)
        {
            try
            {
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                DoEffects(dbPlayer, effects);  

            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

   

        public void DoEffects(DbPlayer dbPlayer, bool effects = true)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!effects) return;

            bool check = false;

            int foodp = dbPlayer.food[0] / (Overfed_death / 100);
            int drinkp = dbPlayer.drink[0] / (Overfed_death / 100);

            if (foodp >= 100 || foodp <= 7 || drinkp > 100 || drinkp <= 7)
            {
                dbPlayer.Player.TriggerNewClient("setNutritionEating", 4);
                dbPlayer.Player.TriggerNewClient("setNutritionDrinking", 4);
                // INJURED CHECKEN decreaseHealth(dbPlayer, 100);
                return;
            }
            
            if(foodp <= 15 || foodp >= 85)
            {
                dbPlayer.Player.TriggerNewClient("setNutritionEating", 4);
                // INJURED CHECKEN if (!check) decreaseHealth(dbPlayer, 10);
                check = true;
            }
            if (drinkp <= 15 || drinkp >= 85)
            {
                dbPlayer.Player.TriggerNewClient("setNutritionDrinking", 4);
                // INJURED CHECKEN if (!check) decreaseHealth(dbPlayer, 10);
                check = true;
            }

            if((foodp > 15&&foodp < 30)||(foodp > 70 && foodp < 85))
            {
                dbPlayer.Player.TriggerNewClient("setNutritionEating", 2);
            }

            if ((drinkp > 15 && drinkp < 30) || (drinkp > 70 && drinkp < 85))
            {
                dbPlayer.Player.TriggerNewClient("setNutritionDrinking", 2);
            }

            if (foodp > 30 && foodp < 70)
            {
                dbPlayer.Player.TriggerNewClient("setNutritionEating", 3);
            }
            if (drinkp > 30 && drinkp < 70)
            {
                dbPlayer.Player.TriggerNewClient("setNutritionDrinking", 3);
            }

        }




        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandsethealthy(Player player, string commandParams)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length != 1) return;

            var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findPlayer == null || !findPlayer.IsValid()) return;
            setHealthy(findPlayer);

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandgetnutrition(Player player, string commandParams)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length != 1) return;

            var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            dbPlayer.SendNewNotification($"Food: {findPlayer.food[0]} - Drink: {findPlayer.drink[0]}");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandaddnutrition(Player player, string commandParams)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (!Configurations.Configuration.Instance.DevMode) return;

            string[] cmd = commandParams.Split(' ');

            if (cmd.Length < 2)
            {
                dbPlayer.SendNewNotification($"/addNutrition food drink");
                return;
            }


            if (!int.TryParse(cmd[0], out int food))
            {
                return;
            }
            if (!int.TryParse(cmd[1], out int drink))
            {
                return;
            }

            dbPlayer.food[0] += food;
            dbPlayer.drink[0] += drink;
            PushNutritionToPlayer(dbPlayer);
        }

    }

    public class LastNutritionItem
    {
        public uint Itemid;
        public DateTime Time;

        public LastNutritionItem(uint itemid, DateTime time)
        {
            Itemid = itemid;
            Time = time;
        }

    }

}
