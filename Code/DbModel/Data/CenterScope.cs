namespace WebStudyServer.Data
{
    // 센터 DB. 소유자 축이 실제로 없으므로(Schedule 은 전체 조회) 이것은 경계가
    // 아니라 DB 선택이다 - 세 스코프 중 유일하게 그렇다.
    //
    // 그래서 OwnedSet<T> 를 쓰지 않는다. 스코프 키가 없어 캐시 키도 자동 WHERE 도
    // 만들 수 없다. 로드 단위가 "전역 전체"인 별도 타입이 S8 에서 들어온다.
    //
    // 지금 Schedule 은 요청마다 테이블 전체를 다시 읽는다. 여기에 GlobalList
    // 캐싱을 넣기로 했고(무효화 + TTL 한도 내 staleness 를 문서화하는 조건),
    // 그 정책 확정이 S8 이다.
    public class CenterScope
    {
        internal CenterScope(GameDb db)
        {
            _db = db;
        }


        private readonly GameDb _db;
    }
}
