using Microsoft.Extensions.Logging;
using Protocol.Raid;

namespace RaidServer.Network
{
    public class PacketProcessor
    {
        public PacketProcessor(IEnumerable<IPacketHandler> packetHandlerList, PacketSerializerProvider serializerProvider, SessionService sessionService, GameQueue gameQueue, ILogger<PacketProcessor> logger)
        {
            _serializerProvider = serializerProvider;
            _sessionService = sessionService;
            _gameQueue = gameQueue;
            _logger = logger;

            foreach (var packetHandler in packetHandlerList)
            {
                _opcodeToHandlerDict.Add(packetHandler.Opcode, packetHandler);
            }
        }

        public async Task AddPacket(string sessionId, byte[] bytes)
        {
            var (opcode, protocolType, payload) = PacketCodec.Parse(bytes);

            if (!_opcodeToHandlerDict.TryGetValue(opcode, out var handler))
            {
                _logger.LogError($"NOT_FOUND_HANDLER Opcode({opcode})");
                return ;
            }

            if (handler.RequireAuth)
            {
                if (!_sessionService.TryGetNetworkSession(sessionId, out var session)
                    || session.State != ESessionState.AUTHENTICATED)
                {
                    _logger.LogWarning($"UNAUTHORIZED Opcode({opcode}) SessionId({sessionId})");
                    return;
                }
            }

            var serializer = _serializerProvider.Get(protocolType);
            var req = await serializer.DeserializeAsync(handler.Req, new MemoryStream(payload));

            await _gameQueue.Post(async () =>
            {
                await handler.RunAsync(sessionId, req);
            });
        }

        private readonly Dictionary<ushort, IPacketHandler> _opcodeToHandlerDict = [];
        private readonly PacketSerializerProvider _serializerProvider;
        private readonly SessionService _sessionService;
        private readonly GameQueue _gameQueue;
        private readonly ILogger<PacketProcessor> _logger;
    }
}
