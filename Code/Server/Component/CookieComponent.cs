using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.GAME;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Component
{
    public class CookieComponent : UserComponentBase<CookieModel>
    {
        public static class Key
        {
            public static CacheKey Single(ulong playerId, int num) => CacheKey.For<CookieModel>(playerId, num);
            public static CacheKey List(ulong playerId) => CacheKey.For<CookieModel>(playerId);
        }

        public CookieComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(CookieModel model) => Key.Single(model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => Key.List(playerId);

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
            outCookie = GetMdl(
                Key.Single(RpcCtx.PlayerId, num),
                db => db.SelectByPk<CookieModel>(new { RpcCtx.PlayerId, Num = num }));
            return outCookie != null;
        }
    }
}
