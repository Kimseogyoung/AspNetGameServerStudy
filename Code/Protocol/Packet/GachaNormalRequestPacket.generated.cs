using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class GachaNormalRequestPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
        [ProtoMember(2)]
        public int ScheduleNum { get; set; } = default;
        
        [ProtoMember(3)]
        public int Cnt { get; set; } = default;
        
        [ProtoMember(4)]
        public CostObjPacket CostObj { get; set; } = new CostObjPacket();
        

        public const string NAME = "gacha/normal";
        public string GetProtocolName() => NAME;

        public GachaNormalRequestPacket( int schedulenum,  int cnt,  CostObjPacket costobj )
	    {   
         
                ScheduleNum = schedulenum; 
                 
                Cnt = cnt; 
                 
                CostObj = costobj; 
                
	    }

    
        public GachaNormalRequestPacket()
        {
        }
        

	}
}
