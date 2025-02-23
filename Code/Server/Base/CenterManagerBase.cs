using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer
{
    public abstract class CenterManagerBase<T> : ManagerBase<T> where T : ModelBase
    {
        protected CenterRepo _centerRepo;

        public CenterManagerBase(CenterRepo centerRepo, T model) : base(model)
        {
            _centerRepo = centerRepo;
        }
    }
}
