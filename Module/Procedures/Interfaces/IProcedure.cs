using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Procedures.Enumerations;

namespace VMP_CNR.Module.Procedures.Interfaces
{
    public abstract class Procedure
    {
        public abstract List<Action<DbPlayer>> Procedures { get; set; }

        public abstract Task FinishProcedure(
            DbPlayer player,
            Dictionary<string, string> procedureData
        );
    }
}
