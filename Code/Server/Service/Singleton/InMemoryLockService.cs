namespace WebStudyServer
{
    // 단일 프로세스 환경(InMemory 모드)에서는 분산 락이 불필요하므로 no-op 구현.
    public class InMemoryLockService : ILockService
    {
        public bool Enter(ulong accountId) => true;
        public bool Exit(ulong accountId) => true;
    }
}
