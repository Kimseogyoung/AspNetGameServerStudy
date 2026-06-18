
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
                new RpcMethod<AuthService, AuthSignUpRequestPacket, AuthSignUpResponsePacket>(AuthSignUpRequestPacket.NAME, (authSvc, req) => { return authSvc.SignUp(req.DeviceKey); }),
                new RpcMethod<AuthService, AuthSignInRequestPacket, AuthSignInResponsePacket>(AuthSignInRequestPacket.NAME, (authSvc, req) => { return authSvc.SignIn(req.ChannelId); }),

                // enter는 Player가 안생겨있을 수 있으므로 includePlayer를 false로 설정
                new RpcGameMethod<GameService, GameEnterRequestPacket, GameEnterResponsePacket>(GameEnterRequestPacket.NAME, (gameSvc, req) => { return gameSvc.Enter(req); }, includePlayer: false),

                // game api들은 RpcGameMethod를 사용해서 인증된 사용자만 사용할 수 있도록 함
                new RpcGameMethod<GameService, KingdomBuyStructureRequestPacket, KingdomBuyStructureResponsePacket>(KingdomBuyStructureRequestPacket.NAME, (gameSvc, req) => { return gameSvc.KingdomStructureBuy(req); }),
                new RpcGameMethod<GameService, KingdomConstructStructureRequestPacket, KingdomConstructStructureResponsePacket>(KingdomConstructStructureRequestPacket.NAME, (gameSvc, req) => { return gameSvc.KingdomConstructStructure(req); }),
                new RpcGameMethod<GameService, KingdomFinishConstructStructureRequestPacket, KingdomFinishConstructStructureResponsePacket>(KingdomFinishConstructStructureRequestPacket.NAME, (gameSvc, req) => { return gameSvc.KingdomFinishConstructStructure(req); }),
                new RpcGameMethod<GameService, KingdomBuyDecoRequestPacket, KingdomBuyDecoResponsePacket>(KingdomBuyDecoRequestPacket.NAME, (gameSvc, req) => { return gameSvc.KingdomDecoBuy(req); }),
                new RpcGameMethod<GameService, KingdomConstructDecoRequestPacket, KingdomConstructDecoResponsePacket>(KingdomConstructDecoRequestPacket.NAME, (gameSvc, req) => { return gameSvc.KingdomConstructDeco(req); }),
                new RpcGameMethod<GameService, KingdomFinishCraftStructureRequestPacket, KingdomFinishCraftStructureResponsePacket>(KingdomFinishCraftStructureRequestPacket.NAME, (gameSvc, req) => { return gameSvc.KingdomFinishCraftStructure(req); }),
                new RpcGameMethod<GameService, CookieEnhanceStarRequestPacket, CookieEnhanceStarResponsePacket>(CookieEnhanceStarRequestPacket.NAME, (gameSvc, req) => { return gameSvc.EnhanceCookieStar(req); }),
                new RpcGameMethod<GameService, CookieEnhanceLvRequestPacket, CookieEnhanceLvResponsePacket>(CookieEnhanceLvRequestPacket.NAME, (gameSvc, req) => { return gameSvc.EnhanceCookieLv(req); }),
                new RpcGameMethod<GameService, GachaNormalRequestPacket, GachaNormalResponsePacket>(GachaNormalRequestPacket.NAME, (gameSvc, req) => { return gameSvc.GachaNormal(req); }),
                new RpcGameMethod<GameService, ScheduleLoadRequestPacket, ScheduleLoadResponsePacket>(ScheduleLoadRequestPacket.NAME, (gameSvc, req) => { return gameSvc.LoadSchedule(req); }),
                new RpcGameMethod<GameService, WorldFinishStageFirstRequestPacket, WorldFinishStageFirstResponsePacket>(WorldFinishStageFirstRequestPacket.NAME, (gameSvc, req) => { return gameSvc.WorldFinishStageFirst(req); }),
                new RpcGameMethod<GameService, WorldFinishStageRepeatRequestPacket, WorldFinishStageRepeatResponsePacket>(WorldFinishStageRepeatRequestPacket.NAME, (gameSvc, req) => { return gameSvc.WorldFinishStageRepeat(req); }),
                new RpcGameMethod<GameService, WorldRewardStarRequestPacket, WorldRewardStarResponsePacket>(WorldRewardStarRequestPacket.NAME, (gameSvc, req) => { return gameSvc.WorldRewardStar(req); }),

                new RpcGameMethod<CheatService, CheatRewardRequestPacket, CheatRewardResponsePacket>(CheatRewardRequestPacket.NAME, (cheatSvc, req) => { return cheatSvc.Reward(req); }),

            };
            services.AddSingleton(sp => new RpcService(rpcMethodList, sp.GetRequiredService<ILogger<RpcService>>()));
        }
    }
}
