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

        public int GetSoulStoneByEnhanceStar(int befStar, int aftStar)
        {
            var needSoulStone = 0;
            for (var star = befStar; star < aftStar; star++)
            {
                var prtCookieStarEnhance = APP.Prt.GetCookieStarEnhancePrt(_prt.GradeType, star);
                needSoulStone += prtCookieStarEnhance.SoulStone;
            }

            return needSoulStone;
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

        public void EnhanceStar(int aftStar, int usedSoulStone)
        {
            var befStar = _model.Star;
            var befSoulStone = _model.SoulStone;
            ReqHelper.ValidContext(befSoulStone >= usedSoulStone, "NOT_ENOUGH_COOKIE_SOUL_STONE", () => new { CookeNum = _prt.Num, SoulStone = befSoulStone, UsedSoulStone = usedSoulStone});

            _model.Star = aftStar;
            _model.SoulStone -= usedSoulStone;
            _userRepo.Cookie.UpdateMdl(_model);
        }

        public void EnhanceLv(int aftLv)
        {
            var befLv = _model.Lv;

            _model.Lv = aftLv;
            _userRepo.Cookie.UpdateMdl(_model);
        }

        private readonly CookieProto _prt = null;
    }
}
