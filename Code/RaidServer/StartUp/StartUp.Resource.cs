using Microsoft.Extensions.DependencyInjection;
using RaidServer.Context;
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

            services.AddScoped<RaidGameContext>();
            services.AddScoped<IGameContext>(sp => sp.GetRequiredService<RaidGameContext>());

            ModelRegistration.Init<SessionModel>("AccountId");
            ModelRegistration.Init<PlayerModel>("Id");

            // [Entity] 스캔 등록. Server 와 같은 목록을 갖게 된다.
            // 위 2줄은 병존 검증용이며 Server 쪽 목록과 함께 제거한다.
            //
            // 등록 범위가 2개에서 전체로 넓어진다. 지금까지는 RaidServer 가 등록하지
            // 않은 모델을 건드리면 InMemoryPkRegistry 의 미등록 예외로 막혔는데,
            // 그 런타임 가드는 사라진다. 대신 두 호스트의 목록이 어긋날 수 없게 된다.
            EntityRegistry.ScanAndRegister(typeof(PlayerModel).Assembly);

            // 손으로 유지하는 캐시 태그 맵을 [Entity] 와 대조한다.
            EntityMeta.VerifyCacheTags();
        }
    }
}
