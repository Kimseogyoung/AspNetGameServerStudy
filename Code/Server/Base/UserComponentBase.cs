using Server.Repo.Database;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public abstract class UserComponentBase<T> where T : ModelBase
    {
        protected readonly IDbLayer _db;
        protected UserRepo _userRepo;
        protected RpcContext RpcCtx => _userRepo.RpcContext;

        protected UserComponentBase(UserRepo userRepo, IDbLayer db)
        {
            _userRepo = userRepo;
            _db = db;
        }

        protected abstract CacheKey KeyFor(T model);
        protected abstract CacheKey ListKeyFor(ulong playerId);

        public T CreateMdl(T newValue)
        {
            newValue.UpdateTime = newValue.CreateTime = DateTime.UtcNow;
            return _db.Insert<T>(newValue, KeyFor(newValue));
        }

        public void UpdateMdl(T mdl)
        {
            mdl.UpdateTime = DateTime.UtcNow;
            _db.Update<T>(mdl, KeyFor(mdl));
        }

        // DB 미스 시 BulkSet으로 캐시 적재
        public List<T> GetMdlList()
            => _db.GetListByPlayerId<T>(ListKeyFor(RpcCtx.PlayerId), RpcCtx.PlayerId, KeyFor);

        // 캐시 히트 시 predicate 적용. 미스 시 캐시 미갱신.
        public List<T> GetMdlList(Func<T, bool> predicate)
            => _db.GetListByPlayerIdAndPredicate<T>(ListKeyFor(RpcCtx.PlayerId), RpcCtx.PlayerId, predicate);

        protected T GetMdl(CacheKey key, Func<IDbExecutor, T> dbFetch)
            => _db.Get<T>(key, dbFetch);

        // IDbExecutor 범위 밖 특수 쿼리 전용 (SelectListByConditions, 집계 SQL 등)
        protected IDbExecutorFactory DbFactory => _db.DbFactory;

        protected ICacheLayer CacheLayer => _db.Cache;
    }
}
