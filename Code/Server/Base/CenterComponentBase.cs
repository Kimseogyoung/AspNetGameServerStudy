using Server.Repo.Database;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public class CenterComponentBase
    {
        protected IRepository _repository;
        protected CenterRepo _centerRepo;
        protected RpcContext RpcCtx => _centerRepo.RpcContext;

        public CenterComponentBase(CenterRepo centerRepo, IRepository repository)
        {
            _centerRepo = centerRepo;
            _repository = repository;
        }

        protected T GetMdl<T>(CacheKey key, Func<IDbExecutor, T> dbFetch) where T : ModelBase
            => _repository.Get<T>(key, dbFetch);

        protected T CreateMdl<T>(T entity, Func<T, CacheKey> keyFactory) where T : ModelBase
        {
            entity.UpdateTime = entity.CreateTime = DateTime.UtcNow;
            return _repository.Insert<T>(entity, keyFactory);
        }

        protected void UpdateMdl<T>(T entity, CacheKey key) where T : ModelBase
        {
            entity.UpdateTime = DateTime.UtcNow;
            _repository.Update<T>(entity, key);
        }

        // IDbExecutor 범위 밖 특수 쿼리 전용 (SelectListByConditions, 집계 SQL 등)
        protected IDbSession DbSession => _repository.Db;
    }
}
