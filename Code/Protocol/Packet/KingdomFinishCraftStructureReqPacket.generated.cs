using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class KingdomFinishCraftStructureReqPacket : IRequestPacket
	{
    
        [ProtoMember(1)]
        public RequestInfoPacket Info { get; set; } = new RequestInfoPacket();
        
        [ProtoMember(2)]
        public ulong KingdomStructureId { get; set; } = default;
        
        [ProtoMember(3)]
        public int KingdomItemNum { get; set; } = default;
        

        public const string NAME = "kingdom/finish-craft-structure";
        public string GetProtocolName() => NAME;

        public KingdomFinishCraftStructureReqPacket( ulong kingdomstructureid,  int kingdomitemnum )
	    {   
         
                KingdomStructureId = kingdomstructureid; 
                 
                KingdomItemNum = kingdomitemnum; 
                
	    }

    
        public KingdomFinishCraftStructureReqPacket()
        {
        }
        

	}
}
