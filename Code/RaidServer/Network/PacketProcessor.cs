using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Protocol.Raid;

namespace RaidServer.Network
{
    public class PacketProcessor : BackgroundService
    {
        private class PacketEnvelope
        {
            public string SessionId { get; init; }
            public byte[] Bytes { get; init; }
            public TaskCompletionSource Tcs { get; init; } = new TaskCompletionSource();
        }

        public PacketProcessor(IEnumerable<IPacketHandler> packetHandlerList, PacketSerializerProvider serializerProvider, SessionService sessionService, ILogger<PacketProcessor> logger)
        {
            _serializerProvider = serializerProvider;
            _sessionService = sessionService;
            _logger = logger;

            foreach (var packetHandler in packetHandlerList)
            {
                _opcodeToHandlerDict.Add(packetHandler.Opcode, packetHandler);
            }
        }

        public Task AddPacket(string sessionId, byte[] bytes)
        {
            var envelope = new PacketEnvelope
            {
                SessionId = sessionId,
                Bytes = bytes,
            };

            if (!_receiveChannel.Writer.TryWrite(envelope))
            {
                _logger.LogError($"FAILED_ADD_PACKET SessionId({sessionId})");
            }
            return envelope.Tcs.Task;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await foreach (var envelope in _receiveChannel.Reader.ReadAllAsync(cancellationToken))
            {
                var (opcode, protocolType, payload) = PacketCodec.Parse(envelope.Bytes);

                if (!_opcodeToHandlerDict.TryGetValue(opcode, out var handler))
                {
                    _logger.LogError($"NOT_FOUND_HANDLER Opcode({opcode})");
                    envelope.Tcs.SetResult();
                    continue;
                }

                if (handler.RequireAuth)
                {
                    if (!_sessionService.TryGetNetworkSession(envelope.SessionId, out var session)
                        || session.State != ESessionState.AUTHENTICATED)
                    {
                        _logger.LogWarning($"UNAUTHORIZED Opcode({opcode}) SessionId({envelope.SessionId})");
                        envelope.Tcs.SetResult();
                        continue;
                    }
                }

                var serializer = _serializerProvider.Get(protocolType);
                var req = await serializer.DeserializeAsync(handler.Req, new MemoryStream(payload));

                await handler.RunAsync(envelope.SessionId, req);
                envelope.Tcs.SetResult();
            }
        }

        private readonly Dictionary<ushort, IPacketHandler> _opcodeToHandlerDict = [];
        private readonly Channel<PacketEnvelope> _receiveChannel = Channel.CreateUnbounded<PacketEnvelope>();
        private readonly PacketSerializerProvider _serializerProvider;
        private readonly SessionService _sessionService;
        private readonly ILogger<PacketProcessor> _logger;
    }
}
