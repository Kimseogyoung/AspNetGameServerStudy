using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace RaidServer.Network
{
    public class SocketService
    {     
        public SocketService(SessionService sessionService, ILogger<SocketService> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        public async Task StartAsync(int port, CancellationToken cancellationToken, Func<string, byte[], Task> handler)
        {
            _handler = handler;
            _cancelToken = cancellationToken;

            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (!_cancelToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(_cancelToken);
                _ = HandleClientAsync(client); // TODO: await 안하는 경우 FireAndForgot
            }

            _sessionService.CloseAllNetworkSession();
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            var session = _sessionService.CreateNetworkSession(client);
            var receiveTask = ReceiveLoopAsync(session);
            var sendTask = WriteLoopAsync(session);

            await Task.WhenAny(receiveTask, sendTask); // 한쪽이 끝나면(에러/연결끊김)

            session.Cts.Cancel(); // 다른 쪽도 정리
            if (!receiveTask.IsCompleted)
            {
                await receiveTask;
            }
            if (!sendTask.IsCompleted)
            {
                await sendTask;
            }

            _sessionService.CloseNetworkSession(session.Id);
        }

        public async Task ReceiveLoopAsync(NetworkSession session)
        {
            var stream = session.Stream;
            var messageBuffer = new MemoryStream();

            // 서버 종료 / 세션 종료 확인
            while (session.IsConnected)
            {
                var lengthBuffer = new byte[4];
                // 메시지 길이 4바이트 읽기
                int read = await ReadExactAsync(lengthBuffer, 4, session.Cts.Token);
                if (read == 0)
                {
                    // 연결 종료
                    break;
                }

                int readMessageCnt = BitConverter.ToInt32(lengthBuffer.Reverse().ToArray(), 0);  // Big endian 처리
                if (readMessageCnt <= 0)
                {
                    _logger.LogError($"잘못된 메시지 길이 ({readMessageCnt})");
                    break;
                }

                // 메시지 본문 (동적으로 할당)
                var messageBytes = new byte[readMessageCnt];
                read = await ReadExactAsync(messageBytes, readMessageCnt, session.Cts.Token);
                if (read == 0)
                {
                    // 연결 종료
                    break;
                }

                // 파싱/비즈니스는 Dispatcher에 위임
                _ = _handler!.Invoke(session.Id, messageBytes); // TODO: FireAndForget
            }

            async Task<int> ReadExactAsync(byte[] buffer, int length, CancellationToken token)
            {
                int totalRead = 0;
                while (totalRead < length)
                {
                    int read = await stream.ReadAsync(buffer, totalRead, length - totalRead, token);
                    if (read == 0)
                    {
                        return 0; // 연결 종료
                    }
                    totalRead += read;
                }
                return totalRead;
            }
        }

        public async Task WriteLoopAsync(NetworkSession session)
        {
            var stream = session.Stream;

            while (session.IsConnected)
            {
                var bytes = await session.WaitSendBytesAsync();
                await stream.WriteAsync(bytes);
            }
        }

        private Func<string, byte[], Task>? _handler;
        private CancellationToken _cancelToken;

        private readonly SessionService _sessionService;
        private readonly ILogger<SocketService> _logger;
    }
}
