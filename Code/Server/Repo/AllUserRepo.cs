using Dapper;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.GAME;
using WebStudyServer.Model;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Repo
{
    public class AllUserRepo 
    {
        private List<DBSqlExecutor> _executorList = new();
        private List<string> _dbConnStrList => APP.Cfg.UserDbConnectionStrList;
        public AllUserRepo(List<DBSqlExecutor> executorList)
        {
        }

        public bool TryGetPlayerByName(string name, out PlayerModel outMdlPlayer)
        {
            // TODO: 캐시
            //

            // 찾기
            PlayerModel foundMdlPlayer = null;
            foreach (DBSqlExecutor executor in _executorList)
            {
                var sql = "SELECT * FROM Player WHERE ProfileName = @ProfileName";
                executor.Excute((sqlConnection, transaction) =>
                {
                    var mdlPlayer = sqlConnection.QueryFirstOrDefault<PlayerModel>(sql, new { ProfileName = name }, transaction);
                    if(mdlPlayer != null)
                    {
                        foundMdlPlayer = mdlPlayer;
                    }
                });

            }

            outMdlPlayer = foundMdlPlayer;
            return outMdlPlayer != null;
        }
    }
}
