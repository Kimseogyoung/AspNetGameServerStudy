using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RaidServer.Network
{
    public class TickService : BackgroundService
    {
        private class TickEntry
        {
            public TimeSpan Interval { get; init; }
            public DateTime NextRunTime { get; set; }
            public Func<Task> Action { get; init; } = () => Task.CompletedTask;
        }

        public TickService(GameQueue gameQueue, ILogger<TickService> logger)
        {
            _gameQueue = gameQueue;
            _logger = logger;
        }

        public void Register(TimeSpan interval, Func<Task> action)
        {
            _entries.Add(new TickEntry
            {
                Interval = interval,
                NextRunTime = DateTime.UtcNow + interval,
                Action = action,
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(Resolution, stoppingToken);

                var now = DateTime.UtcNow;
                var due = _entries.Where(e => e.NextRunTime <= now).ToList();
                if (due.Count == 0)
                {
                    continue;
                }

                foreach (var entry in due)
                {
                    entry.NextRunTime = now + entry.Interval;
                }

                await _gameQueue.Post(async () =>
                {
                    foreach (var entry in due)
                    {
                        try
                        {
                            await entry.Action();
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"TICK_ACTION_FAILED Error({e})");
                        }
                    }
                });
            }
        }

        private static readonly TimeSpan Resolution = TimeSpan.FromMilliseconds(100);

        private readonly List<TickEntry> _entries = new();
        private readonly GameQueue _gameQueue;
        private readonly ILogger<TickService> _logger;
    }
}
