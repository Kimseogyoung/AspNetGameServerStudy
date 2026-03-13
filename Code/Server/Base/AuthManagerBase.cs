using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer
{
    public abstract class AuthManagerBase<T> : ManagerBase<T> where T : ModelBase
    {
        protected AuthRepo _authRepo;

        public AuthManagerBase(AuthRepo authRepo, T model) : base(model)
        {
            _authRepo = authRepo;
        }
    }
}
