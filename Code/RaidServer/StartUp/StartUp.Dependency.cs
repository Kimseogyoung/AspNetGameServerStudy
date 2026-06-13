using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaidServer.Network;

namespace RaidServer
{
    public partial class Startup
    {
        public void Dependency(IServiceCollection services)
        {
            services.AddHostedService<SocketClientListener>();
            services.AddHostedService<SessionTimeoutChecker>();

            services.AddSingleton<RaidConfig>();
            services.AddSingleton<SessionService>();
            services.AddSingleton<SocketService>();
            services.AddSingleton<PacketSerializerProvider>();

            AddPacketHandler(services);

            services.AddSingleton<PacketProcessor>();
            services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<PacketProcessor>());
        }
    }
}
