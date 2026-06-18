using System;
using System.Threading.Tasks;
using Proto;
using Protocol;
namespace ClientCore
{
    public partial class ContextSystem
    {
        public PlayerPacket Player { get; private set; }
        public RpcSystem RpcSystem { get; private set; }

        public readonly ResponseInfoPacket _errorRes = new ResponseInfoPacket { ResultCode = (int)EErrorCode.NO_HANDLING_ERROR };

        public void Init(string serverUrl, TimeSpan timeoutSpan)
        {
            RpcSystem = new RpcSystem();
            RpcSystem.Init(serverUrl, MsgProtocol.ProtoBufContentType, timeoutSpan);
        }

        public void Clear()
        {
            Player = null;
            RpcSystem.Clear();
            RaidSystem.Close();
        }

        public bool IsErrorRes(IResponsePacket res)
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

        public async Task<HealthCheckResponsePacket> RequestHealthCheckAsync()
        {
            var req = new HealthCheckRequestPacket();
            var res = await RpcSystem.RequestAsync<HealthCheckRequestPacket, HealthCheckResponsePacket>(req);
            return res;
        }

        public async Task<AuthSignUpResponsePacket> RequestSignUpAsync(string deviceKey)
        {
            var req = new AuthSignUpRequestPacket(deviceKey);

            var res = await RpcSystem.RequestAsync<AuthSignUpRequestPacket, AuthSignUpResponsePacket>(req);
            RpcSystem.SetSessionKey(res.Result.SessionKey);
            return res;
        }

        public async Task<AuthSignInResponsePacket> RequestSignInAsync(string channelId)
        {
            var req = new AuthSignInRequestPacket(channelId);
            var res = await RpcSystem.RequestAsync<AuthSignInRequestPacket, AuthSignInResponsePacket>(req);
            RpcSystem.SetSessionKey(res.Result.SessionKey);
            return res;
        }

        public async Task<GameEnterResponsePacket> RequestEnterAsync()
        {
            var req = new GameEnterRequestPacket();
            var res = await RpcSystem.RequestAsync<GameEnterRequestPacket, GameEnterResponsePacket>(req);

            Player = res.Player;

            RefreshKingdom();
            return res;
        }

        public async Task<GameChangeNameResponsePacket> RequestChangeNameAsync(string name)
        {
            var befName = Player.ProfileName;
            var req = new GameChangeNameRequestPacket(name);
            var res = await RpcSystem.RequestAsync<GameChangeNameRequestPacket, GameChangeNameResponsePacket>(req);
            Console.WriteLine($"Name  {befName} -> {res.PlayerName}");
            Player.ProfileName = res.PlayerName;
            return res;
        }
    }
}
