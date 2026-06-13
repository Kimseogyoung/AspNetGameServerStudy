using Proto;
using WebStudyServer.GAME;
using WebStudyServer.Helper;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Manager
{
    public partial class WorldStageManager : UserManagerBase<WorldStageModel>
    {
        public int Num => Prt.Num;
        public WorldStageProto Prt { get; } = null;

        public WorldStageManager(UserRepo userRepo, WorldStageModel model) : base(userRepo, model)
        {
            Prt = APP.Prt.GetWorldStagePrt(model.Num);
        }

        public void SetStar(int star)
        {
            _model.Star = star;
            _userRepo.WorldStage.UpdateMdl(_model);
        }
    }
}
