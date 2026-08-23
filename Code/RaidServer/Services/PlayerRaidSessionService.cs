using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Protocol.Raid;
using RaidServer.Context;
using RaidServer.Network;
using Server.Repo;
using WebStudyServer.Data;
using WebStudyServer.Data.Queries;
using WebStudyServer.Model;

namespace RaidServer.Services
{
    public class PlayerRaidSessionService
    {
        public PlayerRaidSessionService(SessionService sessionService, IServiceScopeFactory scopeFactory, ILogger<PlayerRaidSessionService> logger)
        {
            _sessionService = sessionService;
            _scopeFactory = scopeFactory;
            _logger = logger;

            sessionService.RegisterCloseListener(OnSessionClosed);
        }

        public async Task<AuthResponsePacket> AuthenticateAsync(string sessionId, AuthRequestPacket req)
        {
            if (string.IsNullOrEmpty(req.SessionKey))
            {
                return new AuthResponsePacket { Result = EAuthResult.InvalidRequest };
            }

            if (!_sessionService.TryGetNetworkSession(sessionId, out var session))
            {
                return new AuthResponsePacket { Result = EAuthResult.InvalidRequest };
            }

            using var scope = _scopeFactory.CreateScope();
            var raidContext = scope.ServiceProvider.GetRequiredService<RaidGameContext>();
            raidContext.Init(req.DeviceKey);

            using var dbRepo = scope.ServiceProvider.GetRequiredService<GlobalDbRepo>();
            var db = scope.ServiceProvider.GetRequiredService<GameDb>();
            try
            {
                var (foundSession, mdlSession) = await db.Sessions.TryGetByKeyAsync(req.SessionKey);
                if (!foundSession)
                {
                    return new AuthResponsePacket { Result = EAuthResult.SessionNotFound };
                }

                // 소켓 인증에서는 세션을 연장하지 않는다. 읽기만 한다.
                if (mdlSession.IsExpire())
                {
                    return new AuthResponsePacket { Result = EAuthResult.SessionExpired };
                }

                raidContext.SetAccountId(mdlSession.AccountId);
                raidContext.SetPlayerId(mdlSession.PlayerId);
                raidContext.SetShardId(mdlSession.ShardId);

                // 세션이 ShardId 를 들고 있으므로 대상을 바로 연다. 앰비언트("나")에 묶이지 않는다.
                var userScope = db.User(mdlSession.ShardId, mdlSession.PlayerId);
                var playerModel = await userScope.Owned<PlayerModel>().GetAsync();

                // 재접속: 기존 세션 교체
                if (_byPlayerId.TryGetValue(playerModel.Id, out var existing))
                {
                    _bySessionId.TryRemove(existing.SessionId, out _);
                    _logger.LogInformation($"AUTH_RECONNECT PlayerId({playerModel.Id}) OldSession({existing.SessionId})");
                }

                var raidSession = new PlayerRaidSession
                {
                    SessionId = sessionId,
                    ShardId = mdlSession.ShardId,
                    Player = playerModel,
                };
                _bySessionId[sessionId] = raidSession;
                _byPlayerId[playerModel.Id] = raidSession;
                session.Authenticate();

                return new AuthResponsePacket
                {
                    Result = EAuthResult.Success,
                    AccountId = raidSession.Player.AccountId,
                    PlayerId = raidSession.Player.Id,
                    ShardId = raidSession.ShardId,
                };
            }
            catch (Exception)
            {
                await dbRepo.RollbackAsync();
                throw;
            }
        }

        public bool TryGetBySessionId(string sessionId, out PlayerRaidSession? raidSession)
        {
            return _bySessionId.TryGetValue(sessionId, out raidSession);
        }

        public bool TryGetByPlayerId(ulong playerId, out PlayerRaidSession? raidSession)
        {
            return _byPlayerId.TryGetValue(playerId, out raidSession);
        }

        private void Unregister(string sessionId)
        {
            if (!_bySessionId.TryRemove(sessionId, out var raidSession))
            {
                return;
            }

            // 재접속으로 이미 교체된 경우 _byPlayerId는 건드리지 않는다
            _byPlayerId.TryRemove(new KeyValuePair<ulong, PlayerRaidSession>(raidSession.Player.Id, raidSession));
        }

        private void OnSessionClosed(NetworkSession session)
        {
            Unregister(session.Id);
        }

        private readonly ConcurrentDictionary<string, PlayerRaidSession> _bySessionId = new();
        private readonly ConcurrentDictionary<ulong, PlayerRaidSession> _byPlayerId = new();
        private readonly SessionService _sessionService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
    }
}
