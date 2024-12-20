using Proto;
using System.ComponentModel.DataAnnotations;

namespace WebStudyServer.Model
{
    public class CookieModel : ModelBase
    {
        public ulong PlayerId { get; set; } = default;
        public int Num { get; set; } = default;
        public int StarExp { get; set; } = default;
        public int AccStarExp { get; set; } = default;
        public int Star { get; set; } = default;
        public int Flag { get; set; } = default;
        public ECookieState State { get; set; } = default;
    }
}
