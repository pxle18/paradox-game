using System;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Module.Items
{
    public class StaticContainerModule : SqlModule<StaticContainerModule, StaticContainer, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemsModule), typeof(ItemModelModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `container_static_data`;";
        }
    }
}