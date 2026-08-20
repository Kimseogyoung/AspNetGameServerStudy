namespace ServerCore.Repo.Database
{
    public interface IDbExecutor
    {
        Task<T> SelectByPkAsync<T>(object param) where T : class;
        Task<T> SelectByConditionsAsync<T>(object conditions) where T : class;
        Task<IEnumerable<T>> SelectListByConditionsAsync<T>(object conditions) where T : class;
        // 컬럼명을 인자로 받는 조회 - OwnedSet<T> 의 자동 WHERE(스코프 키) 전용
        Task<IEnumerable<T>> SelectListByColumnAsync<T>(string column, object value) where T : class;
        Task<T> InsertAsync<T>(T entity) where T : class;
        Task UpdateAsync<T>(T entity) where T : class;
        // 집계 등 로우 SQL 전용 — InMemory 모드 미지원 (NotSupportedException)
        Task<T> QuerySingleAsync<T>(string sql, object param);
    }
}
