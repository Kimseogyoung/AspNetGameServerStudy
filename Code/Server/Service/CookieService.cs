using AutoMapper;
using Proto;
using Protocol;
using Server.Repo;
using WebStudyServer;
using WebStudyServer.Data;
using WebStudyServer.Helper;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Service;

namespace Server.Service
{
    public class CookieService : ServiceBase
    {
        public CookieService(GlobalDbRepo dbRepo, GameDb db, IMapper mapper, RpcContext rpcContext, ILogger<CookieService> logger) : base(db, rpcContext, logger)
        {
            _dbRepo = dbRepo;
            _mapper = mapper;
        }

        public async Task<CookieEnhanceStarResponsePacket> EnhanceCookieStarAsync(CookieEnhanceStarRequestPacket req)
        {
            var cookieSet = OwnScope.Owned<CookieModel>();
            var cookie = await GetOwnedCookieAsync(cookieSet, req.CookieNum);
            ReqHelper.ValidContext(req.BefStar == cookie.Star, "NOT_EQUAL_COOKIE_STAR", () => new { cookie.Num, req.BefStar, CookieStar = cookie.Star });
            var deltaLv = req.AftStar - req.BefStar;
            ReqHelper.ValidUnderFlowParam(deltaLv, "REQ_COOKIE_ENHANCE_DELTA_STAR");

            var prtCookie = ProtoDb.Get<CookieProto>(cookie.Num);
            var valUsedSoulStone = cookie.GetSoulStoneToEnhanceStar(req.AftStar, prtCookie);
            ReqHelper.ValidContext(req.UsedSoulStone == valUsedSoulStone, "NOT_EQUAL_USED_SOUL_STONE", () => new { cookie.Num, req.UsedSoulStone, ValUsedSoulStone = valUsedSoulStone });

            cookie.EnhanceStar(req.AftStar, valUsedSoulStone, prtCookie);
            await cookieSet.UpdateAsync(cookie);

            return new CookieEnhanceStarResponsePacket
            {
                Cookie = _mapper.Map<CookiePacket>(cookie),
            };
        }

        public async Task<CookieEnhanceLvResponsePacket> EnhanceCookieLvAsync(CookieEnhanceLvRequestPacket req)
        {
            var userScope = OwnScope;
            var cookieSet = userScope.Owned<CookieModel>();
            var cookie = await GetOwnedCookieAsync(cookieSet, req.CookieNum);
            var mgrPlayerDetail = await OwnUser.PlayerDetail.TouchAsync(userScope);
            var cfgLvCost = 10;

            var reason = $"ENHANCE_COOKIE_LV:{req.BefLv}~{req.AftLv}";
            var deltaLv = req.AftLv - req.BefLv;
            ReqHelper.ValidUnderFlowParam(deltaLv, "REQ_COOKIE_ENHANCE_DELTA_LV");
            ReqHelper.ValidContext(req.BefLv == cookie.Lv, "NOT_EQUAL_COOKIE_Lv", () => new { cookie.Num, req.BefLv, CookieLv = cookie.Lv });
            var valCostObj = ReqHelper.ValidCost(req.CostObj, Proto.EObjType.POINT_COOKIE_LV, 0, deltaLv * cfgLvCost, reason);

            var resultCostObj = await mgrPlayerDetail.DecCostAsync(valCostObj, reason);
            cookie.EnhanceLv(req.AftLv);
            await cookieSet.UpdateAsync(cookie);

            return new CookieEnhanceLvResponsePacket
            {
                Cookie = _mapper.Map<CookiePacket>(cookie),
                ChgObj = resultCostObj,
            };
        }

        // 강화는 보유한 쿠키에만 한다. GetOrCreate 로 열면 안 가진 쿠키가 강화 요청만으로 생긴다.
        private static async Task<CookieModel> GetOwnedCookieAsync(OwnedSet<CookieModel> cookieSet, int cookieNum)
        {
            var (found, cookie) = await cookieSet.TryGetAsync(x => x.Num == cookieNum);
            ReqHelper.ValidContext(found && cookie.State == ECookieState.AVAILABLE, "NOT_OWNED_COOKIE",
                () => new { CookieNum = cookieNum });
            return cookie;
        }

        private UserRepo OwnUser => _dbRepo.OwnUser;

        private readonly GlobalDbRepo _dbRepo;
        private readonly IMapper _mapper;
    }
}
