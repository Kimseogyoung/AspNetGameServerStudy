using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class CookieEnhanceStarRequestPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
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

        public CookieEnhanceStarRequestPacket( int cookienum,  int befstar,  int aftstar,  int usedsoulstone )
	    {   
         
                CookieNum = cookienum; 
                 
                BefStar = befstar; 
                 
                AftStar = aftstar; 
                 
                UsedSoulStone = usedsoulstone; 
                
	    }

    
        public CookieEnhanceStarRequestPacket()
        {
        }
        

	}
}
