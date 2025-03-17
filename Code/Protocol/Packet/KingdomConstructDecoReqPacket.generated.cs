using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomConstructDecoReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public int KingdomItemNum { get; set; } = default;
        
        [ProtoMember(3)]
        public TilePosPacket StartTilePos { get; set; } = new();
        

        public const string NAME = "kingdom/construct-deco";
        public string GetProtocolName() => NAME;

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
