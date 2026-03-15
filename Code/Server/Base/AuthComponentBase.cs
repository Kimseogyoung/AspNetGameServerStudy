using Server.Repo.Database;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public class AuthComponentBase
    {
        protected IRepository _repository;
        protected AuthRepo _authRepo;
        protected RpcContext RpcCtx => _authRepo.RpcContext;

        public AuthComponentBase(AuthRepo authRepo, IRepository repository)
        {
            _authRepo = authRepo;
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

        // AccountId 기반 리스트 — 쿼리 방식(AccountId)은 Auth 레이어가 소유, IRepository는 캐시 전략만 담당
        protected List<T> GetMdlListByAccountId<T>(CacheKey listKey, ulong accountId, Func<T, CacheKey> keySelector) where T : ModelBase
            => _repository.GetList<T>(listKey,
                                      db => db.SelectListByConditions<T>(new { AccountId = accountId }).ToList(),
                                      keySelector);

        protected List<T> GetMdlListByAccountId<T>(CacheKey listKey, ulong accountId, Func<T, bool> predicate) where T : ModelBase
            => _repository.GetListFiltered<T>(listKey,
                                              db => db.SelectListByConditions<T>(new { AccountId = accountId }).ToList(),
                                              predicate);

        // IDbExecutor 범위 밖 특수 쿼리 전용 (SelectListByConditions, 집계 SQL 등)
        protected IDbSession DbSession => _repository.Db;
    }
}
