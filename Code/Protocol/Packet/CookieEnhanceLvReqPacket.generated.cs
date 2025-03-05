using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class CookieEnhanceLvReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } 
        
        [ProtoMember(2)]
        public int CookieNum { get; set; } 
        
        [ProtoMember(3)]
        public int BefLv { get; set; } 
        
        [ProtoMember(4)]
        public int AftLv { get; set; } 
        
        [ProtoMember(5)]
        public CostObjPacket CostObj { get; set; } 
        
        public string GetProtocolName() => "cookie/enhance-lv";

        public CookieEnhanceLvReqPacket( int cookienum,  int beflv,  int aftlv,  CostObjPacket costobj )
	    {   
         
                CookieNum = cookienum; 
                 
                BefLv = beflv; 
                 
                AftLv = aftlv; 
                 
                CostObj = costobj; 
                
	    }

        public CookieEnhanceLvReqPacket()
	{
	}

	}
}
