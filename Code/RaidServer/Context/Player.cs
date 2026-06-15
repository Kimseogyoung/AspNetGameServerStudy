namespace RaidServer.Context
{
    public class Player
    {
        public ulong AccountId { get; init; }
        public ulong PlayerId { get; init; }
        public int ShardId { get; init; }
        public RaidPlayerProfile Profile { get; init; } = new();

        // 현재 연결을 가리키는 포인터. 재접속 시 PlayerService가 갱신한다.
        public string SessionId { get; set; } = string.Empty;
    }
}
