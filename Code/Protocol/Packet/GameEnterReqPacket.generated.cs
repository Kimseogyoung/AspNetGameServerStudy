using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class GameEnterReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        

        public const string NAME = "game/enter";
        public string GetProtocolName() => NAME;

        public GameEnterReqPacket()
	    {   
        
	    }

    

	}
}
