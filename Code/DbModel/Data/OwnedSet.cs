using ServerCore.Model;
using ServerCore.Repo.Cache;
using ServerCore.Repo.Database;

namespace WebStudyServer.Data
{
    // 스코프 안에서 엔티티 하나를 다루는 유일한 타입. Component/Manager 를 대체한다.
    //
    // 하는 일은 UserComponentBase 와 같다 - 소유자 리스트를 읽고, 만들고, 고친다.
    // 다른 점은 엔티티마다 클래스를 만들지 않는다는 것뿐이다. 리스트 키도 자동
    // WHERE 컬럼도 [Entity] 에서 나오므로 ListKeyFor/KeyFor override 가 사라진다.
    //
    // 무상태다. 쓰기는 즉시 반영되며 지연 추적(dirty)을 하지 않는다 - 그 판단과
    // 근거는 설계문서 §S2-H 참조.
    //
    // 다루는 것은 "캐시되는 소유자 리스트" 한 종류뿐이다. ScopeKey 와 CacheTag 를
    // 둘 다 요구하며 하나라도 없으면 생성 시점에 던진다 - 캐시 없이 OwnedSet 을
    // 쓰는 경로는 두지 않는다(설계문서 §S2-J). 리뷰 5.4.1 이 말한 "캐시 없음"은
    // OwnedSet 의 두 번째 모드가 아니라 OwnedSet 밖이라는 뜻으로 읽는다.
    //
    // 그래서 여기 못 들어오는 것: 감사 로그(CashChangeLog/GachaLog - ScopeKey 는
    // 있으나 append-only 라 리스트로 읽지 않는다), Session 포인터 캐시, Schedule
    // 전역 캐시. 엔티티 하나에만 해당하는 동작은 일반화하지 않고 전용 코드로 남긴다.
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

        // 여기서 처음 _repository() 를 부른다 = 커넥션도 여기서 열린다(리뷰 5.11).
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

        // IRepository.UpdateAsync 가 DB 쓰기와 캐시 갱신을 한 단위로 처리한다.
        // match 술어는 [Entity].Pk 에서 나온다 - 컴포넌트마다 손으로 쓰던 KeyFor 비교다.
        public Task UpdateAsync(T entity)
        {
            entity.UpdateTime = DateTime.UtcNow;
            return _repository().UpdateAsync(entity, _listKey, EntityMeta<T>.PkMatcher(entity));
        }

        // 자동 WHERE. 지금 UserComponentBase.LoadFromDb 가 하는 일과 같지만,
        // 컬럼명을 [Entity].ScopeKey 에서 받으므로 PlayerModel(스코프 키가 Id)
        // 때문에 override 를 두지 않아도 된다.
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
