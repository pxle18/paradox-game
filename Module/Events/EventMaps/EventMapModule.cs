using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VMP_CNR.Module.MapParser;

namespace VMP_CNR.Module.Events.EventMaps
{
    public class EventMapModule : SqlModule<EventMapModule, EventMap, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `event_maps`;";
        }

        public override Type[] RequiredModules()
        {
            return new[]
            {
                typeof(EventModule)
            };
        }

        protected override void OnItemLoad(EventMap u)
        {
            if (EventModule.Instance.IsEventActive(u.EventId))
            {
                string mapFile = Configurations.Configuration.Instance.Ptr ? $"C:\\MapsTest\\{u.Name}" : $"C:\\MapsLive\\{u.Name}";
                if (!File.Exists(mapFile))
                    return;

                MapParserModule.Instance.ReadMap(mapFile);
            }
        }

        public bool IsEventMap(string file)
        {
            return file.Contains("_event_");
        }
    }
}
