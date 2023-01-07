using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module
{
    public abstract class Loadable<T> : Identifiable<T>
    {
        public Loadable(MySqlDataReader reader) { }
        public Loadable(DbDataReader reader) { }
        public Loadable() { }

        public abstract T GetIdentifier();

        public HashSet<L> GetHashSet<L>(string hashSetString, Action<uint, HashSet<L>> parsingAction)
        {
            var hashSet = new HashSet<L>();
            if (!string.IsNullOrEmpty(hashSetString))
            {
                var splittedHashSet = hashSetString.Split(',');
                foreach (var teamIdString in splittedHashSet)
                {
                    if (!uint.TryParse(teamIdString, out var teamId)) continue;

                    parsingAction(teamId, hashSet);
                }
            }

            return hashSet;
        }
    }
}