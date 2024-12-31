using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;

namespace WebStudyServer.Manager
{
    public partial class PointManager : UserManagerBase<PointModel>
    {
        public double Amount => _model.Amount;
        public double AccAmount => _model.AccAmount;
        public PointManager(UserRepo userRepo, PointModel model) : base(userRepo, model)
        {
        }

        public double DecAmount(double amount)
        {
            var befAmount = _model.Amount;
            var befAccAmount= _model.AccAmount;

            _model.Amount -= amount;
            _model.AccAmount -= amount;
            _userRepo.Point.Update(_model);
            return _model.Amount;
        }

        public double IncAmount(double amount)
        {
            var befAmount = _model.Amount;
            var befAccAmount = _model.AccAmount;

            _model.Amount += amount;
            _model.AccAmount += amount;
            _userRepo.Point.Update(_model);
            return _model.Amount;
        }

    }
}
