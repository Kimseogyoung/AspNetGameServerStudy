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
            if (_model.Num == 0)
            {
                prtNextWorldStage = prtStageList.First();
                return true;
            }

            prtNextWorldStage = prtStageList.FirstOrDefault(x => x.Order > _model.TopFinishStageOrder);
            return prtNextWorldStage != null;
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
            }

            _userRepo.World.UpdateMdl(_model);
        }


        private readonly WorldProto _prt = null;
    }
}
