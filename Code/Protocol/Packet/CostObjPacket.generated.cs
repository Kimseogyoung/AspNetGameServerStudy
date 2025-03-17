using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class CostObjPacket	
        {
        
                [ProtoMember(1)]
                public EObjType Type { get; set; } = new();
                
                [ProtoMember(2)]
                public int Num { get; set; } = default;
                
                [ProtoMember(3)]
                public double Amount { get; set; } = default;
                
                [ProtoMember(4)]
                public double BefAmount { get; set; } = default;
                
                [ProtoMember(5)]
                public double AftAmount { get; set; } = default;
                
	}
}
