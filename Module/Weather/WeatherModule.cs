using System;
using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Weather
{
    public class WeatherModule : SqlModule<WeatherModule, Weather, uint>
    {
        public bool Blackout = false;
        public float WaterHeight = 0.0f;
        public bool OngoingWeatherTransition = false;
        public DateTime WeatherTransitionFinish = DateTime.Now;

        protected override string GetQuery()
        {
            return "SELECT * FROM `weather` ORDER by weekday, hour;";
        }

        protected override bool OnLoad()
        {
            Blackout = false;
            WaterHeight = 0.0f;
            if (Configuration.Instance.DevMode)
            {
                NAPI.World.SetTime(15, 0, 0);
            }
            else
            {
                var now = DateTime.Now;
                NAPI.World.SetTime(now.Hour, now.Minute, now.Second);
            }
            return base.OnLoad();
        }

        public void ChangeWeather(GTANetworkAPI.Weather weather, float transitionTime = 0, bool overrideWeather = false)
        {
            if ((Main.WeatherOverride && overrideWeather) || !Main.WeatherOverride)
            {
                Main.WeatherOverride    = overrideWeather;

                if (transitionTime > 0 && double.TryParse(transitionTime.ToString(), out double transitionTimeDouble))
                {
                    Main.m_DestWeather = weather;
                    OngoingWeatherTransition = true;
                    WeatherTransitionFinish = DateTime.Now.AddSeconds(transitionTimeDouble);
                }
                else
                    Main.m_CurrentWeather = weather;

                foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
                {
                    Task.Run(async () =>
                    {
                        await ChangeWeatherForPlayer(dbPlayer, weather, transitionTime);
                    });
                }
            }
        }

        public async Task ChangeWeatherForPlayer(DbPlayer dbPlayer, GTANetworkAPI.Weather weather, float transitionTime = 0)
        {
            string weatherString = weather.ToString();
            if (Configuration.Instance.DevMode)
                dbPlayer.SendNewNotification($"[DEBUG] Wetter String: {weatherString}");

            if (transitionTime > 0)
                dbPlayer.Player.TriggerNewClient("setWeatherTransition", weatherString, transitionTime);
            else
                dbPlayer.Player.TriggerNewClient("updateWeather", weatherString);
        }

        public override void OnMinuteUpdate()
        {
            var l_Time = DateTime.Now;
            DayOfWeek l_Day = l_Time.DayOfWeek;
            var l_Hour = (uint) l_Time.Hour;

            foreach (var kvp in GetAll())
            {
                if (!kvp.Value.DayOfWeek.Equals(l_Day))
                    continue;

                if (kvp.Value.Hour != l_Hour)
                    continue;

                if (Main.m_CurrentWeather == kvp.Value.NWeather)
                    break;

                if (!OngoingWeatherTransition)
                    ChangeWeather(kvp.Value.NWeather, 300, false);
            }

            NAPI.Task.Run(()=> { NAPI.World.SetTime((int)l_Hour, l_Time.Minute, l_Time.Second); });
        }

        public override void OnTenSecUpdate()
        {
            if (!OngoingWeatherTransition)
                return;

            if (DateTime.Now >= WeatherTransitionFinish)
            {
                Main.m_CurrentWeather = Main.m_DestWeather;
                OngoingWeatherTransition = false;
            }
        }

        public override void OnPlayerFirstSpawn(DbPlayer dbPlayer)
        {
            Task.Run(async () =>
            {
                dbPlayer.Player.TriggerNewClient("setBlackout", Blackout);
                await ChangeWeatherForPlayer(dbPlayer, Main.m_CurrentWeather);

                if (OngoingWeatherTransition)
                {
                    if (WeatherTransitionFinish >= DateTime.Now)
                    {
                        float transitionTime = float.Parse((WeatherTransitionFinish - DateTime.Now).TotalSeconds.ToString());
                        await ChangeWeatherForPlayer(dbPlayer, Main.m_DestWeather, transitionTime);

                        if (Configuration.Instance.DevMode)
                        {
                            dbPlayer.SendNewNotification($"Ongoing Weathertrans! Start: {Main.m_CurrentWeather.ToString()} Dest: {Main.m_DestWeather.ToString()} Time till Transition finishes: {transitionTime}s");
                        }
                    }
                }
            });
        }

        public void SetBlackout(bool blackout)
        {
            Blackout = blackout;
            foreach(DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
            {
                dbPlayer.Player.TriggerNewClient("setBlackout", Blackout);
            }
        }

        public void SetWaterHeight(float height = 0)
        {
            WaterHeight = height;
            foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
            {
                dbPlayer.Player.TriggerNewClient("modifyWater", dbPlayer.Player.Position.X, dbPlayer.Player.Position.Y, height);
                dbPlayer.Player.SendNative(0xC443FD757C3BA637, dbPlayer.Player.Position.X, dbPlayer.Player.Position.Y, 500, height);
            }
        }
    }
}