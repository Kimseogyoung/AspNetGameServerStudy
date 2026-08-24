using Microsoft.Extensions.DependencyInjection;
using ServerCore;
using ServerCore.Extension;
using WebStudyServer.Data;
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
            services.AddScoped<GameDb>();

            // [Entity] 가 붙은 모델을 전부 등록한다. Server 와 같은 목록을 갖는다.
            EntityRegistry.ScanAndRegister(typeof(PlayerModel).Assembly);

            // 손으로 유지하는 캐시 태그 맵을 [Entity] 와 대조한다.
            EntityMeta.VerifyCacheTags();
        }
    }
}
