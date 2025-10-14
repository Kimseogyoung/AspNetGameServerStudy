using ProtoBuf;
using Proto;
using System.Collections.Generic;
namespace Protocol
{
	[ProtoContract]
	public partial class GameChangeNameReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new ReqInfoPacket();
        
        [ProtoMember(2)]
        public string PlayerName { get; set; } = default;
        

        public const string NAME = "game/change-name";
        public string GetProtocolName() => NAME;

        public GameChangeNameReqPacket( string playername )
	    {   
         
                PlayerName = playername; 
                
	    }

    
        public GameChangeNameReqPacket()
        {
        }
        

	}
}
