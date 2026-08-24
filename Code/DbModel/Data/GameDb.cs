using ServerCore;
using ServerCore.Repo.Cache;
using ServerCore.Repo.Database;
using DbType = ServerCore.DbType;

namespace WebStudyServer.Data
{
    // 데이터 접근 진입점이자 트랜잭션 주인.
    //
    // DbSessionManager가 커넥션 문자열로 IDbSession을 캐시하므로 문자열이 같으면 같은 트랜잭션.
    // 커밋은 DB를 먼저 커밋한 뒤 캐시 pending을 flush하고, 롤백은 둘 다 버린다.
    // 세션을 닫는 것은 DbSessionManager가 한다.
    public class GameDb
    {
        public GameDb(DbSessionManager sessions, ICacheSession cache, ILogger<GameDb> logger)
        {
            _sessions = sessions;
            _cache = cache;
            _logger = logger;
        }

        // accountId를 모르는 Auth 조회
        public Identity Identity => _identity ??= new Identity(this);

        // 세션. Auth 에서 유일하게 캐시를 쓰므로 Identity 와 분리돼 있다.
        public SessionStore Sessions => _sessionStore ??= new SessionStore(this);

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

        // 소유자를 모르는 전 샤드 조회. 스코프 밖이다.
        public AllShards AllShards => _allShards ??= new AllShards(this);

        public async Task CommitAsync()
        {
            try
            {
                _sessions.Commit();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DB_COMMIT_FAILED - rolling back");
                await RollbackAsync();
                throw;
            }

            try
            {
                await _cache.FlushPendingWritesAsync();
            }
            catch (Exception e)
            {
                // DB는 이미 커밋됐으므로 되돌릴 수 없다. pending을 버리고 stale로 남긴다.
                _logger.LogError(e, "CACHE_FLUSH_FAILED - cache left stale, DB already committed");
                _cache.DiscardPendingWrites();
            }
        }

        public Task RollbackAsync()
        {
            try
            {
                _sessions.Rollback();
                _cache.DiscardPendingWrites();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DB_ROLLBACK_FAILED - closing sessions");
                _sessions.Close();
                throw;
            }

            return Task.CompletedTask;
        }

        // 샤드를 특정할 수 없는 경로가 커넥션 문자열로 직접 연다.
        internal IDbSession SessionFor(string connectionString)
        {
            return _sessions.GetOrCreate(connectionString);
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

            var dbSession = _sessions.GetOrCreate(connectionString);

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
        private readonly ILogger _logger;

        private Identity _identity;
        private SessionStore _sessionStore;
        private AllShards _allShards;

        private readonly Dictionary<string, IRepository> _repositories = [];
    }
}
