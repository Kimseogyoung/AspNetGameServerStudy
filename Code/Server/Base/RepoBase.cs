using ServerCore.Repo.Database;

namespace WebStudyServer.Base
{
    public abstract class RepoBase
    {
        public int ShardId { get; private set; }
        public IRepository Repository { get; private set; }
        protected abstract void PrepareComp();

        public RepoBase(int shardId, IRepository repository)
        {
            ShardId = shardId;
            Repository = repository;
            PrepareComp();
        }
    }
}
