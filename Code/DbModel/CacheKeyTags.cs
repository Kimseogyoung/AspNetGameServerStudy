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

        // OwnedSet<T>가 쓸 태그 맵. typeof(T).Name을 쓰면 "태그 == 클래스명"이 고정되어
        // 캐시 키 규칙을 못 바꾸므로 손으로 유지. 부팅 시 EntityMeta.VerifyCacheTags가 검사.
        //
        // 여기 있으면 OwnedSet<T>로 다룰 수 있음. 없으면 생성 시 NOT_FOUND_CACHE_TAG.
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
