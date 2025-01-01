using WebStudyServer.Repo;
using WebStudyServer.Model;

namespace WebStudyServer.Manager
{
    public partial class ItemManager : UserManagerBase<ItemModel>
    {
        public ItemManager(UserRepo userRepo, ItemModel model) : base(userRepo, model)
        {
        }
    }
}
