using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomConstructDecoReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public int KingdomItemNum { get; set; } 
        
        [ProtoMember(3)]
        public TilePosPacket StartTilePos { get; set; } 
        
        public string GetProtocolName() => "kingdom/construct-deco";

        public KingdomConstructDecoReqPacket( int kingdomitemnum,  TilePosPacket starttilepos )
	    {   
         
                KingdomItemNum = kingdomitemnum; 
                 
                StartTilePos = starttilepos; 
                
	    }

    
        public KingdomConstructDecoReqPacket()
        {
        }
        

	}
}
