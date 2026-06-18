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

            RaidSystem.RegisterPushHandler(
                (ushort)EPacketType.MatchingCompleteNotify,
                (protocolType, payload) =>
                {
                    var notify = RaidSystem.Deserialize<MatchingCompleteNotifyPacket>(protocolType, payload);
                    Console.WriteLine($"[매칭 완료] RoomId({notify.RoomId}) BossNum({notify.BossNum}) Members({notify.Members.Count})");
                    foreach (var m in notify.Members)
                    {
                        Console.WriteLine($"  - SfId({m.SfId}) Name({m.ProfileName})");
                    }
                });

            var req = new AuthRequestPacket { SessionKey = RpcSystem.SessionId, DeviceKey = RpcSystem.DeviceKey };
            var res = await RaidSystem.RequestWithWaitAsync<AuthRequestPacket, AuthResponsePacket>((ushort)EPacketType.AuthRequest, (ushort)EPacketType.AuthResponse, EProtocolType.Json, req);
            Console.WriteLine($"Raid 인증 응답: Result({res.Result}) AccountId({res.AccountId}) PlayerId({res.PlayerId}) ShardId({res.ShardId})");

            if (res.Result == EAuthResult.Success)
            {
                RaidSystem.StartPingLoop(TimeSpan.FromSeconds(res.PingIntervalSec));
            }
            else
            {
                RaidSystem.Close();
            }
        }

        public async Task RequestRaidMatchingStartAsync(int bossNum)
        {
            var req = new MatchingStartRequestPacket { BossNum = bossNum };
            var res = await RaidSystem.RequestWithWaitAsync<MatchingStartRequestPacket, MatchingStartResponsePacket>((ushort)EPacketType.MatchingStartRequest, (ushort)EPacketType.MatchingStartResponse, EProtocolType.Json, req);
            Console.WriteLine($"매칭 시작 응답: Result({res.Result})");
        }

        public async Task RequestRaidMatchingCancelAsync()
        {
            var req = new MatchingCancelRequestPacket();
            var res = await RaidSystem.RequestWithWaitAsync<MatchingCancelRequestPacket, MatchingCancelResponsePacket>((ushort)EPacketType.MatchingCancelRequest, (ushort)EPacketType.MatchingCancelResponse, EProtocolType.Json, req);
            Console.WriteLine($"매칭 취소 응답: Result({res.Result})");
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
            var res = await RaidSystem.RequestWithWaitAsync<EchoRequestPacket, EchoResponsePacket>((ushort)EPacketType.EchoRequest, (ushort)EPacketType.EchoResponse, EProtocolType.Json, req);
            Console.WriteLine($"Echo 응답: {res.Message}");
            return res;
        }

        public async Task<EchoResponsePacket> RequestRaidEchoAuthAsync(string message)
        {
            var req = new EchoRequestPacket { Message = message };
            var res = await RaidSystem.RequestWithWaitAsync<EchoRequestPacket, EchoResponsePacket>((ushort)EPacketType.EchoAuthRequest, (ushort)EPacketType.EchoAuthResponse, EProtocolType.Json, req);
            Console.WriteLine($"Echo(Auth) 응답: {res.Message}");
            return res;
        }
    }
}
