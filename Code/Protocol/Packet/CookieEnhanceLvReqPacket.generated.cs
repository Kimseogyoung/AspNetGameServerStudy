using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class CookieEnhanceLvReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new ReqInfoPacket();
        
        [ProtoMember(2)]
        public int CookieNum { get; set; } = default;
        
        [ProtoMember(3)]
        public int BefLv { get; set; } = default;
        
        [ProtoMember(4)]
        public int AftLv { get; set; } = default;
        
        [ProtoMember(5)]
        public CostObjPacket CostObj { get; set; } = new CostObjPacket();
        

        public const string NAME = "cookie/enhance-lv";
        public string GetProtocolName() => NAME;

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
