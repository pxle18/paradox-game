using System;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Module.Crime
{
    public class CrimeCategoryModule : SqlModule<CrimeCategoryModule, CrimeCategory, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `crime_categories`;";
        }

        protected override void OnItemLoaded(CrimeCategory crimeCategory)
        {
            // Do something with loaded data....
        }
    }
}