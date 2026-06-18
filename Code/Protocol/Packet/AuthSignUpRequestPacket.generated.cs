using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class AuthSignUpRequestPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
        [ProtoMember(2)]
        public string DeviceKey { get; set; } = default;
        

        public const string NAME = "auth/sign-up";
        public string GetProtocolName() => NAME;

        public AuthSignUpRequestPacket( string devicekey )
	    {   
         
                DeviceKey = devicekey; 
                
	    }

    
        public AuthSignUpRequestPacket()
        {
        }
        

	}
}
