using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomBuyDecoReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public int KingdomItemNum { get; set; } 
        
        [ProtoMember(3)]
        public CostObjPacket CostObj { get; set; } 
        
        public string GetProtocolName() => "kingdom/buy-deco";
	}
}
