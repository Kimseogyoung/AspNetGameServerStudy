using WebStudyServer.Base;
using WebStudyServer;
using WebStudyServer.Repo;
using Proto;
using WebStudyServer.GAME;

namespace WebStudyServer.Service
{
    public class AuthService : ServiceBase
    {
        public AuthService(AuthComponent authComp, RpcContext rpcContext, ILogger<AuthService> logger) :base(rpcContext, logger) 
        {
            _authComp = authComp;
        }

        public AuthSignInResult SignUp(string idfv)
        {
            // idfv 찾기.           
            if (_authComp.Device.TryGetDevice(idfv, out var mgrDevice))
            {
                // 일치하는 idfv가 이미 있다면 해당 계정 정보 리턴

                // 계정 찾기
                if (_authComp.Account.TryGetAccount(mgrDevice.Model.AccountId, out var originMgrAccount))
                {
                    if (_authComp.Channel.TryGetActiveChannel(originMgrAccount.Id, out var originMgrChannel))
                    {
                        var originMgrSession = _authComp.Session.Touch(originMgrAccount.Id);
                        originMgrSession.Start();
                        return new AuthSignInResult
                        {
                            AccountState = originMgrAccount.Model.State,
                            SessionKey = originMgrSession.Model.Key,
                            ChannelKey = originMgrChannel.Model.Key,
                            AccountEnv = APP.Cfg.EnvName,
                            ClientSecret = "",
                        };
                    }
                }
            }
            
            // ~idfv가 없다면

            // Account 생성
            var mgrAccount = _authComp.Account.CreateAccount();
            // Session 생성
            var mgrSession = _authComp.Session.Touch(mgrAccount.Id);
            // Device 정보 생성
            mgrDevice = _authComp.Device.CreateDevice(idfv);
            // 채널 생성
            var mgrChannel = _authComp.Channel.CreateChannel(mgrAccount.Id, EChannelType.GUEST);
            
            // 세션 갱신 및 리턴
            mgrSession.Start();

            return new AuthSignInResult
            {
                AccountState = mgrAccount.Model.State,
                SessionKey = mgrSession.Model.Key,
                ChannelKey = mgrChannel.Model.Key,
                AccountEnv = APP.Cfg.EnvName,
                ClientSecret = "",
            };
        }

        public AuthSignInResult SignIn(string channelId)
        {
            // 채널 찾기

            if (!_authComp.Channel.TryGetChannel(channelId, out var mgrChannel))
            {

            }
            //

            // 채널 -> Account 찾기

            // 세션 갱신 및 리턴
            return null;
        }

        private readonly AuthComponent _authComp;
    }
}
