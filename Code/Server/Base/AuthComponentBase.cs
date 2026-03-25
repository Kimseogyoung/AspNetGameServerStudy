using Server.Repo.Database;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public class AuthComponentBase
    {
        protected readonly IRepository _repository;
        protected readonly AuthRepo _authRepo;
        protected RpcContext RpcCtx => _authRepo.RpcContext;

        public AuthComponentBase(AuthRepo authRepo, IRepository repository)
        {
            _authRepo = authRepo;
            _repository = repository;
        }

        protected T? GetMdl<T>(Func<IDbExecutor, T?> dbFetch) where T : ModelBase
        {
            return _repository.Db.Execute(dbFetch);
        }

        // 단건 캐시 → DB fallback (Auth/Session 전용)
        protected T? GetMdlWithCache<T>(CacheKey cacheKey, Func<IDbExecutor, T?> dbFetch) where T : ModelBase
        {
            if (_repository.Cache.TryGet<T>(cacheKey, out var cached))
            {
                return cached;
            }
            
            var result = _repository.Db.Execute(dbFetch);
            if (result != null)
            {
                _repository.Cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
            }
            return result;
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
