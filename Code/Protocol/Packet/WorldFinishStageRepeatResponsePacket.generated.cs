using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldFinishStageRepeatResponsePacket : IResponsePacket
	{
    
        [ProtoMember(1)]
        public ResponseInfoPacket Info { get; set; } = new ResponseInfoPacket();
        
        [ProtoMember(2)]
        public WorldPacket World { get; set; } = new WorldPacket();
        
        [ProtoMember(3)]
        public WorldStagePacket WorldStage { get; set; } = new WorldStagePacket();
        
        [ProtoMember(4)]
        public List<ChgObjPacket> ChgObjList { get; set; } = new List<ChgObjPacket>();
        
	}
}
