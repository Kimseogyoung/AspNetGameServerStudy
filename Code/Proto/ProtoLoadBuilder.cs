using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Proto
{
    public sealed class ProtoLoadBuilder
    {
        private readonly ProtoDb _db;
        private readonly List<IProtoLoadDescriptor> _descriptors = new List<IProtoLoadDescriptor>();
        private readonly Dictionary<Type, Func<ProtoBase, ValidateResult>> _validateCallbacks = new Dictionary<Type, Func<ProtoBase, ValidateResult>>();

        internal ProtoLoadBuilder(ProtoDb db) { _db = db; }

        public ProtoLoadBuilder Add(IProtoLoadDescriptor descriptor)
        {
            _descriptors.Add(descriptor);
            return this;
        }

        public async Task<IReadOnlyList<ValidateResult>> LoadAllAsync()
        {
            await ExecuteLoadAsync();
            return ProtoDb.ValidateAll();
        }

        internal async Task ExecuteLoadAsync()
        {
            _db.SetLastBuilder(this);
            _db.ClearTables();
            _validateCallbacks.Clear();

            var parallelTasks = new List<Task<List<BindResult>>>();
            foreach (var desc in _descriptors)
            {
                parallelTasks.Add(desc.BuildAsync(_db.Loader));
            }

            if (parallelTasks.Count == 0)
            {
                return;
            }

            var taskResults = await Task.WhenAll(parallelTasks);
            foreach (var t in taskResults)
            {
                foreach (var result in t)
                {
                    _db.RegisterTable(result.ProtoType, result.Table);
                    if (result.ValidateCallback != null)
                    {
                        _validateCallbacks[result.ProtoType] = result.ValidateCallback;
                    }
                }
            }

            parallelTasks.Clear();
        }

        internal Func<ProtoBase, ValidateResult>? GetValidateCallback(Type type)
        {
            _validateCallbacks.TryGetValue(type, out var cb);
            return cb;
        }
    }
}
