using Proto;
using WebStudyServer.GAME;

namespace Server.Helper
{
    public class GachaHelper
    {
        public GachaHelper(GachaScheduleProto prt)
        {
            _prt = prt;
            _prtProb = APP.Prt.GetGachaProbPrt(prt.GachaProbNum);
        }

        private GachaScheduleProto _prt;
        private GachaProbProto _prtProb;
    }

    public static class GachaConstant
    {
        // 미리 가중치 계산
    }
}
