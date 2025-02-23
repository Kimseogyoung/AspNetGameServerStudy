using WebStudyServer.Extension;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public class CenterComponentBase 
    {
        protected DBSqlExecutor _executor;
        protected CenterRepo _centerRepo;

        public CenterComponentBase(CenterRepo centerRepo, DBSqlExecutor executor)
        {
            _centerRepo = centerRepo;
            _executor = executor;
        }
    }
}
