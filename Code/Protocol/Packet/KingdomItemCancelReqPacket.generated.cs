using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomItemCancelReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public int KingdomItemNum { get; set; } 
        
        public string GetProtocolName() => "kingdom-item/cancel";
	}
}
