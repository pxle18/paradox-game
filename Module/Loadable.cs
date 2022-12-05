using MySql.Data.MySqlClient;
using System.Data.Common;

namespace VMP_CNR.Module
{
    public abstract class Loadable<T> : Identifiable<T>
    {
        public Loadable(MySqlDataReader reader)
        {
        }

        public Loadable(DbDataReader reader)
        {

        }

        public Loadable()
        {

        }

        public abstract T GetIdentifier();
    }
}