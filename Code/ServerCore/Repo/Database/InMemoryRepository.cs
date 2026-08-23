using ServerCore.Model;
using ServerCore.Repo.Cache;

namespace ServerCore.Repo.Database
{
    // IRepository의 InMemory 구현.
    // InMemoryStore가 영속 저장소이므로 Cache → DB fallback 없이 DbExecutor에 직접 위임.
    public class InMemoryRepository : IRepository
    {
        public ICacheSession Cache { get; }
        public IDbSession Db { get; }

        public InMemoryRepository(ICacheSession cache, IDbSession dbFactory)
        {
            Cache = cache;
            Db = dbFactory;
        }

        public Task<List<T>> GetListAsync<T>(CacheKey listKey, Func<IDbExecutor, Task<List<T>>> dbFetch) where T : ModelBase
        {
            return Db.ExecuteAsync(dbFetch);
        }

        public Task<T> InsertAsync<T>(T entity, CacheKey listKey) where T : ModelBase
        {
            return Db.ExecuteAsync(db => db.InsertAsync<T>(entity));
        }

        public async Task UpdateAsync<T>(T entity, CacheKey listKey, Func<T, bool> match) where T : ModelBase
        {
            await Db.ExecuteAsync(db => db.UpdateAsync<T>(entity));
        }

        public async Task UpsertListAsync<T>(IReadOnlyList<T> entityList, CacheKey listKey) where T : ModelBase
        {
            await Db.ExecuteAsync(db => db.UpsertListAsync(entityList));
        }
    }
}
