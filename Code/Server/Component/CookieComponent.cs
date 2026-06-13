using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.GAME;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class CookieComponent : UserComponentBase<CookieModel>
    {
        public CookieComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(CookieModel model) => CacheKey.For<CookieModel>(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For<CookieModel>(playerId);

        public CookieManager Touch(int cookieNum)
        {
            if (!TryGetInternal(cookieNum, out var mdlCookie))
            {
                mdlCookie = CreateMdl(new CookieModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = cookieNum,
                    Lv = DEF.DEFAULT_LV,
                    SkillLv = DEF.DEFAULT_LV,
                });
            }

            return new CookieManager(_userRepo, mdlCookie);
        }

        public CookieManager TouchBySoulStone(int soulStoneNum)
        {
            var prt = APP.Prt.GetCookieSoulStonePrt(soulStoneNum);
            return Touch(prt.CookieNum);
        }

        public bool TryGetInternal(int num, out CookieModel outCookie)
        {
            outCookie = GetMdl(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
            return outCookie != null;
        }
    }
}
