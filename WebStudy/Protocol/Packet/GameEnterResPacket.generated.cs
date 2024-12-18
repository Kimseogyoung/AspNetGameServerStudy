using ProtoBuf;

namespace Protocol
{
	[ProtoContract]
	public partial class GameEnterResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
	}
}
