using ServerCore.Repo.Database;
using ServerCore;
using WebStudyServer.Repo;
using WebStudyServer.Data;
using ServerCore.Repo.Cache;
using System.Data;
using DbType = ServerCore.DbType;

namespace Server.Repo
{
    public class GlobalDbRepo : IDisposable
    {
        // Lazy Loading. Auth / Center 는 GameDb 로 옮겨져 여기 없다(S4~S8).
        public AllUserRepo AllUser => _lazyAllUserRepo?.Value ?? throw new ObjectDisposedException(nameof(GlobalDbRepo));

        // TODO: 추후 cacheSession도 CacheSessionManager통해서 만들도록
        public GlobalDbRepo(IGameContext rpcContext, ICacheSession cacheSession, DbSessionManager dbScope, ILogger<GlobalDbRepo> logger)
        {
            _rpcContext = rpcContext;
            _cacheSession = cacheSession;
            _dbSessionManager = dbScope;
            _logger = logger;

            _lazyAllUserRepo = new Lazy<AllUserRepo>(BeginAllUserRepo);
        }

        private AllUserRepo BeginAllUserRepo()
        {
            var factories = Config<CoreConfig>.Get().UserDbConnectionStrList
                .Select(connStr => _dbSessionManager.Open(connStr))
                .ToList();

            return new AllUserRepo(factories);
        }

        public async Task CommitAsync()
        {
            try
            {
                _dbSessionManager.Commit();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DB Commit 오류");
                await RollbackAsync();
                throw;
            }

            try
            {
                await _cacheSession.FlushPendingWritesAsync();
            }
            catch (Exception e)
            {
                // DB는 이미 커밋됨 — Rollback 불가. pending을 버리고 stale 상태로 남긴다.
                _logger.LogError(e, "CACHE_FLUSH_FAILED - cache left stale, DB already committed");
                _cacheSession.DiscardPendingWrites();
            }
        }

        public Task RollbackAsync()
        {
            try
            {
                _dbSessionManager.Rollback();
                _cacheSession.DiscardPendingWrites();
            }
            catch (Exception e)
            {
                // TODO: 오류 종류 파악 후 세분화하기
                _logger.LogError(e, "Rollback 중 오류 발생");
                Close();
                throw;
            }

            return Task.CompletedTask;
        }

        public void Close()
        {
            try
            {
                _dbSessionManager.Close();

                _lazyAllUserRepo = null;
            }
            catch (Exception e)
            {
                // TODO: 오류 종류 파악 후 세분화하기
                _logger.LogError(e, "Close 중 오류 발생");
                throw;
            }
        }

        private IRepository CreateRepository(string dbConnectionString)
        {
            var dbSession = _dbSessionManager.Open(dbConnectionString);

            IRepository repo;
            switch (Config<CoreConfig>.Get().DbType)
            {
                case DbType.InMemory:
                    repo = new InMemoryRepository(_cacheSession, dbSession);
                    break;
                case DbType.MySql:
                    repo = new SqlRepository(_cacheSession, dbSession);
                    break;
                default:
                    throw new NotSupportedException($"NotSupportDbType({Config<CoreConfig>.Get().DbType})");
            }

            return repo;
        }

        public void Dispose()
        {
            // 아무 처리 없이 Close
            Close();
        }

        private Lazy<AllUserRepo>? _lazyAllUserRepo;

        private readonly IGameContext _rpcContext;
        private readonly ICacheSession _cacheSession;
        private readonly DbSessionManager _dbSessionManager;
        private readonly ILogger _logger;
    }
}
