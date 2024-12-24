using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class DeviceModel : ModelBase
	{
    
    		
    		public string Key { get; set; } = default; //
        
    		
    		public ulong AccountId { get; set; } = default; //
        
    		
    		public string Idfa { get; set; } = default; //
        
    		
    		public string GeoIpCountry { get; set; } = default; //
        
    		
    		public string Country { get; set; } = default; //
        
    		
    		public string Language { get; set; } = default; //
        
    		
    		public EDeviceState State { get; set; } = default; //
        
    		
    		public DateTime UpdateTime { get; set; } = default; //
        
    		
    		public DateTime CreateTime { get; set; } = default; //
        
	}
}
