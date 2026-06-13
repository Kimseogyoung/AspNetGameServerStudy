using Microsoft.Extensions.DependencyInjection;
using Protocol.Raid;
using RaidServer.Context;
using Server.Repo;

namespace RaidServer.Network
{
    public class AuthPacketHandler : PacketHandlerBase<AuthReqPacket>
    {
        public override ushort Opcode => (ushort)EPacketType.AuthReq;

        public AuthPacketHandler(SessionService sessionService, IServiceScopeFactory scopeFactory)
        {
            _sessionService = sessionService;
            _scopeFactory = scopeFactory;
        }

        protected override Task RunAsync(string sessionId, AuthReqPacket req)
        {
            var res = Authenticate(sessionId, req);

            _sessionService.Send(sessionId, new MessagePacket
            {
                Opcode = (ushort)EPacketType.AuthRes,
                ProtocolType = EProtocolType.Json,
                Payload = res,
            });

            return Task.CompletedTask;
        }

        private AuthResPacket Authenticate(string sessionId, AuthReqPacket req)
        {
            if (string.IsNullOrEmpty(req.SessionKey))
            {
                return new AuthResPacket { Result = EAuthResult.InvalidRequest };
            }

            using var scope = _scopeFactory.CreateScope();
            var raidContext = scope.ServiceProvider.GetRequiredService<RaidGameContext>();
            raidContext.Init(req.DeviceKey);

            var dbRepo = scope.ServiceProvider.GetRequiredService<GlobalDbRepo>();

            try
            {
                if (!dbRepo.Auth.Session.TryGetByKey(req.SessionKey, out var mgrSession))
                {
                    dbRepo.Rollback();
                    return new AuthResPacket { Result = EAuthResult.SessionNotFound };
                }

                if (mgrSession.IsExpire())
                {
                    dbRepo.Rollback();
                    return new AuthResPacket { Result = EAuthResult.SessionExpired };
                }

                dbRepo.Commit();

                if (_sessionService.TryGetNetworkSession(sessionId, out var session))
                {
                    session.Authenticate(mgrSession.Model.AccountId, mgrSession.Model.PlayerId, mgrSession.Model.ShardId);
                }

                return new AuthResPacket
                {
                    Result = EAuthResult.Success,
                    AccountId = mgrSession.Model.AccountId,
                    PlayerId = mgrSession.Model.PlayerId,
                    ShardId = mgrSession.Model.ShardId,
                };
            }
            catch (Exception)
            {
                dbRepo.Rollback();
                throw;
            }
        }

        private readonly SessionService _sessionService;
        private readonly IServiceScopeFactory _scopeFactory;
    }
}
