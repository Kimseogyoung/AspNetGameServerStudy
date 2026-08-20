using ServerCore.Model;
using ServerCore.Repo.Cache;
using ServerCore.Repo.Database;

namespace WebStudyServer.Data
{
    // 스코프 안에서 엔티티 하나를 다룸. 리스트 키도 자동 WHERE 컬럼도 [Entity]에서
    // 나오므로 엔티티마다 클래스를 두지 않음.
    //
    // 무상태. 쓰기는 즉시 반영.
    //
    // 캐시되는 소유자 리스트 전용. ScopeKey와 CacheTag 둘 다 필요하고 하나라도 없으면 생성 시 예외.
    // 감사 로그(CashChangeLog/GachaLog)나 Session 포인터 캐시처럼 로드 단위가 다른 건 여기로 안 묶음.
    public class OwnedSet<T> where T : ModelBase, new()
    {
        internal OwnedSet(Func<IRepository> repository, object scopeKeyValue)
        {
            if (!EntityMeta<T>.HasScopeKey)
            {
                throw new InvalidOperationException($"NOT_FOUND_SCOPE_KEY:{typeof(T).Name}");
            }

            if (!EntityMeta<T>.HasCacheTag)
            {
                throw new InvalidOperationException($"NOT_FOUND_CACHE_TAG:{typeof(T).Name}");
            }

            _repository = repository;
            _scopeKeyValue = scopeKeyValue;
            _listKey = CacheKey.For(EntityMeta<T>.CacheTag, scopeKeyValue);
        }

        // 첫 _repository() 호출 = 커넥션 오픈
        public Task<List<T>> GetListAsync()
        {
            return _repository().GetListAsync<T>(_listKey, LoadFromDbAsync);
        }

        public async Task<(bool Found, T Value)> TryGetAsync(Func<T, bool> predicate)
        {
            var list = await GetListAsync();
            var found = list.FirstOrDefault(predicate);
            return (found != null, found);
        }

        public Task<T> CreateAsync(T entity)
        {
            entity.CreateTime = entity.UpdateTime = DateTime.UtcNow;
            return _repository().InsertAsync(entity, _listKey);
        }

        // IRepository.UpdateAsync가 DB 쓰기와 캐시 갱신을 한 단위로 처리
        public Task UpdateAsync(T entity)
        {
            entity.UpdateTime = DateTime.UtcNow;
            return _repository().UpdateAsync(entity, _listKey, EntityMeta<T>.PkMatcher(entity));
        }

        // 자동 WHERE. 컬럼명은 [Entity].ScopeKey에서
        private async Task<List<T>> LoadFromDbAsync(IDbExecutor db)
        {
            var rows = await db.SelectListByColumnAsync<T>(EntityMeta<T>.ScopeKey, _scopeKeyValue);
            return [.. rows];
        }

        private readonly Func<IRepository> _repository;
        private readonly object _scopeKeyValue;
        private readonly CacheKey _listKey;
    }
}
