using System.Net;
using Microsoft.OpenApi.Models;
using ProtoBuf.Meta;
using Server;
using Server.Service;
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
            // 예외 응답 생성은 이 핸들러 하나만 담당한다. 아래 어떤 미들웨어/엔드포인트에서
            // 예외가 나든 여기로 모이도록, 반드시 파이프라인 맨 앞에 등록한다.
            app.UseExceptionHandler(builder => builder.Run(
                context =>
                {
                    var errorHandler = context.RequestServices.GetRequiredService<ErrorHandler>();
                    return errorHandler.Handle(context);
                }
            ));

            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            app.UseMiddleware<ReqMiddleware>();

            app.MapAllPostRpc("rpc");
            //app.MapGet("/game/enter", (GameService gameSvc, HttpContext httpCtx) => gameSvc.Enter(limit));
        }
    }
}
