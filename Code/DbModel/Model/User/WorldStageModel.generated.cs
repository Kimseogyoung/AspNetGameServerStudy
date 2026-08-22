using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["PlayerId", "Num"], ScopeKey = "PlayerId")]
	public partial class WorldStageModel : ModelBase, IScopedModel
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int Num { get; set; } = default; //
        
    		
    		public int WorldNum { get; set; } = default; //
        
    		
    		public int Star { get; set; } = default; //
        
		public override bool PkEquals(ModelBase other)
		{
			return other is WorldStageModel otherModel
				&& PlayerId == otherModel.PlayerId
				&& Num == otherModel.Num;
		}

		public ulong GetScopeKey() => PlayerId;
		public void SetScopeKey(ulong value) => PlayerId = value;
	}
}
