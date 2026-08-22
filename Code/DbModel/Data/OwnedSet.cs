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
    // 캐시되는 소유자 리스트 전용. 소유자가 없는 엔티티는 IScopedModel이 아니라서 컴파일이 안 됨.
    // 감사 로그(CashChangeLog/GachaLog)나 Session 포인터 캐시처럼 로드 단위가 다른 건 여기로 안 묶음.
    public class OwnedSet<T> where T : ModelBase, IScopedModel, new()
    {
        internal OwnedSet(Func<IRepository> repository, ulong scopeKeyValue)
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

        // 소유자는 스코프가 정하므로 호출부가 넣은 ScopeKey 값은 덮어씀
        public Task<T> CreateAsync(T entity)
        {
            entity.SetScopeKey(_scopeKeyValue);
            entity.CreateTime = entity.UpdateTime = DateTime.UtcNow;
            return _repository().InsertAsync(entity, _listKey);
        }

        // IRepository.UpdateAsync가 DB 쓰기와 캐시 갱신을 한 단위로 처리
        public Task UpdateAsync(T entity)
        {
            EnsureOwned(entity);
            entity.UpdateTime = DateTime.UtcNow;
            return _repository().UpdateAsync(entity, _listKey, x => x.PkEquals(entity));
        }

        // 다른 소유자의 엔티티를 이 스코프로 저장하면 DB와 캐시 버킷이 어긋난다
        private void EnsureOwned(T entity)
        {
            var owner = entity.GetScopeKey();
            if (owner != _scopeKeyValue)
            {
                throw new InvalidOperationException($"NOT_OWNED_ENTITY:{typeof(T).Name}:{owner}:{_scopeKeyValue}");
            }
        }

        // 자동 WHERE. 컬럼명은 [Entity].ScopeKey에서
        private async Task<List<T>> LoadFromDbAsync(IDbExecutor db)
        {
            var rows = await db.SelectListByColumnAsync<T>(EntityMeta<T>.ScopeKey, _scopeKeyValue);
            return [.. rows];
        }

        private readonly Func<IRepository> _repository;
        private readonly ulong _scopeKeyValue;
        private readonly CacheKey _listKey;
    }
}
