using AutoMapper;
using Proto;
using Protocol;
using Server.Extension;
using Server.Repo;
using WebStudyServer;
using WebStudyServer.Data;
using WebStudyServer.Helper;
using WebStudyServer.Repo;

namespace Server.Service
{
    public class WorldService : ServiceBase
    {
        public WorldService(GlobalDbRepo dbRepo, GameDb db, IMapper mapper, RpcContext rpcContext, ILogger<WorldService> logger) : base(db, rpcContext, logger)
        {
            _dbRepo = dbRepo;
            _mapper = mapper;
        }

        public async Task<WorldFinishStageFirstResponsePacket> WorldFinishStageFirstAsync(WorldFinishStageFirstRequestPacket req)
        {
            var mgrWorld = await OwnUser.World.TouchAsync(req.WorldNum);
            var mgrWorldStage = await OwnUser.WorldStage.TouchAsync(req.StageNum);
            ReqHelper.ValidContext(mgrWorld.TryGetTopOpenStagePrt(out var prtNextWorldStage), "NOT_FOUND_TOP_OPEN_STAGE", () => new { WorldNum = mgrWorld.Prt.Num, mgrWorld.Model.TopFinishStageNum });
            ReqHelper.ValidContext(prtNextWorldStage.Num == req.StageNum, "NOT_EQUAL_FIRST_FINISH_STAGE", () => new { WorldNum = mgrWorld.Prt.Num, ReqStageNum = req.StageNum, ValStageNum = prtNextWorldStage.Num });
            var isFinishPrevWorld = await mgrWorld.IsFinishPrevWorldAsync();
            ReqHelper.ValidContext(isFinishPrevWorld, "NOT_FINISH_PREV_WORLD", () => new { WorldNum = mgrWorld.Prt.Num });

            // 최초 보상
            var prtRewardList = new List<ObjValue>();
            var firstReward = new ObjValue(mgrWorldStage.Prt.FirstRewardTypeList[0], mgrWorldStage.Prt.FirstRewardNumList[0], mgrWorldStage.Prt.FirstRewardAmountList[0]);
            prtRewardList.AddOrInc(firstReward);

            // Star 보상
            ReqHelper.ValidProto(req.Star <= mgrWorldStage.Prt.FirstRewardTypeList.Count, "TOO_MANY_STAGE_STAR", () => new { req.StageNum, ReqStar = req.Star });
            var valStar = req.Star;
            for (var star = 1; star <= valStar; star++)
            {
                var starReward = new ObjValue(mgrWorldStage.Prt.FirstRewardTypeList[star], mgrWorldStage.Prt.FirstRewardNumList[star], mgrWorldStage.Prt.FirstRewardAmountList[star]);
                prtRewardList.AddOrInc(starReward);
            }

            var reason = $"WORLD_FINISH_STAGE_FIRST:{mgrWorldStage.Num}";
            var valRewardList = ReqHelper.ValidRewardList(req.RewardValueList, prtRewardList, reason);

            // 처리
            var changeList = await RewardService.GrantListAsync(OwnScope, valRewardList, reason);

            await mgrWorld.FinishStageAsync(mgrWorldStage.Prt);
            await mgrWorldStage.SetStarAsync(valStar);

            return new WorldFinishStageFirstResponsePacket
            {
                World = _mapper.Map<WorldPacket>(mgrWorld.Model),
                WorldStage = _mapper.Map<WorldStagePacket>(mgrWorldStage.Model),
                ChgObjList = changeList.ToPacketList(),
            };
        }

        public async Task<WorldFinishStageRepeatResponsePacket> WorldFinishStageRepeatAsync(WorldFinishStageRepeatRequestPacket req)
        {
            var mgrWorld = await OwnUser.World.TouchAsync(req.WorldNum);
            var mgrWorldStage = await OwnUser.WorldStage.TouchAsync(req.StageNum);

            ReqHelper.ValidContext(req.StageNum <= mgrWorld.Model.TopFinishStageNum, "NOT_FINISHED_STAGE", () => new { req.WorldNum, req.StageNum, mgrWorld.Model.TopFinishStageNum });

            // Star 보상
            var prtRewardList = new List<ObjValue>();
            ReqHelper.ValidProto(req.Star <= mgrWorldStage.Prt.FirstRewardTypeList.Count, "TOO_MANY_STAGE_STAR", () => new { req.StageNum, ReqStar = req.Star });
            var valStar = req.Star;
            for (var star = mgrWorldStage.Model.Star + 1; star <= valStar; star++)
            {
                if (star == 0)
                {
                    continue;
                }

                var starReward = new ObjValue(mgrWorldStage.Prt.FirstRewardTypeList[star], mgrWorldStage.Prt.FirstRewardNumList[star], mgrWorldStage.Prt.FirstRewardAmountList[star]);
                prtRewardList.AddOrInc(starReward);
            }

            var reason = $"WORLD_FINISH_STAGE_REPEAT:{mgrWorldStage.Num}";
            var valRewardList = ReqHelper.ValidRewardList(req.RewardValueList, prtRewardList, reason);

            // 처리
            var changeList = await RewardService.GrantListAsync(OwnScope, valRewardList, reason);
            await mgrWorld.FinishStageAsync(mgrWorldStage.Prt);
            await mgrWorldStage.SetStarAsync(valStar);

            return new WorldFinishStageRepeatResponsePacket
            {
                World = _mapper.Map<WorldPacket>(mgrWorld.Model),
                WorldStage = _mapper.Map<WorldStagePacket>(mgrWorldStage.Model),
                ChgObjList = changeList.ToPacketList(),
            };
        }

        public async Task<WorldRewardStarResponsePacket> WorldRewardStarAsync(WorldRewardStarRequestPacket req)
        {
            var mgrWorld = await OwnUser.World.TouchAsync(req.WorldNum);

            var valTotalStar = await OwnUser.WorldStage.GetTotalStarAsync(mgrWorld.Model.Num);
            var maxTotalStar = mgrWorld.Prt.RewardStarList[req.AftRewardStar - 1];
            ReqHelper.ValidContext(maxTotalStar <= valTotalStar, "NOT_ENOUGH_TOTAL_STAR", () => new { WorldNum = mgrWorld.Prt.Num, ValTotalStar = valTotalStar, PrtMaxTotalStar = maxTotalStar });
            ReqHelper.ValidContext(req.BefRewardStar >= mgrWorld.Model.RecvStarReward, "ALREADY_RECV_WORLD_STAR_REWARD", () => new { WorldNum = mgrWorld.Prt.Num, ReqBefStar = req.BefRewardStar });

            var prtReward = new ObjValue(EObjType.FREE_CASH, 0, 0);
            for (var starIdx = req.BefRewardStar; starIdx < req.AftRewardStar; starIdx++)
            {
                var cashAmount = mgrWorld.Prt.RewardStarCashList[starIdx];
                prtReward.Value += cashAmount;
            }

            var reason = $"WORLD_REWARD_STAR:{mgrWorld.Prt.Num}:{req.BefRewardStar}~{req.AftRewardStar}";
            var valReward = ReqHelper.ValidReward(req.RewardValue, prtReward, reason);

            // 처리
            await mgrWorld.RewardStarAsync(req.AftRewardStar, valTotalStar);
            var change = await RewardService.GrantAsync(OwnScope, valReward, reason);

            return new WorldRewardStarResponsePacket
            {
                World = _mapper.Map<WorldPacket>(mgrWorld.Model),
                ChgObj = change.ToPacket()
            };
        }

        private UserRepo OwnUser => _dbRepo.OwnUser;

        private readonly GlobalDbRepo _dbRepo;
        private readonly IMapper _mapper;
    }
}
