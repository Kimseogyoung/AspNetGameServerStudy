using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;

namespace WebStudyServer.Manager
{
    public partial class KingdomTileMapManager : UserManagerBase<KingdomTileMapModel>
    {
        public KingdomTileMapManager(UserRepo userRepo, KingdomTileMapModel model) : base(userRepo, model)
        {
        }
    }
}
