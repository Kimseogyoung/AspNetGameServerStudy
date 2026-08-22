using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["PlayerId"], ScopeKey = "PlayerId")]
	public partial class KingdomMapModel : ModelBase, IScopedModel
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int SizeX { get; set; } = default; //
        
    		
    		public int SizeY { get; set; } = default; //
        
    		
    		public string Snapshot { get; set; } = default; //
        
    		
    		public EKingdomTileMapState State { get; set; } = default; //
        
		public override bool PkEquals(ModelBase other)
		{
			return other is KingdomMapModel otherModel
				&& PlayerId == otherModel.PlayerId;
		}

		public ulong GetScopeKey() => PlayerId;
		public void SetScopeKey(ulong value) => PlayerId = value;
	}
}
