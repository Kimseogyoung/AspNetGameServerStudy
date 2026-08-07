using AutoMapper;
using Proto;
using Protocol;
using Server.Extension;
using Server.Repo;
using WebStudyServer;
using WebStudyServer.Helper;
using WebStudyServer.Repo;
using WebStudyServer.Service;

namespace Server.Service
{
    public class WorldService : ServiceBase
    {
        public WorldService(GlobalDbRepo dbRepo, IMapper mapper, RpcContext rpcContext, ILogger<WorldService> logger) : base(rpcContext, logger)
        {
            _dbRepo = dbRepo;
            _mapper = mapper;
        }

        public WorldFinishStageFirstResponsePacket WorldFinishStageFirst(WorldFinishStageFirstRequestPacket req)
        {
            var mgrWorld = OwnUser.World.Touch(req.WorldNum);
            var mgrWorldStage = OwnUser.WorldStage.Touch(req.StageNum);
            ReqHelper.ValidContext(mgrWorld.TryGetTopOpenStagePrt(out var prtNextWorldStage), "NOT_FOUND_TOP_OPEN_STAGE", () => new { WorldNum = mgrWorld.Prt.Num, mgrWorld.Model.TopFinishStageNum });
            ReqHelper.ValidContext(prtNextWorldStage.Num == req.StageNum, "NOT_EQUAL_FIRST_FINISH_STAGE", () => new { WorldNum = mgrWorld.Prt.Num, ReqStageNum = req.StageNum, ValStageNum = prtNextWorldStage.Num });
            ReqHelper.ValidContext(mgrWorld.IsFinishPrevWorld(), "NOT_FINISH_PREV_WORLD", () => new { WorldNum = mgrWorld.Prt.Num });

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
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();
            var chgObjList = mgrPlayerDetail.IncRewardList(valRewardList, reason);

            mgrWorld.FinishStage(mgrWorldStage.Prt);
            mgrWorldStage.SetStar(valStar);

            return new WorldFinishStageFirstResponsePacket
            {
                World = _mapper.Map<WorldPacket>(mgrWorld.Model),
                WorldStage = _mapper.Map<WorldStagePacket>(mgrWorldStage.Model),
                ChgObjList = chgObjList,
            };
        }

        public WorldFinishStageRepeatResponsePacket WorldFinishStageRepeat(WorldFinishStageRepeatRequestPacket req)
        {
            var mgrWorld = OwnUser.World.Touch(req.WorldNum);
            var mgrWorldStage = OwnUser.WorldStage.Touch(req.StageNum);

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
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();
            var chgObjList = mgrPlayerDetail.IncRewardList(valRewardList, reason);
            mgrWorld.FinishStage(mgrWorldStage.Prt);
            mgrWorldStage.SetStar(valStar);

            return new WorldFinishStageRepeatResponsePacket
            {
                World = _mapper.Map<WorldPacket>(mgrWorld.Model),
                WorldStage = _mapper.Map<WorldStagePacket>(mgrWorldStage.Model),
                ChgObjList = chgObjList,
            };
        }

        public WorldRewardStarResponsePacket WorldRewardStar(WorldRewardStarRequestPacket req)
        {
            var mgrWorld = OwnUser.World.Touch(req.WorldNum);

            var valTotalStar = OwnUser.WorldStage.GetTotalStar(mgrWorld.Model.Num);
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
            var mgrPlayerDetail = OwnUser.PlayerDetail.Touch();
            mgrWorld.RewardStar(req.AftRewardStar, valTotalStar);
            var chgObj = mgrPlayerDetail.IncReward(valReward, reason);

            return new WorldRewardStarResponsePacket
            {
                World = _mapper.Map<WorldPacket>(mgrWorld.Model),
                ChgObj = chgObj
            };
        }

        private UserRepo OwnUser => _dbRepo.OwnUser;

        private readonly GlobalDbRepo _dbRepo;
        private readonly IMapper _mapper;
    }
}
