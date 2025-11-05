using Protocol;
using System;
using System.Threading.Tasks;

namespace ClientCore
{
    public partial class ContextSystem
    {
        public async Task<ScheduleLoadResPacket> RequestLoadSchedule()
        {
            var req = new ScheduleLoadReqPacket();
            var res = await RpcSystem.RequestAsync<ScheduleLoadReqPacket, ScheduleLoadResPacket>(req);

            SyncScheduleList(res.ScheduleList);
            return res;
        }

        public async Task<GachaNormalResPacket> RequestGachaNormal(int scheduleNum, int cnt)
        {
            var prtGachaSchedule = APP.Prt.GetGachaSchedulePrt(scheduleNum);

            var costIdx = prtGachaSchedule.CntList.FindIndex(x => x == cnt);
            if(costIdx == -1)
            {
                Console.WriteLine($"INVALID_GACHA_CNT({cnt})");
                return new GachaNormalResPacket { Info = _errorRes };
            }

            var req = new GachaNormalReqPacket(scheduleNum, cnt, new CostObjPacket { Type = prtGachaSchedule.CostTypeList[costIdx], Num = 0, Amount = prtGachaSchedule.CostAmountList[costIdx] * cnt });
            var res = await RpcSystem.RequestAsync<GachaNormalReqPacket, GachaNormalResPacket>(req);

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

        public async Task<CookieEnhanceStarResPacket> RequestCookieEnhanceStar(int cookieNum, int aftStar)
        {
            var prtCookie = APP.Prt.GetCookiePrt(cookieNum);
            var cookie = GetCookieForce(cookieNum);
            var useSoulStone = 0;

            for (var star = cookie.Star; star < aftStar; star++)
            {
                var prtCookieStarEnhance = APP.Prt.GetCookieStarEnhancePrt(prtCookie.GradeType, star);
                useSoulStone += prtCookieStarEnhance.SoulStone;
            }

            var req = new CookieEnhanceStarReqPacket(cookieNum, cookie.Star, aftStar, useSoulStone);
            var res = await RpcSystem.RequestAsync<CookieEnhanceStarReqPacket, CookieEnhanceStarResPacket>(req);

            SyncCookie(res.Cookie);
            return res;
        }

        public async Task<CookieEnhanceLvResPacket> RequestCookieEnhanceLv(int cookieNum, int aftLv)
        {
            var prtCookie = APP.Prt.GetCookiePrt(cookieNum);
            var cookie = GetCookieForce(cookieNum);
            var cfgLvCost = 10;

            
            var req = new CookieEnhanceLvReqPacket(cookieNum, cookie.Lv, aftLv, new CostObjPacket { Type = Proto.EObjType.POINT_COOKIE_LV, Num = 0, Amount = cfgLvCost * (aftLv - cookie.Lv) });
            var res = await RpcSystem.RequestAsync<CookieEnhanceLvReqPacket, CookieEnhanceLvResPacket>(req);

            SyncCookie(res.Cookie);
            return res;
        }
    }
}
