namespace RaidServer.Network
{
    public interface IPacketHandler
    {
        ushort Opcode { get; }
        Type Req { get; }
        bool RequireAuth => false;
        Task RunAsync(string sessionId, object req);
    }
}
