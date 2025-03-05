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

        public KingdomConstructDecoReqPacket( ReqInfoPacket info,  int kingdomitemnum,  TilePosPacket starttilepos )
	    {   
         
                Info = info; 
                 
                KingdomItemNum = kingdomitemnum; 
                 
                StartTilePos = starttilepos; 
                
	    }
	}
}
