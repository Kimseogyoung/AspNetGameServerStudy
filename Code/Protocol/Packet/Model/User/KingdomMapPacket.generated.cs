using ProtoBuf;
using Proto;
using System.Collections.Generic;
using System;

namespace Protocol
{
	[ProtoContract]
	public partial class KingdomMapPacket
	{
    
    		[ProtoMember(1)]
    		public int SizeX { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public int SizeY { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public EKingdomTileMapState State { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public List<PlacedKingdomItemPacket> PlacedKingdomItemList { get; set; } = default; //
        
	}
}
