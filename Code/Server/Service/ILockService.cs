namespace WebStudyServer
{
    public interface ILockService
    {
        Task<bool> EnterAsync(ulong accountId);
        Task<bool> ExitAsync(ulong accountId);
    }
}
