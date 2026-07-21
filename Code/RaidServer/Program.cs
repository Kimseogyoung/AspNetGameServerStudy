using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaidServer;
using ServerCore;

var builder = Host.CreateApplicationBuilder(args);

var startup = new Startup(builder.Configuration);

startup.Config(builder);
startup.Logging(builder);
startup.Resource(builder.Services);
startup.Dependency(builder.Services);

var host = builder.Build();
Logger.Init(host.Services.GetRequiredService<ILoggerFactory>());
host.Run();
