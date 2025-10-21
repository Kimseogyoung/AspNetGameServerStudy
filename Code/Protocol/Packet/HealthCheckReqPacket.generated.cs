using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class HealthCheckReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new ReqInfoPacket();
        

        public const string NAME = "health-check";
        public string GetProtocolName() => NAME;

        public HealthCheckReqPacket()
	    {   
        
	    }

    

	}
}
