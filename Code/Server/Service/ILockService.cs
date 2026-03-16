namespace WebStudyServer
{
    public interface ILockService
    {
        bool Enter(ulong accountId);
        bool Exit(ulong accountId);
    }
}
