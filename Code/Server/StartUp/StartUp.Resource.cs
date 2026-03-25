using Microsoft.EntityFrameworkCore;
using Server.Repo;
using StackExchange.Redis;
using WebStudyServer.Extension;
using WebStudyServer.GAME;
using WebStudyServer.Model;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

// Init<T> 한 줄로 DapperExtension + InMemoryPkRegistry 동시 등록
static class ModelRegistration
{
    public static void Init<T>(params string[] keyFields)
    {
        DapperExtension.Init<T>(keyFields);
        InMemoryPkRegistry.Init<T>(keyFields);
    }
}

namespace WebStudyServer
{
    public partial class Startup
    {
        public void Resource(IServiceCollection services)
        {
            services.AddScoped<UserLockService>();;

            // Db
            switch (APP.Cfg.DbType)
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
                    throw new Exception($"No handling DbType({APP.Cfg.DbType})");
            }

            // Cache
            switch (APP.Cfg.CacheType)
            {
                case CacheType.Redis:
                    services.AddSingleton<IConnectionMultiplexer>(
                    _ => ConnectionMultiplexer.Connect(APP.Cfg.RedisConnectionString));
                    services.AddScoped<InMemoryCacheLayer>();
                    services.AddScoped<RedisCacheLayer>();
                    services.AddScoped<ICacheSession, RedisCompositeCacheLayer>();
                    break;
                case CacheType.InMemory:
                    services.AddScoped<ICacheSession, InMemoryCacheLayer>();
                    break;
                default:
                    throw new Exception($"No handling DbType({APP.Cfg.DbType})");
            }

            services.AddMemoryCache();
            services.AddSingleton<InMemoryStore>();

            services.AddScoped<DbSessionManager>();
            services.AddScoped<GlobalDbRepo>();

            // Auth
            ModelRegistration.Init<AccountModel>("Id");
            ModelRegistration.Init<ChannelModel>("Key");
            ModelRegistration.Init<DeviceModel>("Key");
            ModelRegistration.Init<SessionModel>("AccountId");
            ModelRegistration.Init<PlayerMapModel>("AccountId");

            // User
            ModelRegistration.Init<PlayerModel>("Id");
            ModelRegistration.Init<PlayerDetailModel>("PlayerId");
            ModelRegistration.Init<CookieModel>("PlayerId", "Num");
            ModelRegistration.Init<KingdomMapModel>("PlayerId");
            ModelRegistration.Init<KingdomStructureModel>("SfId");
            ModelRegistration.Init<KingdomDecoModel>("PlayerId", "Num");
            //ModelRegistration.Init<PlacedKingdomItemModel>("Id");
            ModelRegistration.Init<ItemModel>("PlayerId", "Num");
            ModelRegistration.Init<PointModel>("PlayerId", "Num");
            ModelRegistration.Init<TicketModel>("PlayerId", "Num");
            ModelRegistration.Init<CashChangeLogModel>("SfId");
            ModelRegistration.Init<GachaLogModel>("SfId");
            ModelRegistration.Init<WorldModel>("PlayerId", "Num");
            ModelRegistration.Init<WorldStageModel>("PlayerId", "Num");

            // Center
            ModelRegistration.Init<ScheduleModel>("Num");

            if (APP.Cfg.DbType != DbType.InMemory)
            {
                ConnectionTest();
            }
        }

        private void ConnectionTest()
        {
            foreach (var connectionStr in APP.Cfg.UserDbConnectionStrList)
            {
                var excutor = DBSqlExecutor.StartTransaction(connectionStr, System.Data.IsolationLevel.ReadCommitted);
                excutor.Commit();
            }

            foreach (var connectionStr in APP.Cfg.AuthDbConnectionStrList)
            {
                var excutor = DBSqlExecutor.StartTransaction(connectionStr, System.Data.IsolationLevel.ReadCommitted);
                excutor.Commit();
            }

            foreach (var connectionStr in APP.Cfg.CenterDbConnectionStrList)
            {
                var excutor = DBSqlExecutor.StartTransaction(connectionStr, System.Data.IsolationLevel.ReadCommitted);
                excutor.Commit();
            }
        }
    }
}
