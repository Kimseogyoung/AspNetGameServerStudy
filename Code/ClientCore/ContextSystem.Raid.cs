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
        }

        public Task RequestRaidDisconnectAsync()
        {
            RaidSystem.Close();
            Console.WriteLine("Raid 서버 접속 종료");
            return Task.CompletedTask;
        }

        public async Task<EchoResPacket> RequestRaidEchoAsync(string message)
        {
            var req = new EchoReqPacket { Message = message };
            var res = await RaidSystem.RequestAsync<EchoReqPacket, EchoResPacket>((ushort)EPacketType.EchoReq, EProtocolType.Json, req);
            Console.WriteLine($"Echo 응답: {res.Message}");
            return res;
        }
    }
}
