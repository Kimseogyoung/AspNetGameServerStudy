using WebStudyServer.Helper;

namespace WebStudyServer.Model
{
    // 플레이어의 재화 중 자기 행에 있는 것(Gold/Exp/Cash)만 다룬다.
    // Point/Ticket/Item/Cookie 는 각자의 모델에 있고, 둘을 잇는 라우팅은 RewardService 가 한다.
    //
    // Acc* 는 누적 획득량이라 차감에서는 건드리지 않는다.
    public partial class PlayerDetailModel
    {
        public double TotalCash()
        {
            return FreeCash + RealCash;
        }

        public double DecGold(double amount, string reason)
        {
            ReqHelper.ValidUnderFlowParam(amount, reason);
            ReqHelper.ValidEnough(amount, Gold, "PLAYER_GOLD", reason);

            Gold -= amount;
            return Gold;
        }

        public double IncGold(double amount)
        {
            ReqHelper.ValidUnderFlowParam(amount, "INC_GOLD");

            Gold += amount;
            AccGold += amount;
            return Gold;
        }

        public double DecExp(double amount, string reason)
        {
            ReqHelper.ValidUnderFlowParam(amount, reason);
            ReqHelper.ValidEnough(amount, Exp, "PLAYER_EXP", reason);

            Exp -= amount;
            return Exp;
        }

        public double IncExp(double amount)
        {
            ReqHelper.ValidUnderFlowParam(amount, "INC_EXP");

            Exp += amount;
            AccExp += amount;
            return Exp;
        }

        // RealCash 를 먼저 소모하고 모자란 만큼 FreeCash 에서 뺀다.
        // 두 컬럼이 바뀌므로 값을 반환하지 않는다. 바뀐 값은 호출부가 두 프로퍼티에서 읽는다.
        public void DecCash(double amount, string reason)
        {
            ReqHelper.ValidUnderFlowParam(amount, reason);
            ReqHelper.ValidEnough(amount, TotalCash(), "PLAYER_TOTAL_CASH", reason);

            var realCashCost = Math.Min(RealCash, amount);
            RealCash -= realCashCost;
            FreeCash -= amount - realCashCost;
        }

        public double IncFreeCash(double amount)
        {
            ReqHelper.ValidUnderFlowParam(amount, "INC_FREE_CASH");

            FreeCash += amount;
            AccFreeCash += amount;
            return FreeCash;
        }

        public double IncRealCash(double amount)
        {
            ReqHelper.ValidUnderFlowParam(amount, "INC_REAL_CASH");

            RealCash += amount;
            AccRealCash += amount;
            return RealCash;
        }
    }
}
