using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class ScheduleLoadReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public List<EScheduleType> TypeList { get; set; } 
        
        public string GetProtocolName() => "schedule/load";
	}
}
