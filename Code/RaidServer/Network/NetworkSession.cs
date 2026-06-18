using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace RaidServer.Network
{
    public enum ESessionState
    {
        PENDING,
        AUTHENTICATED,
        CLOSED
    }

    public class NetworkSession
    {
        public string Id { get; private set; }
        public TcpClient Client { get; private set; }
        public NetworkStream Stream { get; private set; }
        public DateTime ConnectTime { get; private set; }
        public DateTime LastActivityTime { get; set; }
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

        public void Authenticate()
        {
            State = ESessionState.AUTHENTICATED;
        }

        public void Close()
        {
            Client.Close();
            Cts.Cancel();
            State = ESessionState.CLOSED;

            // 종료 시점에 큐에 남은 미전송 데이터는 연결이 끊긴 클라이언트로 보낼 수 없으므로 의도적으로 폐기한다.
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
            return await _sendQueue.Reader.ReadAsync(Cts.Token);
        }
    }
}
