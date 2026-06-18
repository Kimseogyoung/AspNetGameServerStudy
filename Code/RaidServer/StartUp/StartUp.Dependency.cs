using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaidServer.Network;
using RaidServer.Services;

namespace RaidServer
{
    public partial class Startup
    {
        public void Dependency(IServiceCollection services)
        {
            services.AddHostedService<SocketClientListener>();
            services.AddHostedService<SessionTimeoutChecker>();
            services.AddSingleton<TickService>();
            services.AddHostedService(sp => sp.GetRequiredService<TickService>());

            services.AddSingleton<RaidConfig>();
            services.AddSingleton<SessionService>();
            services.AddSingleton<PlayerRaidSessionService>();
            services.AddSingleton<SocketService>();
            services.AddSingleton<PacketSerializerProvider>();
            services.AddSingleton<GameQueue>();
            services.AddSingleton<MatchingService>();

            AddPacketHandler(services);

            services.AddSingleton<PacketProcessor>();
        }
    }
}
