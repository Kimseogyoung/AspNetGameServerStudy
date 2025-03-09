
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Proto;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;
using System.Net.Mime;
using WebStudyServer.Component;
using WebStudyServer.Filter;
using WebStudyServer.Manager;
using WebStudyServer.Service;
using Server.Service;
using Microsoft.Extensions.Options;
using Server;
using Protocol;

namespace WebStudyServer
{
    public partial class Startup
    {

        public void Dependency(IServiceCollection services)
        {
            AddMiddlewares(services);
            AddFilters(services);
            AddServices(services);

            AddController(services);
            AddSwagger(services);
        }

        private void AddMiddlewares(IServiceCollection services)
        {
        }

        private void AddFilters(IServiceCollection services)
        {
            services.AddScoped<LogFilter>();
            services.AddScoped<AuthFilter>();
            services.AddScoped<AuthTransactionFilter>();
            services.AddScoped<UserTransactionFilter>();
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(AutoMapperProfile));

            services.AddScoped<ErrorHandler>();
            services.AddScoped<UserLockService>();

            services.AddScoped<AuthService>();
            services.AddScoped<GameService>();
            services.AddScoped<CheatService>();

            services.AddScoped<RpcContext>();

            var rpcMethodList = new List<IRpcMethod>()
            {
                new RpcMethod<AuthService, AuthSignUpReqPacket, AuthSignUpResPacket>("auth/sign-up", (authSvc, req) => { return authSvc.SignUp(req.DeviceKey); }),
                new RpcMethod<AuthService, AuthSignInReqPacket, AuthSignInResPacket>("auth/sign-in", (authSvc, req) => { return authSvc.SignIn(req.ChannelId); }),

                // enter는 Player가 안생겨있을 수 있으므로 includePlayer를 false로 설정
                new RpcGameMethod<GameService, GameEnterReqPacket, GameEnterResPacket>("game/enter", (gameSvc, req) => { return gameSvc.Enter(req); }, includePlayer: false),

                new RpcGameMethod<GameService, KingdomBuyStructureReqPacket, KingdomBuyStructureResPacket>("kingdom/buy-structure", (gameSvc, req) => { return gameSvc.KingdomStructureBuy(req); }),
                new RpcGameMethod<GameService, KingdomConstructStructureReqPacket, KingdomConstructStructureResPacket>("kingdom/construct-structure", (gameSvc, req) => { return gameSvc.KingdomConstructStructure(req); }),
                new RpcGameMethod<GameService, KingdomFinishConstructStructureReqPacket, KingdomFinishConstructStructureResPacket>("kingdom/finish-construct-structure", (gameSvc, req) => { return gameSvc.KingdomFinishConstructStructure(req); }),
                new RpcGameMethod<GameService, KingdomBuyDecoReqPacket, KingdomBuyDecoResPacket>("kingdom/buy-deco", (gameSvc, req) => { return gameSvc.KingdomDecoBuy(req); }),
                new RpcGameMethod<GameService, KingdomConstructDecoReqPacket, KingdomConstructDecoResPacket>("kingdom/consturct-deco", (gameSvc, req) => { return gameSvc.KingdomConstructDeco(req); }),
                new RpcGameMethod<GameService, KingdomFinishCraftStructureReqPacket, KingdomFinishCraftStructureResPacket>("kingdom/finish-craft-structure", (gameSvc, req) => { return gameSvc.KingdomFinishCraftStructure(req); }),
                new RpcGameMethod<GameService, CookieEnhanceStarReqPacket, CookieEnhanceStarResPacket>("cookie/enhance-star", (gameSvc, req) => { return gameSvc.EnhanceCookieStar(req); }),
                new RpcGameMethod<GameService, CookieEnhanceLvReqPacket, CookieEnhanceLvResPacket>("cookie/enhance-lv", (gameSvc, req) => { return gameSvc.EnhanceCookieLv(req); }),
                new RpcGameMethod<GameService, GachaNormalReqPacket, GachaNormalResPacket>("gacha/normal", (gameSvc, req) => { return gameSvc.GachaNormal(req); }),
                new RpcGameMethod<GameService, ScheduleLoadReqPacket, ScheduleLoadResPacket>("schedule/load", (gameSvc, req) => { return gameSvc.LoadSchedule(req); }),

                new RpcGameMethod<CheatService, CheatRewardReqPacket, CheatRewardResPacket>("cheat/reward", (cheatSvc, req) => { return cheatSvc.Reward(req); }),

            };
            services.AddSingleton(sp => new RpcService(rpcMethodList, sp.GetRequiredService<ILogger<RpcService>>()));
        }

        private void AddController(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.InputFormatters.Insert(0, new CustomInputFormatter());
                options.OutputFormatters.Insert(0, new CustomOutputFormatter());
            });

            //net 6.0
/*            ).AddMvcOptions(options =>
            {
                options.InputFormatters.Insert(0, new CustomInputFormatter());
                options.OutputFormatters.Insert(0, new CustomOutputFormatter());
            });*/
        }

        private void AddSwagger(IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.OperationFilter<SwaggerOperationFilter>();
            });
        }

        class SwaggerOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                // swagger default req 설정되도록 함.
                var securityScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme,  Id = "Default" }
                };
/*                    var opsSecurityScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Ops" }
                };
*/
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new()
                    {
                        { securityScheme, new List<string>() }
                    },
/*                        new()
                    {
                        { opsSecurityScheme, new List<string>() }
                    }*/
                };
            }
        }
    }
}
