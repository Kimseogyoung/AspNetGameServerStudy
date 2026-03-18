using Server.Repo.Database;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;
using WebStudyServer.Repo.Database;

namespace WebStudyServer.Base
{
    public abstract class UserComponentBase<T> where T : ModelBase
    {
        protected readonly IRepository _repo;
        protected UserRepo _userRepo;
        protected RpcContext RpcCtx => _userRepo.RpcContext;

        protected UserComponentBase(UserRepo userRepo, IRepository repo)
        {
            _userRepo = userRepo;
            _repo = repo;
        }

        // KeyFor: match predicate 생성에 내부적으로만 사용. 외부 호출부에 노출 없음.
        // ListKeyFor: GetList/Insert/Update의 컬렉션 키.
        protected abstract CacheKey KeyFor(T model);
        protected abstract CacheKey ListKeyFor(ulong playerId);

        // PlayerId 기준 전체 로드. 특수 조건 필요 시 override.
        protected virtual List<T> LoadFromDb(IDbExecutor db)
        {
            return db.SelectListByConditions<T>(new { RpcCtx.PlayerId }).ToList();
        }

        public List<T> GetMdlList()
        {
            return _repo.GetList<T>(ListKeyFor(RpcCtx.PlayerId), LoadFromDb);
        }

        public List<T> GetMdlList(Func<T, bool> predicate)
        {
            return GetMdlList().Where(predicate).ToList();
        }

        public T? GetMdl(Func<T, bool> predicate)
        {
            return GetMdlList().FirstOrDefault(predicate);
        }

        public T CreateMdl(T entity)
        {
            entity.UpdateTime = entity.CreateTime = DateTime.UtcNow;
            return _repo.Insert<T>(entity, ListKeyFor(RpcCtx.PlayerId));
        }

        public void UpdateMdl(T entity)
        {
            entity.UpdateTime = DateTime.UtcNow;
            _repo.Update<T>(entity, ListKeyFor(RpcCtx.PlayerId), x => KeyFor(x).Value == KeyFor(entity).Value);
        }

        // IDbExecutor 범위 밖 특수 쿼리 전용 (SelectListByConditions, 집계 SQL 등)
        protected IDbSession DbSession => _repo.Db;

        protected ICacheSession CacheLayer => _repo.Cache;
    }
}
