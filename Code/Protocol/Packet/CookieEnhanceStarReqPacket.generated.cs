using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class CookieEnhanceStarReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public int CookieNum { get; set; } 
        
        [ProtoMember(3)]
        public int BefStar { get; set; } 
        
        [ProtoMember(4)]
        public int AftStar { get; set; } 
        
        [ProtoMember(5)]
        public int UsedSoulStone { get; set; } 
        
        public string GetProtocolName() => "cookie/enhance-star";

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
