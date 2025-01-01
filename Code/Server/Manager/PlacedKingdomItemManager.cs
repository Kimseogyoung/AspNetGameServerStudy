using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.GAME;

namespace WebStudyServer.Manager
{
    public partial class PlacedKingdomItemManager : UserManagerBase<PlacedKingdomItemModel>
    {
        public KingdomItemProto Prt { get; private set; }
        public PlacedKingdomItemManager(UserRepo userRepo, PlacedKingdomItemModel model, KingdomItemProto prt) : base(userRepo, model)
        {
            Prt = prt;
        }

        public PlacedKingdomItemManager(UserRepo userRepo, PlacedKingdomItemModel model) : base(userRepo, model)
        {
            Prt = APP.Prt.GetKingdomItemPrt(model.Num);
        }
    }
}
