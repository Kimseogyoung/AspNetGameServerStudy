using WebStudyServer.Repo;
using WebStudyServer.Model;
using Proto;
using WebStudyServer.Helper;
using WebStudyServer.GAME;

namespace WebStudyServer.Manager
{
    public partial class CookieManager : UserManagerBase<CookieModel>
    {
        public CookieManager(UserRepo userRepo, CookieModel model) : base(userRepo, model)
        {
        }

        public double IncCookie(int amount, string reason)
        {
            // TODO: IncSoulStone과 다르게 처리
            var befStarExp = _model.SoulStone;
            var befAccStarExp = _model.AccSoulStone;

            _model.SoulStone += amount * 20; // TODO: Cfg
            _model.AccSoulStone += amount * 20;
            _userRepo.Cookie.UpdateMdl(_model);
            return _model.AccSoulStone;
        }

        public double IncSoulStone(int amount, string reason)
        {
            var befStarExp = _model.SoulStone;
            var befAccStarExp = _model.AccSoulStone;

            _model.SoulStone += amount;
            _model.AccSoulStone += amount;
            _userRepo.Cookie.UpdateMdl(_model);
            return _model.AccSoulStone;
        }
    }
}
