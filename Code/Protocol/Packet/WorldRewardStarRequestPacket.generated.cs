using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class WorldRewardStarRequestPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
        [ProtoMember(2)]
        public int WorldNum { get; set; } = default;
        
        [ProtoMember(3)]
        public int BefRewardStar { get; set; } = default;
        
        [ProtoMember(4)]
        public int AftRewardStar { get; set; } = default;
        
        [ProtoMember(5)]
        public int TotalStar { get; set; } = default;
        
        [ProtoMember(6)]
        public ObjValue RewardValue { get; set; } = new ObjValue();
        

        public const string NAME = "world/reward-star";
        public string GetProtocolName() => NAME;

        public WorldRewardStarRequestPacket( int worldnum,  int befrewardstar,  int aftrewardstar,  int totalstar,  ObjValue rewardvalue )
	    {   
         
                WorldNum = worldnum; 
                 
                BefRewardStar = befrewardstar; 
                 
                AftRewardStar = aftrewardstar; 
                 
                TotalStar = totalstar; 
                 
                RewardValue = rewardvalue; 
                
	    }

    
        public WorldRewardStarRequestPacket()
        {
        }
        

	}
}
