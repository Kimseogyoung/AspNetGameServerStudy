using ProtoBuf;
using Proto;
using System.Collections.Generic;
using System;

namespace Protocol
{
	[ProtoContract]
	public partial class SessionPacket
	{
    
    		[ProtoMember(1)]
    		public string Key { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public int ShardId { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public string DeviceKey { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public long ExpireTimestamp { get; set; } = default; //
        
    		[ProtoMember(5)]
    		public ESessionState State { get; set; } = default; //
        
    		[ProtoMember(6)]
    		public string ClientSecret { get; set; } = default; //
        
    		[ProtoMember(7)]
    		public string EncryptSecret { get; set; } = default; //
        
    		[ProtoMember(8)]
    		public string EncryptIV { get; set; } = default; //
        
    		[ProtoMember(9)]
    		public string PublicIp { get; set; } = default; //
        
	}
}
