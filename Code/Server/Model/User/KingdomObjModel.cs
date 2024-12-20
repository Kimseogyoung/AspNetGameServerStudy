using Proto;
using System.ComponentModel.DataAnnotations;

namespace WebStudyServer.Model
{
    public class KingdomObjModel : ModelBase
    {
        public ulong Id { get; set; }
        public ulong PlayerId { get; set; } 
        public EKingdomObjType Type { get; set; }
        public int Num { get; set; }
        public EKingdomObjState State { get; set; }
        public DateTime EndTime { get; set; }
        public int StartTileX { get; set; }
        public int StartTileY { get; set; }
        public int EndTileX { get; set; }
        public int EndTileY { get; set; }
    }
}
