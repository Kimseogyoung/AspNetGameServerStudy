using Microsoft.Extensions.Logging;
using Protocol.Raid;
using RaidServer.Context;
using RaidServer.Network;

namespace RaidServer.Services
{
    public class MatchingService
    {
        private class MatchingEntry
        {
            public string SessionId { get; init; } = string.Empty;
            public ulong PlayerId { get; init; }       // 서버 내부 식별용
            public ulong SfId { get; init; }           // 클라이언트 공개용
            public string ProfileName { get; init; } = string.Empty;
            public DateTime EnqueueTime { get; init; }
        }

        public MatchingService(
            SessionService sessionService,
            PlayerRaidSessionService playerRaidSessionService,
            TickService tickService,
            GameQueue gameQueue,
            RaidConfig config,
            ILogger<MatchingService> logger)
        {
            _sessionService = sessionService;
            _playerRaidSessionService = playerRaidSessionService;
            _gameQueue = gameQueue;
            _matchingTimeoutSec = config.MatchingTimeoutSec;
            _logger = logger;

            tickService.Register(TimeSpan.FromSeconds(config.MatchingTickIntervalSec), OnTick);
            sessionService.RegisterCloseListener(OnSessionClosed);
        }

        public MatchingStartResPacket StartMatching(string sessionId, int bossNum)
        {
            if (!_playerRaidSessionService.TryGetBySessionId(sessionId, out var raidSession))
            {
                return new MatchingStartResPacket { Result = EMatchingResult.InvalidBoss };
            }

            if (raidSession!.State == EPlayerRaidState.MATCHING)
            {
                return new MatchingStartResPacket { Result = EMatchingResult.AlreadyMatching };
            }

            if (raidSession.State == EPlayerRaidState.IN_ROOM)
            {
                return new MatchingStartResPacket { Result = EMatchingResult.AlreadyInRoom };
            }

            if (bossNum <= 0)
            {
                return new MatchingStartResPacket { Result = EMatchingResult.InvalidBoss };
            }

            if (!_queueByBoss.TryGetValue(bossNum, out var queue))
            {
                queue = new List<MatchingEntry>();
                _queueByBoss[bossNum] = queue;
            }

            queue.Add(new MatchingEntry
            {
                SessionId = sessionId,
                PlayerId = raidSession.Player.Id,
                SfId = raidSession.Player.SfId,
                ProfileName = raidSession.Player.ProfileName,
                EnqueueTime = DateTime.UtcNow,
            });
            _bossNumBySessionId[sessionId] = bossNum;
            raidSession.State = EPlayerRaidState.MATCHING;

            _logger.LogInformation($"MATCHING_START SessionId({sessionId}) BossNum({bossNum})");
            return new MatchingStartResPacket { Result = EMatchingResult.Success };
        }

        public MatchingCancelResPacket CancelMatching(string sessionId)
        {
            if (!_playerRaidSessionService.TryGetBySessionId(sessionId, out var raidSession)
                || raidSession!.State != EPlayerRaidState.MATCHING)
            {
                return new MatchingCancelResPacket { Result = EMatchingResult.NotMatching };
            }

            RemoveFromQueue(sessionId);
            raidSession.State = EPlayerRaidState.IDLE;

            _logger.LogInformation($"MATCHING_CANCEL SessionId({sessionId})");
            return new MatchingCancelResPacket { Result = EMatchingResult.Success };
        }

        private Task OnTick()
        {
            var now = DateTime.UtcNow;
            var timeout = TimeSpan.FromSeconds(_matchingTimeoutSec);

            foreach (var (bossNum, queue) in _queueByBoss)
            {
                while (queue.Count >= 4)
                {
                    var group = queue.GetRange(0, 4);
                    queue.RemoveRange(0, 4);
                    ConfirmGroup(bossNum, group);
                }

                if (queue.Count >= 1 && (now - queue[0].EnqueueTime) >= timeout)
                {
                    var group = queue.ToList();
                    queue.Clear();
                    ConfirmGroup(bossNum, group);
                }
            }

            return Task.CompletedTask;
        }

        private void ConfirmGroup(int bossNum, List<MatchingEntry> group)
        {
            foreach (var entry in group)
            {
                _bossNumBySessionId.Remove(entry.SessionId);

                if (_playerRaidSessionService.TryGetBySessionId(entry.SessionId, out var raidSession))
                {
                    raidSession!.State = EPlayerRaidState.IN_ROOM;
                }
            }

            // TODO: RoomService.CreateRoom(bossNum, group) 호출 후 RoomId 수신
            var roomId = Guid.NewGuid().ToString("N");

            var members = group.ConvertAll(e => new RoomMemberInfo
            {
                SfId = e.SfId,
                ProfileName = e.ProfileName,
            });

            _sessionService.Broadcast(
                group.Select(e => e.SessionId),
                new MessagePacket
                {
                    Opcode = (ushort)EPacketType.MatchingCompleteNotify,
                    ProtocolType = EProtocolType.Json,
                    Payload = new MatchingCompleteNotifyPacket
                    {
                        RoomId = roomId,
                        BossNum = bossNum,
                        Members = members,
                    },
                });

            _logger.LogInformation($"MATCHING_COMPLETE BossNum({bossNum}) RoomId({roomId}) Members({group.Count})");
        }

        private void RemoveFromQueue(string sessionId)
        {
            if (!_bossNumBySessionId.TryGetValue(sessionId, out var bossNum))
            {
                return;
            }

            _bossNumBySessionId.Remove(sessionId);

            if (_queueByBoss.TryGetValue(bossNum, out var queue))
            {
                queue.RemoveAll(e => e.SessionId == sessionId);
            }
        }

        private void OnSessionClosed(NetworkSession session)
        {
            var sessionId = session.Id;
            _ = _gameQueue.Post(() =>
            {
                RemoveFromQueue(sessionId);
                return Task.CompletedTask;
            });
        }

        private readonly Dictionary<int, List<MatchingEntry>> _queueByBoss = new();
        private readonly Dictionary<string, int> _bossNumBySessionId = new();
        private readonly int _matchingTimeoutSec;
        private readonly SessionService _sessionService;
        private readonly PlayerRaidSessionService _playerRaidSessionService;
        private readonly GameQueue _gameQueue;
        private readonly ILogger _logger;
    }
}
