using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Configurations;

namespace VMP_CNR.Module.Migration
{
    public class MigrationModule : Module<MigrationModule>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ConfigurationModule) };
        }

        public override bool Load(bool reload = false)
        {
            return base.Load(reload);
        }
    }
}
