using System.Data;
using Dapper;
using WebStudyServer.Extension;

namespace WebStudyServer.Repo.Database
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

        public T SelectByPk<T>(object param) where T : class
        {
            return _conn.SelectByPk<T>(param, _tx);
        }

        public IEnumerable<T> SelectListByPlayerId<T>(ulong playerId) where T : class
        {
            return _conn.SelectListByPlayerId<T>(playerId, _tx);
        }

        public T Insert<T>(T entity) where T : class
        {
            return _conn.Insert<T>(entity, _tx);
        }

        public void Update<T>(T entity) where T : class
        {
            _conn.Update(entity, _tx);
        }

        public T QuerySingle<T>(string sql, object param)
        {
            return _conn.QuerySingleOrDefault<T>(sql, param, transaction: _tx);
        }
    }
}
