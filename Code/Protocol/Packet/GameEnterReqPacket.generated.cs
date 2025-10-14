using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class GameEnterReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new ReqInfoPacket();
        

        public const string NAME = "game/enter";
        public string GetProtocolName() => NAME;

        public GameEnterReqPacket()
	    {   
        
	    }

    

	}
}
