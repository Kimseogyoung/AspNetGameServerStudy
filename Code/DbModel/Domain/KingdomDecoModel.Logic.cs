using Proto;
using WebStudyServer.Helper;

namespace WebStudyServer.Model
{
    public partial class KingdomDecoModel
    {
        public void Inc(int cnt, KingdomItemProto prt, string reason)
        {
            ReqHelper.ValidUnderFlowParam(cnt, $"DECO_CNT:{reason}");
            ReqHelper.ValidContext(TotalCnt + cnt <= prt.MaxCnt, "FULL_MAX_DECO_CNT",
                () => new { Num, TotalCnt, PrtMaxCnt = prt.MaxCnt });

            TotalCnt += cnt;
            UnplacedCnt += cnt;
        }

        public void ValidChgAction(int cnt)
        {
            if (cnt > 0)
            {
                // 창고로 cnt 만큼 넣는 것이므로 배치된 게 그만큼 있어야 한다
                var placedCnt = TotalCnt - UnplacedCnt;
                ReqHelper.ValidContext(placedCnt >= cnt, "NOT_ENOUGH_PLACED_KINGDOM_DECO",
                    () => new { Num, PlacedCnt = placedCnt, StoreCnt = cnt });
            }
            else if (cnt < 0)
            {
                // 배치하는 것이므로 창고에 -cnt 만큼 있어야 한다.
                // 옛 코드는 UnplacedCnt >= cnt 라 cnt 가 음수일 때 늘 참이었다.
                ReqHelper.ValidContext(UnplacedCnt >= -cnt, "NOT_ENOUGH_UNPLACED_KINGDOM_DECO",
                    () => new { Num, UnplacedCnt, PlaceCnt = -cnt });
            }
        }

        public void Place(int cnt = 1)
        {
            ReqHelper.ValidContext(UnplacedCnt >= cnt, "NOT_ENOUGH_DECO_CNT",
                () => new { Num, UnplacedCnt, DecCnt = cnt });

            UnplacedCnt -= cnt;
        }

        public void Store(int cnt = 1)
        {
            var placedCnt = TotalCnt - UnplacedCnt;
            ReqHelper.ValidContext(placedCnt >= cnt, "NOT_ENOUGH_DECO_CNT",
                () => new { Num, UnplacedCnt, DecCnt = cnt });

            UnplacedCnt += cnt;
        }
    }
}
