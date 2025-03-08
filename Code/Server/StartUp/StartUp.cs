using Microsoft.OpenApi.Models;
using ProtoBuf.Meta;
using Server;
using Server.Service;
using System.Net;
using WebStudyServer.Filter;
using WebStudyServer.Manager;
using WebStudyServer.Middleware;

namespace WebStudyServer
{
    public partial class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // ------------------ WebApplication
        public void AppConfigure(WebApplication app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            app.UseMiddleware<ReqMiddleware>();

            app.UseExceptionHandler(builder => builder.Run(
                context =>
                {
                    var errorHandler = context.RequestServices.GetRequiredService<ErrorHandler>();
                    return errorHandler.Handle(context);
                }
            ));

            app.MapAllPostRpc("rpc");
            //app.MapGet("/game/enter", (GameService gameSvc, HttpContext httpCtx) => gameSvc.Enter(limit));
        }
    }
}
