using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["Key"])]
	public partial class ChannelModel : ModelBase
	{
    
    		
    		public string Key { get; set; } = default; //
        
    		
    		public ulong AccountId { get; set; } = default; //
        
    		
    		public EChannelType Type { get; set; } = default; //
        
    		
    		public string Token { get; set; } = default; //
        
    		
    		public EChannelState State { get; set; } = default; //
        
		public override bool PkEquals(ModelBase other)
		{
			return other is ChannelModel otherModel
				&& Key == otherModel.Key;
		}
	}
}
