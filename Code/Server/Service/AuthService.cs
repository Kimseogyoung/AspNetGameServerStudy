using Proto;
using Protocol;
using Server.Repo;
using ServerCore;
using WebStudyServer;
using WebStudyServer.Base;
using WebStudyServer.Repo;

namespace WebStudyServer.Service
{
    public class AuthService : ServiceBase
    {
        public AuthService(GlobalDbRepo dbRepo, RpcContext rpcContext, ILogger<AuthService> logger) : base(rpcContext, logger)
        {
            _dbRepo = dbRepo;
        }

        public AuthSignUpResponsePacket SignUp(string idfv)
        {
            // idfv 찾기.           
            if (Auth.Device.TryGet(idfv, out var mgrDevice))
            {
                // 일치하는 idfv가 이미 있다면 해당 계정 정보 리턴

                // 계정 찾기
                if (Auth.Account.TryGet(mgrDevice.Model.AccountId, out var originMgrAccount))
                {
                    if (Auth.Channel.TryGetActive(originMgrAccount.Id, out var originMgrChannel))
                    {
                        var originMgrSession = Auth.Session.Touch(originMgrAccount.Id);
                        originMgrSession.Expire(); // 기존 세션 무효화
                        originMgrSession.Start();

                        return new AuthSignUpResponsePacket
                        {
                            Result = new SignInResultPacket
                            {
                                SessionKey = originMgrSession.Model.Key,
                                ChannelKey = originMgrChannel.Model.Key,
                                AccountState = originMgrAccount.Model.State,
                                AccountEnv = Config<CoreConfig>.Get().EnvName,
                                ClientSecret = ""
                            }
                        };
                    }
                }
            }

            // ~idfv가 없다면

            // Account 생성
            var mgrAccount = Auth.Account.Create();
            // Session 생성
            var mgrSession = Auth.Session.Touch(mgrAccount.Id);
            // Device 정보 생성
            _ = Auth.Device.Create(idfv);
            // 채널 생성
            var mgrChannel = Auth.Channel.Create(mgrAccount.Id, EChannelType.GUEST);

            // 세션 갱신 및 리턴
            mgrSession.Start();

            return new AuthSignUpResponsePacket
            {
                Result = new SignInResultPacket
                {
                    SessionKey = mgrSession.Model.Key,
                    ChannelKey = mgrChannel.Model.Key,
                    AccountState = mgrAccount.Model.State,
                    AccountEnv = Config<CoreConfig>.Get().EnvName,
                    ClientSecret = ""
                }
            };
        }

        public AuthSignInResponsePacket SignIn(string channelId)
        {
            // 채널 찾기
            var mgrChannel = Auth.Channel.Get(channelId);

            // 채널 -> Account 찾기
            var mgrAccount = Auth.Account.GetActive(mgrChannel.Model.AccountId);

            // 세션 갱신 및 리턴
            var mgrSession = Auth.Session.Touch(mgrAccount.Id);
            mgrSession.Expire(); // 기존 세션 무효화
            mgrSession.Start();
            return new AuthSignInResponsePacket
            {
                Result = new SignInResultPacket
                {
                    SessionKey = mgrSession.Model.Key,
                    ChannelKey = mgrChannel.Model.Key,
                    AccountState = mgrAccount.Model.State,
                    AccountEnv = Config<CoreConfig>.Get().EnvName,
                    ClientSecret = ""
                }
            };
        }

        private readonly GlobalDbRepo _dbRepo;
        private AuthRepo Auth => _dbRepo.Auth;
    }
}
