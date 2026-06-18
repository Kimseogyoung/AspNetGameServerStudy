using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Protocol.Raid;
using RaidServer.Context;
using RaidServer.Network;
using Server.Repo;
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

        public AuthResponsePacket Authenticate(string sessionId, AuthRequestPacket req)
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
            try
            {
                if (!dbRepo.Auth.Session.TryGetByKey(req.SessionKey, out var mgrSession))
                {
                    return new AuthResponsePacket { Result = EAuthResult.SessionNotFound };
                }

                if (mgrSession.IsExpire())
                {
                    return new AuthResponsePacket { Result = EAuthResult.SessionExpired };
                }

                raidContext.SetAccountId(mgrSession.Model.AccountId);
                raidContext.SetPlayerId(mgrSession.Model.PlayerId);
                raidContext.SetShardId(mgrSession.Model.ShardId);

                dbRepo.BeginOwnUserRepo();
                var playerModel = dbRepo.OwnUser.Player.Get().Model;

                // 재접속: 기존 세션 교체
                if (_byPlayerId.TryGetValue(playerModel.Id, out var existing))
                {
                    _bySessionId.TryRemove(existing.SessionId, out _);
                    _logger.LogInformation($"AUTH_RECONNECT PlayerId({playerModel.Id}) OldSession({existing.SessionId})");
                }

                var raidSession = new PlayerRaidSession
                {
                    SessionId = sessionId,
                    ShardId = mgrSession.Model.ShardId,
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
                dbRepo.Rollback();
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
