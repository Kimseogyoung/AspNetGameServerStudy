using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RaidServer.Network;

namespace RaidServer
{
    public partial class Startup
    {
        public void AddPacketHandler(IServiceCollection services)
        {
            var handlerTypeList = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IPacketHandler).IsAssignableFrom(t));

            foreach (var handlerType in handlerTypeList)
            {
                services.AddSingleton(typeof(IPacketHandler), handlerType);
            }
        }
    }
}
