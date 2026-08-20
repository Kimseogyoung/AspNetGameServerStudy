using System.Data;
using Dapper;
using ServerCore.Extension;

namespace ServerCore.Repo.Database
{
    // DBSqlExecutor가 보유한 IDbConnection / IDbTransaction을 래핑해 IDbExecutor 구현.
    // DapperExtension 메서드에 위임하며 connection / transaction은 외부로 노출하지 않는다.
    public class DapperDbExecutor : IDbExecutor
    {
        private readonly IDbConnection _conn;
        private readonly IDbTransaction _tx;

        public DapperDbExecutor(IDbConnection conn, IDbTransaction tx)
        {
            _conn = conn;
            _tx = tx;
        }

        public Task<T> SelectByPkAsync<T>(object param) where T : class
        {
            return _conn.SelectByPkAsync<T>(param, _tx);
        }

        public Task<T> SelectByConditionsAsync<T>(object conditions) where T : class
        {
            return _conn.SelectByConditionsAsync<T>(conditions, _tx);
        }

        public Task<IEnumerable<T>> SelectListByConditionsAsync<T>(object conditions) where T : class
        {
            return _conn.SelectListByConditionsAsync<T>(conditions, _tx);
        }

        public Task<IEnumerable<T>> SelectListByColumnAsync<T>(string column, object value) where T : class
        {
            return _conn.SelectListByColumnAsync<T>(column, value, _tx);
        }

        public Task<T> InsertAsync<T>(T entity) where T : class
        {
            return _conn.InsertAsync<T>(entity, _tx);
        }

        public Task UpdateAsync<T>(T entity) where T : class
        {
            return _conn.UpdateAsync(entity, _tx);
        }

        public Task<T> QuerySingleAsync<T>(string sql, object param)
        {
            return _conn.QuerySingleOrDefaultAsync<T>(sql, param, transaction: _tx);
        }
    }
}
