using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["AccountId"])]
	public partial class SessionModel : ModelBase
	{
    
    		
    		public string Key { get; set; } = default; //
        
    		
    		public ulong AccountId { get; set; } = default; //
        
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int ShardId { get; set; } = default; //
        
    		
    		public string DeviceKey { get; set; } = default; //
        
    		
    		public long ExpireTimestamp { get; set; } = default; //
        
    		
    		public ESessionState State { get; set; } = default; //
        
    		
    		public string ClientSecret { get; set; } = default; //
        
    		
    		public string EncryptSecret { get; set; } = default; //
        
    		
    		public string EncryptIV { get; set; } = default; //
        
    		
    		public string PublicIp { get; set; } = default; //
        
		public override bool PkEquals(ModelBase other)
		{
			return other is SessionModel otherModel
				&& AccountId == otherModel.AccountId;
		}
	}
}
