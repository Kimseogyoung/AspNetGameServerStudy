using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomStoreReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public List<ulong> KingdomPlacedItemIdList { get; set; } = new();
        

        public const string NAME = "kingdom/store";
        public string GetProtocolName() => NAME;

        public KingdomStoreReqPacket( List<ulong> kingdomplaceditemidlist )
	    {   
         
                KingdomPlacedItemIdList = kingdomplaceditemidlist; 
                
	    }

    
        public KingdomStoreReqPacket()
        {
        }
        

	}
}
