using Proto;
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
            Prt = ProtoDb.Get<WorldStageProto>(model.Num);
        }

        public async Task SetStarAsync(int star)
        {
            _model.Star = star;
            await _userRepo.WorldStage.UpdateMdlAsync(_model);
        }
    }
}
