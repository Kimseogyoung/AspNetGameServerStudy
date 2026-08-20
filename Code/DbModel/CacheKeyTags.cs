using WebStudyServer.Model;

namespace WebStudyServer
{
    // CacheKey.For(tag, ids)에 넘길 태그 모음. 모델 클래스명을 리플렉션으로 따오지 않고
    // 여기 문자열 상수로 명시해서, 모델 리네임이 캐시 키를 조용히 바꾸는 걸 막는다.
    // 태그를 바꾸면(=리네임하면) 기존 캐시가 깨진다는 걸 리뷰에서 바로 알아챌 수 있음.
    public static class CacheKeyTags
    {
        public const string PlayerModel = "PlayerModel";
        public const string PlayerDetailModel = "PlayerDetailModel";
        public const string CookieModel = "CookieModel";
        public const string KingdomMapModel = "KingdomMapModel";
        public const string KingdomStructureModel = "KingdomStructureModel";
        public const string KingdomDecoModel = "KingdomDecoModel";
        public const string ItemModel = "ItemModel";
        public const string PointModel = "PointModel";
        public const string TicketModel = "TicketModel";
        public const string WorldModel = "WorldModel";
        public const string WorldStageModel = "WorldStageModel";
        public const string SessionModel = "SessionModel";
        public const string RpcResponseCache = "RpcResponseCache";

        // OwnedSet<T>는 제네릭 하나뿐이라 태그를 손으로 적을 자리가 없다.
        // typeof(T).Name 을 쓰지 않는 이유는 그렇게 하면 "태그 == 클래스명"이
        // 구조적으로 고정되어, 캐시 키 규칙을 바꿀 수 없게 되기 때문이다.
        //
        // 손으로 유지하는 목록이므로 누락/드리프트가 위험하다. EntityMeta.VerifyCacheTags 가
        // 부팅 시 이 맵을 검사한다(엔트리의 타입에 [Entity] 가 없으면 부팅 실패).
        //
        // 이 맵에 있다 = OwnedSet<T> 로 다룰 수 있다. 없으면 OwnedSet<T> 생성 시
        // NOT_FOUND_CACHE_TAG 로 즉시 실패한다 - "캐시를 안 쓰는 채로 OwnedSet 을
        // 쓴다"는 경로는 없다(설계문서 §S2-J). ScopeKey 가 있으면서 여기 없는 것은
        // CashChangeLog/GachaLog 둘뿐이고, 감사 로그라 로드 단위 자체가 다르다.
        public static readonly IReadOnlyDictionary<Type, string> ByModelType = new Dictionary<Type, string>
        {
            [typeof(PlayerModel)] = PlayerModel,
            [typeof(PlayerDetailModel)] = PlayerDetailModel,
            [typeof(CookieModel)] = CookieModel,
            [typeof(KingdomMapModel)] = KingdomMapModel,
            [typeof(KingdomStructureModel)] = KingdomStructureModel,
            [typeof(KingdomDecoModel)] = KingdomDecoModel,
            [typeof(ItemModel)] = ItemModel,
            [typeof(PointModel)] = PointModel,
            [typeof(TicketModel)] = TicketModel,
            [typeof(WorldModel)] = WorldModel,
            [typeof(WorldStageModel)] = WorldStageModel,
            [typeof(SessionModel)] = SessionModel,
        };
    }
}
