using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldFinishStageFirstReqPacket : IReqPacket
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
        public List<ObjValue> RewardValueList { get; set; } 
        

        public const string NAME = "world/finish-stage-first";
        public string GetProtocolName() => NAME;

        public WorldFinishStageFirstReqPacket( int worldnum,  int stagenum,  int star,  List<ObjValue> rewardvaluelist )
	    {   
         
                WorldNum = worldnum; 
                 
                StageNum = stagenum; 
                 
                Star = star; 
                 
                RewardValueList = rewardvaluelist; 
                
	    }

    
        public WorldFinishStageFirstReqPacket()
        {
        }
        

	}
}
