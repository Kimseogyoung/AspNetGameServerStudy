using System.Collections.Generic;
using System;
namespace Proto
{
	public partial class GachaItemProto : ProtoBase
	{
    
    		public int Num { get; set; }
        
    		public EGachaItemType Type { get; set; }
        
    		public string Tag { get; set; }
        
    		public int Seq { get; set; }
        

		protected internal override void SetField(string name, string value)
		{
			switch (name)
			{
    
        
    				case "Num": Num = int.Parse(value); break;
        
        
        
    				case "Type": Type = Enum.Parse<EGachaItemType>(value); break;
        
        
        
    				case "Tag": Tag = value; break;
        
        
        
    				case "Seq": Seq = int.Parse(value); break;
        
        
			}
		}
	}
}
