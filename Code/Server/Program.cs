using WebStudyServer;
var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);

startup.Config(builder);
startup.Logging(builder);

startup.Proto(builder.Services);
startup.Dependency(builder.Services);
startup.Resource(builder.Services);
startup.AddRpcMethod(builder.Services);

var app = builder.Build();
startup.AppConfigure(app, app.Environment);
app.Run();

