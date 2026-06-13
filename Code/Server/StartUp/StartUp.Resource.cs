using Microsoft.EntityFrameworkCore;
using Server.Repo;
using ServerCore;
using ServerCore.Extension;
using StackExchange.Redis;
using WebStudyServer.Model;
using ServerCore.Repo.Cache;
using ServerCore.Repo.Database;

namespace WebStudyServer
{
    public partial class Startup
    {
        public void Resource(IServiceCollection services)
        {
            services.AddScoped<UserLockService>();;

            // Db
            switch (Core.Cfg.DbType)
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
                    throw new Exception($"No handling DbType({Core.Cfg.DbType})");
            }

            // Cache
            switch (Core.Cfg.CacheType)
            {
                case CacheType.Redis:
                    services.AddSingleton<IConnectionMultiplexer>(
                    _ => ConnectionMultiplexer.Connect(Core.Cfg.RedisConnectionString));
                    services.AddScoped<InMemoryCacheLayer>();
                    services.AddScoped<RedisCacheLayer>();
                    services.AddScoped<ICacheSession, RedisCompositeCacheLayer>();
                    break;
                case CacheType.InMemory:
                    services.AddScoped<ICacheSession, InMemoryCacheLayer>();
                    break;
                default:
                    throw new Exception($"No handling DbType({Core.Cfg.DbType})");
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

            if (Core.Cfg.DbType != DbType.InMemory)
            {
                ConnectionTest();
            }
        }

        private void ConnectionTest()
        {
            foreach (var connectionStr in Core.Cfg.UserDbConnectionStrList)
            {
                var excutor = DBSqlExecutor.StartTransaction(connectionStr, System.Data.IsolationLevel.ReadCommitted);
                excutor.Commit();
            }

            foreach (var connectionStr in Core.Cfg.AuthDbConnectionStrList)
            {
                var excutor = DBSqlExecutor.StartTransaction(connectionStr, System.Data.IsolationLevel.ReadCommitted);
                excutor.Commit();
            }

            foreach (var connectionStr in Core.Cfg.CenterDbConnectionStrList)
            {
                var excutor = DBSqlExecutor.StartTransaction(connectionStr, System.Data.IsolationLevel.ReadCommitted);
                excutor.Commit();
            }
        }
    }
}
