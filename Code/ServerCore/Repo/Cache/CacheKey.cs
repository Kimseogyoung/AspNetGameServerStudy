namespace ServerCore.Repo.Cache
{
    // 캐시 키 값 객체. 문자열 키를 타입 안전하게 래핑한다.
    // 각 Component의 static Key 클래스에서 이 팩토리 메서드를 통해 키를 생성한다.
    // 키 형식: TypeName:id1:id2:...
    public readonly struct CacheKey
    {
        public string Value { get; }

        public CacheKey(string value)
        {
            Value = value;
        }

        // 태그 + ids 를 ':' 로 이어붙여 키 생성.
        // 태그는 타입명을 리플렉션으로 따오지 않고 호출부에서 명시적으로 넘긴다
        // (타입 리네임 시 캐시 키가 조용히 바뀌는 걸 막기 위함 - CacheKeyTags 참고).
        // 예) For(CacheKeyTags.CookieModel, playerId, num) → "CookieModel:{playerId}:{num}"
        //     For(CacheKeyTags.PlayerModel, playerId)      → "PlayerModel:{playerId}"  (리스트 키 겸용)
        //     For(CacheKeyTags.SessionModel, "AccountId", id) → "SessionModel:AccountId:{id}"
        public static CacheKey For(string tag, params object[] ids)
        {
            return new CacheKey($"{tag}:{string.Join(":", ids)}");
        }
    }
}
