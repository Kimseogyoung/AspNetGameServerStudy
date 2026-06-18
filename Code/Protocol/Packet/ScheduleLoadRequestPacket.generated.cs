using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class ScheduleLoadRequestPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
        [ProtoMember(2)]
        public List<EScheduleType> TypeList { get; set; } = new List<EScheduleType>();
        

        public const string NAME = "schedule/load";
        public string GetProtocolName() => NAME;

        public ScheduleLoadRequestPacket( List<EScheduleType> typelist )
	    {   
         
                TypeList = typelist; 
                
	    }

    
        public ScheduleLoadRequestPacket()
        {
        }
        

	}
}
