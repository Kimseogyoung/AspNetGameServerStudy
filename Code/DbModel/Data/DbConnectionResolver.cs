using ServerCore;

namespace WebStudyServer.Data
{
    // 샤드 -> 커넥션 문자열 결정. GlobalDbRepo 가 private 으로 갖고 있던 것을
    // 꺼낸 것이다. GameDb 가 같은 판단을 해야 하는데 샤드 맵을 두 벌 두면
    // 조용히 갈라질 수 있어서, 이관 기간 동안 양쪽이 이 하나를 본다.
    public static class DbConnectionResolver
    {
        // InMemory 모드에서 모든 Repo 가 단일 세션을 공유하도록 동일한 키를 쓴다.
        public const string InMemoryConnectionKey = "__inmemory__";

        public const int MaxShardCount = 64;

        public static string User(int shardId)
        {
            var connList = Config<CoreConfig>.Get().UserDbConnectionStrList;
            if (connList.Count == 0)
            {
                return InMemoryConnectionKey;
            }

            if (MaxShardCount <= shardId)
            {
                throw new ArgumentOutOfRangeException(nameof(shardId),
                    $"ShardId({shardId})가 최대값({MaxShardCount})을 초과합니다.");
            }

            var shardIdx = _shardMap[shardId];
            if (shardIdx >= connList.Count)
            {
                shardIdx %= connList.Count;
            }

            return connList[shardIdx];
        }

        public static string Auth()
        {
            var connList = Config<CoreConfig>.Get().AuthDbConnectionStrList;
            return connList.Count > 0 ? connList[0] : InMemoryConnectionKey;
        }

        public static string Center()
        {
            var connList = Config<CoreConfig>.Get().CenterDbConnectionStrList;
            return connList.Count > 0 ? connList[0] : InMemoryConnectionKey;
        }

        private static readonly int[] _shardMap =
        [   0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 3, 4, 0, 1, 2, 3, 4,
            0, 1, 2, 4 ];
    }
}
