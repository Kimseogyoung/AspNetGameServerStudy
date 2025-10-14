using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class ScheduleLoadResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new ResInfoPacket();
        
        [ProtoMember(2)]
        public List<SchedulePacket> ScheduleList { get; set; } = new List<SchedulePacket>();
        
	}
}
