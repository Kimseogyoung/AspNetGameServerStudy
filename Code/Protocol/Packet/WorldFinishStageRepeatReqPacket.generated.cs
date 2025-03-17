using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldFinishStageRepeatReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public int WorldNum { get; set; } = default;
        
        [ProtoMember(3)]
        public int StageNum { get; set; } = default;
        
        [ProtoMember(4)]
        public int Star { get; set; } = default;
        
        [ProtoMember(5)]
        public List<ObjValue> RewardValueList { get; set; } = new();
        

        public const string NAME = "world/finish-stage-repeat";
        public string GetProtocolName() => NAME;

        public WorldFinishStageRepeatReqPacket( int worldnum,  int stagenum,  int star,  List<ObjValue> rewardvaluelist )
	    {   
         
                WorldNum = worldnum; 
                 
                StageNum = stagenum; 
                 
                Star = star; 
                 
                RewardValueList = rewardvaluelist; 
                
	    }

    
        public WorldFinishStageRepeatReqPacket()
        {
        }
        

	}
}
