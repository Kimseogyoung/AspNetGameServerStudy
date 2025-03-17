using ProtoBuf;
using Proto;
namespace Protocol
{
	[ProtoContract]
	public partial class GameChangeNameReqPacket : IReqPacket
	{
    
        [ProtoMember(1)]
        public ReqInfoPacket Info { get; set; } = new();
        
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
