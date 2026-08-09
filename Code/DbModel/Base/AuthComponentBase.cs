using ServerCore.Repo.Database;
using ServerCore;
using ServerCore.Model;
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

        protected Task<T?> GetMdlAsync<T>(Func<IDbExecutor, Task<T?>> dbFetch) where T : ModelBase
        {
            return _repository.Db.ExecuteAsync(dbFetch);
        }

        // 단건 캐시 → DB fallback (Auth/Session 전용)
        // slidingTtl: 캐시 히트 시 TTL 갱신 (Sliding Expiration). null이면 갱신 없음.
        protected async Task<T?> GetMdlWithCacheAsync<T>(CacheKey cacheKey, Func<IDbExecutor, Task<T?>> dbFetch, TimeSpan? slidingTtl = null) where T : ModelBase
        {
            var cached = await _repository.Cache.TryGetAsync<T>(cacheKey, slidingTtl);
            if (cached.Hit)
            {
                return cached.Value;
            }

            var result = await _repository.Db.ExecuteAsync(dbFetch);
            if (result != null)
            {
                await _repository.Cache.SetAsync(cacheKey, result, slidingTtl ?? Config<CoreConfig>.Get().CacheDefaultTtl);
            }
            return result;
        }

        protected Task<List<T>> GetMdlListAsync<T>(Func<IDbExecutor, Task<List<T>>> dbFetch) where T : ModelBase
        {
            return _repository.Db.ExecuteAsync(dbFetch);
        }

        protected async Task<T> CreateMdlAsync<T>(T entity) where T : ModelBase
        {
            entity.UpdateTime = entity.CreateTime = DateTime.UtcNow;
            return await _repository.Db.ExecuteAsync(db => db.InsertAsync<T>(entity));
        }

        protected async Task UpdateMdlAsync<T>(T entity) where T : ModelBase
        {
            entity.UpdateTime = DateTime.UtcNow;
            await _repository.Db.ExecuteAsync(db => db.UpdateAsync<T>(entity));
        }

        // IDbExecutor 범위 밖 특수 쿼리 전용 (SelectListByConditions, 집계 SQL 등)
        protected IDbSession DbSession => _repository.Db;
    }
}
