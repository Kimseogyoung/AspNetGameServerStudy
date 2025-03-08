using MySqlConnector;
using System.Data;
using WebStudyServer;
using WebStudyServer.GAME;
using WebStudyServer.Repo;
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
        private Lazy<AuthRepo> _lazyAuthRepo = null;
        private Lazy<CenterRepo> _lazyCenterRepo = null;
        private Lazy<AllUserRepo> _lazyAllUserRepo = null;

        private Dictionary<string, DBSqlExecutor> _dbExecutorDict = new();
        private readonly RpcContext _rpcContext;
        
        public DbRepo(RpcContext rpcContext)
        {
            _rpcContext = rpcContext;

            _lazyAuthRepo = new Lazy<AuthRepo>(BeginAuthRepo);
            _lazyCenterRepo = new Lazy<CenterRepo>(BeginCenterRepo);
            _lazyAllUserRepo = new Lazy<AllUserRepo>(BeginAllUserRepo);
        }

        public void BeginOwnUserRepo()
        {
            if(OwnUser != null)
            {
                return;
            }

            var dbConnStr = GetUserDbConnectionStr(_rpcContext.ShardId);
            var dbExecutor = TouchDbSqlExecutor(dbConnStr, IsolationLevel.ReadCommitted);
            var userRepo = new UserRepo(_rpcContext);
            userRepo.Init(_rpcContext.ShardId, dbExecutor);
            OwnUser = userRepo;
        }

        private AuthRepo BeginAuthRepo()
        {  
            var dbExecutor = TouchDbSqlExecutor(APP.Cfg.AuthDbConnectionStrList[0], IsolationLevel.ReadCommitted);
            var authRepo = new AuthRepo(_rpcContext);
            authRepo.Init(0, dbExecutor);
         
            return authRepo;
        }

        private CenterRepo BeginCenterRepo()
        {
            var dbExecutor = TouchDbSqlExecutor(APP.Cfg.CenterDbConnectionStrList[0], IsolationLevel.ReadCommitted);
            var centerRepo = new CenterRepo(_rpcContext);
            centerRepo.Init(0, dbExecutor);
            return centerRepo;
        }

        private AllUserRepo BeginAllUserRepo()
        {
            var dbExcutorList = new List<DBSqlExecutor>();
            foreach(var dbStr in APP.Cfg.UserDbConnectionStrList)
            {
                var dbExecutor = TouchDbSqlExecutor(dbStr, IsolationLevel.ReadCommitted);
                dbExcutorList.Add(dbExecutor);
            }
            var allUserRepo = new AllUserRepo(dbExcutorList);
            return allUserRepo;
        }

        public void Commit()
        {
            try
            {
                foreach (var dbExecutor in _dbExecutorDict.Values)
                {
                    dbExecutor.Commit();
                }

                _dbExecutorDict.Clear();
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
                foreach (var dbExecutor in _dbExecutorDict.Values)
                {
                    dbExecutor.Rollback();
                }

                _dbExecutorDict.Clear();
            }
            catch (Exception e)
            {
                // TODO: 오류 종류 파악 후 세분화하기
                Console.WriteLine(e);
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                if (_dbExecutorDict.Count > 0)
                {
                    foreach (var dbExecutor in _dbExecutorDict.Values)
                    {
                        dbExecutor.Close();
                    }

                    _dbExecutorDict.Clear();
                }
            
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

        private DBSqlExecutor TouchDbSqlExecutor(string dbConnStr, IsolationLevel level = IsolationLevel.ReadCommitted) //<- Level 좀 이상
        {
            if (!_dbExecutorDict.TryGetValue(dbConnStr, out var outDbExecutor))
            {
                outDbExecutor = DBSqlExecutor.StartTransaction(dbConnStr, level);
                _dbExecutorDict.Add(dbConnStr, outDbExecutor);
            }

            return outDbExecutor;
        }

        private string GetUserDbConnectionStr(int shardId)
        {
            if (c_maxShardCnt <= shardId)
            {
                throw new Exception("dd");
            }

            var shardIdx = shardMap[shardId];
            var dbConnCnt = APP.Cfg.UserDbConnectionStrList.Count;
            if (shardIdx >= dbConnCnt)
            {
                shardIdx = shardIdx % dbConnCnt;
            }

            return APP.Cfg.UserDbConnectionStrList[shardIdx];
        }
        
        public int c_maxShardCnt = 64;

        private int[] shardMap = new int[]
        {   0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 4 };
    }
}
