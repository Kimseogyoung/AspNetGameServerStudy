
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

/*
    { 205, new ApiFunc(){ ApiPath = typeof(KingdomConstructDecoReqPacket).ToString(), Desc = "KingdomDeco 건설 (Num , X, Y)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomConstructDeco(int.Parse(valueArr[0]), int.Parse(valueArr[1]), int.Parse(valueArr[2])) } },
    { 206, new ApiFunc(){ ApiPath = typeof(KingdomFinishCraftStructureReqPacket).ToString(), Desc = "KingdomStructure 생산 물품 받기 (StructureId)",
        Action = async (valueArr) =>  await APP.Ctx.RequestKingdomFinishCraftStructure(ulong.Parse(valueArr[0]))} },

    { 300, new ApiFunc(){ ApiPath = "CookieList Print", Desc = "", Action = (valueArr) => { APP.Ctx.PrintCookieList(); return Task.CompletedTask; } } },
    { 301, new ApiFunc(){ ApiPath = typeof(CookieEnhanceStarReqPacket).ToString(), Desc = "Cookie Enhance Star (CookieNum, Star)",
        Action = async (valueArr) =>  await APP.Ctx.RequestCookieEnhanceStar(int.Parse(valueArr[0]), int.Parse(valueArr[1])) } },
    { 302, new ApiFunc(){ ApiPath = typeof(CookieEnhanceLvReqPacket).ToString(), Desc = "Cookie Enhance Lv (CookieNum, Lv)",
        Action = async (valueArr) =>  await APP.Ctx.RequestCookieEnhanceLv(int.Parse(valueArr[0]), int.Parse(valueArr[1])) } },


    { 400, new ApiFunc(){ ApiPath = typeof(GachaNormalReqPacket).ToString(), Desc = "GachaNormal (ScheduleNum, Cnt)",
        Action = async (valueArr) =>  await APP.Ctx.RequestGachaNormal(int.Parse(valueArr[0]), int.Parse(valueArr[1])) } },

    { 500, new ApiFunc(){ ApiPath = typeof(ScheduleLoadReqPacket).ToString(), Desc = "ScheduleLoad ",
        Action = async (valueArr) =>  await APP.Ctx.RequestLoadSchedule() }},

    { 9001, new ApiFunc(){ ApiPath = typeof(CheatRewardReqPacket).ToString(), Desc = "Chaet 보상 획득 (ObjType, ObjNum, ObjAmount)",*/
            var rpcMethodList = new List<IRpcMethod>()
            {
                new RpcMethod<AuthService, AuthSignUpReqPacket, AuthSignUpResPacket>("auth/sign-up", (authSvc, req) => { return authSvc.SignUp(req.DeviceKey); }),
                new RpcMethod<AuthService, AuthSignInReqPacket, AuthSignInResPacket>("auth/sign-in", (authSvc, req) => { return authSvc.SignIn(req.ChannelId); }),

                new RpcMethod<GameService, GameEnterReqPacket, GameEnterResPacket>("game/enter", (gameSvc, req) => { return gameSvc.Enter(req); }),
                new RpcMethod<GameService, KingdomBuyStructureReqPacket, KingdomBuyStructureResPacket>("kingdom/buy-structure", (gameSvc, req) => { return gameSvc.KingdomStructureBuy(req); }),
                new RpcMethod<GameService, KingdomConstructStructureReqPacket, KingdomConstructStructureResPacket>("kingdom/construct-structure", (gameSvc, req) => { return gameSvc.KingdomConstructStructure(req); }),
                new RpcMethod<GameService, KingdomFinishConstructStructureReqPacket, KingdomFinishConstructStructureResPacket>("kingdom/finish-construct-structure", (gameSvc, req) => { return gameSvc.KingdomFinishConstructStructure(req); }),
                new RpcMethod<GameService, KingdomBuyDecoReqPacket, KingdomBuyDecoResPacket>("kingdom/buy-deco", (gameSvc, req) => { return gameSvc.KingdomDecoBuy(req); }),
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
