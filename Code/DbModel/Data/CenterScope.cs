namespace WebStudyServer.Data
{
    // 센터 DB. 소유자 축이 없어서 경계가 아니라 DB 선택.
    //
    // 스코프 키가 없어 캐시 키도 자동 WHERE도 못 만듦. OwnedSet<T> 안 씀.
    // Schedule 이관 시 전역 로드 타입이 들어올 자리.
    public class CenterScope
    {
        internal CenterScope(GameDb db)
        {
            _db = db;
        }

        private readonly GameDb _db;
    }
}
