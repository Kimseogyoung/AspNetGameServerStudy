using ServerCore;
using ServerCore.Repo.Database;
using ServerCore.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Base
{
    public abstract class UserComponentBase<T> where T : ModelBase
    {
        protected readonly IRepository _repo;
        protected UserRepo _userRepo;
        protected IGameContext RpcCtx => _userRepo.RpcContext;

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

        public Task<List<T>> GetMdlListAsync()
        {
            return _repo.GetListAsync<T>(ListKeyFor(RpcCtx.PlayerId), LoadFromDb);
        }

        public async Task<List<T>> GetMdlListAsync(Func<T, bool> predicate)
        {
            var list = await GetMdlListAsync();
            return list.Where(predicate).ToList();
        }

        public async Task<T?> GetMdlAsync(Func<T, bool> predicate)
        {
            var list = await GetMdlListAsync();
            return list.FirstOrDefault(predicate);
        }

        public Task<T> CreateMdlAsync(T entity)
        {
            entity.UpdateTime = entity.CreateTime = DateTime.UtcNow;
            return _repo.InsertAsync<T>(entity, ListKeyFor(RpcCtx.PlayerId));
        }

        public Task UpdateMdlAsync(T entity)
        {
            entity.UpdateTime = DateTime.UtcNow;
            return _repo.UpdateAsync<T>(entity, ListKeyFor(RpcCtx.PlayerId), x => KeyFor(x).Value == KeyFor(entity).Value);
        }

        // IDbExecutor 범위 밖 특수 쿼리 전용 (SelectListByConditions, 집계 SQL 등)
        protected IDbSession DbSession => _repo.Db;

        protected ICacheSession CacheLayer => _repo.Cache;
    }
}
