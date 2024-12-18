using ProtoBuf;

namespace Protocol
{
	[ProtoContract]
	public partial class GameChangeNameResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public string PlayerName { get; set; } 
        
	}
}
