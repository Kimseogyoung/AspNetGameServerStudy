using Proto;
using ProtoBuf;

namespace Protocol
{
    [ProtoContract]
    public struct ObjKey : IEquatable<ObjKey>
    {
        [ProtoMember(1)]
        public EObjType Type { get; set; }
        [ProtoMember(2)]
        public int Num { get; set; }

        public ObjKey() { }

        public ObjKey(EObjType type, int num)
        {
            Type = type;
            Num = num;
        }

        public override bool Equals(object? obj)
        {
            return obj is ObjKey other && Equals(other);
        }

        public bool Equals(ObjKey obj)
        {
            return this.Type == obj.Type && this.Num == obj.Num;
        }

        public static bool operator ==(ObjKey left, ObjKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ObjKey left, ObjKey right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Type, this.Num);
        }
    }

    [ProtoContract]
    public class ObjValue
    {
        [ProtoMember(1)]
        public ObjKey Key { get; set; }
        [ProtoMember(2)]
        public double Value { get; set; }

        public ObjValue() { }

        public ObjValue(ObjKey key, double value)
        {
            Key = key;
            Value = value;
        }

        public ObjValue(EObjType type, int num, double value)
        {
            Key = new ObjKey(type, num);
            Value = value;
        }
    }
}
