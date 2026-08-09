namespace ServerCore.Repo.Database
{
    public interface IDbExecutor
    {
        Task<T> SelectByPkAsync<T>(object param) where T : class;
        Task<T> SelectByConditionsAsync<T>(object conditions) where T : class;
        Task<IEnumerable<T>> SelectListByConditionsAsync<T>(object conditions) where T : class;
        Task<T> InsertAsync<T>(T entity) where T : class;
        Task UpdateAsync<T>(T entity) where T : class;
        // 집계 등 로우 SQL 전용 — InMemory 모드 미지원 (NotSupportedException)
        Task<T> QuerySingleAsync<T>(string sql, object param);
    }
}
