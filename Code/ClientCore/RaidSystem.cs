using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Protocol.Raid;

namespace ClientCore
{
    // RaidServer TCP 소켓 클라이언트. 프레이밍은 Protocol.Raid.PacketCodec과 동일한 포맷을 사용.
    public class RaidSystem
    {
        public bool IsConnected => _client?.Connected ?? false;

        public async Task ConnectAsync(string host, int port)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();
            _ = ReceiveLoopAsync();
        }

        public void Close()
        {
            _pingCts?.Cancel();
            _pingCts = null;
            _client?.Close();
            _client = null;
            _stream = null;
        }

        // 연결되어 있는 동안 주기적으로 PingReq를 보내 LastActivityTime을 갱신시킨다.
        public void StartPingLoop(TimeSpan interval)
        {
            _pingCts?.Cancel();
            _pingCts = new CancellationTokenSource();
            _ = PingLoopAsync(interval, _pingCts.Token);
        }

        // TODO: 서버가 Pong을 보내도 클라이언트가 못 받는 이슈 조사 중 (RaidServer WriteLoopAsync에 WRITE_LOOP_UNEXPECTED_ERROR 로그 추가해둠, 재현 후 로그 확인 필요)
        private async Task PingLoopAsync(TimeSpan interval, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && IsConnected)
                {
                    await Task.Delay(interval, token);
                    if (!IsConnected)
                    {
                        break;
                    }

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] PING_SEND");
                    await RequestWithWaitAsync<PingRequestPacket, PongResponsePacket>((ushort)EPacketType.PingRequest, (ushort)EPacketType.PongResponse, EProtocolType.Json, new PingRequestPacket());
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] PING_RECV");
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine($"RAID_PING_LOOP_END Error({e.Message})");
            }
        }

        public void RegisterPushHandler(ushort opcode, Action<EProtocolType, byte[]> handler)
        {
            _pushHandlers[opcode] = handler;
        }

        // 응답을 기다리지 않고 보내기만 한다 (전송 자체는 세마포어로 직렬화).
        public async Task RequestAsync<TReq>(ushort opcode, EProtocolType protocolType, TReq req)
        {
            if (_stream == null)
            {
                throw new Exception("RAID_NOT_CONNECTED");
            }

            await _sendLock.WaitAsync();
            try
            {
                var payloadBytes = Serialize(protocolType, req);
                var frame = PacketCodec.Encode(opcode, protocolType, payloadBytes);
                await _stream.WriteAsync(frame);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        // 보낸 뒤 waitOpcode로 들어오는 응답만 받아들여 기다린다 (한 번에 하나의 요청만 처리).
        public async Task<TRes> RequestWithWaitAsync<TReq, TRes>(ushort opcode, ushort waitOpcode, EProtocolType protocolType, TReq req)
        {
            if (_stream == null)
            {
                throw new Exception("RAID_NOT_CONNECTED");
            }

            await _sendLock.WaitAsync();
            try
            {
                var payloadBytes = Serialize(protocolType, req);
                var frame = PacketCodec.Encode(opcode, protocolType, payloadBytes);

                _pendingResponseOpcode = waitOpcode;
                _pendingTcs = new TaskCompletionSource<(ushort Opcode, EProtocolType ProtocolType, byte[] Payload)>();
                await _stream.WriteAsync(frame);

                var (_, resProtocolType, payload) = await _pendingTcs.Task;
                return Deserialize<TRes>(resProtocolType, payload);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        private async Task ReceiveLoopAsync()
        {
            try
            {
                while (IsConnected)
                {
                    var lengthBytes = new byte[4];
                    await ReadExactAsync(lengthBytes);
                    var bodyLength = BinaryPrimitives.ReadInt32BigEndian(lengthBytes);

                    var bodyBytes = new byte[bodyLength];
                    await ReadExactAsync(bodyBytes);

                    var (opcode, protocolType, payload) = PacketCodec.Parse(bodyBytes);
                    if (_pushHandlers.TryGetValue(opcode, out var handler))
                    {
                        handler(protocolType, payload);
                    }
                    else if (_pendingTcs != null && opcode == _pendingResponseOpcode)
                    {
                        _pendingTcs.TrySetResult((opcode, protocolType, payload));
                    }
                    else
                    {
                        Console.WriteLine($"UNEXPECTED_RESPONSE Opcode({opcode}) Expected({_pendingResponseOpcode})");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"RAID_RECEIVE_LOOP_END Error({e.Message})");
                _pendingTcs?.TrySetException(e);
            }
        }

        private async Task ReadExactAsync(byte[] buffer)
        {
            var totalRead = 0;
            while (totalRead < buffer.Length)
            {
                var read = await _stream!.ReadAsync(buffer, totalRead, buffer.Length - totalRead);
                if (read == 0)
                {
                    throw new Exception("RAID_CONNECTION_CLOSED");
                }
                totalRead += read;
            }
        }

        private static byte[] Serialize<T>(EProtocolType protocolType, T obj)
        {
            switch (protocolType)
            {
                case EProtocolType.Json:
                    return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
                case EProtocolType.Protobuf:
                    using (var ms = new MemoryStream())
                    {
                        ProtoBuf.Serializer.Serialize(ms, obj);
                        return ms.ToArray();
                    }
                default:
                    throw new Exception($"NOT_SUPPORTED_PROTOCOL({protocolType})");
            }
        }

        public static T Deserialize<T>(EProtocolType protocolType, byte[] bytes)
        {
            switch (protocolType)
            {
                case EProtocolType.Json:
                    return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(bytes))!;
                case EProtocolType.Protobuf:
                    using (var ms = new MemoryStream(bytes))
                    {
                        return ProtoBuf.Serializer.Deserialize<T>(ms);
                    }
                default:
                    throw new Exception($"NOT_SUPPORTED_PROTOCOL({protocolType})");
            }
        }

        private readonly Dictionary<ushort, Action<EProtocolType, byte[]>> _pushHandlers = new Dictionary<ushort, Action<EProtocolType, byte[]>>();
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private TcpClient? _client;
        private NetworkStream? _stream;
        private TaskCompletionSource<(ushort Opcode, EProtocolType ProtocolType, byte[] Payload)>? _pendingTcs;
        private ushort _pendingResponseOpcode;
        private CancellationTokenSource? _pingCts;
    }
}
