namespace Protocol.Raid
{
    public enum EPacketType : ushort
    {
        EchoReq,
        EchoRes,
        AuthReq,
        AuthRes,
        PingReq,
        PongRes,
        EchoAuthReq,
        EchoAuthRes,
    }
}
