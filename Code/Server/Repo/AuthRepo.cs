using Dapper;
using System.Numerics;
using System.Threading.Channels;
using WebStudyServer.Base;
using WebStudyServer.Extension;
using WebStudyServer.GAME;
using WebStudyServer.Model;

namespace WebStudyServer.Repo
{
    public class AuthRepo : RepoBase
    {
        public RpcContext RpcContext {  get; private set; }
        protected override List<string> _dbConnStrList => APP.Cfg.AuthDbConnectionStrList;
        public AuthRepo(RpcContext rpcContext)
        {
            //Transation Filter에서 Init중
            //Init(rpcContext.ShardId);

            RpcContext = rpcContext;
        }

        public static AuthRepo CreateInstance(RpcContext rpcContext)
        {
            var authRepo = new AuthRepo(rpcContext);
            authRepo.Init(0);
            return authRepo;
        }

        #region ACCOUNT
        public AccountModel CreateAccount(AccountModel newAccount)
        {
            // 데이터베이스에 삽입
            _executor.Excute((sqlConnection, transaction) =>
            {
                newAccount = sqlConnection.Insert<AccountModel>(newAccount, transaction);
            });

            return newAccount; // 새로 생성된 계정 모델 반환
        }

        public AccountModel GetAccount(ulong id)
        {
            AccountModel mdlAccount = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlAccount = sqlConnection.SelectByPk<AccountModel>(new { Id = id }, transaction);
            });

            return mdlAccount;
        }

        public void UpdateAccount(AccountModel mdlAccount)
        {
            _executor.Excute((sqlConnection, transaction) =>
            {
                sqlConnection.Update(mdlAccount, transaction);
            });
        }
        #endregion

        #region DEVICE
        public DeviceModel CreateDevice(DeviceModel inChannel)
        {
            DeviceModel newDevice = null;
            // 데이터베이스에 삽입
            _executor.Excute((sqlConnection, transaction) =>
            {
                newDevice = sqlConnection.Insert(inChannel, transaction);
            });

            return newDevice; // 새로 생성된 계정 모델 반환
        }

        public DeviceModel GetDevice(string deviceKey)
        {
            DeviceModel mdlDevice = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlDevice = sqlConnection.SelectByPk<DeviceModel>(new { Key = deviceKey }, transaction);
            });

            return mdlDevice;
        }

        public void UpdateDevice(DeviceModel mdlDevice)
        {
            _executor.Excute((sqlConnection, transaction) =>
            {
                sqlConnection.Update(mdlDevice, transaction);
            });
        }
        #endregion

        #region CHANNEL
        public ChannelModel CreateChannel(ChannelModel inChannel)
        {
            ChannelModel newChannel = null;
            // 데이터베이스에 삽입
            _executor.Excute((sqlConnection, transaction) =>
            { 
                newChannel = sqlConnection.Insert(inChannel, transaction);
            });

            return newChannel; // 새로 생성된 계정 모델 반환
        }

        public List<ChannelModel> GetChannelList(ulong accountId)
        {
           var mdlChannelList = new List<ChannelModel>();

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlChannelList = sqlConnection.SelectListByConditions<ChannelModel>(new { AccountId = accountId }, transaction).ToList();
            });

            return mdlChannelList;
        }


        public ChannelModel GetChannel(string key)
        {
            ChannelModel mdlChannel = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlChannel = sqlConnection.SelectByPk<ChannelModel>(new { Key = key }, transaction);
            });

            return mdlChannel;
        }

        public void UpdateChannel(ChannelModel mdlChannel)
        {
            _executor.Excute((sqlConnection, transaction) =>
            {
                sqlConnection.Update(mdlChannel, transaction);
            });
        }
        #endregion

        #region SESSION
        public SessionModel CreateSession(SessionModel inSession)
        {
            SessionModel newSession = null;
            // 데이터베이스에 삽입
            _executor.Excute((sqlConnection, transaction) =>
            {
                newSession = sqlConnection.Insert(inSession, transaction);
            });

            return newSession; // 새로 생성된 계정 모델 반환
        }

        public SessionModel GetSession(string key)
        {
            SessionModel mdlSession = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlSession = sqlConnection.SelectByConditions<SessionModel>(new { Key = key}, transaction);
            });

            return mdlSession;
        }

        public SessionModel GetSessionByAccountId(ulong accountId)
        {
            SessionModel mdlSession = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                mdlSession = sqlConnection.SelectByConditions<SessionModel>(new { AccountId = accountId }, transaction);
            });

            return mdlSession;
        }

        public void UpdateSession(SessionModel mdlSession)
        {
            _executor.Excute((sqlConnection, transaction) =>
            {
                sqlConnection.Update(mdlSession, transaction);
            });
        }
        #endregion

        #region PLAYER_MAP
        public PlayerMapModel CreatePlayerMap(PlayerMapModel inPlayerMap)
        {
            PlayerMapModel newPlayerMap = null;
            // 데이터베이스에 삽입
            _executor.Excute((sqlConnection, transaction) =>
            {
                newPlayerMap = sqlConnection.Insert(inPlayerMap, transaction);
            });

            return newPlayerMap;
        }

        public bool TryGetPlayerMap(ulong accountId, out PlayerMapModel outPlayerMap)
        {
            PlayerMapModel playerMap = null;

            _executor.Excute((sqlConnection, transaction) =>
            {
                playerMap = sqlConnection.SelectByPk<PlayerMapModel>(new { AccountId = accountId}, transaction);
            });

            outPlayerMap = playerMap;
            return outPlayerMap != null;
        }
        #endregion


    }
}
