using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldRewardStarReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public int WorldNum { get; set; } 
        
        [ProtoMember(3)]
        public int BefRewardStar { get; set; } 
        
        [ProtoMember(4)]
        public int AftRewardStar { get; set; } 
        
        [ProtoMember(5)]
        public ObjValue RewardValue { get; set; } 
        

        public const string NAME = "world/reward-star";
        public string GetProtocolName() => NAME;

        public WorldRewardStarReqPacket( int worldnum,  int befrewardstar,  int aftrewardstar,  ObjValue rewardvalue )
	    {   
         
                WorldNum = worldnum; 
                 
                BefRewardStar = befrewardstar; 
                 
                AftRewardStar = aftrewardstar; 
                 
                RewardValue = rewardvalue; 
                
	    }

    
        public WorldRewardStarReqPacket()
        {
        }
        

	}
}
