namespace ServerCore.Repo.Database
{
    public interface IDbExecutor
    {
        Task<T> SelectByPk<T>(object param) where T : class;
        Task<T> SelectByConditions<T>(object conditions) where T : class;
        Task<IEnumerable<T>> SelectListByConditions<T>(object conditions) where T : class;
        Task<T> Insert<T>(T entity) where T : class;
        Task Update<T>(T entity) where T : class;
        // 집계 등 로우 SQL 전용 — InMemory 모드 미지원 (NotSupportedException)
        Task<T> QuerySingle<T>(string sql, object param);
    }
}
