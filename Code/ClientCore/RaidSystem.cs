using System;
using System.Buffers.Binary;
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

                    await RequestAsync<PingReqPacket, PongResPacket>((ushort)EPacketType.PingReq, EProtocolType.Json, new PingReqPacket());
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

        // 한 번에 하나의 요청만 처리 (테스트 클라이언트 용도, 동시 요청은 지원하지 않음)
        public async Task<TRes> RequestAsync<TReq, TRes>(ushort opcode, EProtocolType protocolType, TReq req)
        {
            if (_stream == null)
            {
                throw new Exception("RAID_NOT_CONNECTED");
            }

            var payloadBytes = Serialize(protocolType, req);
            var frame = PacketCodec.Encode(opcode, protocolType, payloadBytes);

            _pendingTcs = new TaskCompletionSource<(ushort Opcode, EProtocolType ProtocolType, byte[] Payload)>();
            await _stream.WriteAsync(frame);

            var (_, resProtocolType, payload) = await _pendingTcs.Task;
            return Deserialize<TRes>(resProtocolType, payload);
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
                    _pendingTcs?.TrySetResult((opcode, protocolType, payload));
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

        private static T Deserialize<T>(EProtocolType protocolType, byte[] bytes)
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

        private TcpClient? _client;
        private NetworkStream? _stream;
        private TaskCompletionSource<(ushort Opcode, EProtocolType ProtocolType, byte[] Payload)>? _pendingTcs;
        private CancellationTokenSource? _pingCts;
    }
}
