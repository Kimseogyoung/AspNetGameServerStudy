using Proto;
using System.ComponentModel.DataAnnotations;

namespace WebStudyServer.Model
{
    public class PointModel : ModelBase
    {
        public ulong PlayerId { get; set; }
        public int Num { get; set; }
        public double Amount { get; set; }
        public double AccAmount { get; set; }
    }
}
