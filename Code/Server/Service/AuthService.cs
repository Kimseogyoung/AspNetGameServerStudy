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

        public async Task<AuthSignUpResponsePacket> SignUpAsync(string idfv)
        {
            // idfv 찾기.
            var (foundDevice, mgrDevice) = await Auth.Device.TryGetAsync(idfv);
            if (foundDevice)
            {
                // 일치하는 idfv가 이미 있다면 해당 계정 정보 리턴

                // 계정 찾기
                var (foundAccount, originMgrAccount) = await Auth.Account.TryGetAsync(mgrDevice.Model.AccountId);
                if (foundAccount)
                {
                    var (foundChannel, originMgrChannel) = await Auth.Channel.TryGetActiveAsync(originMgrAccount.Id);
                    if (foundChannel)
                    {
                        var originMgrSession = await Auth.Session.TouchAsync(originMgrAccount.Id);
                        await originMgrSession.ExpireAsync(); // 기존 세션 무효화
                        await originMgrSession.StartAsync();

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
            var mgrAccount = await Auth.Account.CreateAsync();
            // Session 생성
            var mgrSession = await Auth.Session.TouchAsync(mgrAccount.Id);
            // Device 정보 생성
            _ = await Auth.Device.CreateAsync(idfv);
            // 채널 생성
            var mgrChannel = await Auth.Channel.CreateAsync(mgrAccount.Id, EChannelType.GUEST);

            // 세션 갱신 및 리턴
            await mgrSession.StartAsync();

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

        public async Task<AuthSignInResponsePacket> SignInAsync(string channelId)
        {
            // 채널 찾기
            var mgrChannel = await Auth.Channel.GetAsync(channelId);

            // 채널 -> Account 찾기
            var mgrAccount = await Auth.Account.GetActiveAsync(mgrChannel.Model.AccountId);

            // 세션 갱신 및 리턴
            var mgrSession = await Auth.Session.TouchAsync(mgrAccount.Id);
            await mgrSession.ExpireAsync(); // 기존 세션 무효화
            await mgrSession.StartAsync();
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
