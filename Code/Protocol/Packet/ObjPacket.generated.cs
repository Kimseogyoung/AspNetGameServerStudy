using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class ObjPacket	
        {
        
                [ProtoMember(1)]
                public EObjType Type { get; set; } = new EObjType();
                
                [ProtoMember(2)]
                public int Num { get; set; } = default;
                
                [ProtoMember(3)]
                public double Amount { get; set; } = default;
                
	}
}
