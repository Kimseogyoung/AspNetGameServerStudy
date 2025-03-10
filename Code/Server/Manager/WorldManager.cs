using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;

namespace WebStudyServer.Manager
{
    public partial class WorldManager : UserManagerBase<WorldModel>
    {
        public WorldProto Prt => _prt;

        public WorldManager(UserRepo userRepo, WorldModel model) : base(userRepo, model)
        {
            _prt = APP.Prt.GetWorldPrt(model.Num);
        }

        public WorldStageProto GetTopOpenStagePrt()
        {
            var prt = APP.Prt.GetWorldStagePrt(1);
            return prt;
        }

        public void RewardStar(int star)
        {
            // TODO: FLAG Helper 구현
            _model.Flag = 1;
            _userRepo.World.UpdateMdl(_model);
        }

        private readonly WorldProto _prt = null;
    }
}
