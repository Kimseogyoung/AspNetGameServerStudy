using WebStudyServer.Base;
using WebStudyServer;
using WebStudyServer.Repo;
using Proto;
using WebStudyServer.GAME;
using Protocol;

namespace WebStudyServer.Service
{
    public class AuthService : ServiceBase
    {
        public AuthService(AuthRepo authRepo, RpcContext rpcContext, ILogger<AuthService> logger) :base(rpcContext, logger) 
        {
            _authRepo = authRepo;
        }

        public SignInResultPacket SignUp(string idfv)
        {
            // idfv 찾기.           
            if (_authRepo.Device.TryGet(idfv, out var mgrDevice))
            {
                // 일치하는 idfv가 이미 있다면 해당 계정 정보 리턴

                // 계정 찾기
                if (_authRepo.Account.TryGet(mgrDevice.Model.AccountId, out var originMgrAccount))
                {
                    if (_authRepo.Channel.TryGetActive(originMgrAccount.Id, out var originMgrChannel))
                    {
                        var originMgrSession = _authRepo.Session.Touch(originMgrAccount.Id);
                        originMgrSession.Start();
                        return new SignInResultPacket
                        {                           
                            SessionKey = originMgrSession.Model.Key,
                            ChannelKey = originMgrChannel.Model.Key,
                            AccountState = originMgrAccount.Model.State,
                            AccountEnv = APP.Cfg.EnvName,
                            ClientSecret = ""                      
                        };
                    }
                }
            }
            
            // ~idfv가 없다면

            // Account 생성
            var mgrAccount = _authRepo.Account.Create();
            // Session 생성
            var mgrSession = _authRepo.Session.Touch(mgrAccount.Id);
            // Device 정보 생성
            mgrDevice = _authRepo.Device.Create(idfv);
            // 채널 생성
            var mgrChannel = _authRepo.Channel.Create(mgrAccount.Id, EChannelType.GUEST);
            
            // 세션 갱신 및 리턴
            mgrSession.Start();

            return new SignInResultPacket
            {
                SessionKey = mgrSession.Model.Key,
                ChannelKey = mgrChannel.Model.Key,
                AccountState = mgrAccount.Model.State,
                AccountEnv = APP.Cfg.EnvName,
                ClientSecret = ""
            };
        }

        public SignInResultPacket SignIn(string channelId)
        {
            // 채널 찾기
            var mgrChannel = _authRepo.Channel.Get(channelId);

            // 채널 -> Account 찾기
            var mgrAccount = _authRepo.Account.GetActive(mgrChannel.Model.AccountId);

            // 세션 갱신 및 리턴
            var mgrSession = _authRepo.Session.Touch(mgrAccount.Id);
            mgrSession.Start();
            return new SignInResultPacket
            {
                SessionKey = mgrSession.Model.Key,
                ChannelKey = mgrChannel.Model.Key,
                AccountState = mgrAccount.Model.State,
                AccountEnv = APP.Cfg.EnvName,
                ClientSecret = ""
            };
        }

        private readonly AuthRepo _authRepo;
    }
}
