using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class GameEnterReqPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        

        public const string NAME = "game/enter";
        public string GetProtocolName() => NAME;

        public GameEnterReqPacket()
	    {   
        
	    }

    

	}
}
