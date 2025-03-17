using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class ScheduleLoadReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public List<EScheduleType> TypeList { get; set; } = new();
        

        public const string NAME = "schedule/load";
        public string GetProtocolName() => NAME;

        public ScheduleLoadReqPacket( List<EScheduleType> typelist )
	    {   
         
                TypeList = typelist; 
                
	    }

    
        public ScheduleLoadReqPacket()
        {
        }
        

	}
}
