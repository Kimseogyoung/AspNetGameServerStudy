using Microsoft.Extensions.Hosting;
using RaidServer;

var builder = Host.CreateApplicationBuilder(args);

var startup = new Startup(builder.Configuration);

startup.Config(builder);
startup.Logging(builder);
startup.Resource(builder.Services);
startup.Dependency(builder.Services);

var host = builder.Build();
host.Run();
