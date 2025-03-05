using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomStoreReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public List<ulong> KingdomPlacedItemIdList { get; set; } 
        
        public string GetProtocolName() => "kingdom/store";

        public KingdomStoreReqPacket( List<ulong> kingdomplaceditemidlist )
	    {   
         
                KingdomPlacedItemIdList = kingdomplaceditemidlist; 
                
	    }
	}
}
