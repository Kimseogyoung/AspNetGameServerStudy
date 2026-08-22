using WebStudyServer.Helper;

namespace WebStudyServer.Model
{
    public partial class TicketModel
    {
        public double DecAmount(double amount, string reason)
        {
            ReqHelper.ValidEnough(amount, Amount, $"TICKET_{Num}", reason);

            Amount -= amount;
            AccAmount -= amount;
            return Amount;
        }

        public double IncAmount(double amount)
        {
            Amount += amount;
            AccAmount += amount;
            return Amount;
        }
    }
}
