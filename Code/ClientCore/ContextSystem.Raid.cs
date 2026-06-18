using System;
using System.Threading.Tasks;
using Protocol.Raid;

namespace ClientCore
{
    public partial class ContextSystem
    {
        public RaidSystem RaidSystem { get; } = new RaidSystem();

        public async Task RequestRaidConnectAsync(string host, int port)
        {
            await RaidSystem.ConnectAsync(host, port);
            Console.WriteLine($"Raid 서버 접속: {host}:{port}");

            var req = new AuthRequestPacket { SessionKey = RpcSystem.SessionId, DeviceKey = RpcSystem.DeviceKey };
            var res = await RaidSystem.RequestAsync<AuthRequestPacket, AuthResponsePacket>((ushort)EPacketType.AuthRequest, EProtocolType.Json, req);
            Console.WriteLine($"Raid 인증 응답: Result({res.Result}) AccountId({res.AccountId}) PlayerId({res.PlayerId}) ShardId({res.ShardId})");

            if (res.Result == EAuthResult.Success)
            {
                RaidSystem.StartPingLoop(TimeSpan.FromSeconds(10));
            }
            else
            {
                RaidSystem.Close();
            }
        }

        public Task RequestRaidDisconnectAsync()
        {
            RaidSystem.Close();
            Console.WriteLine("Raid 서버 접속 종료");
            return Task.CompletedTask;
        }

        public async Task<EchoResponsePacket> RequestRaidEchoAsync(string message)
        {
            var req = new EchoRequestPacket { Message = message };
            var res = await RaidSystem.RequestAsync<EchoRequestPacket, EchoResponsePacket>((ushort)EPacketType.EchoRequest, EProtocolType.Json, req);
            Console.WriteLine($"Echo 응답: {res.Message}");
            return res;
        }

        public async Task<EchoResponsePacket> RequestRaidEchoAuthAsync(string message)
        {
            var req = new EchoRequestPacket { Message = message };
            var res = await RaidSystem.RequestAsync<EchoRequestPacket, EchoResponsePacket>((ushort)EPacketType.EchoAuthRequest, EProtocolType.Json, req);
            Console.WriteLine($"Echo(Auth) 응답: {res.Message}");
            return res;
        }
    }
}
