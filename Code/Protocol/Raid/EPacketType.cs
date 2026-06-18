namespace Protocol.Raid
{
    public enum EPacketType : ushort
    {
        EchoRequest,
        EchoResponse,
        AuthRequest,
        AuthResponse,
        PingRequest,
        PongResponse,
        EchoAuthRequest,
        EchoAuthResponse,

        // --- Matching ---
        MatchingStartRequest,
        MatchingStartResponse,
        MatchingCancelRequest,
        MatchingCancelResponse,
        MatchingCompleteNotify,   // S -> C, 매칭 성립 알림
    }
}
