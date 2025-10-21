using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class HealthCheckResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new ResInfoPacket();
        
        [ProtoMember(2)]
        public string Msg { get; set; } = default;
        
	}
}
