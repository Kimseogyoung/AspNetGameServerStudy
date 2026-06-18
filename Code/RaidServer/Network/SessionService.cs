using System.Collections.Concurrent;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Protocol.Raid;

namespace RaidServer.Network
{ 

    public class SessionService
    {
        public SessionService(PacketSerializerProvider serializerProvider, ILoggerFactory loggerFactory, ILogger<SessionService> logger)
        {
            _serializerProvider = serializerProvider;
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public NetworkSession CreateNetworkSession(TcpClient client)
        {
            var NetworkSession = new NetworkSession(client, _loggerFactory.CreateLogger<NetworkSession>());
            if (!_NetworkSessionDict.TryAdd(NetworkSession.Id, NetworkSession))
            {
                throw new Exception($"FAILED_ADD_NetworkSession Guid({NetworkSession.Id})");
            }
            return NetworkSession;
        }

        public NetworkSession GetNetworkSession(string guid)
        {
            if (!_NetworkSessionDict.TryGetValue(guid, out var NetworkSessionCtx))
            {
                throw new Exception($"NOT_FOUND_NetworkSession Guid({guid})");
            }

            return NetworkSessionCtx;
        }

        public bool TryGetNetworkSession(string guid, out NetworkSession session)
        {
            return _NetworkSessionDict.TryGetValue(guid, out session);
        }

        public IEnumerable<NetworkSession> GetAllNetworkSessions()
        {
            return _NetworkSessionDict.Values;
        }

        public void CloseAllNetworkSession()
        {
            try
            {
                foreach(var networkSession in _NetworkSessionDict.Values)
                {
                    networkSession.Close();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"FAILED_CLOSE_NetworkSession Error({e})");
            }
        }

        public void CloseNetworkSession(string guid)
        {
            if (!_NetworkSessionDict.TryRemove(guid, out var networkSession))
            {
                return;
            }

            foreach (var listener in _closeListeners)
            {
                listener(networkSession);
            }

            try
            {
                networkSession.Close();
            }
            catch (Exception e)
            {
                _logger.LogError($"FAILED_CLOSE_NetworkSession_CLIENT Guid({guid}) Error({e})");
            }
        }

        // 연결 종료 시 정리할 작업을 등록한다 (예: PlayerService의 레지스트리 해제).
        public void RegisterCloseListener(Action<NetworkSession> listener)
        {
            _closeListeners.Add(listener);
        }

        public void Send(string sessionId, MessagePacket packet)
        {
            var bytes = Encode(packet);
            GetNetworkSession(sessionId).Send(bytes);
        }

        public void Broadcast(IEnumerable<string> sessionIds, MessagePacket packet)
        {
            var bytes = Encode(packet); // 1회만 직렬화/인코딩 후 byte[]를 재사용 (중복 직렬화 방지)
            foreach (var sessionId in sessionIds)
            {
                if (TryGetNetworkSession(sessionId, out var session))
                    session.Send(bytes);
            }
        }

        private byte[] Encode(MessagePacket packet)
        {
            var serializer = _serializerProvider.Get(packet.ProtocolType);
            var payloadBytes = serializer.Serialize(packet.Payload);
            return PacketCodec.Encode(packet.Opcode, packet.ProtocolType, payloadBytes);
        }

        private readonly PacketSerializerProvider _serializerProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, NetworkSession> _NetworkSessionDict = new();
        private readonly List<Action<NetworkSession>> _closeListeners = new();
    }
}
