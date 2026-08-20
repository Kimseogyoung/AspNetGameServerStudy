using ServerCore;
using ServerCore.Repo.Cache;
using ServerCore.Repo.Database;
using DbType = ServerCore.DbType;

namespace WebStudyServer.Data
{
    // 데이터 접근 진입점.
    //
    // GlobalDbRepo와 같은 DbSessionManager를 주입받음. DbSessionManager가 커넥션 문자열로
    // IDbSession을 캐시하므로 문자열이 같으면 같은 트랜잭션. 이관 기간에 옛 경로와 섞어도
    // 원자성이 안 깨짐.
    //
    // 커밋 주체는 이관이 끝날 때까지 GlobalDbRepo 하나. 여기서는 tx를 안 건드림.
    public class GameDb
    {
        public GameDb(DbSessionManager sessions, ICacheSession cache)
        {
            _sessions = sessions;
            _cache = cache;
        }

        // accountId를 모르는 Auth 조회
        public Identity Identity => _identity ??= new Identity(this);

        // 어느 샤드의 누구든 열 수 있음. 스코프 객체만 만들고 커넥션은 첫 조회에서 열림.
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

        private Identity _identity;

        private readonly Dictionary<string, IRepository> _repositories = [];
    }
}
