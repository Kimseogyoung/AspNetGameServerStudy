using ServerCore;

namespace RaidServer.Context
{
    public class RaidGameContext : IGameContext
    {
        public ulong AccountId { get; private set; }
        public ulong PlayerId { get; private set; }
        public int ShardId { get; private set; }
        public DateTime ServerTime { get; private set; } = DateTime.UtcNow;
        public string DeviceKey { get; private set; } = string.Empty;
        public string Ip { get; private set; } = string.Empty;

        public void Init(string deviceKey)
        {
            DeviceKey = deviceKey;
            ServerTime = DateTime.UtcNow;
        }

        public void SetAccountId(ulong accountId) => AccountId = accountId;
        public void SetPlayerId(ulong playerId) => PlayerId = playerId;
        public void SetShardId(int shardId) => ShardId = shardId;
        public void SetSessionKey(string sessionKey) { }
    }
}
