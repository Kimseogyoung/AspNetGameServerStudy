
using WebStudyServer.Service;
using Server.Service;
using Server;
using Protocol;

namespace WebStudyServer
{
    public partial class Startup
    {
        public void AddRpcMethod(IServiceCollection services)
        {
            // RpcMethod 등록
            var rpcMethodList = new List<IRpcMethod>()
            {
                new RpcMethod<CommonService, HealthCheckReqPacket, HealthCheckResPacket>(HealthCheckReqPacket.NAME, (commonSvc, req) => { return commonSvc.HealthCheck(); }),

                // Auth api는 RpcMethod를 사용하여 인증 없이도 사용할 수 있도록 함.
                new RpcMethod<AuthService, AuthSignUpReqPacket, AuthSignUpResPacket>(AuthSignUpReqPacket.NAME, (authSvc, req) => { return authSvc.SignUp(req.DeviceKey); }),
                new RpcMethod<AuthService, AuthSignInReqPacket, AuthSignInResPacket>(AuthSignInReqPacket.NAME, (authSvc, req) => { return authSvc.SignIn(req.ChannelId); }),

                // enter는 Player가 안생겨있을 수 있으므로 includePlayer를 false로 설정
                new RpcGameMethod<GameService, GameEnterReqPacket, GameEnterResPacket>(GameEnterReqPacket.NAME, (gameSvc, req) => { return gameSvc.Enter(req); }, includePlayer: false),

                // game api들은 RpcGameMethod를 사용해서 인증된 사용자만 사용할 수 있도록 함
                new RpcGameMethod<GameService, KingdomBuyStructureReqPacket, KingdomBuyStructureResPacket>(KingdomBuyStructureReqPacket.NAME, (gameSvc, req) => { return gameSvc.KingdomStructureBuy(req); }),
                new RpcGameMethod<GameService, KingdomConstructStructureReqPacket, KingdomConstructStructureResPacket>(KingdomConstructStructureReqPacket.NAME, (gameSvc, req) => { return gameSvc.KingdomConstructStructure(req); }),
                new RpcGameMethod<GameService, KingdomFinishConstructStructureReqPacket, KingdomFinishConstructStructureResPacket>(KingdomFinishConstructStructureReqPacket.NAME, (gameSvc, req) => { return gameSvc.KingdomFinishConstructStructure(req); }),
                new RpcGameMethod<GameService, KingdomBuyDecoReqPacket, KingdomBuyDecoResPacket>(KingdomBuyDecoReqPacket.NAME, (gameSvc, req) => { return gameSvc.KingdomDecoBuy(req); }),
                new RpcGameMethod<GameService, KingdomConstructDecoReqPacket, KingdomConstructDecoResPacket>(KingdomConstructDecoReqPacket.NAME, (gameSvc, req) => { return gameSvc.KingdomConstructDeco(req); }),
                new RpcGameMethod<GameService, KingdomFinishCraftStructureReqPacket, KingdomFinishCraftStructureResPacket>(KingdomFinishCraftStructureReqPacket.NAME, (gameSvc, req) => { return gameSvc.KingdomFinishCraftStructure(req); }),
                new RpcGameMethod<GameService, CookieEnhanceStarReqPacket, CookieEnhanceStarResPacket>(CookieEnhanceStarReqPacket.NAME, (gameSvc, req) => { return gameSvc.EnhanceCookieStar(req); }),
                new RpcGameMethod<GameService, CookieEnhanceLvReqPacket, CookieEnhanceLvResPacket>(CookieEnhanceLvReqPacket.NAME, (gameSvc, req) => { return gameSvc.EnhanceCookieLv(req); }),
                new RpcGameMethod<GameService, GachaNormalReqPacket, GachaNormalResPacket>(GachaNormalReqPacket.NAME, (gameSvc, req) => { return gameSvc.GachaNormal(req); }),
                new RpcGameMethod<GameService, ScheduleLoadReqPacket, ScheduleLoadResPacket>(ScheduleLoadReqPacket.NAME, (gameSvc, req) => { return gameSvc.LoadSchedule(req); }),
                new RpcGameMethod<GameService, WorldFinishStageFirstReqPacket, WorldFinishStageFirstResPacket>(WorldFinishStageFirstReqPacket.NAME, (gameSvc, req) => { return gameSvc.WorldFinishStageFirst(req); }),
                new RpcGameMethod<GameService, WorldFinishStageRepeatReqPacket, WorldFinishStageRepeatResPacket>(WorldFinishStageRepeatReqPacket.NAME, (gameSvc, req) => { return gameSvc.WorldFinishStageRepeat(req); }),
                new RpcGameMethod<GameService, WorldRewardStarReqPacket, WorldRewardStarResPacket>(WorldRewardStarReqPacket.NAME, (gameSvc, req) => { return gameSvc.WorldRewardStar(req); }),

                new RpcGameMethod<CheatService, CheatRewardReqPacket, CheatRewardResPacket>(CheatRewardReqPacket.NAME, (cheatSvc, req) => { return cheatSvc.Reward(req); }),

            };
            services.AddSingleton(sp => new RpcService(rpcMethodList, sp.GetRequiredService<ILogger<RpcService>>()));
        }
    }
}
