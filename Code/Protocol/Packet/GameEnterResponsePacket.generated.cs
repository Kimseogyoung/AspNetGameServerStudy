using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class GameEnterResponsePacket : IResponsePacket
	{
    
        [ProtoMember(1)]
        public ResponseInfoPacket Info { get; set; } = new ResponseInfoPacket();
        
        [ProtoMember(2)]
        public PlayerPacket Player { get; set; } = new PlayerPacket();
        
	}
}
