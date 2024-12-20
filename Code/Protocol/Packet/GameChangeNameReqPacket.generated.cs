using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class GameChangeNameReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public string PlayerName { get; set; } 
        
        public string GetProtocolName() => "game/change-name";
	}
}
