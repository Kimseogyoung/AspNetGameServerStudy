using Proto;
using Protocol;
using Server.Repo;
using ServerCore;
using WebStudyServer;
using WebStudyServer.Base;
using WebStudyServer.Data;
using WebStudyServer.Data.Queries;
using WebStudyServer.Repo;

namespace WebStudyServer.Service
{
    public class AuthService : ServiceBase
    {
        // 이관 기간에는 두 진입점을 동시에 듦. 같은 DbSessionManager라 같은 트랜잭션.
        public AuthService(GlobalDbRepo dbRepo, GameDb db, RpcContext rpcContext, ILogger<AuthService> logger) : base(rpcContext, logger)
        {
            _dbRepo = dbRepo;
            _db = db;
        }

        public async Task<AuthSignUpResponsePacket> SignUpAsync(string idfv)
        {
            // idfv 찾기.
            var (foundDevice, device) = await _db.Identity.TryGetDeviceAsync(idfv);
            if (foundDevice)
            {
                // 일치하는 idfv가 이미 있다면 해당 계정 정보 리턴
                var foundAuthScope = _db.Auth(device.AccountId);

                // 계정 찾기
                var (hasAccount, foundAccount) = await foundAuthScope.TryGetAccountAsync();
                if (hasAccount)
                {
                    var foundChannel = (await foundAuthScope.GetChannelListAsync()).Active();
                    if (foundChannel != null)
                    {
                        var foundMgrSession = await Auth.Session.TouchAsync(foundAccount.Id);
                        await foundMgrSession.ExpireAsync(); // 기존 세션 무효화
                        await foundMgrSession.StartAsync();

                        return new AuthSignUpResponsePacket
                        {
                            Result = new SignInResultPacket
                            {
                                SessionKey = foundMgrSession.Model.Key,
                                ChannelKey = foundChannel.Key,
                                AccountState = foundAccount.State,
                                AccountEnv = Config<CoreConfig>.Get().EnvName,
                                ClientSecret = ""
                            }
                        };
                    }
                }
            }

            // ~idfv가 없다면

            // Account 생성
            var newAccount = await _db.Identity.CreateAccountAsync();
            var newAuthScope = _db.Auth(newAccount.Id);

            // SessionComponent가 RpcContext.ShardId를 읽음. Session 이관 시 함께 제거.
            RpcContext.SetShardId(newAccount.ShardId);

            // Session 생성
            var newMgrSession = await Auth.Session.TouchAsync(newAccount.Id);
            // Device 정보 생성
            _ = await newAuthScope.CreateDeviceAsync(idfv);
            // 채널 생성
            var newChannel = await newAuthScope.CreateChannelAsync(EChannelType.GUEST);

            // 세션 갱신 및 리턴
            await newMgrSession.StartAsync();

            return new AuthSignUpResponsePacket
            {
                Result = new SignInResultPacket
                {
                    SessionKey = newMgrSession.Model.Key,
                    ChannelKey = newChannel.Key,
                    AccountState = newAccount.State,
                    AccountEnv = Config<CoreConfig>.Get().EnvName,
                    ClientSecret = ""
                }
            };
        }

        public async Task<AuthSignInResponsePacket> SignInAsync(string channelId)
        {
            // 채널 찾기
            var channel = await _db.Identity.GetChannelAsync(channelId);

            // 채널 -> Account 찾기
            var authScope = _db.Auth(channel.AccountId);
            var account = (await authScope.GetAccountAsync()).EnsureActive();

            // 세션 갱신 및 리턴
            var mgrSession = await Auth.Session.TouchAsync(account.Id);
            await mgrSession.ExpireAsync(); // 기존 세션 무효화
            await mgrSession.StartAsync();
            return new AuthSignInResponsePacket
            {
                Result = new SignInResultPacket
                {
                    SessionKey = mgrSession.Model.Key,
                    ChannelKey = channel.Key,
                    AccountState = account.State,
                    AccountEnv = Config<CoreConfig>.Get().EnvName,
                    ClientSecret = ""
                }
            };
        }

        private readonly GlobalDbRepo _dbRepo;
        private readonly GameDb _db;
        private AuthRepo Auth => _dbRepo.Auth;
    }
}
