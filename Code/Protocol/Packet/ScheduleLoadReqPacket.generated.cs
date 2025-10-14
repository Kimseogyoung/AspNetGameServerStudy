using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class ScheduleLoadReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new ReqInfoPacket();
        
        [ProtoMember(2)]
        public List<EScheduleType> TypeList { get; set; } = new List<EScheduleType>();
        

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
