using ServerCore.Model;

namespace WebStudyServer.Data
{
    // 한 플레이어의 데이터만 보이는 경계. UserRepo 의 후계자다.
    //
    // UserRepo 는 "저장소"라 불렸지만 실제로 한 일은 경계 긋기였다
    // (LoadFromDb 의 자동 WHERE PlayerId, ListKeyFor(playerId) 의 플레이어별
    // 캐시 버킷). 그 경계를 이름에 담은 것이고, 컴포넌트 11개를 손으로 나열하던
    // 자리가 Owned<T>() 하나로 바뀐다.
    //
    // RpcContext 를 읽지 않는다. playerId 를 인자로 받으므로 요청당 1명이라는
    // 제약이 없어지고, 길드/우편/거래처럼 여러 플레이어를 다루는 연산이 성립한다.
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

        public OwnedSet<T> Owned<T>() where T : ModelBase, new()
        {
            return new OwnedSet<T>(() => _db.UserRepository(ShardId), PlayerId);
        }


        private readonly GameDb _db;
    }
}
