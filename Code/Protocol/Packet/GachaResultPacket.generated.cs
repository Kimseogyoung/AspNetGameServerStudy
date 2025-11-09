using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class GachaResultPacket	
        {
        
                [ProtoMember(1)]
                public ObjValue ResultObjValue { get; set; } = new ObjValue();
                
                [ProtoMember(2)]
                public int SoulStoneNum { get; set; } = default;
                
                [ProtoMember(3)]
                public int SoulStoneAmount { get; set; } = default;
                
	}
}
