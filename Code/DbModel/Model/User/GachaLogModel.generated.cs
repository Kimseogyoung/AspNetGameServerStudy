using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["SfId"], ScopeKey = "PlayerId")]
	public partial class GachaLogModel : ModelBase, IScopedModel
	{
    
    		
    		public ulong SfId { get; set; } = default; //
        
    		
    		public ulong PlayerId { get; set; } = default; //
        
    		
    		public int ScheduleNum { get; set; } = default; //
        
    		
    		public int Cnt { get; set; } = default; //
        
    		
    		public int ChgRealCash { get; set; } = default; //
        
    		
    		public int ChgFreeCash { get; set; } = default; //
        
    		
    		public EObjType ChgObjType { get; set; } = default; //
        
    		
    		public int ChgObjAmount { get; set; } = default; //
        
    		
    		public string ExtraData { get; set; } = default; //
        
		public override bool PkEquals(ModelBase other)
		{
			return other is GachaLogModel otherModel
				&& SfId == otherModel.SfId;
		}

		public ulong GetScopeKey() => PlayerId;
		public void SetScopeKey(ulong value) => PlayerId = value;
	}
}
