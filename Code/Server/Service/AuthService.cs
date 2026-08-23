using Proto;
using Protocol;
using Server.Repo;
using ServerCore;
using WebStudyServer;
using WebStudyServer.Base;
using WebStudyServer.Data;
using WebStudyServer.Data.Queries;
using WebStudyServer.Model;
using WebStudyServer.Repo;

namespace WebStudyServer.Service
{
    public class AuthService : ServiceBase
    {
        // 이관 기간에는 두 진입점을 동시에 듦. 같은 DbSessionManager라 같은 트랜잭션.
        public AuthService(GlobalDbRepo dbRepo, GameDb db, RpcContext rpcContext, ILogger<AuthService> logger) : base(db, rpcContext, logger)
        {
            _dbRepo = dbRepo;
        }

        public async Task<AuthSignUpResponsePacket> SignUpAsync(string idfv)
        {
            // idfv 찾기.
            var (foundDevice, device) = await Db.Identity.TryGetDeviceAsync(idfv);
            if (foundDevice)
            {
                // 일치하는 idfv가 이미 있다면 해당 계정 정보 리턴
                var foundAuthScope = Db.Auth(device.AccountId);

                // 계정 찾기
                var (hasAccount, foundAccount) = await foundAuthScope.TryGetAccountAsync();
                if (hasAccount)
                {
                    var foundChannel = (await foundAuthScope.GetChannelListAsync()).Active();
                    if (foundChannel != null)
                    {
                        var foundMdlSession = await TouchSessionAsync(foundAccount.Id, foundAccount.ShardId);
                        RpcContext.SetSessionKey(await Db.Sessions.StartAsync(foundMdlSession, Stamp));

                        return new AuthSignUpResponsePacket
                        {
                            Result = new SignInResultPacket
                            {
                                SessionKey = foundMdlSession.Key,
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
            var newAccount = await Db.Identity.CreateAccountAsync();
            var newAuthScope = Db.Auth(newAccount.Id);

            // Session 생성
            var newMdlSession = await TouchSessionAsync(newAccount.Id, newAccount.ShardId);
            // Device 정보 생성
            _ = await newAuthScope.CreateDeviceAsync(idfv);
            // 채널 생성
            var newChannel = await newAuthScope.CreateChannelAsync(EChannelType.GUEST);

            // 세션 갱신 및 리턴
            RpcContext.SetSessionKey(await Db.Sessions.StartAsync(newMdlSession, Stamp));

            return new AuthSignUpResponsePacket
            {
                Result = new SignInResultPacket
                {
                    SessionKey = newMdlSession.Key,
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
            var channel = await Db.Identity.GetChannelAsync(channelId);

            // 채널 -> Account 찾기
            var authScope = Db.Auth(channel.AccountId);
            var account = (await authScope.GetAccountAsync()).EnsureActive();

            // 세션 갱신 및 리턴
            var mdlSession = await TouchSessionAsync(account.Id, account.ShardId);
            RpcContext.SetSessionKey(await Db.Sessions.StartAsync(mdlSession, Stamp));
            return new AuthSignInResponsePacket
            {
                Result = new SignInResultPacket
                {
                    SessionKey = mdlSession.Key,
                    ChannelKey = channel.Key,
                    AccountState = account.State,
                    AccountEnv = Config<CoreConfig>.Get().EnvName,
                    ClientSecret = ""
                }
            };
        }

        // 없으면 만든다. StartAsync 가 어차피 상태를 ACTIVE 로 덮으므로 따로 만료시키지 않는다.
        private async Task<SessionModel> TouchSessionAsync(ulong accountId, int shardId)
        {
            var (found, mdlSession) = await Db.Sessions.TryGetByAccountIdAsync(accountId);
            return found ? mdlSession : await Db.Sessions.CreateAsync(accountId, shardId, Stamp);
        }

        private readonly GlobalDbRepo _dbRepo;
        private AuthRepo Auth => _dbRepo.Auth;
    }
}
