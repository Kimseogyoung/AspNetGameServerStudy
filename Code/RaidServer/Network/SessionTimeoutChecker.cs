using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RaidServer.Network
{
    public class SessionTimeoutChecker : BackgroundService
    {
        public SessionTimeoutChecker(SessionService sessionService, RaidConfig config, ILogger<SessionTimeoutChecker> logger)
        {
            _sessionService = sessionService;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var checkInterval = TimeSpan.FromSeconds(Math.Max(1, _config.SessionTimeoutSec / 3));

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(checkInterval, stoppingToken);

                var now = DateTime.UtcNow;
                var timeout = TimeSpan.FromSeconds(_config.SessionTimeoutSec);

                foreach (var session in _sessionService.GetAllNetworkSessions())
                {
                    if (session.IsConnected && now - session.LastActivityTime > timeout)
                    {
                        _logger.LogInformation($"SESSION_TIMEOUT SessionId({session.Id})");
                        _sessionService.CloseNetworkSession(session.Id);
                    }
                }
            }
        }

        private readonly SessionService _sessionService;
        private readonly RaidConfig _config;
        private readonly ILogger<SessionTimeoutChecker> _logger;
    }
}
