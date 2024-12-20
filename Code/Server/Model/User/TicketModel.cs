namespace WebStudyServer.Model
{
    public class TicketModel : ModelBase
    {
        public ulong PlayerId { get; set; }
        public int Num { get; set; }
        public int Amount { get; set; }
        public int AccAmount { get; set; }
        public DateTime EndTime { get; set; }
    }
}
