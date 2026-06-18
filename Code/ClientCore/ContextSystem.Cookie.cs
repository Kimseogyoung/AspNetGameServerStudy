using System;
using System.Threading.Tasks;
using Proto;
using Protocol;

namespace ClientCore
{
    public partial class ContextSystem
    {
        public async Task<ScheduleLoadResponsePacket> RequestLoadSchedule()
        {
            var req = new ScheduleLoadRequestPacket();
            var res = await RpcSystem.RequestAsync<ScheduleLoadRequestPacket, ScheduleLoadResponsePacket>(req);

            SyncScheduleList(res.ScheduleList);
            return res;
        }

        public async Task<GachaNormalResponsePacket> RequestGachaNormal(int scheduleNum, int cnt)
        {
            var prtGachaSchedule = APP.Prt.GetGachaSchedulePrt(scheduleNum);
            return await RequestGachaNormal(scheduleNum, prtGachaSchedule.CostTypeList[0], prtGachaSchedule.CostAmountList[0], cnt);
        }

        public async Task<GachaNormalResponsePacket> RequestGachaNormal(int scheduleNum, EObjType costType, int costAmount, int cnt)
        {
            var prtGachaSchedule = APP.Prt.GetGachaSchedulePrt(scheduleNum);

            var costIdx = prtGachaSchedule.CntList.FindIndex(x => x == cnt);
            if (costIdx == -1)
            {
                Console.WriteLine($"INVALID_GACHA_CNT({cnt})");
                return new GachaNormalResponsePacket { Info = _errorRes };
            }

            var req = new GachaNormalRequestPacket(scheduleNum, cnt, new CostObjPacket { Type = costType, Num = 0, Amount = costAmount * cnt });
            var res = await RpcSystem.RequestAsync<GachaNormalRequestPacket, GachaNormalResponsePacket>(req);

            SyncChgObjList(res.GachaResultChgObjList);
            SyncChgObj(res.CostChgObj);
            return res;
        }


        public void PrintCookieList()
        {
            foreach (var cookie in Player.CookieList)
            {
                var prtCookie = APP.Prt.GetCookiePrt(cookie.Num);
                Console.WriteLine($"CookieNum:{cookie.Num}, Name:{prtCookie.Name}, Star:{cookie.Star}, Lv:{cookie.Lv}, SoulStone:{cookie.SoulStone}, State:{cookie.State.ToString()}");
            }
        }

        public async Task<CookieEnhanceStarResponsePacket> RequestCookieEnhanceStar(int cookieNum, int aftStar)
        {
            var prtCookie = APP.Prt.GetCookiePrt(cookieNum);
            var cookie = GetCookieForce(cookieNum);
            var useSoulStone = 0;

            for (var star = cookie.Star; star < aftStar; star++)
            {
                var prtCookieStarEnhance = APP.Prt.GetCookieStarEnhancePrt(prtCookie.GradeType, star);
                useSoulStone += prtCookieStarEnhance.SoulStone;
            }

            var req = new CookieEnhanceStarRequestPacket(cookieNum, cookie.Star, aftStar, useSoulStone);
            var res = await RpcSystem.RequestAsync<CookieEnhanceStarRequestPacket, CookieEnhanceStarResponsePacket>(req);

            SyncCookie(res.Cookie);
            return res;
        }

        public async Task<CookieEnhanceLvResponsePacket> RequestCookieEnhanceLv(int cookieNum, int aftLv)
        {
            var prtCookie = APP.Prt.GetCookiePrt(cookieNum);
            var cookie = GetCookieForce(cookieNum);
            var cfgLvCost = 10;


            var req = new CookieEnhanceLvRequestPacket(cookieNum, cookie.Lv, aftLv, new CostObjPacket { Type = Proto.EObjType.POINT_COOKIE_LV, Num = 0, Amount = cfgLvCost * (aftLv - cookie.Lv) });
            var res = await RpcSystem.RequestAsync<CookieEnhanceLvRequestPacket, CookieEnhanceLvResponsePacket>(req);

            SyncCookie(res.Cookie);
            SyncChgObj(res.ChgObj);
            return res;
        }
    }
}
