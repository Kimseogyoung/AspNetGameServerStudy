using ServerCore.Repo.Database;
using ServerCore;
using ServerCore.Model;
using WebStudyServer.GAME;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Base
{
    public class AuthComponentBase
    {
        protected readonly IRepository _repository;
        protected readonly AuthRepo _authRepo;
        protected IGameContext RpcCtx => _authRepo.RpcContext;

        public AuthComponentBase(AuthRepo authRepo, IRepository repository)
        {
            _authRepo = authRepo;
            _repository = repository;
        }

        protected T? GetMdl<T>(Func<IDbExecutor, T?> dbFetch) where T : ModelBase
        {
            return _repository.Db.Execute(dbFetch);
        }

        // 단건 캐시 → DB fallback (Auth/Session 전용)
        // slidingTtl: 캐시 히트 시 TTL 갱신 (Sliding Expiration). null이면 갱신 없음.
        protected T? GetMdlWithCache<T>(CacheKey cacheKey, Func<IDbExecutor, T?> dbFetch, TimeSpan? slidingTtl = null) where T : ModelBase
        {
            if (_repository.Cache.TryGet<T>(cacheKey, out var cached, slidingTtl))
            {
                return cached;
            }

            var result = _repository.Db.Execute(dbFetch);
            if (result != null)
            {
                _repository.Cache.Set(cacheKey, result, slidingTtl ?? Config<CoreConfig>.Get().CacheDefaultTtl);
            }
            return result;
        }

        protected List<T> GetMdlList<T>(Func<IDbExecutor, List<T>> dbFetch) where T : ModelBase
        {
            return _repository.Db.Execute(dbFetch);
        }

        protected T CreateMdl<T>(T entity) where T : ModelBase
        {
            entity.UpdateTime = entity.CreateTime = DateTime.UtcNow;
            return _repository.Db.Execute(db => db.Insert<T>(entity));
        }

        protected void UpdateMdl<T>(T entity) where T : ModelBase
        {
            entity.UpdateTime = DateTime.UtcNow;
            _repository.Db.Execute(db => db.Update<T>(entity));
        }

        // IDbExecutor 범위 밖 특수 쿼리 전용 (SelectListByConditions, 집계 SQL 등)
        protected IDbSession DbSession => _repository.Db;
    }
}
