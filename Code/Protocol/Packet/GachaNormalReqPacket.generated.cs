using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class GachaNormalReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public int ScheduleNum { get; set; } = default;
        
        [ProtoMember(3)]
        public int Cnt { get; set; } = default;
        
        [ProtoMember(4)]
        public CostObjPacket CostObj { get; set; } = new();
        

        public const string NAME = "gacha/normal";
        public string GetProtocolName() => NAME;

        public GachaNormalReqPacket( int schedulenum,  int cnt,  CostObjPacket costobj )
	    {   
         
                ScheduleNum = schedulenum; 
                 
                Cnt = cnt; 
                 
                CostObj = costobj; 
                
	    }

    
        public GachaNormalReqPacket()
        {
        }
        

	}
}
