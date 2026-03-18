# 2026-03-19 개발 노트

> 💬 "드디어 조금 원하는 구조가 됨"

오랫동안 쌓여온 인프라 레이어의 구조적 문제들을 이번에 한꺼번에 정리했다. 핵심 목표는 두 가지였다: **Component 호출부에서 CacheKey와 dbFetch 람다를 완전히 제거**하는 것, 그리고 **Redis 캐시를 Hash 방식에서 String(JSON 배열) 방식으로 정정**하는 것.

물론 완벽한 상황아니고 차근차근 개선중이다.


## 1. Cache Layer — 컬렉션 단위 저장으로 전환

기존에는 캐시를 Redis hashSet으로 항목별 저장하려고했다. 이 방식은 호출부에 `itemKey`를 노출시키고, `keySelector` 람다를 곳곳에 전달해야 하는 복잡성을 만들었다. 

새 방식은 **`listKey → List<T>` 한 덩어리**를 저장한다. `itemKey`와 `keySelector`는 `ICacheSession` 어디에도 존재하지 않는다. 부분 업데이트(Set)는 `Func<T, bool> match` predicate로 대상 항목을 찾고, 무효화(Invalidate)는 단순히 `KeyDelete`로 컬렉션 전체를 날린다.
게임 최초 진입 시 List를 캐싱하고 이후 접근시 캐싱된 값들을 참조하고 redis기반으로 읽는 방식이다.

```
GetList  → null이면 DB 로드 후 BulkSet
Set      → StringGet → match 항목 교체(없으면 추가) → StringSet
Invalidate → KeyDelete  (다음 GetList에서 DB 재로드)
```

## 2. IRepository / UserComponentBase — predicate 기반 API

기존 `GetMdl(CacheKey, dbFetch)` 형태는 Component마다 동일한 DB 조회 람다를 반복 작성하게 했다. 이제 `UserComponentBase<T>`가 `LoadFromDb`를 내부적으로 처리하고, Component는 predicate만 넘긴다. 이것도 1번 변경사항이 있어서 가능했음.

```csharp
// 변경 전
GetMdl(Key.Single(playerId, num), db => db.SelectByPk<CookieModel>(...))

// 변경 후
GetMdl(x => x.Num == num)
```

`IRepository`는 `GetList`, `Insert`, `Update`, `Cache`, `Db` 다섯 가지만 남겼다. `Get(listKey, itemKey, ...)`, `GetListFiltered` 등 개별 조회 메서드는 모두 제거.

## 3. Auth/CenterComponentBase — DB Only 정리

Auth/Center Component는 당장은 캐시 안쓰고 Db로만 접근하도록 정리했다. 추후 캐시 활용하도록 정리가 필요하다. 특히 Session은 무조건 캐시 써야한다. 
아무튼 우선 억지로 `IRepository`를 거쳐 캐시 로직을 태우는 대신 **DB Only**로 명확히 분리했다. `IDbSession`을 직접 주입하던 것에서 `IRepository`를 저장하되 `_repository.Db.Execute(...)` 로만 접근한다.

## 4. InMemory 버그 수정

InMemory 모드에서 발생하던 버그를 수정했다.

- **Auto-increment 미부여**: `Insert` 시 `Id == 0`이면 `InMemoryStore`의 타입별 카운터에서 순번을 발급. `Id != 0`이면(예: `PlayerModel`의 `accountId * 10`) 그대로 유지.
- **`PlayerModel.LoadFromDb` 컬럼명 불일치**: 기본 구현이 `WHERE PlayerId = ...`를 생성하는데 Player 테이블 PK는 `Id`. `PlayerComponent`에서 `LoadFromDb` override로 `WHERE Id = ...` 사용.
현재 Sql에서 리플렉션으로 쿼리생성하고있는것도 무조건 개선해야한다.

---


| 커밋 | 메시지 |
|------|--------|
| `8041d16` | [Step7] IRepository/Cache 리팩토링 + InMemory 버그 수정 |
