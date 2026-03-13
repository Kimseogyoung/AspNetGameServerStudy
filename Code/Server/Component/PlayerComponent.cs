using Server.Repo.Database;
using WebStudyServer.Base;
using WebStudyServer.Helper;
using WebStudyServer.Manager;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Repo.Cache;

namespace WebStudyServer.Component
{
    public class PlayerComponent : UserComponentBase<PlayerModel>
    {
        public static class Key
        {
            public static CacheKey Single(ulong playerId) => CacheKey.For<PlayerModel>(playerId, playerId);
            public static CacheKey ByAccount(ulong accountId) => CacheKey.Raw($"PlayerModel:AccountId:{accountId}");
            public static CacheKey List(ulong playerId) => CacheKey.ListFor<PlayerModel>(playerId);
        }

        public PlayerComponent(UserRepo userRepo, IDbLayer db) : base(userRepo, db) { }

        protected override CacheKey KeyFor(PlayerModel model) => Key.Single(model.Id);
        protected override CacheKey ListKeyFor(ulong playerId) => Key.List(playerId);

        public PlayerManager Touch()
        {
            var playerId = _userRepo.RpcContext.PlayerId;
            var accountId = _userRepo.RpcContext.AccountId;

            if (playerId == 0)
            {
                var mdlPlayer = CreateMdl(new PlayerModel
                {
                    Id = accountId * 10,
                    AccountId = accountId,
                    SfId = IdHelper.GenerateSfId(),
                    ProfileName = "",
                });
                _userRepo.RpcContext.SetPlayerId(mdlPlayer.Id);

                if (mdlPlayer == null)
                {
                    throw new Exception("NOT_FOUND_PLAYER");
                }

                return new PlayerManager(_userRepo, mdlPlayer);
            }

            return Get();
        }

        public PlayerManager Get()
        {
            var playerId = _userRepo.RpcContext.PlayerId;
            ReqHelper.ValidContext(playerId != 0, "ZERO_PLAYER_ID", () => new { PlayerId = playerId });
            ReqHelper.ValidContext(TryGet(playerId, out var outMdlPlayer), "NOT_FOUND_PLAYER", () => new { PlayerId = playerId });
            return new PlayerManager(_userRepo, outMdlPlayer);
        }

        public bool TryGet(ulong id, out PlayerModel outPlayer)
        {
            outPlayer = GetMdl(
                Key.Single(id),
                db => db.SelectByPk<PlayerModel>(new { Id = id }));
            return outPlayer != null;
        }

        public bool TryGetByAccountId(ulong accountId, out PlayerModel outPlayer)
        {
            outPlayer = GetMdl(
                Key.ByAccount(accountId),
                db => db.SelectByPk<PlayerModel>(new { AccountId = accountId }));
            return outPlayer != null;
        }
    }
}
