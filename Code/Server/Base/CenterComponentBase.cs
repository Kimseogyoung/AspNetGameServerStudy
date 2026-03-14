using WebStudyServer.Extension;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public class CenterComponentBase
    {
        protected IDbExecutorFactory _dbFactory;
        protected CenterRepo _centerRepo;

        public CenterComponentBase(CenterRepo centerRepo, IDbExecutorFactory dbFactory)
        {
            _centerRepo = centerRepo;
            _dbFactory = dbFactory;
        }
    }
}
