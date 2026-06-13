using ServerCore;
using ServerCore.Model;
using WebStudyServer.Repo;

namespace WebStudyServer
{
    public abstract class UserManagerBase<T> : ManagerBase<T> where T : ModelBase
    {
        protected UserRepo _userRepo;
        protected IGameContext RpcCtx => _userRepo.RpcContext;

        public UserManagerBase(UserRepo userRepo, T model) : base(model)
        {
            _userRepo = userRepo;
        }
    }
}
