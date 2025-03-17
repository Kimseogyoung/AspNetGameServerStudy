using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class CookieEnhanceStarReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        
        [ProtoMember(2)]
        public int CookieNum { get; set; } = default;
        
        [ProtoMember(3)]
        public int BefStar { get; set; } = default;
        
        [ProtoMember(4)]
        public int AftStar { get; set; } = default;
        
        [ProtoMember(5)]
        public int UsedSoulStone { get; set; } = default;
        

        public const string NAME = "cookie/enhance-star";
        public string GetProtocolName() => NAME;

        public CookieEnhanceStarReqPacket( int cookienum,  int befstar,  int aftstar,  int usedsoulstone )
	    {   
         
                CookieNum = cookienum; 
                 
                BefStar = befstar; 
                 
                AftStar = aftstar; 
                 
                UsedSoulStone = usedsoulstone; 
                
	    }

    
        public CookieEnhanceStarReqPacket()
        {
        }
        

	}
}
