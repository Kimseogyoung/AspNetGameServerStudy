using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ServerCore.Repo.Cache
{
    // RedisCacheLayer가 internal(Composite 전용)이라 Server/RaidServer 등 다른 프로젝트에서
    // 직접 AddScoped<RedisCacheLayer>()를 호출할 수 없다 - 그래서 등록 자체를 여기로 옮겼다.
    public static class CacheServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheSession(this IServiceCollection services, CacheType cacheType, string redisConnectionString)
        {
            switch (cacheType)
            {
                case CacheType.Redis:
                    services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
                    services.AddScoped<InMemoryCacheLayer>();
                    services.AddScoped<RedisCacheLayer>();
                    services.AddScoped<ICacheSession, RedisCompositeCacheLayer>();
                    break;
                case CacheType.InMemory:
                    services.AddScoped<ICacheSession, InMemoryCacheLayer>();
                    break;
                default:
                    throw new NotSupportedException($"NotSupportedCacheType({cacheType})");
            }

            return services;
        }
    }
}
