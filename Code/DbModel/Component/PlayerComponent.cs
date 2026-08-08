using ServerCore.Helper;
using ServerCore.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using ServerCore.Repo.Cache;

namespace WebStudyServer.Component
{
    public class PlayerComponent : UserComponentBase<PlayerModel>
    {
        public PlayerComponent(UserRepo userRepo, IRepository repository) : base(userRepo, repository) { }

        protected override CacheKey KeyFor(PlayerModel model) => CacheKey.For(CacheKeyTags.PlayerModel, model.Id);
        protected override CacheKey ListKeyFor(ulong playerId) => CacheKey.For(CacheKeyTags.PlayerModel, playerId);

        // PlayerModel의 PK는 Id (PlayerId 컬럼 없음)
        protected override List<PlayerModel> LoadFromDb(IDbExecutor db)
        {
            return db.SelectListByConditions<PlayerModel>(new { Id = RpcCtx.PlayerId }).ToList();
        }

        public async Task<PlayerManager> TouchAsync()
        {
            var playerId = _userRepo.RpcContext.PlayerId;
            var accountId = _userRepo.RpcContext.AccountId;

            if (playerId == 0)
            {
                _userRepo.RpcContext.SetPlayerId(accountId * 10);
                var mdlPlayer = await CreateMdlAsync(new PlayerModel
                {
                    Id = _userRepo.RpcContext.PlayerId,
                    AccountId = accountId,
                    SfId = IdHelper.GenerateSfId(),
                    ProfileName = "",
                });
                return new PlayerManager(_userRepo, mdlPlayer);
            }

            return await GetAsync();
        }

        public async Task<PlayerManager> GetAsync()
        {
            var playerId = _userRepo.RpcContext.PlayerId;
            ReqHelper.ValidContext(playerId != 0, "ZERO_PLAYER_ID", () => new { PlayerId = playerId });
            var outMdlPlayer = await TryGetAsync(playerId);
            ReqHelper.ValidContext(outMdlPlayer != null, "NOT_FOUND_PLAYER", () => new { PlayerId = playerId });
            return new PlayerManager(_userRepo, outMdlPlayer);
        }

        public Task<PlayerModel?> TryGetAsync(ulong id)
        {
            return GetMdlAsync(x => x.Id == id);
        }

        public bool TryGetByAccountId(ulong accountId, out PlayerModel outPlayer)
        {
            // AccountId는 ListKey(PlayerId) 기준 컬렉션 밖의 조회 → DB 직접 접근
            outPlayer = DbSession.Execute(db => db.SelectByConditions<PlayerModel>(new { AccountId = accountId }));
            return outPlayer != null;
        }
    }
}
