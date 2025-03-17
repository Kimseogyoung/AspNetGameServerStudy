using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldRewardStarReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public int WorldNum { get; set; } = default;
        
        [ProtoMember(3)]
        public int BefRewardStar { get; set; } = default;
        
        [ProtoMember(4)]
        public int AftRewardStar { get; set; } = default;
        
        [ProtoMember(5)]
        public int TotalStar { get; set; } = default;
        
        [ProtoMember(6)]
        public ObjValue RewardValue { get; set; } = new();
        

        public const string NAME = "world/reward-star";
        public string GetProtocolName() => NAME;

        public WorldRewardStarReqPacket( int worldnum,  int befrewardstar,  int aftrewardstar,  int totalstar,  ObjValue rewardvalue )
	    {   
         
                WorldNum = worldnum; 
                 
                BefRewardStar = befrewardstar; 
                 
                AftRewardStar = aftrewardstar; 
                 
                TotalStar = totalstar; 
                 
                RewardValue = rewardvalue; 
                
	    }

    
        public WorldRewardStarReqPacket()
        {
        }
        

	}
}
