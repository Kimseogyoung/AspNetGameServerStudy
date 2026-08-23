using AutoMapper;
using Proto;
using Protocol;
using Server.Extension;
using Server.Helper;
using Server.Repo;
using ServerCore;
using WebStudyServer;
using WebStudyServer.Data;

namespace Server.Service
{
    public class GachaService : ServiceBase
    {
        public GachaService(GlobalDbRepo dbRepo, GameDb db, IMapper mapper, RpcContext rpcContext, ILogger<GachaService> logger) : base(db, rpcContext, logger)
        {
            _dbRepo = dbRepo;
            _mapper = mapper;
        }

        public async Task<ScheduleLoadResponsePacket> LoadScheduleAsync(ScheduleLoadRequestPacket req)
        {
            var centerRepo = _dbRepo.Center;
            var mgrScheduleList = await centerRepo.Schedule.GetListAsync();
            return new ScheduleLoadResponsePacket
            {
                ScheduleList = _mapper.Map<List<SchedulePacket>>(mgrScheduleList),
            };
        }

        public async Task<GachaNormalResponsePacket> GachaNormalAsync(GachaNormalRequestPacket req)
        {
            var centerRepo = _dbRepo.Center;
            var scheduleMgr = await centerRepo.Schedule.GetAsync(req.ScheduleNum, EScheduleTimeType.TOTAL);

            // Cost일치하는지 체크
            var valCnt = scheduleMgr.ValidGachaCnt(req.Cnt);
            var valCost = scheduleMgr.ValidGachaCost(req.CostObj, valCnt);

            // 재화 소모
            var costChange = await RewardService.PayAsync(OwnScope, valCost, scheduleMgr.MakeGachaReason(valCnt));

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
            var changeList = await RewardService.GrantListAsync(OwnScope, rewardObjValList, scheduleMgr.MakeGachaReason(valCnt));

            return new GachaNormalResponsePacket
            {
                CostChgObj = costChange.ToPacket(),
                GachaResultChgObjList = changeList.ToPacketList(),
                GachaResultList = gachaResultList,
            };
        }

        private readonly GlobalDbRepo _dbRepo;
        private readonly IMapper _mapper;
    }
}
