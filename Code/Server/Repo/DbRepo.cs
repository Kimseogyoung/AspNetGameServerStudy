using WebStudyServer;
using WebStudyServer.GAME;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace Server.Repo
{
    public class DbRepo
    {
        public UserRepo OwnUser { get; private set; } = null;

        // Lazy Loading
        public AuthRepo Auth => _lazyAuthRepo.Value;
        public CenterRepo Center => _lazyCenterRepo.Value;
        public AllUserRepo AllUser => _lazyAllUserRepo.Value;

        public DbRepo(RpcContext rpcContext, ICacheLayer cacheLayer, DbScope dbScope, ILogger<DbRepo> logger)
        {
            _rpcContext = rpcContext;
            _cacheLayer = cacheLayer;
            _dbScope = dbScope;
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
            var dbFactory = _dbScope.Open(connStr);
            var userRepo = new UserRepo(_rpcContext, _cacheLayer);
            userRepo.Init(_rpcContext.ShardId, dbFactory);
            OwnUser = userRepo;
        }

        private AuthRepo BeginAuthRepo()
        {
            var connStr = APP.Cfg.AuthDbConnectionStrList.Count > 0 ? APP.Cfg.AuthDbConnectionStrList[0] : string.Empty;
            var dbFactory = _dbScope.Open(connStr);
            var authRepo = new AuthRepo(_rpcContext);
            authRepo.Init(0, dbFactory);
            return authRepo;
        }

        private CenterRepo BeginCenterRepo()
        {
            var connStr = APP.Cfg.CenterDbConnectionStrList.Count > 0 ? APP.Cfg.CenterDbConnectionStrList[0] : string.Empty;
            var dbFactory = _dbScope.Open(connStr);
            var centerRepo = new CenterRepo(_rpcContext);
            centerRepo.Init(0, dbFactory);
            return centerRepo;
        }

        private AllUserRepo BeginAllUserRepo()
        {
            var factories = APP.Cfg.UserDbConnectionStrList
                .Select(connStr => _dbScope.Open(connStr))
                .ToList();

            return new AllUserRepo(factories);
        }

        public void Commit()
        {
            try
            {
                _dbScope.Commit();
                _cacheLayer.FlushPendingWrites();
            }
            catch (Exception e)
            {
                // TODO: 오류 종류 파악 후 세분화하기
                Console.WriteLine(e);
                Rollback();
                throw;
            }
        }

        public void Rollback()
        {
            try
            {
                _dbScope.Rollback();
                _cacheLayer.DiscardPendingWrites();
            }
            catch (Exception e)
            {
                // TODO: 오류 종류 파악 후 세분화하기
                Console.WriteLine(e);
                Close();
                throw;
            }
        }

        public void Close()
        {
            try
            {
                _dbScope.Close();

                _lazyAuthRepo = null;
                _lazyCenterRepo = null;
                _lazyAllUserRepo = null;

                OwnUser = null;
            }
            catch (Exception e)
            {
                // TODO: 오류 종류 파악 후 세분화하기
                Console.WriteLine(e);
                throw;
            }
        }

        private string GetUserDbConnectionStr(int shardId)
        {
            var connList = APP.Cfg.UserDbConnectionStrList;
            if (connList.Count == 0)
            {
                return string.Empty;
            }

            if (c_maxShardCnt <= shardId)
            {
                throw new Exception("dd");
            }

            var shardIdx = _shardMap[shardId];
            if (shardIdx >= connList.Count)
            {
                shardIdx %= connList.Count;
            }

            return connList[shardIdx];
        }

        public int c_maxShardCnt = 64;

        private readonly int[] _shardMap =
        [   0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 4 ];

        private Lazy<AuthRepo> _lazyAuthRepo = null;
        private Lazy<CenterRepo> _lazyCenterRepo = null;
        private Lazy<AllUserRepo> _lazyAllUserRepo = null;

        private readonly RpcContext _rpcContext;
        private readonly ICacheLayer _cacheLayer;
        private readonly DbScope _dbScope;
        private readonly ILogger _logger;
    }
}
