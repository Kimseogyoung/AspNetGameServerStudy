using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public class UserComponentBase 
    {
        protected DBSqlExecutor _executor;
        protected UserRepo _userRepo;

        public UserComponentBase(UserRepo userRepo, DBSqlExecutor executor)
        {
            _userRepo = userRepo;
            _executor = executor;
        }
    }
}
