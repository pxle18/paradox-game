using System.Collections.Generic;

namespace VMP_CNR.Module
{
    public abstract class SqlModule<T, TLoadable, TId> : SqlBaseModule<T, TLoadable>
        where T : Module<T> //, new()
        where TLoadable : Loadable<TId>
    {
        private Dictionary<TId, TLoadable> _list;

        protected override bool OnLoad()
        {
            _list = new Dictionary<TId, TLoadable>();

            return base.OnLoad();
        }

        protected override void OnItemLoad(TLoadable u)
        {
            if (u == null) return;
            var identifier = u.GetIdentifier();

            if (_list.ContainsKey(identifier)) return;

            _list.Add(identifier, u);
        }

        public Dictionary<TId, TLoadable> GetAll() => _list;

        public bool Contains(TId key) => _list.ContainsKey(key);

        public TLoadable this[TId key]
        {
            get => !_list.TryGetValue(key, out TLoadable value) ? null : value;
            set => _list[key] = value;
        }

        public TLoadable Get(TId key)
        {
            var module = Instance as SqlModule<T, TLoadable, TId>;
            return module?[key];
        }

        public void Add(TId key, TLoadable value) => _list.Add(key, value);

        public void Edit(TId key, TLoadable value) => _list[key] = value;
        public void Remove(TId key) => _list.Remove(key);

        public void Update(TId key, TLoadable value, string tableName, string condition, params object[] data)
        {
            Edit(key, value);
            Change(tableName, condition, data);
        }

        public void Insert(TId key, TLoadable value, string tableName, params object[] data)
        {
            Add(key, value);
            Execute(tableName, data);
        }
    }
}