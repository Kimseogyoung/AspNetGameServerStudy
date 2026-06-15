using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Protocol.Raid;
using RaidServer.Context;
using RaidServer.Network;
using Server.Repo;

namespace RaidServer.Services
{
    public class PlayerService
    {
        public PlayerService(SessionService sessionService, IServiceScopeFactory scopeFactory)
        {
            _sessionService = sessionService;
            _scopeFactory = scopeFactory;

            _sessionService.RegisterCloseListener(OnSessionClosed);
        }

        public AuthResPacket Authenticate(string sessionId, AuthReqPacket req)
        {
            if (string.IsNullOrEmpty(req.SessionKey))
            {
                return new AuthResPacket { Result = EAuthResult.InvalidRequest };
            }

            if (!_sessionService.TryGetNetworkSession(sessionId, out var session))
            {
                return new AuthResPacket { Result = EAuthResult.InvalidRequest };
            }

            using var scope = _scopeFactory.CreateScope();
            var raidContext = scope.ServiceProvider.GetRequiredService<RaidGameContext>();
            raidContext.Init(req.DeviceKey);

            using var dbRepo = scope.ServiceProvider.GetRequiredService<GlobalDbRepo>();
            try
            {
                if (!dbRepo.Auth.Session.TryGetByKey(req.SessionKey, out var mgrSession))
                {
                    return new AuthResPacket { Result = EAuthResult.SessionNotFound };
                }

                if (mgrSession.IsExpire())
                {
                    return new AuthResPacket { Result = EAuthResult.SessionExpired };
                }

                // dbRepo에서 Player얻어오기 위함. (Server랑 DbRepo 클래스를 같이써야해서 약간 억지형태, 추후 개선)
                raidContext.SetAccountId(mgrSession.Model.AccountId);
                raidContext.SetPlayerId(mgrSession.Model.PlayerId);
                raidContext.SetShardId(mgrSession.Model.ShardId);

                dbRepo.BeginOwnUserRepo();
                var mgrPlayer = dbRepo.OwnUser.Player.Get();

                // 여기서는 기본 정보만 로드 (실제 게임 시작하면 드감)
                var player = new Player
                {
                    AccountId = mgrSession.Model.AccountId,
                    PlayerId = mgrSession.Model.PlayerId,
                    ShardId = mgrSession.Model.ShardId,
                    SessionId = sessionId,
                    Profile = new RaidPlayerProfile
                    {
                        // TODO: 대표쿠키정도는 있어야함
                        ProfileName = mgrPlayer.Model.ProfileName,
                        Lv = mgrPlayer.Model.Lv,
                        CastleLv = mgrPlayer.Model.CastleLv,
                    },
                };

                if (_playersByPlayerId.ContainsKey(player.PlayerId))
                {
                    // 이미 인증된 플레이어
                    session.Authenticate(player);
                    return new AuthResPacket
                    {
                        Result = EAuthResult.Success,
                        AccountId = player.AccountId,
                        PlayerId = player.PlayerId,
                        ShardId = player.ShardId,
                    };
                }

                _playersByPlayerId[player.PlayerId] = player;
                session.Authenticate(player);

                return new AuthResPacket
                {
                    Result = EAuthResult.Success,
                    AccountId = player.AccountId,
                    PlayerId = player.PlayerId,
                    ShardId = player.ShardId,
                };
            }
            catch (Exception)
            {
                dbRepo.Rollback();
                throw;
            }
        }

        public bool TryGetByPlayerId(ulong playerId, out Player player)
        {
            return _playersByPlayerId.TryGetValue(playerId, out player);
        }

        public void Unregister(Player player)
        {
            _playersByPlayerId.TryRemove(new KeyValuePair<ulong, Player>(player.PlayerId, player));
        }

        private void OnSessionClosed(NetworkSession session)
        {
            if (session.Player != null)
            {
                Unregister(session.Player);
            }
        }

        private readonly ConcurrentDictionary<ulong, Player> _playersByPlayerId = new();
        private readonly SessionService _sessionService;
        private readonly IServiceScopeFactory _scopeFactory;
    }
}
