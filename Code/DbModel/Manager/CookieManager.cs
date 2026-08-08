using Proto;
using WebStudyServer.Helper;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Manager
{
    public partial class CookieManager : UserManagerBase<CookieModel>
    {
        public CookieManager(UserRepo userRepo, CookieModel model) : base(userRepo, model)
        {
            _prt = ProtoDb.Get<CookieProto>(model.Num);
        }

        public int GetSoulStoneByEnhanceStar(int befStar, int aftStar)
        {
            var needSoulStone = 0;
            for (var star = befStar; star < aftStar; star++)
            {
                var prtCookieStarEnhance = ProtoDb.Get<CookieStarEnhanceProto>((_prt.GradeType, star));
                needSoulStone += prtCookieStarEnhance.SoulStone;
            }

            return needSoulStone;
        }

        public async Task<double> IncCookieAsync(int amount, string reason)
        {
            _ = _model.SoulStone;

            _ = _model.AccSoulStone;

            var soulStoneCnt = amount * _prt.InitSoulStone;
            if (_model.State != ECookieState.AVAILABLE)
            {
                _model.State = ECookieState.AVAILABLE;
                soulStoneCnt -= _prt.InitSoulStone;
            }

            if (soulStoneCnt > 0)
            {
                _model.SoulStone += soulStoneCnt;
                _model.AccSoulStone += soulStoneCnt;
            }

            await _userRepo.Cookie.UpdateMdlAsync(_model);
            return _model.AccSoulStone;
        }

        public async Task<double> IncSoulStoneAsync(int amount, string reason)
        {
            _ = _model.SoulStone;

            _ = _model.AccSoulStone;

            _model.SoulStone += amount;
            _model.AccSoulStone += amount;
            await _userRepo.Cookie.UpdateMdlAsync(_model);
            return _model.AccSoulStone;
        }

        public async Task EnhanceStarAsync(int aftStar, int usedSoulStone)
        {
            _ = _model.Star;
            var befSoulStone = _model.SoulStone;
            ReqHelper.ValidEnough(usedSoulStone, befSoulStone, $"COOKIE_SOUL_STONE:{_prt.Num}", "ENHANCE_STAR");

            _model.Star = aftStar;
            _model.SoulStone -= usedSoulStone;
            await _userRepo.Cookie.UpdateMdlAsync(_model);
        }

        public async Task EnhanceLvAsync(int aftLv)
        {
            _ = _model.Lv;

            _model.Lv = aftLv;
            await _userRepo.Cookie.UpdateMdlAsync(_model);
        }

        private readonly CookieProto _prt = null;
    }
}
