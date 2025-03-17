using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomBuyStructureReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public int KingdomItemNum { get; set; } = default;
        
        [ProtoMember(3)]
        public CostObjPacket CostObj { get; set; } = new();
        

        public const string NAME = "kingdom/buy-structure";
        public string GetProtocolName() => NAME;

        public KingdomBuyStructureReqPacket( int kingdomitemnum,  CostObjPacket costobj )
	    {   
         
                KingdomItemNum = kingdomitemnum; 
                 
                CostObj = costobj; 
                
	    }

    
        public KingdomBuyStructureReqPacket()
        {
        }
        

	}
}
