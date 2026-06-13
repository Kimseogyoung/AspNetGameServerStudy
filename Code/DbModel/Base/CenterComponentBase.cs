using ServerCore;
using ServerCore.Repo.Database;
using ServerCore.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Base
{
    public class CenterComponentBase
    {
        protected readonly IRepository _repository;
        protected readonly CenterRepo _centerRepo;
        protected IGameContext RpcCtx => _centerRepo.RpcContext;

        public CenterComponentBase(CenterRepo centerRepo, IRepository repository)
        {
            _centerRepo = centerRepo;
            _repository = repository;
        }

        protected T? GetMdl<T>(Func<IDbExecutor, T?> dbFetch) where T : ModelBase
        {
            return _repository.Db.Execute(dbFetch);
        }

        protected List<T> GetMdlList<T>(Func<IDbExecutor, List<T>> dbFetch) where T : ModelBase
        {
            return _repository.Db.Execute(dbFetch);
        }

        protected T CreateMdl<T>(T entity) where T : ModelBase
        {
            entity.UpdateTime = entity.CreateTime = DateTime.UtcNow;
            return _repository.Db.Execute(db => db.Insert<T>(entity));
        }

        protected void UpdateMdl<T>(T entity) where T : ModelBase
        {
            entity.UpdateTime = DateTime.UtcNow;
            _repository.Db.Execute(db => db.Update<T>(entity));
        }

        // IDbExecutor 범위 밖 특수 쿼리 전용 (SelectListByConditions, 집계 SQL 등)
        protected IDbSession DbSession => _repository.Db;
    }
}
