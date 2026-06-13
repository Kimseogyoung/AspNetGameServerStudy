using Microsoft.Extensions.DependencyInjection;
using ServerCore;
using ServerCore.Repo.Cache;
using ServerCore.Repo.Database;
using StackExchange.Redis;

namespace RaidServer
{
    public partial class Startup
    {
        public void Resource(IServiceCollection services)
        {
            // Db (보상 지급 등 공유 DB Write 용도)
            switch (Core.Cfg.DbType)
            {
                case DbType.MySql:
                    services.AddSingleton<IDbSessionFactory, MySqlDbSessionFactory>();
                    break;
                case DbType.InMemory:
                    services.AddSingleton<IDbSessionFactory, InMemoryDbSessionFactory>();
                    break;
                default:
                    throw new Exception($"No handling DbType({Core.Cfg.DbType})");
            }

            // Cache (세션 검증 등 공유 Redis 용도)
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
                    throw new Exception($"No handling CacheType({Core.Cfg.CacheType})");
            }
        }
    }
}
