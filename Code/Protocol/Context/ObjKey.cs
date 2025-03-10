using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{
    public struct ObjKey : IEquatable<ObjKey>
    {
        public EObjType Type { get; set; }
        public int Num { get; set; }
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

    public class ObjValue
    {
        public ObjKey Key { get; set; }
        public double Value { get; set; }
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
