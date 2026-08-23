using WebStudyServer.Helper;

namespace WebStudyServer.Model
{
    public partial class ItemModel
    {
        // AccAmount 는 누적 획득량이라 차감에서는 건드리지 않는다.
        // 음수 금액은 증감의 방향을 뒤집으므로 모델에서 막는다.
        public double DecAmount(double amount, string reason)
        {
            ReqHelper.ValidUnderFlowParam(amount, reason);
            ReqHelper.ValidEnough(amount, Amount, $"ITEM_{Num}", reason);

            Amount -= amount;
            return Amount;
        }

        public double IncAmount(double amount)
        {
            ReqHelper.ValidUnderFlowParam(amount, "INC_ITEM");

            Amount += amount;
            AccAmount += amount;
            return Amount;
        }
    }
}
