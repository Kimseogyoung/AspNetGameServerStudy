using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldFinishStageReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public int WorldNum { get; set; } 
        
        [ProtoMember(3)]
        public int StageNum { get; set; } 
        
        [ProtoMember(4)]
        public int Star { get; set; } 
        
        [ProtoMember(5)]
        public bool IsFirst { get; set; } 
        
        [ProtoMember(6)]
        public List<ObjValue> RewardValueList { get; set; } 
        

        public const string NAME = "world/finish-stage";
        public string GetProtocolName() => NAME;

        public WorldFinishStageReqPacket( int worldnum,  int stagenum,  int star,  bool isfirst,  List<ObjValue> rewardvaluelist )
	    {   
         
                WorldNum = worldnum; 
                 
                StageNum = stagenum; 
                 
                Star = star; 
                 
                IsFirst = isfirst; 
                 
                RewardValueList = rewardvaluelist; 
                
	    }

    
        public WorldFinishStageReqPacket()
        {
        }
        

	}
}
