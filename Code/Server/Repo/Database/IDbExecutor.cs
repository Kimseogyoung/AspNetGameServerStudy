namespace WebStudyServer.Repo.Database
{
    public interface IDbExecutor
    {
        T SelectByPk<T>(object param) where T : class;
        IEnumerable<T> SelectListByPlayerId<T>(ulong playerId) where T : class;
        T Insert<T>(T entity) where T : class;
        void Update<T>(T entity) where T : class;
    }
}
