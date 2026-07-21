using Microsoft.OpenApi.Models;
using Server.Service;
using ServerCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebStudyServer.Filter;
using WebStudyServer.Service;

namespace WebStudyServer
{
    public partial class Startup
    {

        public void Dependency(IServiceCollection services)
        {
            AddMiddlewares(services);
            AddFilters(services);
            AddServices(services);
            AddRaidServerLauncher(services);

            AddController(services);
            AddSwagger(services);
        }

        private void AddMiddlewares(IServiceCollection services)
        {
        }

        private void AddFilters(IServiceCollection services)
        {
            services.AddScoped<LogFilter>();
        }

        private void AddRaidServerLauncher(IServiceCollection services)
        {
            if (Configuration.GetValue<bool>("RaidServer:Enabled"))
            {
                services.AddHostedService<RaidServerLauncher>();
            }
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddAutoMapper((config) =>
            {
                config.AddProfile(typeof(AutoMapperProfile));
            });

            services.AddScoped<ErrorHandler>();
            services.AddScoped<CommonService>();
            services.AddScoped<AuthService>();
            services.AddScoped<GameService>();
            services.AddScoped<CheatService>();

            services.AddScoped<RpcContext>();
            services.AddScoped<IGameContext>(sp => sp.GetRequiredService<RpcContext>());
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
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Default" }
                };
                /*                    var opsSecurityScheme = new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Ops" }
                                };
                */
                operation.Security =
                [
                    new()
                    {
                        { securityScheme, new List<string>() }
                    },
/*                        new()
                    {
                        { opsSecurityScheme, new List<string>() }
                    }*/
                ];
            }
        }
    }
}
