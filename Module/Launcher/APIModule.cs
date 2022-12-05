using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Helper;
using VMP_CNR.Module.Injury;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Zone;

namespace VMP_CNR.Module.Launcher
{
    public sealed class APIModule : Module<APIModule>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ConfigurationModule) };
        }

        protected override bool OnLoad()
        {

            return base.OnLoad();
        }
    }
}
