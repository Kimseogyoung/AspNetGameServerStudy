using ServerCore;
using ServerCore.Repo.Cache;
using ServerCore.Repo.Database;
using DbType = ServerCore.DbType;

namespace WebStudyServer.Data
{
    // 데이터 접근 진입점. GlobalDbRepo 의 후계자다.
    //
    // GlobalDbRepo 와 같은 DbSessionManager 인스턴스를 주입받는다.
    // DbSessionManager 는 커넥션 문자열로 IDbSession 을 캐시하므로, 커넥션
    // 문자열이 같으면 같은 세션 = 같은 트랜잭션이 된다. 이관 기간에 옛 경로와
    // 새 경로를 한 요청 안에서 섞어도 원자성이 깨지지 않는 근거가 이것이다.
    //
    // 커밋 주체는 이관이 끝날 때까지 GlobalDbRepo 하나다(설계문서 7.1-②).
    // 여기서는 tx 를 건드리지 않는다.
    public class GameDb
    {
        public GameDb(DbSessionManager sessions, ICacheSession cache)
        {
            _sessions = sessions;
            _cache = cache;
        }

        // 어느 샤드의 누구든 열 수 있다. "본편"과 "운영툴"의 구분이 없다.
        // 여기서는 스코프 객체만 만든다 - 커넥션은 OwnedSet 의 첫 조회에서 열린다.
        public UserScope User(int shardId, ulong playerId)
        {
            return new UserScope(this, shardId, playerId);
        }

        public AuthScope Auth(ulong accountId)
        {
            return new AuthScope(this, accountId);
        }

        public CenterScope Center()
        {
            return new CenterScope(this);
        }

        internal IRepository UserRepository(int shardId)
        {
            return GetOrCreateRepository(DbConnectionResolver.User(shardId));
        }

        internal IRepository AuthRepository()
        {
            return GetOrCreateRepository(DbConnectionResolver.Auth());
        }

        internal IRepository CenterRepository()
        {
            return GetOrCreateRepository(DbConnectionResolver.Center());
        }

        private IRepository GetOrCreateRepository(string connectionString)
        {
            if (_repositories.TryGetValue(connectionString, out var existing))
            {
                return existing;
            }

            var dbSession = _sessions.Open(connectionString);

            IRepository repo;
            switch (Config<CoreConfig>.Get().DbType)
            {
                case DbType.InMemory:
                    repo = new InMemoryRepository(_cache, dbSession);
                    break;
                case DbType.MySql:
                    repo = new SqlRepository(_cache, dbSession);
                    break;
                default:
                    throw new NotSupportedException($"NotSupportDbType({Config<CoreConfig>.Get().DbType})");
            }

            _repositories[connectionString] = repo;
            return repo;
        }

        private readonly DbSessionManager _sessions;
        private readonly ICacheSession _cache;

        private readonly Dictionary<string, IRepository> _repositories = [];
    }
}
