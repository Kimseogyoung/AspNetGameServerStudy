using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["AccountId"])]
	public partial class PlayerMapModel : ModelBase
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public ulong AccountId { get; set; } = default; //
        
    		
    		public int ShardId { get; set; } = default; //
        
		public override bool PkEquals(ModelBase other)
		{
			return other is PlayerMapModel otherModel
				&& AccountId == otherModel.AccountId;
		}
	}
}
