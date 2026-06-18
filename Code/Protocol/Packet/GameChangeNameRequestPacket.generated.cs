using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class GameChangeNameRequestPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
        [ProtoMember(2)]
        public string PlayerName { get; set; } = default;
        

        public const string NAME = "game/change-name";
        public string GetProtocolName() => NAME;

        public GameChangeNameRequestPacket( string playername )
	    {   
         
                PlayerName = playername; 
                
	    }

    
        public GameChangeNameRequestPacket()
        {
        }
        

	}
}
