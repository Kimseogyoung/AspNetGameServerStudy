namespace ServerCore
{
    // DbModel(Manager/Component/Repo)이 의존하는 RpcContext의 최소 인터페이스.
    // Server의 RpcContext, RaidServer의 RaidGameContext가 각각 구현한다.
    public interface IGameContext
    {
        ulong AccountId { get; }
        ulong PlayerId { get; }
        int ShardId { get; }
        DateTime ServerTime { get; }
        string DeviceKey { get; }
        string Ip { get; }

        void SetAccountId(ulong accountId);
        void SetPlayerId(ulong playerId);
        void SetShardId(int shardId);
        void SetSessionKey(string sessionKey);
    }
}
