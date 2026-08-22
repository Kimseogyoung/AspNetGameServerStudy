using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["Key"])]
	public partial class DeviceModel : ModelBase
	{
    
    		
    		public string Key { get; set; } = default; //
        
    		
    		public ulong AccountId { get; set; } = default; //
        
    		
    		public string Idfa { get; set; } = default; //
        
    		
    		public string GeoIpCountry { get; set; } = default; //
        
    		
    		public string Country { get; set; } = default; //
        
    		
    		public string Language { get; set; } = default; //
        
    		
    		public EDeviceState State { get; set; } = default; //
        
		public override bool PkEquals(ModelBase other)
		{
			return other is DeviceModel otherModel
				&& Key == otherModel.Key;
		}
	}
}
