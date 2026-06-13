using System.Net.Sockets;
using System.Threading.Channels;

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
        public ulong AccountId { get; private set; }
        public ulong PlayerId { get; private set; }
        public int ShardId { get; private set; }
        public ESessionState State { get; private set; }
        public CancellationTokenSource Cts { get; private set; } = new CancellationTokenSource();
        public bool IsConnected => State != ESessionState.CLOSED;

        private Channel<byte[]> _sendQueue = Channel.CreateUnbounded<byte[]>();

        public NetworkSession(TcpClient client)
        {
            Id = Guid.NewGuid().ToString();
            Client = client;
            Stream = client.GetStream();
            ConnectTime = DateTime.UtcNow;
            LastActivityTime = DateTime.UtcNow;
            State = ESessionState.PENDING;
        }

        public void Authenticate(ulong accountId, ulong playerId, int shardId)
        {
            AccountId = accountId;
            PlayerId = playerId;
            ShardId = shardId;
            State = ESessionState.AUTHENTICATED;
        }

        public void Close()
        {
            Client.Close();
            Cts.Cancel();
            State = ESessionState.CLOSED;

            // TODO: _sendQueue에 값이 있는 상황이라면?
        }

        public void Send(byte[] bytes)
        {
            var result = _sendQueue.Writer.TryWrite(bytes);
            if (!result)
            {
                // TODO: 로그
            }
        }

        public async Task<byte[]> WaitSendBytesAsync()
        {
            var bytes = await _sendQueue.Reader.ReadAsync(Cts.Token);
            return bytes;
        }
    }

}
