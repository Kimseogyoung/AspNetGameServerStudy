namespace WebStudyServer.Repo.Cache
{
    // 캐시 키 값 객체. 문자열 키를 타입 안전하게 래핑한다.
    // 각 Component의 static Key 클래스에서 이 팩토리 메서드를 통해 키를 생성한다.
    // 키 형식: TypeName:id1:id2:...
    public readonly struct CacheKey
    {
        public string Value { get; }

        private CacheKey(string value)
        {
            Value = value;
        }

        // 타입명 + ids 를 ':' 로 이어붙여 키 생성
        // 예) For<CookieModel>(playerId, num)  → "CookieModel:{playerId}:{num}"
        //     For<PlayerModel>(playerId)        → "PlayerModel:{playerId}"  (리스트 키 겸용)
        //     For<PlayerModel>("AccountId", id) → "PlayerModel:AccountId:{id}"
        public static CacheKey For<T>(params object[] ids)
        {
            return new CacheKey($"{typeof(T).Name}:{string.Join(":", ids)}");
        }
    }
}
