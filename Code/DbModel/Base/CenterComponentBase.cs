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

        protected Task<T?> GetMdlAsync<T>(Func<IDbExecutor, Task<T?>> dbFetch) where T : ModelBase
        {
            return _repository.Db.ExecuteAsync(dbFetch);
        }

        protected Task<List<T>> GetMdlListAsync<T>(Func<IDbExecutor, Task<List<T>>> dbFetch) where T : ModelBase
        {
            return _repository.Db.ExecuteAsync(dbFetch);
        }

        protected async Task<T> CreateMdlAsync<T>(T entity) where T : ModelBase
        {
            entity.UpdateTime = entity.CreateTime = DateTime.UtcNow;
            return await _repository.Db.ExecuteAsync(db => db.Insert<T>(entity));
        }

        protected async Task UpdateMdlAsync<T>(T entity) where T : ModelBase
        {
            entity.UpdateTime = DateTime.UtcNow;
            await _repository.Db.ExecuteAsync(db => db.Update<T>(entity));
        }

        // IDbExecutor 범위 밖 특수 쿼리 전용 (SelectListByConditions, 집계 SQL 등)
        protected IDbSession DbSession => _repository.Db;
    }
}
