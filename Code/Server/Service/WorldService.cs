using AutoMapper;
using Proto;
using Protocol;
using Server.Extension;
using WebStudyServer;
using WebStudyServer.Data;
using WebStudyServer.Data.Queries;
using WebStudyServer.Helper;
using WebStudyServer.Model;

namespace Server.Service
{
    public class WorldService : ServiceBase
    {
        public WorldService(GameDb db, IMapper mapper, RpcContext rpcContext, ILogger<WorldService> logger) : base(db, rpcContext, logger)
        {
            _mapper = mapper;
        }

        public async Task<WorldFinishStageFirstResponsePacket> WorldFinishStageFirstAsync(WorldFinishStageFirstRequestPacket req)
        {
            var userScope = OwnScope;
            var worldSet = userScope.Owned<WorldModel>();
            var stageSet = userScope.Owned<WorldStageModel>();

            var mdlWorld = await worldSet.GetOrCreateAsync(req.WorldNum);
            var prtWorld = ProtoDb.Get<WorldProto>(req.WorldNum);
            var prtStage = ProtoDb.Get<WorldStageProto>(req.StageNum);
            var mdlWorldStage = await stageSet.GetOrCreateAsync(req.StageNum, prtStage.WorldNum);

            ReqHelper.ValidContext(mdlWorld.TryGetTopOpenStagePrt(out var prtNextWorldStage), "NOT_FOUND_TOP_OPEN_STAGE", () => new { WorldNum = prtWorld.Num, mdlWorld.TopFinishStageNum });
            ReqHelper.ValidContext(prtNextWorldStage.Num == req.StageNum, "NOT_EQUAL_FIRST_FINISH_STAGE", () => new { WorldNum = prtWorld.Num, ReqStageNum = req.StageNum, ValStageNum = prtNextWorldStage.Num });
            var isFinishPrevWorld = await worldSet.IsFinishPrevWorldAsync(prtWorld);
            ReqHelper.ValidContext(isFinishPrevWorld, "NOT_FINISH_PREV_WORLD", () => new { WorldNum = prtWorld.Num });

            // 최초 보상
            var prtRewardList = new List<ObjValue>();
            prtRewardList.AddOrInc(MakeStarReward(prtStage, 0));

            // Star 보상
            // 0성 보상까지 리스트에 있으므로 유효한 별은 0 ~ Count-1 이다. <= 로 두면 마지막 인덱스를 넘긴다.
            ReqHelper.ValidProto(req.Star < prtStage.FirstRewardTypeList.Count, "TOO_MANY_STAGE_STAR", () => new { req.StageNum, ReqStar = req.Star });
            var valStar = req.Star;
            for (var star = 1; star <= valStar; star++)
            {
                prtRewardList.AddOrInc(MakeStarReward(prtStage, star));
            }

            var reason = $"WORLD_FINISH_STAGE_FIRST:{prtStage.Num}";
            var valRewardList = ReqHelper.ValidRewardList(req.RewardValueList, prtRewardList, reason);

            // 처리
            var changeList = await RewardService.GrantListAsync(userScope, valRewardList, reason);

            mdlWorld.FinishStage(prtStage);
            await worldSet.UpdateAsync(mdlWorld);

            mdlWorldStage.Star = valStar;
            await stageSet.UpdateAsync(mdlWorldStage);

            return new WorldFinishStageFirstResponsePacket
            {
                World = _mapper.Map<WorldPacket>(mdlWorld),
                WorldStage = _mapper.Map<WorldStagePacket>(mdlWorldStage),
                ChgObjList = changeList.ToPacketList(),
            };
        }

        public async Task<WorldFinishStageRepeatResponsePacket> WorldFinishStageRepeatAsync(WorldFinishStageRepeatRequestPacket req)
        {
            var userScope = OwnScope;
            var worldSet = userScope.Owned<WorldModel>();
            var stageSet = userScope.Owned<WorldStageModel>();

            var mdlWorld = await worldSet.GetOrCreateAsync(req.WorldNum);
            var prtStage = ProtoDb.Get<WorldStageProto>(req.StageNum);
            var mdlWorldStage = await stageSet.GetOrCreateAsync(req.StageNum, prtStage.WorldNum);

            ReqHelper.ValidContext(req.StageNum <= mdlWorld.TopFinishStageNum, "NOT_FINISHED_STAGE", () => new { req.WorldNum, req.StageNum, mdlWorld.TopFinishStageNum });

            // Star 보상. 이미 받은 별 다음부터만 준다.
            var prtRewardList = new List<ObjValue>();
            // 0성 보상까지 리스트에 있으므로 유효한 별은 0 ~ Count-1 이다. <= 로 두면 마지막 인덱스를 넘긴다.
            ReqHelper.ValidProto(req.Star < prtStage.FirstRewardTypeList.Count, "TOO_MANY_STAGE_STAR", () => new { req.StageNum, ReqStar = req.Star });
            var valStar = req.Star;
            for (var star = mdlWorldStage.Star + 1; star <= valStar; star++)
            {
                if (star == 0)
                {
                    continue;
                }

                prtRewardList.AddOrInc(MakeStarReward(prtStage, star));
            }

            var reason = $"WORLD_FINISH_STAGE_REPEAT:{prtStage.Num}";
            var valRewardList = ReqHelper.ValidRewardList(req.RewardValueList, prtRewardList, reason);

            // 처리
            var changeList = await RewardService.GrantListAsync(userScope, valRewardList, reason);

            mdlWorld.FinishStage(prtStage);
            await worldSet.UpdateAsync(mdlWorld);

            mdlWorldStage.Star = valStar;
            await stageSet.UpdateAsync(mdlWorldStage);

            return new WorldFinishStageRepeatResponsePacket
            {
                World = _mapper.Map<WorldPacket>(mdlWorld),
                WorldStage = _mapper.Map<WorldStagePacket>(mdlWorldStage),
                ChgObjList = changeList.ToPacketList(),
            };
        }

        public async Task<WorldRewardStarResponsePacket> WorldRewardStarAsync(WorldRewardStarRequestPacket req)
        {
            var userScope = OwnScope;
            var worldSet = userScope.Owned<WorldModel>();

            var mdlWorld = await worldSet.GetOrCreateAsync(req.WorldNum);
            var prtWorld = ProtoDb.Get<WorldProto>(req.WorldNum);

            var valTotalStar = await userScope.Owned<WorldStageModel>().GetTotalStarAsync(req.WorldNum);
            var maxTotalStar = prtWorld.RewardStarList[req.AftRewardStar - 1];
            ReqHelper.ValidContext(maxTotalStar <= valTotalStar, "NOT_ENOUGH_TOTAL_STAR", () => new { WorldNum = prtWorld.Num, ValTotalStar = valTotalStar, PrtMaxTotalStar = maxTotalStar });
            ReqHelper.ValidContext(req.BefRewardStar >= mdlWorld.RecvStarReward, "ALREADY_RECV_WORLD_STAR_REWARD", () => new { WorldNum = prtWorld.Num, ReqBefStar = req.BefRewardStar });

            var prtReward = new ObjValue(EObjType.FREE_CASH, 0, 0);
            for (var starIdx = req.BefRewardStar; starIdx < req.AftRewardStar; starIdx++)
            {
                prtReward.Value += prtWorld.RewardStarCashList[starIdx];
            }

            var reason = $"WORLD_REWARD_STAR:{prtWorld.Num}:{req.BefRewardStar}~{req.AftRewardStar}";
            var valReward = ReqHelper.ValidReward(req.RewardValue, prtReward, reason);

            // 처리
            mdlWorld.RewardStar(req.AftRewardStar);
            await worldSet.UpdateAsync(mdlWorld);

            var changeList = await RewardService.GrantAsync(userScope, valReward, reason);

            return new WorldRewardStarResponsePacket
            {
                World = _mapper.Map<WorldPacket>(mdlWorld),
                ChgObjList = changeList.ToPacketList(),
            };
        }

        private static ObjValue MakeStarReward(WorldStageProto prtStage, int star)
        {
            return new ObjValue(prtStage.FirstRewardTypeList[star], prtStage.FirstRewardNumList[star], prtStage.FirstRewardAmountList[star]);
        }

        private readonly IMapper _mapper;
    }
}
