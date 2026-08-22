namespace ServerCore.Model
{
    // 소유자 축이 있는 모델. OwnedSet<T>가 다룰 수 있는 대상을 타입으로 한정한다.
    //
    // 프로퍼티가 아니라 메서드인 이유: DapperExtension이 GetProperties로 INSERT/UPDATE
    // 컬럼 목록을 만들기 때문에, public 프로퍼티를 붙이면 없는 컬럼이 SQL에 들어간다.
    //
    // 소유자 컬럼의 SQL 이름은 [Entity].ScopeKey에 있다. 여기는 값 접근만 한다.
    public interface IScopedModel
    {
        ulong GetScopeKey();
        void SetScopeKey(ulong value);
    }
}
