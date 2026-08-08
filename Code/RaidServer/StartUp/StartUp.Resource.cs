using Microsoft.Extensions.DependencyInjection;
using RaidServer.Context;
using Server.Repo;
using ServerCore;
using ServerCore.Extension;
using ServerCore.Repo.Cache;
using ServerCore.Repo.Database;
using WebStudyServer.Model;

namespace RaidServer
{
    public partial class Startup
    {
        public void Resource(IServiceCollection services)
        {
            // Db (보상 지급 등 공유 DB Write 용도)
            switch (Config<CoreConfig>.Get().DbType)
            {
                case DbType.MySql:
                    services.AddSingleton<IDbSessionFactory, MySqlDbSessionFactory>();
                    break;
                case DbType.InMemory:
                    services.AddSingleton<IDbSessionFactory, InMemoryDbSessionFactory>();
                    break;
                default:
                    throw new Exception($"No handling DbType({Config<CoreConfig>.Get().DbType})");
            }

            // Cache (세션 검증 등 공유 Redis 용도)
            services.AddCacheSession(Config<CoreConfig>.Get().CacheType, Config<CoreConfig>.Get().RedisConnectionString);

            services.AddSingleton<InMemoryStore>();

            services.AddScoped<DbSessionManager>();
            services.AddScoped<GlobalDbRepo>();

            services.AddScoped<RaidGameContext>();
            services.AddScoped<IGameContext>(sp => sp.GetRequiredService<RaidGameContext>());

            ModelRegistration.Init<SessionModel>("AccountId");
            ModelRegistration.Init<PlayerModel>("Id");
        }
    }
}
