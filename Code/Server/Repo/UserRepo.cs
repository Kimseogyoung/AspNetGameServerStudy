using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.GAME;
using WebStudyServer.Model;

namespace WebStudyServer.Repo
{
    public class UserRepo : RepoBase
    {
        public RpcContext RpcContext { get; private set; }
        protected override List<string> _dbConnStrList => APP.Cfg.UserDbConnectionStrList;
        public UserRepo(RpcContext rpcContext)
        {
            RpcContext = rpcContext;
        }

        public static UserRepo CreateInstance(RpcContext rpcContext)
        {
            var userRepo = new UserRepo(rpcContext);
            return userRepo;
        }

        #region Player
        public PlayerModel CreatePlayer(PlayerModel newPlayer)
        {
            // 데이터베이스에 삽입
            _executor.Excute((sqlConnection, transaction) =>
            {
                newPlayer = sqlConnection.Insert<PlayerModel>(newPlayer, transaction);
            });

            return newPlayer; // 새로 생성된 플레이어 모델 반환
        }

        public bool TryGetPlayerByAccountId(ulong accountId, out PlayerModel outPlayer)
        {
            PlayerModel mdlPlayer = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlPlayer = sqlConnection.SelectByPk<PlayerModel>(new { AccountId = accountId }, transaction);
            });

            outPlayer = mdlPlayer;
            return outPlayer != null;
        }

        public bool TryGetPlayer(ulong id, out PlayerModel outPlayer)
        {
            PlayerModel mdlPlayer = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlPlayer = sqlConnection.SelectByPk<PlayerModel>(new { Id = id }, transaction);
            });

            outPlayer = mdlPlayer;
            return outPlayer != null;
        }

        public void UpdatePlayer(PlayerModel mdlPlayer)
        {
            _executor.Excute((sqlConnection, transaction) =>
            {
                sqlConnection.Update(mdlPlayer, transaction);
            });
        }
        #endregion
    }
}
