using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldFinishStageFirstReqPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
        [ProtoMember(2)]
        public int WorldNum { get; set; } = default;
        
        [ProtoMember(3)]
        public int StageNum { get; set; } = default;
        
        [ProtoMember(4)]
        public int Star { get; set; } = default;
        
        [ProtoMember(5)]
        public List<ObjValue> RewardValueList { get; set; } = new List<ObjValue>();
        

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
