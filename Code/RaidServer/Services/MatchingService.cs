using Microsoft.Extensions.Logging;

namespace RaidServer.Services
{
    // 이 서비스는 스레드세이프가 보장됨
    public class MatchingService
    {
        public MatchingService(ILogger<MatchingService> logger)
        {
            _logger = logger;
        }

        public void StartMatching()
        {

        }

        private readonly ILogger _logger;
    }
}
