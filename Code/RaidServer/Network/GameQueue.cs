using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;

namespace RaidServer.Network
{
    // 게임 상태를 건드리는 모든 작업(패킷 처리, 틱 등)의 단일 직렬 실행 지점.
    // MaxDegreeOfParallelism = 1 이므로 Post된 작업은 들어온 순서대로 하나씩만 실행된다.
    public class GameQueue
    {
        public GameQueue(ILogger<GameQueue> logger)
        {
            _logger = logger;
            _block = new ActionBlock<Func<Task>>(
                action => action(),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });
        }

        public Task Post(Func<Task> action)
        {
            var tcs = new TaskCompletionSource();

            if (!_block.Post(async () =>
            {
                try
                {
                    await action();
                    tcs.SetResult();
                }
                catch (Exception e)
                {
                    _logger.LogError($"GAME_QUEUE_ACTION_FAILED Error({e})");
                    tcs.SetException(e);
                }
            }))
            {
                _logger.LogError("FAILED_POST_GAME_QUEUE");
            }

            return tcs.Task;
        }

        private readonly ActionBlock<Func<Task>> _block;
        private readonly ILogger<GameQueue> _logger;
    }
}
