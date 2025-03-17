using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldFinishStageFirstResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public WorldPacket World { get; set; } = new();
        
        [ProtoMember(3)]
        public WorldStagePacket WorldStage { get; set; } = new();
        
        [ProtoMember(4)]
        public List<ChgObjPacket> ChgObjList { get; set; } = new();
        
	}
}
