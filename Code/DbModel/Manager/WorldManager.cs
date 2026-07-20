using Proto;
using Protocol;
using Server.Extension;
using WebStudyServer.Helper;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Manager
{
    public partial class WorldManager : UserManagerBase<WorldModel>
    {
        public WorldProto Prt { get; } = null;

        public WorldManager(UserRepo userRepo, WorldModel model) : base(userRepo, model)
        {
            Prt = ProtoDb.Get<WorldProto>(model.Num);
        }

        public bool TryGetTopOpenStagePrt(out WorldStageProto prtNextWorldStage)
        {
            var worldNum = _model.Num;

            var prtStageList = ProtoDb.GetByMk<WorldStageProto>(worldNum);
            prtNextWorldStage = prtStageList.FirstOrDefault(x => x.Order > _model.TopFinishStageOrder);
            return prtNextWorldStage != null;
        }

        public bool IsFinishPrevWorld()
        {
            var worldNum = _model.Num;

            var prtWorldList = ProtoDb.GetByMk<WorldProto>(Prt.Type);
            var prtPrevWorld = prtWorldList.LastOrDefault(x => x.Order < Prt.Order);

            if (prtPrevWorld == null)
            {
                // 첫번째 월드인 경우
                return true;
            }

            if (!_userRepo.World.TryGetInternal(prtPrevWorld.Num, out var outWorldMdl))
            {
                return false;
            }

            return outWorldMdl.State == FinishState; // FINISH STATE
        }

        public void RewardStar(int valAftStar, int valTotalStar)
        {
            _ = _model.RecvStarReward;
            _model.RecvStarReward = valAftStar;
            _userRepo.World.UpdateMdl(_model);
        }

        public void FinishStage(WorldStageProto prtStage)
        {
            _model.LastPlayStageNum = prtStage.Num;

            if (_model.TopFinishStageOrder < prtStage.Order)
            {
                _model.TopFinishStageOrder = prtStage.Order;
                _model.TopFinishStageNum = prtStage.Num;

                // 끝난경우 상태 변경
                var prtLastStage = ProtoDb.GetByMk<WorldStageProto>(Prt.Num).Last();
                if (prtLastStage.Num == prtStage.Num)
                {
                    _model.State = FinishState;
                }
            }

            _userRepo.World.UpdateMdl(_model);
        }

        private const int FinishState = 10;
    }
}
