using ServerCore;

namespace WebStudyServer.Data
{
    // 샤드 -> 커넥션 문자열. 샤드 맵이 두 벌이 되지 않게 데이터 계층은 전부 여기를 봄.
    public static class DbConnectionResolver
    {
        // InMemory 모드에서 모든 Repo가 단일 세션을 공유하도록 동일한 키 사용
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

        // 소유자를 모르는 조회는 샤드를 특정할 수 없어 전부 훑는다. InMemory는 단일 키다.
        public static IReadOnlyList<string> AllUsers()
        {
            var connList = Config<CoreConfig>.Get().UserDbConnectionStrList;
            return connList.Count > 0 ? connList : [InMemoryConnectionKey];
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
