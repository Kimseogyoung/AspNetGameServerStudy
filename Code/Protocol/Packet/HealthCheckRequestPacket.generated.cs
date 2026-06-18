using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class HealthCheckRequestPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        

        public const string NAME = "health-check";
        public string GetProtocolName() => NAME;

        public HealthCheckRequestPacket()
	    {   
        
	    }

    

	}
}
