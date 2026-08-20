namespace WebStudyServer.Data
{
    // 한 계정의 인증 데이터 경계. AuthScope(accountId) 가 존재한다는 것은
    // 확정됐다(설계문서 S1-G Q1) - Auth 의 데이터 모델은 Account.Id 를 루트로
    // 나머지 넷이 AccountId 를 갖는, User 와 같은 모양이다.
    //
    // 다만 경계를 긋는 방식이 User 와 다르다. User 는 [Entity].ScopeKey 로 자동
    // WHERE 를 걸지만 Auth 는 그러지 않는다(Q2 잠정 = 아니오). 근거는 census 다.
    //   - Session 을 빼면 Auth 에 캐시가 하나도 없다. 소유자 리스트 캐시가 놀게 된다
    //   - 소유자 컬렉션이 의미 있는 것은 Channel 하나뿐이다 (Device 는 AccountId 로
    //     조회하는 코드가 0개, Session/PlayerMap 은 AccountId 가 곧 PK)
    //   - ScopeKey 를 붙이면 기기 키/채널 키 조회에 WHERE AccountId 가 붙어 0행이 된다
    // 그래서 경계는 자동 WHERE 가 아니라 "인자 고정"으로 긋는다. accountId 를 여기
    // 묶어두고 조회 메서드가 그것을 넘기므로, 호출부가 다른 계정을 조회할 수 없다.
    //
    // AccountId 를 모르는 조회(기기 키/채널 키/세션 키/계정 생성)는 여기 들어올 수
    // 없다. GameDb.Identity 로 나간다 - "스코프를 여는 데 필요한 조회는 스코프 밖에
    // 둔다"는 A안의 기존 규칙 그대로다.
    //
    // S4 에서 채울 표면 (전부 즉시 쓰기):
    //   GetAccountAsync() / GetChannelListAsync() / GetOrCreateSessionAsync()
    //   GetPlayerMapAsync() / CreateDeviceAsync(idfv) / CreateChannelAsync(type)
    //   UpdateAsync<T>(entity)
    // OwnedSet 과 같이 쓰기는 즉시 반영이다. 지연 추적(dirty)은 넣지 않기로 했다(§S2-H).
    public class AuthScope
    {
        public ulong AccountId { get; }

        internal AuthScope(GameDb db, ulong accountId)
        {
            _db = db;
            AccountId = accountId;
        }


        private readonly GameDb _db;
    }
}
