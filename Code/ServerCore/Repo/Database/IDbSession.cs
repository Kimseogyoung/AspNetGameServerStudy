namespace ServerCore.Repo.Database
{
    public interface IDbSession
    {
        Task ExecuteAsync(Func<IDbExecutor, Task> action);
        Task<T> ExecuteAsync<T>(Func<IDbExecutor, Task<T>> func);
        void Commit();
        void Rollback();
        void Close();
    }
}
