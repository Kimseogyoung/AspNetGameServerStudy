using Proto;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class CookieComponent : UserComponentBase<CookieModel>
    {
        public CookieComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(CookieModel model) => CacheKey.For(CacheKeyTags.CookieModel, model.PlayerId, model.Num);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For(CacheKeyTags.CookieModel, playerId);

        public async Task<CookieManager> TouchAsync(int cookieNum)
        {
            var mdlCookie = await TryGetInternalAsync(cookieNum);
            if (mdlCookie == null)
            {
                mdlCookie = await CreateMdlAsync(new CookieModel
                {
                    PlayerId = _userRepo.RpcContext.PlayerId,
                    Num = cookieNum,
                    Lv = DEF.DEFAULT_LV,
                    SkillLv = DEF.DEFAULT_LV,
                });
            }

            return new CookieManager(_userRepo, mdlCookie);
        }

        public Task<CookieManager> TouchBySoulStoneAsync(int soulStoneNum)
        {
            var prt = ProtoDb.Get<CookieSoulStoneProto>(soulStoneNum);
            return TouchAsync(prt.CookieNum);
        }

        public Task<CookieModel?> TryGetInternalAsync(int num)
        {
            return GetMdlAsync(x => x.PlayerId == RpcCtx.PlayerId && x.Num == num);
        }
    }
}
