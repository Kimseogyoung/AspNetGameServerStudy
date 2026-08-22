using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["PlayerId"], ScopeKey = "PlayerId")]
	public partial class PlayerDetailModel : ModelBase, IScopedModel
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public double Exp { get; set; } = default; //
        
    		
    		public double AccExp { get; set; } = default; //
        
    		
    		public double Gold { get; set; } = default; //
        
    		
    		public double AccGold { get; set; } = default; //
        
    		
    		public double RealCash { get; set; } = default; //
        
    		
    		public double FreeCash { get; set; } = default; //
        
    		
    		public double AccRealCash { get; set; } = default; //
        
    		
    		public double AccFreeCash { get; set; } = default; //
        
		public override bool PkEquals(ModelBase other)
		{
			return other is PlayerDetailModel otherModel
				&& PlayerId == otherModel.PlayerId;
		}

		public ulong GetScopeKey() => PlayerId;
		public void SetScopeKey(ulong value) => PlayerId = value;
	}
}
