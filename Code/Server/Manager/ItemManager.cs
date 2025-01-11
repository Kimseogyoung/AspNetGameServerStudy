using WebStudyServer.Repo;
using WebStudyServer.Model;

namespace WebStudyServer.Manager
{
    public partial class ItemManager : UserManagerBase<ItemModel>
    {
        public ItemManager(UserRepo userRepo, ItemModel model) : base(userRepo, model)
        {
        }

        public double DecAmount(double amount, string reason)
        {
            var befAmount = _model.Amount;
            var befAccAmount = _model.AccAmount;

            _model.Amount -= amount;
            _model.AccAmount -= amount;
            _userRepo.Item.UpdateMdl(_model);
            return _model.Amount;
        }

        public double IncAmount(double amount, string reason)
        {
            var befAmount = _model.Amount;
            var befAccAmount = _model.AccAmount;

            _model.Amount += amount;
            _model.AccAmount += amount;
            _userRepo.Item.UpdateMdl(_model);
            return _model.Amount;
        }
    }
}
