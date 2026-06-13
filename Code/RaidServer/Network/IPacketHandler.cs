namespace RaidServer.Network
{
    public interface IPacketHandler
    {
        ushort Opcode { get; }
        Type Req { get; }
        Task RunAsync(string sessionId, object req);
    }
}
