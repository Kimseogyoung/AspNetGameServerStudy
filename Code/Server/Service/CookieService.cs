using AutoMapper;
using Protocol;
using Server.Repo;
using WebStudyServer;
using WebStudyServer.Helper;
using WebStudyServer.Repo;
using WebStudyServer.Service;

namespace Server.Service
{
    public class CookieService : ServiceBase
    {
        public CookieService(GlobalDbRepo dbRepo, IMapper mapper, RpcContext rpcContext, ILogger<CookieService> logger) : base(rpcContext, logger)
        {
            _dbRepo = dbRepo;
            _mapper = mapper;
        }

        public CookieEnhanceStarResponsePacket EnhanceCookieStar(CookieEnhanceStarRequestPacket req)
        {
            var mgrCookie = OwnUser.Cookie.Touch(req.CookieNum);
            ReqHelper.ValidContext(req.BefStar == mgrCookie.Model.Star, "NOT_EQUAL_COOKIE_STAR", () => new { CookieNum = mgrCookie.Model.Num, req.BefStar, CookieStar = mgrCookie.Model.Star });
            var deltaLv = req.AftStar - req.BefStar;
            ReqHelper.ValidUnderFlowParam(deltaLv, "REQ_COOKIE_ENHANCE_DELTA_STAR");

            var valUsedSoulStone = mgrCookie.GetSoulStoneByEnhanceStar(mgrCookie.Model.Star, req.AftStar);
            ReqHelper.ValidContext(req.UsedSoulStone == valUsedSoulStone, "NOT_EQUAL_USED_SOUL_STONE", () => new { CookieNum = mgrCookie.Model.Num, req.UsedSoulStone, ValUsedSoulStone = valUsedSoulStone });

            mgrCookie.EnhanceStar(req.AftStar, valUsedSoulStone);

            return new CookieEnhanceStarResponsePacket
            {
                Cookie = _mapper.Map<CookiePacket>(mgrCookie.Model),
            };
        }

        public CookieEnhanceLvResponsePacket EnhanceCookieLv(CookieEnhanceLvRequestPacket req)
        {
            var mgrCookie = OwnUser.Cookie.Touch(req.CookieNum);
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();
            var cfgLvCost = 10;

            var reason = $"ENHANCE_COOKIE_LV:{req.BefLv}~{req.AftLv}";
            var deltaLv = req.AftLv - req.BefLv;
            ReqHelper.ValidUnderFlowParam(deltaLv, "REQ_COOKIE_ENHANCE_DELTA_LV");
            ReqHelper.ValidContext(req.BefLv == mgrCookie.Model.Lv, "NOT_EQUAL_COOKIE_Lv", () => new { CookieNum = mgrCookie.Model.Num, req.BefLv, CookieLv = mgrCookie.Model.Lv });
            var valCostObj = ReqHelper.ValidCost(req.CostObj, Proto.EObjType.POINT_COOKIE_LV, 0, deltaLv * cfgLvCost, reason);

            var resultCostObj = mgrPlayerDetail.DecCost(valCostObj, reason);
            mgrCookie.EnhanceLv(req.AftLv);

            return new CookieEnhanceLvResponsePacket
            {
                Cookie = _mapper.Map<CookiePacket>(mgrCookie.Model),
                ChgObj = resultCostObj,
            };
        }

        private UserRepo OwnUser => _dbRepo.OwnUser;

        private readonly GlobalDbRepo _dbRepo;
        private readonly IMapper _mapper;
    }
}
