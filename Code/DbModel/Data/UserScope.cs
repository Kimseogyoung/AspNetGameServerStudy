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

        private readonly GameDb _db;
    }
}
