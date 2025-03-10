using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;

namespace WebStudyServer.Manager
{
    public partial class WorldStageManager : UserManagerBase<WorldStageModel>
    {
        public WorldStageManager(UserRepo userRepo, WorldStageModel model) : base(userRepo, model)
        {
            _prt = APP.Prt.GetWorldStagePrt(model.Num);
        }


        private readonly WorldStageProto _prt = null;
    }
}
