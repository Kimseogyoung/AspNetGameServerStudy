using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Proto
{
    public interface IProtoLoadDescriptor
    {
        Task<List<BindResult>> BuildAsync(IProtoLoader loader);
    }

    public sealed class ParallelLoadDescriptor<T> : IProtoLoadDescriptor
        where T : ProtoBase, new()
    {
        public Action<T>? OnLoaded { get; set; }
        public Func<T, ValidateResult>? OnValidate { get; set; }

        Task<List<BindResult>> IProtoLoadDescriptor.BuildAsync(IProtoLoader loader)
        {
            var name = typeof(T).Name.Replace("Proto", "");
            var result = loader.Load<T>(name);
            var table = BuildTable(result);

            foreach (var item in result.Items)
            {
                item.OnLoaded();
                OnLoaded?.Invoke(item);
            }

            Func<ProtoBase, ValidateResult>? validateCb = OnValidate != null
                ? (ProtoBase item) => OnValidate((T)item)
                : (Func<ProtoBase, ValidateResult>?)null;

            return Task.FromResult(new List<BindResult>() { new BindResult(typeof(T), table, validateCb) });
        }

        private static ProtoTable BuildTable(ProtoLoadResult<T> result)
        {
            var table = new ProtoTable();
            for (int i = 0; i < result.Items.Count; i++)
            {
                var item = result.Items[i];
                item.Idx = i;
                table.Items.Add(item);

                if (result.PkFields.Count > 0)
                {
                    var key = ComputeKey(item, result.PkFields);
                    if (table.PkDict.ContainsKey(key))
                    {
                        throw new InvalidOperationException($"Duplicate PK in {typeof(T).Name} at row {i}: key={key}");
                    }
                    table.PkDict[key] = i;
                }

                if (result.MkFields.Count > 0)
                {
                    var mk = ComputeKey(item, result.MkFields);
                    if (!table.MkDict.TryGetValue(mk, out var list))
                    {
                        table.MkDict[mk] = list = new List<int>();
                    }
                    list.Add(i);
                }
            }
            return table;
        }

        private static object ComputeKey(T item, List<string> fields)
        {
            if (fields.Count == 1)
            {
                return typeof(T).GetProperty(fields[0])!.GetValue(item)!;
            }

            var hc = new HashCode();
            foreach (var f in fields)
            {
                hc.Add(typeof(T).GetProperty(f)?.GetValue(item));
            }
            return hc.ToHashCode();
        }
    }

    public sealed class OrderedLoadDescriptor : IProtoLoadDescriptor
    {
        private IReadOnlyList<IProtoLoadDescriptor> _inner;

        public OrderedLoadDescriptor(params IProtoLoadDescriptor[] descriptors)
        {
            _inner = descriptors;
        }

        async Task<List<BindResult>> IProtoLoadDescriptor.BuildAsync(IProtoLoader loader)
        {
            var results = new List<BindResult>();
            foreach (var descriptor in _inner)
            {
                var result = await descriptor.BuildAsync(loader);
                results.AddRange(result);
            }
            return results;

        }
    }

    public sealed class BindResult
    {
        public Type ProtoType { get; }
        public ProtoTable Table { get; }
        public Func<ProtoBase, ValidateResult>? ValidateCallback { get; }

        public BindResult(Type protoType, ProtoTable table, Func<ProtoBase, ValidateResult>? validateCallback = null)
        {
            ProtoType = protoType;
            Table = table;
            ValidateCallback = validateCallback;
        }
    }

    public sealed class ProtoTable
    {
        public List<ProtoBase> Items { get; } = new List<ProtoBase>();
        public Dictionary<object, int> PkDict { get; } = new Dictionary<object, int>();
        public Dictionary<object, List<int>> MkDict { get; } = new Dictionary<object, List<int>>();
    }
}
