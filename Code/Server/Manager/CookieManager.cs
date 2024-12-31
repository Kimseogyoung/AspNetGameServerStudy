using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;

namespace WebStudyServer.Manager
{
    public partial class CookieManager : UserManagerBase<CookieModel>
    {
        public CookieManager(UserRepo userRepo, CookieModel model) : base(userRepo, model)
        {
        }

        public double IncStarExp(int amount)
        {
            var befStarExp = _model.StarExp;
            var befAccStarExp = _model.AccStarExp;

            _model.StarExp += amount;
            _model.AccStarExp += amount;
            _userRepo.Cookie.Update(_model);
            return _model.AccStarExp;
        }
    }
}
