using ServerCore.Model;
using ServerCore.Repo.Cache;

namespace ServerCore.Repo.Database
{
    // IRepository의 InMemory 구현.
    // InMemoryStore가 영속 저장소이므로 Cache → DB fallback 없이 DbExecutor에 직접 위임.
    public class InMemoryRepository : IRepository
    {
        public InMemoryRepository(IDbSession dbSession)
        {
            _db = dbSession;
        }

        public Task<List<T>> GetListAsync<T>(CacheKey listKey, Func<IDbExecutor, Task<List<T>>> dbFetch) where T : ModelBase
        {
            return _db.ExecuteAsync(dbFetch);
        }

        public Task<T> InsertAsync<T>(T entity, CacheKey listKey) where T : ModelBase
        {
            return _db.ExecuteAsync(db => db.InsertAsync<T>(entity));
        }

        public Task<T> InsertAsync<T>(T entity) where T : ModelBase
        {
            return _db.ExecuteAsync(db => db.InsertAsync<T>(entity));
        }

        public async Task UpdateAsync<T>(T entity, CacheKey listKey) where T : ModelBase
        {
            await _db.ExecuteAsync(db => db.UpdateAsync<T>(entity));
        }

        public async Task UpsertListAsync<T>(IReadOnlyList<T> entityList, CacheKey listKey) where T : ModelBase
        {
            await _db.ExecuteAsync(db => db.UpsertListAsync(entityList));
        }

        private readonly IDbSession _db;
    }
}
