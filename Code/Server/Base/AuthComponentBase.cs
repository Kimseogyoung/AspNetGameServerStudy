using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public class AuthComponentBase
    {
        protected IDbSession _dbFactory;
        protected AuthRepo _authRepo;

        public AuthComponentBase(AuthRepo authRepo, IDbSession dbFactory)
        {
            _authRepo = authRepo;
            _dbFactory = dbFactory;
        }
    }
}
