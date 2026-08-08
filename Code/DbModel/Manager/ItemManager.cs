using WebStudyServer.Helper;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Manager
{
    public partial class ItemManager : UserManagerBase<ItemModel>
    {
        public ItemManager(UserRepo userRepo, ItemModel model) : base(userRepo, model)
        {
        }

        public async Task<double> DecAmountAsync(double amount, string reason)
        {
            var befAmount = _model.Amount;

            _ = _model.AccAmount;
            ReqHelper.ValidEnough(amount, befAmount, $"ITEM_{_model.Num}", reason);

            _model.Amount -= amount;
            _model.AccAmount -= amount;
            await _userRepo.Item.UpdateMdlAsync(_model);
            return _model.Amount;
        }

        public async Task<double> IncAmountAsync(double amount, string reason)
        {
            _ = _model.Amount;

            _ = _model.AccAmount;

            _model.Amount += amount;
            _model.AccAmount += amount;
            await _userRepo.Item.UpdateMdlAsync(_model);
            return _model.Amount;
        }
    }
}
