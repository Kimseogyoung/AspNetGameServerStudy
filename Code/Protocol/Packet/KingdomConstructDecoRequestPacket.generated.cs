using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomConstructDecoRequestPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
        [ProtoMember(2)]
        public int KingdomItemNum { get; set; } = default;
        
        [ProtoMember(3)]
        public TilePosPacket StartTilePos { get; set; } = new TilePosPacket();
        

        public const string NAME = "kingdom/construct-deco";
        public string GetProtocolName() => NAME;

        public KingdomConstructDecoRequestPacket( int kingdomitemnum,  TilePosPacket starttilepos )
	    {   
         
                KingdomItemNum = kingdomitemnum; 
                 
                StartTilePos = starttilepos; 
                
	    }

    
        public KingdomConstructDecoRequestPacket()
        {
        }
        

	}
}
