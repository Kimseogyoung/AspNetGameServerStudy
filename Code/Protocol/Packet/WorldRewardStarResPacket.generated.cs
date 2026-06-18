using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldRewardStarResPacket : IResponsePacket
	{
    
        [ProtoMember(1)]
        public ResponseInfoPacket Info { get; set; } = new ResponseInfoPacket();
        
        [ProtoMember(2)]
        public WorldPacket World { get; set; } = new WorldPacket();
        
        [ProtoMember(3)]
        public ChgObjPacket ChgObj { get; set; } = new ChgObjPacket();
        
	}
}
