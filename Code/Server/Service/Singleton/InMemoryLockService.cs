namespace WebStudyServer
{
    // 단일 프로세스 환경(InMemory 모드)에서는 분산 락이 불필요하므로 no-op 구현.
    public class InMemoryLockService : ILockService
    {
        public Task<bool> EnterAsync(ulong accountId) => Task.FromResult(true);
        public Task<bool> ExitAsync(ulong accountId) => Task.FromResult(true);
    }
}
