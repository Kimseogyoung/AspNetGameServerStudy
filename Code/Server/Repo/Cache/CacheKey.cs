namespace WebStudyServer.Repo.Cache
{
    // 캐시 키 값 객체. 문자열 키를 타입 안전하게 래핑한다.
    // 각 Component의 static Key 클래스에서 이 팩토리 메서드를 통해 키를 생성한다.
    public readonly struct CacheKey
    {
        public string Value { get; }

        private CacheKey(string value)
        {
            Value = value;
        }

        // 단일 ulong PK 모델용 (예: PlayerModel)
        public static CacheKey For<T>(ulong ownerId, ulong id)
        {
            return new CacheKey($"{typeof(T).Name}:{ownerId}:{id}");
        }

        // 복합 PK 모델용 (예: CookieModel — PlayerId + Num)
        public static CacheKey For<T>(ulong ownerId, ulong id1, int id2)
        {
            return new CacheKey($"{typeof(T).Name}:{ownerId}:{id1}:{id2}");
        }

        // PlayerId 기준 리스트 키
        public static CacheKey ListFor<T>(ulong ownerId)
        {
            return new CacheKey($"{typeof(T).Name}:{ownerId}");
        }

        // 임의 문자열 키 (커스텀 캐시 용도)
        public static CacheKey Raw(string key)
        {
            return new CacheKey(key);
        }
    }
}
