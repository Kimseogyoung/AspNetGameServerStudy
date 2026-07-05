using System.Collections.Generic;

namespace Proto
{
    public interface IProtoLoader
    {
        ProtoLoadResult<T> Load<T>(string name) where T : ProtoBase, new();
    }

    public sealed class ProtoLoadResult<T> where T : ProtoBase, new()
    {
        public List<T> Items { get; set; } = new List<T>();
        public List<string> PkFields { get; set; } = new List<string>();
        public List<string> MkFields { get; set; } = new List<string>();
    }
}
