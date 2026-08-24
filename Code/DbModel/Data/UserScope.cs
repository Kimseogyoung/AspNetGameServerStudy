using ServerCore.Model;

namespace WebStudyServer.Data
{
    // 한 플레이어의 데이터 경계. 소유자 리스트 캐시 버킷과 자동 WHERE PlayerId가 여기서 나옴.
    //
    // playerId를 인자로 받으므로 요청당 1명 제약이 없음. 길드/우편/거래처럼 여러 플레이어를
    // 다루는 연산이 가능.
    public class UserScope
    {
        public int ShardId { get; }
        public ulong PlayerId { get; }

        internal UserScope(GameDb db, int shardId, ulong playerId)
        {
            _db = db;
            ShardId = shardId;
            PlayerId = playerId;
        }

        public OwnedSet<T> Owned<T>() where T : ModelBase, IScopedModel, new()
        {
            return new OwnedSet<T>(() => _db.UserRepository(ShardId), PlayerId);
        }

        // 리스트 캐시가 없는 엔티티에 한 행을 넣는다. 읽기 경로가 없으므로 캐시도 안 지난다.
        // 캐시되는 엔티티를 이 경로로 넣으면 캐시가 조용히 낡으므로 막는다.
        public Task InsertAsync<T>(T entity) where T : ModelBase, IScopedModel
        {
            if (EntityMeta<T>.HasCacheTag)
            {
                throw new InvalidOperationException($"CACHED_ENTITY_CANNOT_INSERT:{typeof(T).Name}");
            }

            entity.SetScopeKey(PlayerId);
            entity.CreateTime = entity.UpdateTime = DateTime.UtcNow;
            return _db.UserRepository(ShardId).InsertAsync(entity);
        }

        private readonly GameDb _db;
    }
}
