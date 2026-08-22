using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["Id"])]
	public partial class AccountModel : ModelBase
	{
    
    		
    		public ulong Id { get; set; } = default; //
        
    		
    		public int ShardId { get; set; } = default; //
        
    		
    		public EAccountState State { get; set; } = default; //
        
    		
    		public string ClientSecret { get; set; } = default; //
        
    		
    		public int AdditionalPlayerCnt { get; set; } = default; //
        
    		
    		public ulong Flag { get; set; } = default; //
        
    		
    		public int Age { get; set; } = default; //
        
		public override bool PkEquals(ModelBase other)
		{
			return other is AccountModel otherModel
				&& Id == otherModel.Id;
		}
	}
}
