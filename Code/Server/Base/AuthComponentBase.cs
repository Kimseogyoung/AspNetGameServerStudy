using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public class AuthComponentBase
    {
        protected IDbExecutorFactory _dbFactory;
        protected AuthRepo _authRepo;

        public AuthComponentBase(AuthRepo authRepo, IDbExecutorFactory dbFactory)
        {
            _authRepo = authRepo;
            _dbFactory = dbFactory;
        }
    }
}
