using ProtoBuf;
using Proto;
using ServerCore.Model;

namespace WebStudyServer.Model
{
	[Entity(Pk = ["Id"], ScopeKey = "Id")]
	public partial class PlayerModel : ModelBase, IScopedModel
	{
    
    		
    		public ulong Id { get; set; } = default; //
        
    		
    		public ulong AccountId { get; set; } = default; //
        
    		
    		public ulong SfId { get; set; } = default; //
        
    		
    		public string ProfileName { get; set; } = default; //
        
    		
    		public int Lv { get; set; } = 1; //
        
    		
    		public int CastleLv { get; set; } = 0; //
        
    		
    		public ulong Flag { get; set; } = default; //
        
    		
    		public EPlayerState State { get; set; } = default; //
        
    		
    		public int ProfileTitleNum { get; set; } = default; //
        
    		
    		public int ProfileIconNum { get; set; } = default; //
        
    		
    		public int ProfileFrameNum { get; set; } = default; //
        
    		
    		public int ProfileCookieNum { get; set; } = default; //
        
    		
    		public ulong GuildId { get; set; } = default; //
        
    		
    		public int KingdomExp { get; set; } = default; //
        
		public override bool PkEquals(ModelBase other)
		{
			return other is PlayerModel otherModel
				&& Id == otherModel.Id;
		}

		public ulong GetScopeKey() => Id;
		public void SetScopeKey(ulong value) => Id = value;
	}
}
