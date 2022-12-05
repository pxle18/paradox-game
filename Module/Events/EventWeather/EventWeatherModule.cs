using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Events.EventMaps;
using VMP_CNR.Module.Events.EventNpc;
using VMP_CNR.Module.MapParser;
using VMP_CNR.Module.NpcSpawner;
using VMP_CNR.Module.Weather;

namespace VMP_CNR.Module.Events.EventWeather
{
    public sealed class EventWeatherModule : SqlModule<EventWeatherModule, EventWeather, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `event_weather`;";
        }

        public override Type[] RequiredModules()
        {
            return new[]
            {
                typeof(WeatherModule),
                typeof(EventModule)
            };
        }

        protected override void OnItemLoad(EventWeather u)
        {
            if (EventModule.Instance.IsEventActive(u.EventId))
            {
                WeatherModule.Instance.ChangeWeather((GTANetworkAPI.Weather)u.WeatherId);
            }
        }
    }
}
