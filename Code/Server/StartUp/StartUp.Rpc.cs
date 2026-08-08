
using Protocol;
using Server;
using Server.Service;
using WebStudyServer.Service;

namespace WebStudyServer
{
    public partial class Startup
    {
        public void AddRpcMethod(IServiceCollection services)
        {
            // RpcMethod 등록
            var rpcMethodList = new List<IRpcMethod>()
            {
                new RpcMethod<CommonService, HealthCheckRequestPacket, HealthCheckResponsePacket>(HealthCheckRequestPacket.NAME, (commonSvc, req) => { return commonSvc.HealthCheck(); }),

                // Auth api는 RpcMethod를 사용하여 인증 없이도 사용할 수 있도록 함.
                new RpcMethod<AuthService, AuthSignUpRequestPacket, AuthSignUpResponsePacket>(AuthSignUpRequestPacket.NAME, (authSvc, req) => { return authSvc.SignUpAsync(req.DeviceKey); }),
                new RpcMethod<AuthService, AuthSignInRequestPacket, AuthSignInResponsePacket>(AuthSignInRequestPacket.NAME, (authSvc, req) => { return authSvc.SignInAsync(req.ChannelId); }),

                // enter는 Player가 안생겨있을 수 있으므로 includePlayer를 false로 설정
                new RpcGameMethod<GameService, GameEnterRequestPacket, GameEnterResponsePacket>(GameEnterRequestPacket.NAME, (gameSvc, req) => { return gameSvc.EnterAsync(req); }, includePlayer: false),

                // game api들은 RpcGameMethod를 사용해서 인증된 사용자만 사용할 수 있도록 함
                new RpcGameMethod<KingdomService, KingdomBuyStructureRequestPacket, KingdomBuyStructureResponsePacket>(KingdomBuyStructureRequestPacket.NAME, (kingdomSvc, req) => { return kingdomSvc.KingdomStructureBuyAsync(req); }),
                new RpcGameMethod<KingdomService, KingdomConstructStructureRequestPacket, KingdomConstructStructureResponsePacket>(KingdomConstructStructureRequestPacket.NAME, (kingdomSvc, req) => { return kingdomSvc.KingdomConstructStructureAsync(req); }),
                new RpcGameMethod<KingdomService, KingdomFinishConstructStructureRequestPacket, KingdomFinishConstructStructureResponsePacket>(KingdomFinishConstructStructureRequestPacket.NAME, (kingdomSvc, req) => { return kingdomSvc.KingdomFinishConstructStructureAsync(req); }),
                new RpcGameMethod<KingdomService, KingdomBuyDecoRequestPacket, KingdomBuyDecoResponsePacket>(KingdomBuyDecoRequestPacket.NAME, (kingdomSvc, req) => { return kingdomSvc.KingdomDecoBuyAsync(req); }),
                new RpcGameMethod<KingdomService, KingdomConstructDecoRequestPacket, KingdomConstructDecoResponsePacket>(KingdomConstructDecoRequestPacket.NAME, (kingdomSvc, req) => { return kingdomSvc.KingdomConstructDecoAsync(req); }),
                new RpcGameMethod<KingdomService, KingdomFinishCraftStructureRequestPacket, KingdomFinishCraftStructureResponsePacket>(KingdomFinishCraftStructureRequestPacket.NAME, (kingdomSvc, req) => { return kingdomSvc.KingdomFinishCraftStructureAsync(req); }),
                new RpcGameMethod<CookieService, CookieEnhanceStarRequestPacket, CookieEnhanceStarResponsePacket>(CookieEnhanceStarRequestPacket.NAME, (cookieSvc, req) => { return cookieSvc.EnhanceCookieStarAsync(req); }),
                new RpcGameMethod<CookieService, CookieEnhanceLvRequestPacket, CookieEnhanceLvResponsePacket>(CookieEnhanceLvRequestPacket.NAME, (cookieSvc, req) => { return cookieSvc.EnhanceCookieLvAsync(req); }),
                new RpcGameMethod<GachaService, GachaNormalRequestPacket, GachaNormalResponsePacket>(GachaNormalRequestPacket.NAME, (gachaSvc, req) => { return gachaSvc.GachaNormalAsync(req); }),
                new RpcGameMethod<GachaService, ScheduleLoadRequestPacket, ScheduleLoadResponsePacket>(ScheduleLoadRequestPacket.NAME, (gachaSvc, req) => { return Task.FromResult(gachaSvc.LoadSchedule(req)); }),
                new RpcGameMethod<WorldService, WorldFinishStageFirstRequestPacket, WorldFinishStageFirstResponsePacket>(WorldFinishStageFirstRequestPacket.NAME, (worldSvc, req) => { return worldSvc.WorldFinishStageFirstAsync(req); }),
                new RpcGameMethod<WorldService, WorldFinishStageRepeatRequestPacket, WorldFinishStageRepeatResponsePacket>(WorldFinishStageRepeatRequestPacket.NAME, (worldSvc, req) => { return worldSvc.WorldFinishStageRepeatAsync(req); }),
                new RpcGameMethod<WorldService, WorldRewardStarRequestPacket, WorldRewardStarResponsePacket>(WorldRewardStarRequestPacket.NAME, (worldSvc, req) => { return worldSvc.WorldRewardStarAsync(req); }),

                new RpcGameMethod<CheatService, CheatRewardRequestPacket, CheatRewardResponsePacket>(CheatRewardRequestPacket.NAME, (cheatSvc, req) => { return cheatSvc.RewardAsync(req); }),

            };
            services.AddSingleton(sp => new RpcMethodRegistry(rpcMethodList));
            services.AddScoped<RpcService>();
        }
    }
}
