namespace ServerCore.Repo.Database
{
    public interface IDbSession
    {
        void Execute(Action<IDbExecutor> action);
        T Execute<T>(Func<IDbExecutor, T> func);
        void Commit();
        void Rollback();
        void Close();
    }
}
