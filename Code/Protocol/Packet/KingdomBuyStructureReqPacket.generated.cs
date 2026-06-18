using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomBuyStructureReqPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
        [ProtoMember(2)]
        public int KingdomItemNum { get; set; } = default;
        
        [ProtoMember(3)]
        public CostObjPacket CostObj { get; set; } = new CostObjPacket();
        

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
