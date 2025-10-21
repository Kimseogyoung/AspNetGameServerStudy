using Proto;
using Protocol;
using System;
using System.Threading.Tasks;
namespace ClientCore
{
    public partial class ContextSystem
    {
        public PlayerPacket Player { get; private set; }
        public RpcSystem RpcSystem { get; private set; }

        public readonly ResInfoPacket _errorRes = new ResInfoPacket { ResultCode = (int)EErrorCode.NO_HANDLING_ERROR };

        public void Init(string serverUrl, TimeSpan timeoutSpan)
        {
            RpcSystem = new RpcSystem();
            RpcSystem.Init(serverUrl, MsgProtocol.ProtoBufContentType, timeoutSpan);
        }

        public void Clear()
        {
            Player = null;
            RpcSystem.Clear();
        }

        public bool IsErrorRes(IResPacket res)
        {
            return res.Info.ResultCode != (int)EErrorCode.OK;
        }

        public async Task<bool> IsSuccessConnect()
        {
            var res = await RequestHealthCheckAsync();
            if (string.IsNullOrEmpty(res.Msg))
            {
                return false;
            }

            Console.WriteLine(res);
            return true;
        }

        public async Task<HealthCheckResPacket> RequestHealthCheckAsync()
        {
            var req = new HealthCheckReqPacket();
            var res = await RpcSystem.RequestAsync<HealthCheckReqPacket, HealthCheckResPacket>(req);
            return res;
        }

        public async Task<AuthSignUpResPacket> RequestSignUpAsync(string deviceKey)
        {
            var req = new AuthSignUpReqPacket(deviceKey);

            var res = await RpcSystem.RequestAsync<AuthSignUpReqPacket, AuthSignUpResPacket>(req);
            RpcSystem.SetSessionKey(res.Result.SessionKey);
            return res;
        }

        public async Task<AuthSignInResPacket> RequestSignInAsync(string channelId)
        {
            var req = new AuthSignInReqPacket(channelId);
            var res = await RpcSystem.RequestAsync<AuthSignInReqPacket, AuthSignInResPacket>(req);
            RpcSystem.SetSessionKey(res.Result.SessionKey);
            return res;
        }

        public async Task<GameEnterResPacket> RequestEnterAsync()
        {
            var req = new GameEnterReqPacket();
            var res = await RpcSystem.RequestAsync<GameEnterReqPacket, GameEnterResPacket>(req);
            
            Player = res.Player;

            RefreshKingdom();
            return res;
        }

        public async Task<GameChangeNameResPacket> RequestChangeNameAsync(string name)
        {
            var befName = Player.ProfileName;
            var req = new GameChangeNameReqPacket(name);
            var res = await RpcSystem.RequestAsync<GameChangeNameReqPacket, GameChangeNameResPacket>(req);
            Console.WriteLine($"Name  {befName} -> {res.PlayerName}");
            Player.ProfileName = res.PlayerName;
            return res;
        }
    }
}
