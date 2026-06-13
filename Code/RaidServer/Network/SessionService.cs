using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Protocol.Raid;

namespace RaidServer.Network
{ 

    public class SessionService
    {
        public SessionService(PacketSerializerProvider serializerProvider, ILogger<SessionService> logger)
        {
            _serializerProvider = serializerProvider;
            _logger = logger;
        }

        public NetworkSession CreateNetworkSession(TcpClient client)
        {
            var NetworkSession = new NetworkSession(client);
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

        public NetworkSession GetNetworkSessionByAccountId(ulong accountId)
        {
            var NetworkSessionCtx = _NetworkSessionDict.FirstOrDefault(x => x.Value.AccountId == accountId).Value;
            if (NetworkSessionCtx == null)
            {
                throw new Exception($"NOT_FOUND_NetworkSession AccountId({accountId})");
            }

            return NetworkSessionCtx;
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

            try
            {
                networkSession.Close();
            }
            catch (Exception e)
            {
                _logger.LogError($"FAILED_CLOSE_NetworkSession_CLIENT Guid({guid}) Error({e})");
            }
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
                GetNetworkSession(sessionId).Send(bytes);
            }
        }

        private byte[] Encode(MessagePacket packet)
        {
            var serializer = _serializerProvider.Get(packet.ProtocolType);
            var payloadBytes = serializer.Serialize(packet.Payload);
            return PacketCodec.Encode(packet.Opcode, packet.ProtocolType, payloadBytes);
        }

        private readonly PacketSerializerProvider _serializerProvider;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, NetworkSession> _NetworkSessionDict = new();
    }
}
