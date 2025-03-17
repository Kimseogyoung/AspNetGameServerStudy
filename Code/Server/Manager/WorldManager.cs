using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;
using Protocol;
using Server.Extension;

namespace WebStudyServer.Manager
{
    public partial class WorldManager : UserManagerBase<WorldModel>
    {
        public WorldProto Prt => _prt;

        public WorldManager(UserRepo userRepo, WorldModel model) : base(userRepo, model)
        {
            _prt = APP.Prt.GetWorldPrt(model.Num);
        }

        public bool TryGetTopOpenStagePrt(out WorldStageProto prtNextWorldStage)
        {
            var worldNum = _model.Num;

            var prtStageList = APP.Prt.GetWorldStagePrtListByMk(worldNum);
            prtNextWorldStage = prtStageList.FirstOrDefault(x => x.Order > _model.TopFinishStageOrder);
            return prtNextWorldStage != null;
        }

        public bool IsFinishPrevWorld()
        {
            var worldNum = _model.Num;

            var prtWorldList = APP.Prt.GetWorldPrtListByMk(_prt.Type);
            var prtPrevWorld = prtWorldList.LastOrDefault(x => x.Order < _prt.Order);

            if (prtPrevWorld == null)
            {
                // 첫번째 월드인 경우
                return true;
            }

            if (!_userRepo.World.TryGetInternal(prtPrevWorld.Num, out var outWorldMdl))
            {
                return false;
            }

            return outWorldMdl.State == c_finishState; // FINISH STATE
        }

        public void RewardStar(int valAftStar, int valTotalStar)
        {
            var befRecvStarReward = _model.RecvStarReward;
            _model.RecvStarReward = valAftStar;
            _userRepo.World.UpdateMdl(_model);
        }

        public void FinishStage(WorldStageProto prtStage)
        {
            _model.LastPlayStageNum = prtStage.Num;
            
            if(_model.TopFinishStageOrder < prtStage.Order)
            {
                _model.TopFinishStageOrder = prtStage.Order;
                _model.TopFinishStageNum = prtStage.Num;

                // 끝난경우 상태 변경
                var prtLastStage = APP.Prt.GetWorldStagePrtListByMk(_prt.Num).Last();
                if (prtLastStage.Num == prtStage.Num)
                {
                    _model.State = c_finishState;
                }
            }

            _userRepo.World.UpdateMdl(_model);
        }

        private const int c_finishState = 10;
        private readonly WorldProto _prt = null;
    }
}
