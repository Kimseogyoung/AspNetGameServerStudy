using Proto;
using WebStudyServer.Helper;

namespace WebStudyServer.Model
{
    public partial class KingdomStructureModel
    {
        public void ValidChgAction(int cnt)
        {
            if (cnt > 0)
            {
                // 창고로 옮기는 것이므로 배치 상태여야 한다
                ReqHelper.ValidContext(State is not EKingdomItemState.STORED and not EKingdomItemState.NONE,
                    "NOT_PLACED_KINGDOM_STRUCTURE", () => new { State });
            }
            else if (cnt < 0)
            {
                // 배치하는 것이므로 창고에 있어야 한다
                ReqHelper.ValidContext(State == EKingdomItemState.STORED,
                    "PLACED_KINGDOM_STRUCTURE", () => new { State });
            }
        }

        public void Construct(KingdomItemProto prt, DateTime serverTime)
        {
            State = EKingdomItemState.CONSTRUCTING;
            EndTime = serverTime + TimeSpan.FromSeconds(prt.ConstructSec);

            if (prt.ConstructSec == 0)
            {
                State = EKingdomItemState.READY;
                EndTime = DateTime.MinValue;
            }
        }

        // 건설/제작 완료 처리.
        //
        // "끝났다" 는 EndTime 이 지났다는 뜻이다. 옛 코드는 부등호가 반대여서
        // 진행 중일 때 통과하고 시간이 지나면 오히려 막았다 - 건설 시간을 기다릴
        // 이유가 없고 기다리면 완료가 안 되는 상태였다.
        public void SetReady(EKingdomItemState correctBefState, DateTime serverTime)
        {
            ReqHelper.ValidContext(State == correctBefState, "NOT_EQUAL_CORRECT_BEF_KINGDOM_STRUCTURE_STATE",
                () => new { State, CorrectBefState = correctBefState });
            ReqHelper.ValidContext(EndTime <= serverTime, "NOT_FINISHED_KINGDOM_STRUCTURE",
                () => new { EndTime, ServerTime = serverTime });

            EndTime = DateTime.MinValue;
            State = EKingdomItemState.READY;
        }

        public void Store()
        {
            State = EKingdomItemState.STORED;
            EndTime = DateTime.MinValue;
        }

        public void Place()
        {
            State = EKingdomItemState.READY;
            EndTime = DateTime.MinValue;
        }

        // 남은 시간을 캐시로 없앤다
        public void DecTime()
        {
            EndTime = DateTime.MinValue;
            State = EKingdomItemState.READY;
        }
    }
}
