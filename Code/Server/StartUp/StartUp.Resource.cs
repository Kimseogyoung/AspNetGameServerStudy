using Server.Service;
using ServerCore;
using ServerCore.Extension;
using WebStudyServer.Data;
using WebStudyServer.Model;
using ServerCore.Repo.Cache;
using ServerCore.Repo.Database;

namespace WebStudyServer
{
    public partial class Startup
    {
        public void Resource(IServiceCollection services)
        {
            services.AddScoped<UserLockService>();

            // Db
            switch (Config<CoreConfig>.Get().DbType)
            {
                case DbType.MySql:
                    services.AddScoped<ILockService, MySqlLockService>();
                    services.AddSingleton<IDbSessionFactory, MySqlDbSessionFactory>();
                    break;
                case DbType.InMemory:
                    services.AddScoped<ILockService, InMemoryLockService>();
                    services.AddSingleton<IDbSessionFactory, InMemoryDbSessionFactory>();
                    break;
                default:
                    throw new Exception($"No handling DbType({Config<CoreConfig>.Get().DbType})");
            }

            // Cache
            services.AddCacheSession(Config<CoreConfig>.Get().CacheType, Config<CoreConfig>.Get().RedisConnectionString);

            services.AddMemoryCache();
            services.AddSingleton<InMemoryStore>();

            services.AddScoped<DbSessionManager>();
            services.AddScoped<GameDb>();
            services.AddScoped<ResponseCacheService>();

            // [Entity] 가 붙은 모델을 전부 등록한다. 두 호스트가 같은 목록을 갖는다.
            EntityRegistry.ScanAndRegister(typeof(PlayerModel).Assembly);

            // 손으로 유지하는 캐시 태그 맵을 [Entity] 와 대조한다.
            EntityMeta.VerifyCacheTags();

            if (Config<CoreConfig>.Get().DbType != DbType.InMemory)
            {
                ConnectionTest();
            }
        }

        private void ConnectionTest()
        {
            foreach (var connectionStr in Config<CoreConfig>.Get().UserDbConnectionStrList)
            {
                var excutor = DBSqlExecutor.StartTransaction(connectionStr, System.Data.IsolationLevel.ReadCommitted);
                excutor.Commit();
            }

            foreach (var connectionStr in Config<CoreConfig>.Get().AuthDbConnectionStrList)
            {
                var excutor = DBSqlExecutor.StartTransaction(connectionStr, System.Data.IsolationLevel.ReadCommitted);
                excutor.Commit();
            }

            foreach (var connectionStr in Config<CoreConfig>.Get().CenterDbConnectionStrList)
            {
                var excutor = DBSqlExecutor.StartTransaction(connectionStr, System.Data.IsolationLevel.ReadCommitted);
                excutor.Commit();
            }
        }
    }
}
