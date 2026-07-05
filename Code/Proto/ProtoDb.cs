using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Proto
{
    public sealed class ProtoDb
    {
        public static ProtoDb Instance { get; private set; } = null!;

        private readonly IProtoLoader _loader;
        private readonly Dictionary<Type, ProtoTable> _tables = new Dictionary<Type, ProtoTable>();
        private ProtoLoadBuilder? _lastBuilder;

        private ProtoDb(IProtoLoader loader)
        {
            _loader = loader;
        }

        public static void Initialize(IProtoLoader loader)
        {
            Instance = new ProtoDb(loader);
        }

        public static ProtoLoadBuilder CreateBuilder()
        {
            return new ProtoLoadBuilder(Instance);
        }

        public static async Task<IReadOnlyList<ValidateResult>> ReloadAsync()
        {
            await Instance._lastBuilder!.ExecuteLoadAsync();
            return ValidateAll();
        }

        public static IReadOnlyList<ValidateResult> ValidateAll()
        {
            var failures = new List<ValidateResult>();
            var lb = Instance._lastBuilder;

            foreach (var pair in Instance._tables)
            {
                var type = pair.Key;
                var table = pair.Value;

                for (int i = 0; i < table.Items.Count; i++)
                {
                    var item = table.Items[i];

                    var vr = item.OnValidate();
                    if (!vr.IsValid)
                    {
                        failures.Add(ValidateResult.Fail(vr.Reason, type, i));
                    }

                    if (lb != null)
                    {
                        var cb = lb.GetValidateCallback(type);
                        if (cb != null)
                        {
                            var cbVr = cb(item);
                            if (!cbVr.IsValid)
                            {
                                failures.Add(ValidateResult.Fail(cbVr.Reason, type, i));
                            }
                        }
                    }
                }
            }
            return failures;
        }

        // --- static query API ---

        public static TProto Get<TProto>(object pk) where TProto : ProtoBase, new()
            => Instance.GetCore<TProto>(pk);

        public static bool TryGet<TProto>(object pk, out TProto? prt) where TProto : ProtoBase, new()
            => Instance.TryGetCore(pk, out prt);

        public static List<TProto> GetByMk<TProto>(object mk) where TProto : ProtoBase, new()
            => Instance.GetByMkCore<TProto>(mk);

        public static List<TProto> GetAll<TProto>() where TProto : ProtoBase, new()
            => Instance.GetAllCore<TProto>();

        public static TProto GetFirst<TProto>() where TProto : ProtoBase, new()
            => Instance.GetFirstCore<TProto>();

        public static TProto GetByIndex<TProto>(int idx) where TProto : ProtoBase, new()
            => Instance.GetByIndexCore<TProto>(idx);

        public static TProto GetNext<TProto>(TProto prt) where TProto : ProtoBase, new()
            => Instance.GetNextCore(prt);

        public static int GetCount<TProto>() where TProto : ProtoBase, new()
            => Instance.GetCountCore<TProto>();

        // --- internal API for ProtoLoadBuilder ---

        internal IProtoLoader Loader => _loader;

        internal void SetLastBuilder(ProtoLoadBuilder builder) { _lastBuilder = builder; }

        internal void RegisterTable(Type type, ProtoTable table) { _tables[type] = table; }

        internal void ClearTables() { _tables.Clear(); }

        // --- core implementations ---

        private TProto GetCore<TProto>(object pk) where TProto : ProtoBase, new()
        {
            var table = GetTable<TProto>();
            var key = NormalizeKey(pk);
            if (!table.PkDict.TryGetValue(key, out int idx))
            {
                throw new InvalidOperationException($"Proto not found: {typeof(TProto).Name}[{pk}]");
            }
            return (TProto)table.Items[idx];
        }

        private bool TryGetCore<TProto>(object pk, out TProto? prt) where TProto : ProtoBase, new()
        {
            var table = GetTable<TProto>();
            var key = NormalizeKey(pk);
            if (!table.PkDict.TryGetValue(key, out int idx))
            {
                prt = null;
                return false;
            }
            prt = (TProto)table.Items[idx];
            return true;
        }

        private List<TProto> GetByMkCore<TProto>(object mk) where TProto : ProtoBase, new()
        {
            var table = GetTable<TProto>();
            var key = NormalizeKey(mk);
            if (!table.MkDict.TryGetValue(key, out var idxList))
            {
                return new List<TProto>();
            }
            return idxList.Select(i => (TProto)table.Items[i]).ToList();
        }

        private List<TProto> GetAllCore<TProto>() where TProto : ProtoBase, new()
            => GetTable<TProto>().Items.Cast<TProto>().ToList();

        private TProto GetFirstCore<TProto>() where TProto : ProtoBase, new()
            => (TProto)GetTable<TProto>().Items[0];

        private TProto GetByIndexCore<TProto>(int idx) where TProto : ProtoBase, new()
            => (TProto)GetTable<TProto>().Items[idx];

        private TProto GetNextCore<TProto>(TProto prt) where TProto : ProtoBase, new()
        {
            var table = GetTable<TProto>();
            var nextIdx = prt.Idx + 1;
            return nextIdx < table.Items.Count ? (TProto)table.Items[nextIdx] : prt;
        }

        private int GetCountCore<TProto>() where TProto : ProtoBase, new()
            => GetTable<TProto>().Items.Count;

        private ProtoTable GetTable<TProto>() where TProto : ProtoBase, new()
        {
            if (!_tables.TryGetValue(typeof(TProto), out var table))
            {
                throw new InvalidOperationException($"{typeof(TProto).Name} is not loaded. Call LoadAll first.");
            }
            return table;
        }

        // tuple (a, b) → hash so composite PK matches ComputeKey behavior
        private static object NormalizeKey(object key)
        {
            if (key is ITuple tuple)
            {
                var hc = new HashCode();
                for (int i = 0; i < tuple.Length; i++)
                {
                    hc.Add(tuple[i]);
                }
                return hc.ToHashCode();
            }
            return key;
        }
    }
}
