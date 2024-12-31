using WebStudyServer.Base;
using WebStudyServer.Manager;
using WebStudyServer.Repo;
using WebStudyServer.Model;
using WebStudyServer.Helper;
using WebStudyServer.Repo.Database;
using System;
using WebStudyServer.Extension;

namespace WebStudyServer.Component
{
    public class PlayerComponent : UserComponentBase<PlayerModel>
    {
        public PlayerComponent(UserRepo userRepo, DBSqlExecutor excutor) : base(userRepo, excutor)
        {
            
        }

        public PlayerManager Touch()
        {
            var playerId = _userRepo.RpcContext.PlayerId;
            var accountId = _userRepo.RpcContext.AccountId;

            PlayerModel mdlPlayer = null;
            if (playerId == 0)
            {
                var newPlayerId = accountId * 10;
                // 신규 플레이어 생성
                mdlPlayer = Create(new PlayerModel
                {
                    Id = newPlayerId,
                    AccountId = accountId,
                    Lv = 1,
                    ProfileName = IdHelper.GenerateRandomName(), // TODO: 중복 닉네임 제한 하고싶음.
                });
                _userRepo.RpcContext.SetPlayerId(mdlPlayer.Id);

                if (mdlPlayer == null)
                    throw new Exception("NOT_FOUND_PLAYER"); // TODO:  오류 발생

            /*    _authRepo.CreatePlayerMap(new PlayerMapModel
                {
                    AccountId = accountId,
                    PlayerId = mdlPlayer.Id,
                    ShardId = _userRepo.ShardId,
                });

                if (_authRepo.TryGetSessionByAccountId(accountId, out var mdlSession))
                {
                    mdlSession.PlayerId = newPlayerId;
                    _authRepo.UpdateSession(mdlSession);
                }

                _authRepo.Commit(); // TODO: 개선*/

                var newMgrPlayer = new PlayerManager(_userRepo, mdlPlayer);
                return newMgrPlayer;
            }
            else
            {
                // 기존 플레이어 로드
                var mgrPlayer = Get();
                return mgrPlayer;
            }
        }

        public PlayerManager Get()
        {
            var playerId = _userRepo.RpcContext.PlayerId;
            ReqHelper.ValidContext(playerId != 0, "ZERO_PLAYER_ID", () => new { PlayerId = playerId });
            ReqHelper.ValidContext(TryGet(playerId, out var outMdlPlayer), "NOT_FOUND_PLAYER", ()=> new {PlayerId = playerId});
            var mgrPlayer = new PlayerManager(_userRepo, outMdlPlayer);
            return mgrPlayer;
        }

        public bool TryGetByAccountId(ulong accountId, out PlayerModel outPlayer)
        {
            PlayerModel mdlPlayer = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlPlayer = sqlConnection.SelectByPk<PlayerModel>(new { AccountId = accountId }, transaction);
            });

            outPlayer = mdlPlayer;
            return outPlayer != null;
        }

        public bool TryGet(ulong id, out PlayerModel outPlayer)
        {
            PlayerModel mdlPlayer = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlPlayer = sqlConnection.SelectByPk<PlayerModel>(new { Id = id }, transaction);
            });

            outPlayer = mdlPlayer;
            return outPlayer != null;
        }
    }
}
