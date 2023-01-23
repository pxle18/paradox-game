using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Maps.Models;

namespace VMP_CNR.Module.Maps
{
    class LoadableScriptMapModule : SqlModule<LoadableScriptMapModule, LoadableScriptMapModel, uint>
    {
        protected override string GetQuery() => "SELECT * FROM `loadable_script_maps`;";
    }
}
