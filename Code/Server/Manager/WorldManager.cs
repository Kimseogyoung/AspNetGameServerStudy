using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;

namespace WebStudyServer.Manager
{
    public partial class WorldManager : UserManagerBase<WorldModel>
    {
        public WorldManager(UserRepo userRepo, WorldModel model) : base(userRepo, model)
        {
            _prt = APP.Prt.GetWorldPrt(model.Num);
        }

       
        private readonly WorldProto _prt = null;
    }
}
