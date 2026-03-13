namespace WebStudyServer.Repo.Database
{
    public interface IDbExecutor
    {
        T SelectByPk<T>(object param) where T : class;
        IEnumerable<T> SelectListByPlayerId<T>(ulong playerId) where T : class;
        T Insert<T>(T entity) where T : class;
        void Update<T>(T entity) where T : class;
        // 집계 등 로우 SQL 전용 — InMemory 모드 미지원 (NotSupportedException)
        T QuerySingle<T>(string sql, object param);
    }
}
