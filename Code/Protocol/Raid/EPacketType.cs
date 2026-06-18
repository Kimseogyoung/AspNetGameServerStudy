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

        // --- Matching ---
        MatchingStartReq,
        MatchingStartRes,
        MatchingCancelReq,
        MatchingCancelRes,
        MatchingCompleteNotify,   // S -> C, 매칭 성립 알림
    }
}
