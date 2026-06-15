using ServerCore.Repo.Database;
using ServerCore;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;
using System.Data;
using DbType = ServerCore.DbType;

namespace Server.Repo
{
    public class GlobalDbRepo : IDisposable
    {
        public UserRepo OwnUser { get; private set; } = null;

        // Lazy Loading
        public AuthRepo Auth => _lazyAuthRepo?.Value ?? throw new ObjectDisposedException(nameof(GlobalDbRepo));
        public CenterRepo Center => _lazyCenterRepo?.Value ?? throw new ObjectDisposedException(nameof(GlobalDbRepo));
        public AllUserRepo AllUser => _lazyAllUserRepo?.Value ?? throw new ObjectDisposedException(nameof(GlobalDbRepo));

        // TODO: 추후 cacheSession도 CacheSessionManager통해서 만들도록
        public GlobalDbRepo(IGameContext rpcContext, ICacheSession cacheSession, DbSessionManager dbScope, ILogger<GlobalDbRepo> logger)
        {
            _rpcContext = rpcContext;
            _cacheSession = cacheSession;
            _dbSessionManager = dbScope;
            _logger = logger;

            _lazyAuthRepo = new Lazy<AuthRepo>(BeginAuthRepo);
            _lazyCenterRepo = new Lazy<CenterRepo>(BeginCenterRepo);
            _lazyAllUserRepo = new Lazy<AllUserRepo>(BeginAllUserRepo);
        }

        public void BeginOwnUserRepo()
        {
            if (OwnUser != null)
            {
                return;
            }

            var connStr = GetUserDbConnectionStr(_rpcContext.ShardId);
            var repository = CreateRepository(connStr);
            var userRepo = new UserRepo(_rpcContext, repository);
            OwnUser = userRepo;
        }

        private AuthRepo BeginAuthRepo()
        {
            var connStr = Core.Cfg.AuthDbConnectionStrList.Count > 0 ? Core.Cfg.AuthDbConnectionStrList[0] : InMemoryConnectionKey;
            var repository = CreateRepository(connStr);
            var authRepo = new AuthRepo(_rpcContext, repository);
            return authRepo;
        }

        private CenterRepo BeginCenterRepo()
        {
            var connStr = Core.Cfg.CenterDbConnectionStrList.Count > 0 ? Core.Cfg.CenterDbConnectionStrList[0] : InMemoryConnectionKey;
            var repository = CreateRepository(connStr);
            var centerRepo = new CenterRepo(_rpcContext, repository);
            return centerRepo;
        }

        private AllUserRepo BeginAllUserRepo()
        {
            var factories = Core.Cfg.UserDbConnectionStrList
                .Select(connStr => _dbSessionManager.Open(connStr))
                .ToList();

            return new AllUserRepo(factories);
        }

        public void Commit()
        {
            try
            {
                _dbSessionManager.Commit();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DB Commit 오류");
                Rollback();
                throw;
            }

            try
            {
                _cacheSession.FlushPendingWrites();
            }
            catch (Exception e)
            {
                // DB는 이미 커밋됨 — Rollback 불가. pending을 버리고 stale 상태로 남긴다.
                _logger.LogError(e, "Redis flush 오류 — stale cache 상태");
                _cacheSession.DiscardPendingWrites();
            }
        }

        public void Rollback()
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
        }

        public void Close()
        {
            try
            {
                _dbSessionManager.Close();

                _lazyAuthRepo = null;
                _lazyCenterRepo = null;
                _lazyAllUserRepo = null;

                OwnUser = null;
            }
            catch (Exception e)
            {
                // TODO: 오류 종류 파악 후 세분화하기
                _logger.LogError(e, "Close 중 오류 발생");
                throw;
            }
        }

        private string GetUserDbConnectionStr(int shardId)
        {
            var connList = Core.Cfg.UserDbConnectionStrList;
            if (connList.Count == 0)
            {
                return InMemoryConnectionKey;
            }

            if (MaxShardCount <= shardId)
            {
                throw new ArgumentOutOfRangeException(nameof(shardId),
                    $"ShardId({shardId})가 최대값({MaxShardCount})을 초과합니다.");
            }

            var shardIdx = _shardMap[shardId];
            if (shardIdx >= connList.Count)
            {
                shardIdx %= connList.Count;
            }

            return connList[shardIdx];
        }


        private IRepository CreateRepository(string dbConnectionString)
        {
            var dbSession = _dbSessionManager.Open(dbConnectionString);

            IRepository repo;
            switch (Core.Cfg.DbType)
            {
                case DbType.InMemory:
                    repo = new InMemoryRepository(_cacheSession, dbSession);
                    break;
                case DbType.MySql:
                    repo = new SqlRepository(_cacheSession, dbSession);
                    break;
                default:
                    throw new NotSupportedException($"NotSupportDbType({Core.Cfg.DbType})");
            }

            return repo;
        }

        public void Dispose()
        {
            // 아무 처리 없이 Close
            Close();
        }

        // InMemory 모드에서 모든 Repo가 단일 세션을 공유하도록 동일한 키를 사용한다.
        private const string InMemoryConnectionKey = "__inmemory__";
        private const int MaxShardCount = 64;

        private readonly int[] _shardMap =
        [   0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 4 ];

        private Lazy<AuthRepo>? _lazyAuthRepo;
        private Lazy<CenterRepo>? _lazyCenterRepo;
        private Lazy<AllUserRepo>? _lazyAllUserRepo;

        private readonly IGameContext _rpcContext;
        private readonly ICacheSession _cacheSession;
        private readonly DbSessionManager _dbSessionManager;
        private readonly ILogger _logger;
    }
}
