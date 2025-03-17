using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class CookieEnhanceStarResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public CookiePacket Cookie { get; set; } = new();
        
	}
}
