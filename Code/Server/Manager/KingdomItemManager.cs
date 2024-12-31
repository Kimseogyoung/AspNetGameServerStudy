using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;

namespace WebStudyServer.Manager
{
    public partial class KingdomItemManager : UserManagerBase<KingdomItemModel>
    {
        public KingdomItemManager(UserRepo userRepo, KingdomItemModel model) : base(userRepo, model)
        {
        }
    }
}
