using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldFinishStageResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public WorldPacket World { get; set; } 
        
        [ProtoMember(3)]
        public WorldStagePacket WorldStage { get; set; } 
        
        [ProtoMember(4)]
        public List<ChgObjPacket> ChgObjList { get; set; } 
        
	}
}
