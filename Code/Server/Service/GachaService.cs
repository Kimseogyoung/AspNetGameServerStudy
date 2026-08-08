using AutoMapper;
using Proto;
using Protocol;
using Server.Extension;
using Server.Helper;
using Server.Repo;
using ServerCore;
using WebStudyServer;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Service;

namespace Server.Service
{
    public class GachaService : ServiceBase
    {
        public GachaService(GlobalDbRepo dbRepo, IMapper mapper, RpcContext rpcContext, ILogger<GachaService> logger) : base(rpcContext, logger)
        {
            _dbRepo = dbRepo;
            _mapper = mapper;
        }

        public ScheduleLoadResponsePacket LoadSchedule(ScheduleLoadRequestPacket req)
        {
            var centerRepo = _dbRepo.Center;
            var mgrScheduleList = centerRepo.Schedule.GetList();
            return new ScheduleLoadResponsePacket
            {
                ScheduleList = _mapper.Map<List<SchedulePacket>>(mgrScheduleList),
            };
        }

        public async Task<GachaNormalResponsePacket> GachaNormalAsync(GachaNormalRequestPacket req)
        {
            var centerRepo = _dbRepo.Center;
            var scheduleMgr = centerRepo.Schedule.Get(req.ScheduleNum, EScheduleTimeType.TOTAL);
            var mgrPlayerDetail = await OwnUser.PlayerDetail.TouchAsync();

            // Cost일치하는지 체크
            var valCnt = scheduleMgr.ValidGachaCnt(req.Cnt);
            var valCost = scheduleMgr.ValidGachaCost(req.CostObj, valCnt);

            // 재화 소모
            var resultCostObj = await mgrPlayerDetail.DecCostAsync(valCost, scheduleMgr.MakeGachaReason(valCnt));

            var gachaRandom = new GachaRandom(scheduleMgr.GachaPrt, RpcContext.ServerTime);
            var rewardObjValList = new List<ObjValue>();
            var gachaResultList = new List<GachaResultPacket>();
            for (var i = 0; i < valCnt; i++)
            {
                var resultObjValue = gachaRandom.Roll(isNormal: true);
                rewardObjValList.AddOrInc(resultObjValue);

                GachaResultPacket gachaResult;
                switch (resultObjValue.Key.Type)
                {
                    case EObjType.COOKIE:
                        var prtCookie = ProtoDb.Get<CookieProto>(resultObjValue.Key.Num);
                        gachaResult = new GachaResultPacket
                        {
                            ResultObjValue = resultObjValue,
                            SoulStoneNum = prtCookie.SoulStoneNum,
                            SoulStoneAmount = prtCookie.InitSoulStone * (int)resultObjValue.Value
                        };
                        break;
                    case EObjType.SOUL_STONE:
                        gachaResult = new GachaResultPacket
                        {
                            ResultObjValue = resultObjValue,
                            SoulStoneNum = resultObjValue.Key.Num,
                            SoulStoneAmount = (int)resultObjValue.Value
                        };
                        break;
                    default:
                        throw new GameException(EErrorCode.NO_HANDLING_ERROR, "NO_HANDLING_GACHA_RESULT", new { ObjType = resultObjValue.Key.Type });
                }
                gachaResultList.Add(gachaResult);
            }

            // TODO: 가챠 전용 Inc로 ㄱㄱ
            var chgObjList = await mgrPlayerDetail.IncRewardListAsync(rewardObjValList, scheduleMgr.MakeGachaReason(valCnt));

            return new GachaNormalResponsePacket
            {
                CostChgObj = resultCostObj,
                GachaResultChgObjList = chgObjList,
                GachaResultList = gachaResultList,
            };
        }

        private UserRepo OwnUser => _dbRepo.OwnUser;

        private readonly GlobalDbRepo _dbRepo;
        private readonly IMapper _mapper;
    }
}
