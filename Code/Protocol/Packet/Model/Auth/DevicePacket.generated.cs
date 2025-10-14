using ProtoBuf;
using Proto;
using System.Collections.Generic;
using System;

namespace Protocol
{
	[ProtoContract]
	public partial class DevicePacket
	{
    
    		[ProtoMember(1)]
    		public string Key { get; set; } = default; //
        
    		[ProtoMember(2)]
    		public string Idfa { get; set; } = default; //
        
    		[ProtoMember(3)]
    		public string GeoIpCountry { get; set; } = default; //
        
    		[ProtoMember(4)]
    		public string Country { get; set; } = default; //
        
    		[ProtoMember(5)]
    		public string Language { get; set; } = default; //
        
    		[ProtoMember(6)]
    		public EDeviceState State { get; set; } = default; //
        
	}
}
