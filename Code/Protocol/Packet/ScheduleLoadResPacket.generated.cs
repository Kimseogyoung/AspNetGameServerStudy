using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class ScheduleLoadResPacket : IResPacket
	{
    
        [ProtoMember(1)]
        public ResInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public List<SchedulePacket> ScheduleList { get; set; } 
        
	}
}
