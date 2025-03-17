using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class ChgObjPacket	
        {
        
                [ProtoMember(1)]
                public EObjType Type { get; set; } = new();
                
                [ProtoMember(2)]
                public int Num { get; set; } = default;
                
                [ProtoMember(3)]
                public double Amount { get; set; } = default;
                
                [ProtoMember(4)]
                public double TotalAmount { get; set; } = default;
                
	}
}
