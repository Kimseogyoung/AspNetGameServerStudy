using Server.Repo.Database;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public abstract class UserComponentBase<T> where T : ModelBase
    {
        protected readonly IRepository _db;
        protected UserRepo _userRepo;
        protected RpcContext RpcCtx => _userRepo.RpcContext;

        protected UserComponentBase(UserRepo userRepo, IRepository db)
        {
            _userRepo = userRepo;
            _db = db;
        }

        // [prefix 계약] ICacheSession.BulkSet+GetList prefix 계약을 준수해야 한다:
        //   ListKeyFor(playerId).Value 는 KeyFor(item).Value 의 prefix여야 한다.
        //   예시: ListKeyFor(12345) → "CookieModel:12345"
        //         KeyFor(item)      → "CookieModel:12345:1"  ✅
        protected abstract CacheKey KeyFor(T model);
        protected abstract CacheKey ListKeyFor(ulong playerId);

        public T CreateMdl(T newValue)
        {
            newValue.UpdateTime = newValue.CreateTime = DateTime.UtcNow;
            return _db.Insert<T>(newValue, KeyFor);
        }

        public void UpdateMdl(T mdl)
        {
            mdl.UpdateTime = DateTime.UtcNow;
            _db.Update<T>(mdl, KeyFor(mdl));
        }

        // DB 미스 시 BulkSet으로 캐시 적재
        public List<T> GetMdlList()
            => _db.GetList<T>(ListKeyFor(RpcCtx.PlayerId),
                              db => db.SelectListByConditions<T>(new { RpcCtx.PlayerId }).ToList(),
                              KeyFor);

        // 캐시 히트 시 predicate 적용. 미스 시 캐시 미갱신.
        public List<T> GetMdlList(Func<T, bool> predicate)
            => _db.GetListFiltered<T>(ListKeyFor(RpcCtx.PlayerId),
                                     db => db.SelectListByConditions<T>(new { RpcCtx.PlayerId }).ToList(),
                                     predicate);

        protected T GetMdl(CacheKey key, Func<IDbExecutor, T> dbFetch)
            => _db.Get<T>(key, dbFetch);

        // IDbExecutor 범위 밖 특수 쿼리 전용 (SelectListByConditions, 집계 SQL 등)
        protected IDbSession DbSession => _db.Db;

        protected ICacheSession CacheLayer => _db.Cache;
    }
}
