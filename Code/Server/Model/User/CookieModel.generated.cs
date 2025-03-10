using ProtoBuf;
using Proto;

namespace WebStudyServer.Model
{
	
	public partial class CookieModel : ModelBase
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
        
	}
}
