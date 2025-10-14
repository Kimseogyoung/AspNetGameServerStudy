using Protocol;

namespace ClientCore
{
    public partial class ContextSystem
    {
        public string SessionId => _rpcSystem.SessionId;

        public void Init(string serverUrl)
        {
            _rpcSystem = new RpcSystem();
            _rpcSystem.Init(serverUrl, MsgProtocol.ProtoBufContentType);
        }

        public void Clear()
        {
            _player = null;
            _rpcSystem.Clear();
        }

        public async Task RequestSignUpAsync(string deviceKey)
        {
            var req = new AuthSignUpReqPacket(deviceKey);

            var res = await _rpcSystem.RequestAsync<AuthSignUpReqPacket, AuthSignUpResPacket>(req);
            _rpcSystem.SetSessionKey(res.Result.SessionKey);
        }

        public async Task RequestSignInAsync(string channelId)
        {
            var req = new AuthSignInReqPacket(channelId);
            var res = await _rpcSystem.RequestAsync<AuthSignInReqPacket, AuthSignInResPacket>(req);
            _rpcSystem.SetSessionKey(res.Result.SessionKey);
        }

        public async Task RequestEnterAsync()
        {
            var req = new GameEnterReqPacket();
            var res = await _rpcSystem.RequestAsync<GameEnterReqPacket, GameEnterResPacket>(req);
            
            _player = res.Player;

            RefreshKingdom();
        }

        public async Task RequestChangeNameAsync(string name)
        {
            var befName = _player.ProfileName;
            var req = new GameChangeNameReqPacket(name);
            var res = await _rpcSystem.RequestAsync<GameChangeNameReqPacket, GameChangeNameResPacket>(req);
            Console.WriteLine($"Name  {befName} -> {res.PlayerName}");
            _player.ProfileName = res.PlayerName;
        }

        private PlayerPacket _player;
        private RpcSystem _rpcSystem;
    }
}
