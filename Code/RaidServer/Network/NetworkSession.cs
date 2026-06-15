using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using RaidServer.Context;

namespace RaidServer.Network
{
    public enum ESessionState
    {
        PENDING,    // 아직 인증/로그인 전
        AUTHENTICATED, // 인증/로그인 완료
        CLOSED
    }

    public class NetworkSession
    {
        public string Id { get; private set; }
        public TcpClient Client { get; private set; }
        public NetworkStream Stream { get; private set; }
        public DateTime ConnectTime { get; private set; }
        public DateTime LastActivityTime { get; set; }
        public Player? Player { get; private set; }
        public ESessionState State { get; private set; }
        public CancellationTokenSource Cts { get; private set; } = new CancellationTokenSource();
        public bool IsConnected => State != ESessionState.CLOSED;

        private readonly ILogger<NetworkSession> _logger;
        private Channel<byte[]> _sendQueue = Channel.CreateUnbounded<byte[]>();

        public NetworkSession(TcpClient client, ILogger<NetworkSession> logger)
        {
            _logger = logger;
            Id = Guid.NewGuid().ToString();
            Client = client;
            Stream = client.GetStream();
            ConnectTime = DateTime.UtcNow;
            LastActivityTime = DateTime.UtcNow;
            State = ESessionState.PENDING;
        }

        public void Authenticate(Player player)
        {
            Player = player;
            State = ESessionState.AUTHENTICATED;
        }

        public void Close()
        {
            Client.Close();
            Cts.Cancel();
            State = ESessionState.CLOSED;

            // 종료 시점에 큐에 남은 미전송 데이터는 연결이 끊긴 클라이언트로 보낼 수 없으므로 의도적으로 폐기한다.
            // Writer를 완료시켜 이후 Send() 호출은 TryWrite 실패로 안전하게 무시되고 로그만 남긴다.
            _sendQueue.Writer.TryComplete();
        }

        public void Send(byte[] bytes)
        {
            var result = _sendQueue.Writer.TryWrite(bytes);
            if (!result)
            {
                _logger.LogWarning($"SEND_QUEUE_CLOSED SessionId({Id})");
            }
        }

        public async Task<byte[]> WaitSendBytesAsync()
        {
            var bytes = await _sendQueue.Reader.ReadAsync(Cts.Token);
            return bytes;
        }
    }

}
