namespace RaidServer.Network
{
    public abstract class PacketHandlerBase<TReq> : IPacketHandler
    {
        public abstract ushort Opcode { get; }
        public Type Req => typeof(TReq);

        public Task RunAsync(string sessionId, object req)
        {
            return RunAsync(sessionId, (TReq)req);
        }

        protected abstract Task RunAsync(string sessionId, TReq req);
    }
}
