using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using VMP_CNR.Handler;
using VMP_CNR.Module.AnimationMenu;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Crime;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Phone;
using VMP_CNR.Module.Shops;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Voice;
using VMP_CNR.Module.Weapons;
using VMP_CNR.Module.Weapons.Data;
using VMP_CNR.Module.Zone;

namespace VMP_CNR.Module.Robbery
{
    public sealed class RobberyModule : Module<RobberyModule>
    {
        // Global Defines

        public static uint MarkierteScheineID = 881;

        public const int Juwelier = -2;

        public Dictionary<int, Rob> Robberies;

        public DateTime LastScenario = DateTime.Now.AddHours(-2);

        public Dictionary<uint, int> RobbedAtms = new Dictionary<uint, int>();
        public override bool Load(bool reload = false)
        {
            Robberies = new Dictionary<int, Rob>();
            RobbedAtms = new Dictionary<uint, int>();
            return true;
        }
        
        public bool CanJuweRobbed()
        {
            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            if (StaatsbankRobberyModule.Instance.IsActive)
            {
                return false;
            }

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 30)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 30)
                    {
                        return false;
                    }

                    break;
            }

            return true;
        }
        
        public bool CanAtmRobbed()
        {
            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 45)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 25)
                    {
                        return false;
                    }

                    break;
            }

            return true;
        }

        public Rob Get(int id)
        {
            return Robberies.TryGetValue(id, out var rob) ? rob : null;
        }
        
        public void Add(int id, DbPlayer dbPlayer, int startinterval, RobType type = RobType.Shop, int copinterval = 2, int endinterval = 30)
        {
            if (Robberies.ContainsKey(id))
            {
                Robberies[id].Player = dbPlayer;
            }
            else
            {
                var rob = new Rob
                {
                    Id = id,
                    Player = dbPlayer,
                    Interval = startinterval,
                    CopInterval = copinterval,
                    EndInterval = endinterval,
                    Disabled = false,
                    Type = type
                };
                Robberies.Add(id, rob);
            }
        }

        public void RemovePlayerRobs(DbPlayer dbPlayer)
        {
            var index = 0;
            while (index < Robberies.Count)
            {
                if (index >= Robberies.Count) break;
                var rob = Robberies.ElementAt(index++).Value;
                if (rob.Player.Id == dbPlayer.Id)
                {
                    Robberies.Remove(rob.Id);
                }
            }
        }

        public bool IsActive(int robid)
        {
            var rob = Get(robid);
            if (rob == null) return false;
            return !rob.Disabled && rob.Interval > 0;
        }

        public List<Rob> GetActiveShopRobs()
        {
            return (from rob in Robberies
                    where rob.Value.Id > 0 && rob.Value.Type == RobType.Shop && rob.Value.Player != null &&
                          !rob.Value.Disabled
                    select rob.Value).ToList();
        }

        public bool IsAnyShopInRobbing()
        {
            return Robberies.Any(rob =>
                rob.Value.Id > 0 && !rob.Value.Disabled && rob.Value.Interval > 0 && rob.Value.Player != null &&
                rob.Value.Type == RobType.Shop);
        }

        public Dictionary<int, Rob> GetRobs()
        {
            return Robberies;
        }

        public List<Rob> GetActiveRobs(bool displayonly = false)
        {
            return (from rob in Robberies where !rob.Value.Disabled && (rob.Value.Interval > 2 || rob.Value.Type != RobType.Shop || !displayonly) && rob.Value.Player != null select rob.Value)
                .ToList();
        }

        public void SetTeamScenario(string robtype,uint teamid)
        {
            string query = $"INSERT INTO team_scenario (robtype,teamid,lastrob) VALUES ('{robtype}',{teamid},NOW())";
            MySQLHandler.ExecuteAsync(query);
        }

        public class ValidResponse
        {
            public bool check { get; set; }
            public DateTime lastrob { get; set; }
        }

        public ValidResponse ValidTeamScenario(string robtype, uint teamid)
        {
            ValidResponse res = new ValidResponse();
            try
            {
                var query = $"SELECT * FROM team_scenario WHERE lastrob >= DATE(NOW()) - INTERVAL 7 DAY AND robtype='{robtype}' AND teamid={teamid}";
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @query;
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                res.check = false;
                                res.lastrob = reader.GetDateTime("lastrob").AddDays(7);
                            }
                        }
                        else
                        {
                            res.check = true;
                            res.lastrob = new DateTime();
                        }
                    }
                }

                return res;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                res.check = false;
                res.lastrob = new DateTime();
                return res;
            }

        }


        public override void OnMinuteUpdate()
        {
            var rnd = new Random();
            var hour = DateTime.Now.Hour;
            
            var robs = GetActiveRobs();
            if (robs == null) return;
            var index = 0;
            while (index < robs.Count)
            {
                if (index >= Robberies.Count) break;
                var rob = robs[index++];
                if (rob == null) continue;

                DbPlayer dbPlayer = rob.Player;
                // Cancelrob On missing Player?!
                if (dbPlayer == null || dbPlayer.Player == null)
                {
                    rob.Disabled = true;
                }
                else
                {
                    dbPlayer.IsInRob = true;
                    if (rob.Type == RobType.Juwelier)
                    {
                        if (rob.Player.Player.Position.DistanceTo(new Vector3(-622.5494, -229.5598, 38.05706)) < 10.0f && !dbPlayer.IsInjured())
                        {
                            if (rob.Interval > 0)
                            {
                                if (rob.Interval == 19)
                                {
                                    dbPlayer.SendNewNotification(

                                        "Du wurdest bei dem Raubueberfall von den Kameras aufgezeichnet!");
                                    dbPlayer.SendNewNotification(

                                        "Deine Identitaet wurde nicht festgestellt, allerdings die Polizei verstaendigt.");
                                }
                                if (rob.Interval < 17)
                                {
                                    var erhalt = rnd.Next(5, 9);
                                    if (!dbPlayer.Container.AddItem(21, erhalt))
                                    {
                                        dbPlayer.SendNewNotification("Dein Inventar ist leider voll!");
                                    }
                                }
                                rob.Interval--;
                            }
                            else
                            {
                                var erhalt = rnd.Next(75, 90);
                                if (!dbPlayer.Container.AddItem(21, erhalt))
                                {
                                    dbPlayer.SendNewNotification("Deine Taschen sind jetzt randvoll!");
                                }
                                dbPlayer.SendNewNotification(
                                    "Es sind keine Diamanten mehr auf Lager! (Du solltest nun fluechten)");
                                rob.Disabled = true;
                                dbPlayer.IsInRob = false;
                                Logger.AddRobLog(dbPlayer.Id, rob.Type.ToString(), 20 - rob.Interval, true);
                            }
                        }
                        else
                        {
                            dbPlayer.SendNewNotification(

                                "Der Raubueberfall auf den Juwelier wurde abgebrochen!");
                            TeamModule.Instance.Get((int)TeamTypes.TEAM_POLICE).SendNotification(
                                "An Alle Einheiten, der Juwelier Raubueberfall wurde erfolgreich verhindert!");
                            TeamModule.Instance.Get((int)TeamTypes.TEAM_FIB).SendNotification(
                                "An Alle Einheiten, der Juwelier Raubueberfall wurde erfolgreich verhindert!");
                            rob.Disabled = true;
                            dbPlayer.IsInRob = false;
                            Logger.AddRobLog(dbPlayer.Id, rob.Type.ToString(), 20 - rob.Interval, false);
                        }
                    }
                    else if (rob.Type == (int)RobType.Shop)
                    {
                        var shop = ShopsModule.Instance.GetThisShop(dbPlayer.Player.Position, 15.0f);
                        if (shop != null && !dbPlayer.IsInjured() && (Math.Abs(dbPlayer.Player.Position.Z - shop.Position.Z) <= 2f))
                        {
                            if (rob.Interval >= 2)
                            {
                                if(rob.Interval == rob.CopInterval)
                                {
                                    Zone.Zone zone = ZoneModule.Instance.GetZone(dbPlayer.Player.Position);
                                    TeamModule.Instance.Get((int)TeamTypes.TEAM_POLICE).SendNotification($"An Alle Einheiten, ein Einbruch in einen Store in {zone.Name} wurde gemeldet!");
                                }
                                else if (rob.Interval >= rob.EndInterval)
                                {
                                    dbPlayer.SendNewNotification("Es ist kein Geld mehr in der Kasse! (Du solltest nun fluechten)");
                                    dbPlayer.IsInRob = false;
                                    rob.Disabled = true;
                                    Logger.AddRobLog(dbPlayer.Id, rob.Type.ToString(), rob.Interval, true);
                                }
                                else
                                {                          
                                    var erhalt = rnd.Next(110, 190) * rob.Interval;
                                    dbPlayer.Container.AddItem(MarkierteScheineID, erhalt);
                                    dbPlayer.SendNewNotification($"${erhalt} erbeutet!", title: "Raubüberfall");
                                }
                            }
                            rob.Interval++;
                        }
                        else
                        {
                            dbPlayer.SendNewNotification("Der Raubueberfall auf den Store wurde abgebrochen!");
                            TeamModule.Instance.Get((int)TeamTypes.TEAM_POLICE).SendNotification("An Alle Einheiten, der Raubueberfall wurde erfolgreich verhindert!");

                            if (rob.Interval < rob.CopInterval + 2)
                                dbPlayer.AddCrime("Leitstelle", CrimeReasonModule.Instance.Get(159), "Wurde vom Sicherheitssystem eines Shops erfasst.");

                            dbPlayer.IsInRob = false;
                            rob.Disabled = true;
                            Logger.AddRobLog(dbPlayer.Id, rob.Type.ToString(), rob.Interval, false);
                        }
                    }
                }
            }
        }
    }
}