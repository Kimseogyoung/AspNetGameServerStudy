using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public class AuthComponentBase 
    {
        protected DBSqlExecutor _executor;
        protected AuthRepo _authRepo;

        public AuthComponentBase(AuthRepo authRepo, DBSqlExecutor executor)
        {
            _authRepo = authRepo;
            _executor = executor;
        }
    }
}
