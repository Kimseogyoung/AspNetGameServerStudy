using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class AuthSignInResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public SignInResultPacket Result { get; set; } = new();
        
	}
}
