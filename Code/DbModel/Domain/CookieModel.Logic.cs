using Proto;
using WebStudyServer.Helper;

namespace WebStudyServer.Model
{
    public partial class CookieModel
    {
        // 현재 Star에서 aftStar까지 올리는 데 드는 소울스톤
        public int GetSoulStoneToEnhanceStar(int aftStar, CookieProto prt)
        {
            var needSoulStone = 0;
            for (var star = Star; star < aftStar; star++)
            {
                var prtCookieStarEnhance = ProtoDb.Get<CookieStarEnhanceProto>((prt.GradeType, star));
                needSoulStone += prtCookieStarEnhance.SoulStone;
            }

            return needSoulStone;
        }

        public void EnhanceStar(int aftStar, int usedSoulStone, CookieProto prt)
        {
            ReqHelper.ValidEnough(usedSoulStone, SoulStone, $"COOKIE_SOUL_STONE:{prt.Num}", "ENHANCE_STAR");

            Star = aftStar;
            SoulStone -= usedSoulStone;
        }

        public void EnhanceLv(int aftLv)
        {
            Lv = aftLv;
        }

        // 첫 획득이면 한 장이 쿠키 자체가 되고 나머지가 소울스톤이 된다
        public double IncCookie(int amount, CookieProto prt)
        {
            var soulStoneCnt = amount * prt.InitSoulStone;
            if (State != ECookieState.AVAILABLE)
            {
                State = ECookieState.AVAILABLE;
                soulStoneCnt -= prt.InitSoulStone;
            }

            if (soulStoneCnt > 0)
            {
                SoulStone += soulStoneCnt;
                AccSoulStone += soulStoneCnt;
            }

            return AccSoulStone;
        }

        public double IncSoulStone(int amount)
        {
            SoulStone += amount;
            AccSoulStone += amount;
            return AccSoulStone;
        }
    }
}
