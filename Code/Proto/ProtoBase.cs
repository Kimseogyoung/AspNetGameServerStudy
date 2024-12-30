using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proto
{
    [ProtoContract]
    public class ProtoBase
    {
        [ProtoMember(1)]
        public int Idx { get; set; }
    }
}
