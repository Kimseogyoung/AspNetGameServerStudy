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
            _prt = APP.Prt.GetCookiePrt(model.Num);
        }

        public double IncCookie(int amount, string reason)
        {
            var befStarExp = _model.SoulStone;
            var befAccStarExp = _model.AccSoulStone;

            var soulStoneCnt = amount * _prt.InitSoulStone;
            if (_model.State != ECookieState.AVAILABLE)
            {
                _model.State = ECookieState.AVAILABLE;
                soulStoneCnt -= _prt.InitSoulStone;
            }

            if(soulStoneCnt > 0)
            {
                _model.SoulStone += soulStoneCnt; 
                _model.AccSoulStone += soulStoneCnt;
            }

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

        private readonly CookieProto _prt = null;
    }
}
