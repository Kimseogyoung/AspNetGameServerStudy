using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["PlayerId", "Num"], ScopeKey = "PlayerId")]
	public partial class CookieModel : ModelBase, IScopedModel
	{
    
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int Num { get; set; } = default; //
        
    		
    		public int SoulStone { get; set; } = default; //
        
    		
    		public int AccSoulStone { get; set; } = default; //
        
    		
    		public int Star { get; set; } = default; //
        
    		
    		public int Lv { get; set; } = default; //
        
    		
    		public int SkillLv { get; set; } = default; //
        
    		
    		public int Flag { get; set; } = default; //
        
    		
    		public ECookieState State { get; set; } = default; //
        
		public override bool PkEquals(ModelBase other)
		{
			return other is CookieModel otherModel
				&& PlayerId == otherModel.PlayerId
				&& Num == otherModel.Num;
		}

		public ulong GetScopeKey() => PlayerId;
		public void SetScopeKey(ulong value) => PlayerId = value;
	}
}
