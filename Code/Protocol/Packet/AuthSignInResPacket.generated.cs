using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class AuthSignInResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new ResInfoPacket();
        
        [ProtoMember(2)]
        public SignInResultPacket Result { get; set; } = new SignInResultPacket();
        
	}
}
