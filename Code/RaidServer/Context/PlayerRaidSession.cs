using WebStudyServer.Model;

namespace RaidServer.Context
{
    public enum EPlayerRaidState
    {
        IDLE,
        MATCHING,
        IN_ROOM,
    }

    public class PlayerRaidSession
    {
        public string SessionId { get; init; } = string.Empty;
        public int ShardId { get; init; }        // SessionModel에서 로드, PlayerModel에 없음
        public PlayerModel Player { get; init; } = null!;
        public EPlayerRaidState State { get; set; } = EPlayerRaidState.IDLE;
    }
}
