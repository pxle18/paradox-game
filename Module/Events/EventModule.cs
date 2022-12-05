using System;
using System.Collections.Generic;
using System.Linq;
using VMP_CNR.Module.Events.EventMaps;
using VMP_CNR.Module.Events.EventNpc;
using VMP_CNR.Module.Events.EventWeather;
using VMP_CNR.Module.MapParser;
using VMP_CNR.Module.NpcSpawner;

namespace VMP_CNR.Module.Events
{
    public sealed class EventModule : SqlModule<EventModule, EventData, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `event_info`;";
        }

        public bool IsEventActive(uint eventId)
        {
            if (Get(eventId) != null)
                return Get(eventId).IsActive;

            return false;
        }
    }
}