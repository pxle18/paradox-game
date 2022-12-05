using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Module
{
    public abstract class SqlBaseModule<T, TLoadable> : Module<T>
        where T : Module<T>
    {
        protected abstract string GetQuery();

        protected override bool OnLoad()
        {
            try
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = GetQuery();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return false;
                        while (reader.Read())
                        {
                            if (!(Activator.CreateInstance(typeof(TLoadable), reader) is TLoadable u)) continue;
                            OnItemLoad(u);
                            OnItemLoaded(u);
                        }
                    }
                }

                OnLoaded();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return true;
        }

        protected virtual void OnItemLoad(TLoadable loadable)
        {
        }

        protected virtual void OnItemLoaded(TLoadable loadable)
        {
        }

        protected virtual void OnLoaded()
        {
            Logging.Logger.Debug("Loaded Module " + this.ToString());
        }

        internal void Execute(string tableName, params object[] data)
        {
            MySQLHandler.InsertAsync(tableName, data);
        }

        internal void Change(string tableName, string condition, params object[] data)
        {
            MySQLHandler.UpdateAsync(tableName, condition, data);
        }
    }
}