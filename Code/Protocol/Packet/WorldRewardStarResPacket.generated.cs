using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldRewardStarResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public WorldPacket World { get; set; } = new();
        
        [ProtoMember(3)]
        public ChgObjPacket ChgObj { get; set; } = new();
        
	}
}
