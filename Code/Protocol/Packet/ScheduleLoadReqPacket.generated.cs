using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class ScheduleLoadReqPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
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
