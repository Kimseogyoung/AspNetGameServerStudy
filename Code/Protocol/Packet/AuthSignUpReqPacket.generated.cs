using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class AuthSignUpReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new ReqInfoPacket();
        
        [ProtoMember(2)]
        public string DeviceKey { get; set; } = default;
        

        public const string NAME = "auth/sign-up";
        public string GetProtocolName() => NAME;

        public AuthSignUpReqPacket( string devicekey )
	    {   
         
                DeviceKey = devicekey; 
                
	    }

    
        public AuthSignUpReqPacket()
        {
        }
        

	}
}
