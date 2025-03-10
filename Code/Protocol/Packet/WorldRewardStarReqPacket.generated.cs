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
        public int Star { get; set; } 
        
        [ProtoMember(4)]
        public ObjValue RewardValue { get; set; } 
        

        public const string NAME = "world/reward-star";
        public string GetProtocolName() => NAME;

        public WorldRewardStarReqPacket( int worldnum,  int star,  ObjValue rewardvalue )
	    {   
         
                WorldNum = worldnum; 
                 
                Star = star; 
                 
                RewardValue = rewardvalue; 
                
	    }

    
        public WorldRewardStarReqPacket()
        {
        }
        

	}
}
