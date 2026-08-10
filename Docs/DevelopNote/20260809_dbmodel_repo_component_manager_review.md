# 2026-08-09 DbLayer 리뷰 + Repo/Component/Manager 구조 고민

> XPDProject로 서버 인프라를 포팅하기 전, `Code/ServerCore/Repo/Database`(엔진 계층)와
> `Code/DbModel`(Repo/Component/Manager 도메인 계층)을 리뷰하고, 도메인 계층 구조 방향을
> 고민 중인 노트. `20260720_aspnet_server_core_review.md`의 "다음 논의 후보" ①(Async DB)을
> 이번에 처리했고, 이어서 도메인 계층(Component/Manager) 구조 자체를 다시 들여다봤다.

---

## 1. 이번에 처리한 것 (엔진 계층)

`20260720` 리뷰에서 남겨둔 이슈 ① "DB 계층 전체가 동기 I/O"를 처리했다. 커밋 순서:

| 커밋 | 내용 |
|---|---|
| `889d398` | EF Core 죽은 코드 제거 (`DbModel/Repo/Legacy/*`, `DbContextBase`, `FactoryBase`, 관련 패키지/설정) |
| `02a829f` | `IDbExecutor`/`IDbSession`/`DapperExtension`을 Dapper 비동기 API로 전환, DbModel 계층까지 async 전파. `TryGetX(out T)` 계열은 `Task<(bool Found, T? Value)>` 튜플 리턴으로 전환 |
| `8822df3` | `IDbExecutor` 인터페이스 메서드에 `Async` 접미사 통일, `DBSqlExcutor.cs`(오타) → `DBSqlExecutor.cs`, `Excute`/`Excute<T>`(동기/비동기 겸용 트램폴린) → `ExecuteAsync`/`ExecuteAsync<T>`(Task 전용, 타입으로 계약 강제) |

**의도적으로 범위를 좁힌 부분**: 커넥션 Open/BeginTransaction과 `IDbSession.Commit/Rollback/Close`는 여전히 동기다. `GlobalDbRepo`의 `Lazy<AuthRepo/CenterRepo/AllUserRepo>` 구조도 그대로. `20260720` 노트의 이슈 ②("커넥션이 리포 참조 시점에 열림")는 아직 열려있는 상태 — 아래 8장의 제안 구조와 맞물리는 지점이라 함께 검토 필요.

---

## 2. 도메인 계층 구조 고민 — 배경

지금 `DbModel`은 `GlobalDbRepo → AuthRepo/CenterRepo/UserRepo → XxxComponent(CRUD) → XxxManager(비즈니스 로직)` 3단 구조다. 이 구조를 만들 때, 이전에 런칭했던 프로젝트(`D:\PiedPixels\GSA\서버참고2`)에서 불편했던 부분을 개선하려고 했다는데, 막상 Component/Manager 구조로 가도 "묘하게 불편하고 보일러플레이트가 많다"는 느낌이 있어서 — 서버참고2와 지금 구조를 직접 코드로 비교했다.

## 3. 서버참고2(구 프로젝트) 구조 — 실제로 뭐가 불편했는지

> **확인됨**: 아래 3.1~3.4가 실제로 서버참고2에서 겪었던 불편함이 맞고, 지금 구조(Repo/Component/Manager +
> 리플렉션 기반 제네릭 CRUD)는 정확히 이 문제들을 고치려고 만든 것이었다고 본인이 확인함.

`GSAGameServer`는 EF Core 기반의 실제 런칭 프로젝트(AWS 스테이징/라이브 config, Migrations 존재). 구조는 `Controller → Service → Repo(EF DbContext)` 3단이었는데:

### 3.1 Repo가 "엔티티 축"이 아니라 "CRUD 동사 축"으로 쪼개진 God 클래스

`Repos/UserRepo.*.cs`가 `.Create.cs`/`.Get.cs`/`.Update.cs`/`.TryGet.cs`/`.Touch.cs`/`.Remove.cs`/`.Rank.cs`/`.Schedule.cs`로 partial 분할되어 있는데, **쪼갠 축이 동사**다. `UserRepo.Update.cs` 한 파일이 **2,425줄** — Player, Achievement, AdTicket, Item, Gear, Costume 등 수십 개 엔티티의 Update 메서드가 전부 이 한 파일에 섞여 있다.

### 3.2 모델 하나당 손으로 쓴 CRUD가 통째로 반복

```csharp
public Player UpdatePlayer(Player inPlayer)
{
    inPlayer.UpdateTime = DateTime.UtcNow;
    if (ConfigService.DbType == EDbType.INMEMORY)
    {
        var dbPlayer = DbCtx.Players.Find(inPlayer.Id);
        // 필드 20개를 하나하나 대입 ...
    }
    else
    {
        DbCtx.Players.Where(x => x.Id == inPlayer.Id).ExecuteUpdate(setters => setters
            .SetProperty(b => b.AccountId, inPlayer.AccountId)
            // 같은 필드 20개를 SetProperty로 또 반복 ...
        );
    }
    var cacheKey = CacheKey.Create(CacheKey.PlayerKey, inPlayer.Id);
    Cache.TrySetEntity(cacheKey, inPlayer);
    return inPlayer;
}
```

이 패턴이 `UpdateAchievement`, `UpdateAdTicket` 등 모델 수만큼 반복된다. **필드를 하나 추가하면 최소 2곳(InMemory 분기 + SQL 분기)을 손으로 맞춰야 하고, 잊어도 컴파일러가 못 잡는다** — 2,425줄의 정체.

### 3.3 Manager 개념이 없음 — Service가 raw Model+Proto를 계속 파라미터로 실어나름

`Services/User/GachaService.cs`(1,164줄):

```csharp
public GachaResultPacket GachaNormal(PlayerDetail repoPlayerDetail, Gacha repoGacha,
    int valGachaCnt, GachaScheduleProto prtGachaSchedule, ReqCostPacket valCost)
{
    var reason = MakeGachaReason(prtGachaSchedule, valGachaCnt);
    var costObj = _playerService.ValidDecCost(repoPlayerDetail, valCost, reason);
    var result = GachaInternal(repoPlayerDetail, repoGacha, valGachaCnt, prtGachaSchedule, null, costObj.ObjType, out _);
    ...
}
```

모델과 Proto를 계속 개별 파라미터로 넘기고 반환하는 절차적(transaction script) 패턴. `GachaInternal` 같은 private 헬퍼는 파라미터가 6~7개. `repo`/`prt`/`val`/`req` 접두사로 "이게 뭔지"를 변수명 컨벤션에만 의존해서 표시한다 — 타입 시스템이 아니라 팀 규율.

### 3.4 DB 타입 분기가 메서드마다 반복

모든 CRUD 메서드 내부에 `if (ConfigService.DbType == EDbType.INMEMORY) {...} else {...}`가 박혀 있다.

---

## 4. 지금 구조가 그 문제들을 해결한 방식

| 서버참고2 문제 | 지금 구조의 해법 |
|---|---|
| 필드별 손 CRUD 반복 (①②) | `DapperExtension.Init<T>()` 리플렉션 기반 제네릭 CRUD — 필드 매핑 코드 자체가 없음 |
| 동사 축 God 클래스 (①) | 엔티티 축 Component 파일 분리 (`AccountComponent`, `ChannelComponent`, ...) |
| raw Model+Proto 파라미터 실어나르기 (③) | Manager가 Model(+가끔 Proto)을 감싸 메서드를 객체에 붙임 (`mgrSession.ExpireAsync()`) |
| 메서드마다 DbType 분기 (④) | DI 시점에 `SqlRepository`/`InMemoryRepository` 스왑 (`GlobalDbRepo.CreateRepository`) |

방향 자체는 유효했다.

---

## 5. 지금 구조(DbModel)에 남아있는 불편함 — 최종 정리

여러 라운드에 걸쳐 코멘트/정정하며 확정한 목록.

### 5.1 Manager 관련

**5.1.1 Manager 유무가 극단적으로 불균형함.** `ChannelManager`/`DeviceManager`(14줄, 본문 없음) ~ `KingdomMapManager`(412줄)까지 스펙트럼이 넓은데, "이 엔티티는 Manager가 필요 없다"를 판단하는 기준이 코드에 없어서 일단 다 만들고 있음.

**5.1.2 (정정됨) "Manager=Model+Proto 결합"이 예외적이라는 진단은 틀림 — 진짜 문제는 1:1 가정.** Cookie 등 대부분의 데이터는 결국 Model:Proto가 1:1일 것(아직 안 붙었을 뿐). 진짜 문제는 **모든 DB 로드가 Manager를 거치게 되어 있는데, 이게 "Manager 1개 = Model 1개"라는 1:1 관계를 전제로 한다**는 것. 운영툴이나 리스트 로드처럼 N개를 벌크로 다루는 유스케이스와 이 전제가 충돌함 (`ScheduleComponent.GetListAsync()`가 스케줄 N개를 전부 `ScheduleManager`로 감싸는 게 예시 — 그냥 리스트만 필요해도 래핑 비용을 피할 방법이 없음).

**5.1.3 Manager가 캡슐화를 실제로 강제하지 않음.** `ManagerBase<T>.Model`이 public getter라 `mgr.Model.SomeField = x`로 Manager 메서드를 우회해서 직접 필드를 바꾸는 것도 구조적으로 막혀있지 않음.

**5.1.4 Manager가 자기 Component가 아니라 전체 Repo를 들고 있음 — 근본 원인 확인됨.** 비즈니스 로직이 다른 Model과 얽히는 순간(예: 여러 Model이 함께 바뀌어야 하는 연산) Manager가 자기 Component만으론 부족해서 전체 Repo(`_userRepo`/`_authRepo`)를 들고 있어야 하는 상황이 됨. **이건 5.4(비즈니스 로직 처리 주체 불명확)와 동일한 문제의 다른 얼굴** — 여러 Model이 한 번에 바뀌는 연산의 "처리 주체"가 불명확해서, 결국 넓은 접근 권한을 미리 확보해두는 쪽으로 흘러간 것.

### 5.2 Component 관련

**5.2.1 새 엔티티 추가 시 터치포인트가 여러 곳으로 흩어짐, 그중 하나는 컴파일러가 못 잡음.** 새 모델 하나 추가 시: ① `Component/XxxComponent.cs` ② `Manager/XxxManager.cs` ③ 부모 `Repo.PrepareComp()`에 `new XxxComponent(...)` 한 줄 ④ `StartUp.Resource.cs`에 `ModelRegistration.Init<T>(...)` 한 줄. 이 중 **④를 빠뜨려도 컴파일은 통과하고, 그 모델을 실제로 쓰는 요청이 들어올 때만 `NOT_FOUND_QUERY_PARAM` 런타임 예외로 터짐** — 개수가 많은 게 문제가 아니라 "등록"이 컴파일러 보호를 못 받는 수동 체크리스트라는 게 문제.

**5.2.2 엔진 계층은 리플렉션으로 완전히 제네릭화됐는데, Component 계층은 여전히 손으로 반복.** `DapperExtension`은 `Init<T>()` 한 번으로 모든 모델의 CRUD를 제네릭하게 처리하는데, Component는 TryGet/Get/Create/Update 4형식을 모델마다 손으로 다시 씀. (왜 이렇게 했는지는 기억 안 남 — 이유 없이 반복되는 보일러플레이트로 확정.)

**5.2.3 "표준 CRUD 밖" 쿼리가 필요할 때마다 캡슐화 경계가 뚫림.** `ScheduleComponent.GetListAsync`, `PlayerComponent.TryGetByAccountIdAsync`, `WorldStageComponent.GetTotalStarAsync`처럼 `DbSession`/`CacheLayer`에 직접 접근하는 탈출구가 Base 클래스에 열려있고, 드문 예외가 아니라 Component 상당수에서 반복됨.

### 5.3 Repo(AuthRepo/CenterRepo/UserRepo) 관련

**5.3.1 인프라 그루핑과 도메인 그루핑이 뒤섞여 있음.** 의도(각 Repo가 물리적으로 다른 DB/샤드에 속한 Component들을 묶음)는 타당한데, 이게 "도메인 그루핑"(Auth 관련 개념들)과 우연히 일치해서 어느 게 이 클래스의 진짜 존재 이유인지 코드에 드러나지 않음. 그리고 만약 Repo가 나중에 "자기 Component들 사이의 조율 로직"을 갖게 된다면, 5.4의 "처리 주체" 질문에 **Repo까지 4번째 후보로 끼어드는 셈**이라 애매함이 더 커짐.

**5.3.2 (보류) Auth 계열과 User 계열의 캐싱 정책이 다름.** `UserComponentBase<T>`는 Create/Update가 자동으로 캐시까지 갱신하는데 `AuthComponentBase`는 캐시를 전혀 안 건드림(DB Only). 이건 `2026-03-19` 노트에서 이미 "추후 정리 필요"로 인지된 **개발 중** 상태 — 지금은 문제 목록이 아니라 진행 중 트래킹 항목.

### 5.4 핵심 미해결 질문 — 비즈니스 로직의 처리 주체

**여러 Model에 걸친 비즈니스 로직(원자성이 필요한 연산)을 Component/Manager/Service/Repo 중 어디서 처리할지가 불명확함.** 하나의 함수로 뭉치면 그 함수가 어느 Model 소유인지 애매해지고(→ 5.1.4), 여러 함수로 쪼개면 조각 하나만 봐선 전체 그림이 안 보임. Manager에 로직을 몰면 Service에서 "무슨 일이 일어나는지" 한눈에 안 보이는 문제도 있음. 이건 지금까지 나온 것 중 **가장 근본적인 미해결 질문**.

### 5.5 여러 프로세스에서 공유할 때 (RaidServer 사례)

**5.5.1 Get/Create/Update가 전부 "현재 인증된 나"(`RpcCtx.PlayerId`)에 암묵적으로 묶여 있음.** `UserComponentBase<T>.LoadFromDb`가 `WHERE PlayerId = {RpcCtx.PlayerId}`를 하드코딩하고, Insert/Update의 캐시 키도 `ListKeyFor(RpcCtx.PlayerId)`로 현재 컨텍스트를 씀. **임의의 대상 플레이어를 파라미터로 지정해서 조회/수정하는 경로가 없음.** 운영툴처럼 "요청 파라미터로 대상을 지정"해야 하는 유스케이스와 정반대. 게다가 넣는 엔티티의 실제 PlayerId 필드와 캐시 키 계산에 쓰이는 `RpcCtx.PlayerId`가 서로 다를 수 있는데 검증하지 않음 — 어긋나면 DB엔 정상 저장되고 캐시만 엉뚱한 버킷에 쓰이는 정합성 버그 가능성.

**5.5.2 `IGameContext`가 앰비언트 의존성으로 Component/Manager 안쪽 깊숙이 박혀 있음.** 이건 5.5.1과 **같은 원인**. `IGameContext`(6개 프로퍼티+4개 setter)는 메인 `Server`가 필요한 걸 기준으로 만들어져 있어서, 다른 소비자(`RaidServer`)는 일부를 흉내만 냄 — `RaidGameContext.SetSessionKey`는 빈 구현, `Ip`는 항상 `""`. `Ip`는 `SessionComponent`/`SessionManager`가 실제로 `Model.PublicIp`에 저장하는 값이라, RaidServer가 세션 생성 경로를 타면 **조용히 빈 IP가 저장되는 실제 버그**가 될 수 있음(아직 안 터졌을 뿐).

**5.5.3 네임스페이스가 "Server 소유"로 선언되어 있음 (실수/정리 대상, 구조적 문제는 아님).** `GlobalDbRepo`는 물리적으로 `DbModel` 프로젝트에 있는데 `namespace Server.Repo`, `AuthRepo`/`UserRepo`/Component/Manager는 `namespace WebStudyServer.*`. `RaidServer`는 `using Server.Repo;`로 이걸 가져다 씀 — DbModel이 원래 Server의 일부였다가 분리되며 네임스페이스만 안 바뀐 흔적으로 추정. 정리하면 끝나는 문제.

**5.5.4 모델 등록이 프로세스마다 중복 선언됨.** `RaidServer/StartUp/StartUp.Resource.cs`가 `Server`와 동일한 `ModelRegistration.Init<SessionModel>("AccountId")` 등을 그대로 복붙. PK 정의는 모델 자체에 속하는 지식인데 소비 프로세스마다 재선언 — 한쪽이 바뀌면 다른 쪽은 아무 신호 없이 조용히 어긋남.

### 5.6 엔진 계층 기능 격차 (운영툴 관점에서 드러남)

- **Delete가 아예 없음.** `IDbExecutor`/`DapperExtension` 어디에도 삭제 연산이 없음.
- **부분 필드 업데이트 경로가 없음.** `Update`는 항상 PK 제외 전체 필드를 덮어씀. 필드 하나만 바꾸고 싶어도 전체 엔티티를 로드해서 통째로 다시 써야 함.

### 5.7 무시하기로 한 것

**mdl/mgr 접두사 컨벤션 의존 (서버참고2의 `repo`/`prt`/`val` 접두사 의존과 본질적으로 같은 종류지만, 개수가 2개뿐이라) — 그냥 프로젝트 성향으로 두기로 함.**

---

## 6. GSA에서 기억해둘 패턴 (버리지 말고 가져올 것)

서버참고2의 `Controllers/Ops/`에는 엔티티당 하나씩(`OpsPlayerItemController`, `OpsPlayerGearController` 등) **수십 개의 운영툴 컨트롤러**가 있었고, 전부 아래 패턴을 공유했다:

```csharp
protected UserRepo InitUserRepo(ulong playerId) // 밖에서 꼭 using 사용
{
    PlayerMapService.TryGetUserRepoByPlayerId(playerId, out var userRepo, out _);
    return userRepo;
}
```

`PlayerMapService.TryGetUserRepoByPlayerId(playerId, ...)`가 **임의의 playerId를 받아 그 사람의 샤드를 찾고, 그 샤드에 연결된 `UserRepo`를 새로 만들어 반환**하는 팩토리. "현재 인증된 나"에 묶인 기본 경로와 별개로, "이 playerId 대상으로 임시 Repo를 열어줘"가 **1급으로 지원되는 정식 오퍼레이션**이었음. `using var userRepo = InitUserRepo(playerId);`로 요청마다 대상을 바꿔가며 쓰고 버림.

부분 필드 수정도 `JsonPutEntity<T>` + `.ApplyTo(existingEntity)`(이미 로드한 엔티티에 요청 JSON에 있는 필드만 머지)로 풀었음 — 저장 자체는 여전히 "row 전체 덮어쓰기"지만 API 레벨에선 "보낸 필드만 바뀐다"가 보장됨.

**지금 프로젝트엔 이 두 메커니즘이 없음** — 5.5.1/5.6과 직결.

---

## 7. 논의 중 확인된 두 가지 (설계 입력)

### 7.1 로깅 — 현재 구조도 못 풀고 있는 문제였음

"비즈니스 로직을 Model에 직접 붙이면 로깅이 애매하다"는 지적에서 출발해 확인한 결과:

- **`DbModel` 전체에 비즈니스 로깅이 하나도 없다.** `_logger`는 `GlobalDbRepo`의 커밋/롤백 에러용뿐(`GlobalDbRepo.cs:78,90,105,128`).
- **`CashChangeLogModel` / `GachaLogModel`은 `StartUp.Resource.cs:60-61`에 등록만 되어 있고 어디서도 쓰이지 않는다.** 감사 로그가 미구현 상태.
- 반면 `PlayerDetailManager.IncRewardAsync`는 이미 `ChgObjPacket`(Type/Num/Amount/TotalAmount)을 반환하고, 모든 재화 메서드가 `reason` 문자열을 인자로 받고 있다 — **감사 기록의 뼈대는 절반쯤 이미 존재**한다.

→ 결론: 도메인 계층은 로깅하지 않고 **"뭐가 변했는지"를 반환**하고, 그 값 하나로 ①응답 패킷 ②구조화 로그 ③`CashChangeLog` 행 세 가지를 만든다. 컨텍스트(AccountId/PlayerId/TraceId)는 Transport가 로거 스코프에 한 번 바인딩하므로 도메인까지 내려갈 필요가 없다. A안·B안 모두 이 원칙을 채택.

### 7.2 여러 Model 복합 처리 — 실물 사례 확인

`PlayerDetailManager`가 `_userRepo.Point`(267,274) / `.Ticket`(283,290) / `.Cookie`(299,306) / `.Item`(315,322)을 전부 찌르고 있다. 이름은 "PlayerDetail의 Manager"인데 실체는 **재화 원장(ledger) 오케스트레이터**다. 5.1.4가 왜 생겼는지에 대한 물증이자, 5.4의 대표 사례.

→ 결론: 이런 것은 Manager가 아니라 **이름 있는 도메인 서비스**(`ObjectLedger`)로 승격하고, Manager/Model은 자기 자신 밖으로 못 나가게 타입으로 제한한다. A안·B안 모두 이 원칙을 채택.

---

## 8. 방향 제안 — 별도 문서 2개로 분리

| 안 | 문서 | 요지 |
|---|---|---|
| **A안 (신규 구조)** | `Docs/Design/DbLayer_A_NewStructure.md` | Repo/Component/Manager를 폐기하고 `GameDb(UoW) → DataScope → DataSet<T>` + 순수 도메인(Model partial) + 도메인 서비스 + App Service로 재설계. 엔진 계층과 코드젠 모델만 재사용 |
| **B안 (기존 구조 개선)** | `Docs/Design/DbLayer_B_Incremental.md` | Repo/Component/Manager 이름·계층을 유지하고 세 가지 결합(↔`IGameContext`, Manager↔전체Repo, 엔티티↔손으로 쓴 Component)만 끊음. 호출부 대부분이 그대로 살아 슬라이스 단위 진행 가능 |

각 문서에 **현재 구조 불편함(5장)·GSA 불편점(3장)이 항목별로 어떻게 해소되는지** 표로 정리되어 있다.

---

## 9. 결론 (2026-08-11 갱신)

### 9.1 **A안 채택.** B안은 폐기하지 않고 "검토했으나 미채택"으로 남긴다.

근거 요약 (상세는 `DbLayer_A_NewStructure.md` §0):
- **B안의 종착지가 이미 A안이다.** B의 1.2+1.3+1.4+1.7을 다 적용하면 제네릭 CRUD + 얇은 로직 래퍼 + 변경 반환 = A안의 `DataSet<T>` + Model partial + `ChangeSet`이며 **이름만 다르다.** 같은 곳에 도착할 거면 두 번 갈 이유가 없다.
- **B는 5.4(가장 근본적 질문)·5.1.3·5.2.3을 △로 남긴다.** 이 리뷰의 출발점이 5.4였다.
- **규모가 감당 가능하다.** 실측 Component 997줄 + Manager 1,691줄 + 호출부 978줄 ≈ 3,700줄이고, `ServerTest` 1,218줄이 HTTP 레벨이라 데이터 계층 교체에 걸리지 않는다.
- **"B의 앞부분은 A/B 공통"이라는 초기 판단은 틀렸다.** B 1.8(네임스페이스)은 A로 가면 전량 낭비, 1.1(`IGameContext` 제거, 최대 슬라이스)은 `UserRepo` 자체가 사라지므로 대부분 낭비다.

### 9.2 확정된 설계 결정

| | 결정 | 위치 |
|---|---|---|
| 저장 모델 | **dirty 플래그 + 커밋 시 flush** (`MarkDirty()`). EF식 스냅샷 추적 미도입 | A안 §3.8 |
| 커밋 경계 | **유저 락 안으로 이동** — dirty 모델은 그대로 두면 lost update. A안 착수 전 **선행 커밋** | A안 §3.8, StepByStep §5.1 |
| 변경 반환 타입 | **`ChangeSet` 존치** — 근거는 "세 싱크 단일 출처"(철회)가 아니라 **와이어 계약 분리**. `Reason`/`Acc*` 제외 | A안 §3.5 |
| 감사 로그 | 싱크별 **개별 조립**. `CashChangeLogModel`은 **유료 재화 전용**이며 비-Cash DB 원장은 **비목표(의도된 설계)** | A안 §3.5, §6, S0-3 |
| 표준 CRUD 밖 조회 | **4티어 분류**(T0 메타데이터 / T1 확장메서드·새 SQL 금지 / T2 보조인덱스 / T3 `scope.Raw`). 락 등은 flush 안 하는 `GameDb.Utility` | A안 §3.9 |
| 캐시 정책 | `[Entity(Cache=…, SlidingTtl=…)]` **5종 열거**. Session의 기존 포인터 캐시는 **유지** | A안 §3.9 |
| Center 캐시 | **`GlobalList` 캐싱 도입** (현재 매 요청 전량 조회) | A안 S8 |
| 도메인 서비스 이름 | `ObjectLedger` → **`RewardHelper`** (실체가 ObjKey 라우터) | A안 §3.6 |

### 9.3 이슈 ②(커넥션 오픈 시점) — **A안 S2에 포함하기로 확정**

`GameDb.User(shardId, playerId)`는 스코프만 만들고 `DataSet<T>` **첫 조회 시점**에 커넥션을 연다. `RepoBase` 생성자가 `PrepareComp()`를 부르며 즉시 여는 현재 구조를 여기서 해소한다.

### 9.4 남은 미확인 — 하나뿐

- **코드젠(`ClassGenerator`)이 PK/Owner/Cache를 attribute로 찍어낼 수 있는지.** 불가하면 모델 20개 수작업이며 **작업량에만 영향**을 준다. 실행을 막지 않는다.

### 9.5 실행 순서

```
1. S0-2 확인      ClassGenerator attribute 생성 가능 여부
2. S0-4 선행 커밋  커밋 경계를 유저 락 안으로 (A안과 독립적으로 이득)
3. S1~S13         DbLayer_A_StepByStep.md §2 참조
```

상세 계획은 `Docs/Design/DbLayer_A_NewStructure.md` §7, 스텝별 실코드 before/after와 자체 리뷰 12건은 `Docs/Design/DbLayer_A_StepByStep.md`.

---

<!-- 이하는 A/B 분리 이전의 1차 초안. 위 8장의 두 문서로 대체되었으며 기록용으로만 남김. -->

## 부록. (구) 1차 초안 — A/B 문서로 대체됨

### 8.1 두 가지 핵심 원칙

**원칙 A — DbModel은 "누가 요청했는가"를 몰라야 한다.**
지금 문제의 상당수(5.1.4, 5.4, 5.5.1, 5.5.2)가 결국 하나의 뿌리에서 나온다: `IGameContext`라는 **요청당 하나뿐인 가변 앰비언트 객체**를 Component/Manager 내부 깊숙이에서 직접 읽는다는 것. 이걸 뒤집는다 — DbModel의 어떤 타입도 `IGameContext`/`RpcContext`를 참조하지 않는다. 대신 "어느 샤드의, 누구 소유 데이터인가"를 **평범한 스칼라 파라미터(`shardId`, `ownerId`)** 로 Repo 생성 시점에 명시적으로 받는다. "현재 로그인한 나"라는 개념은 그 스칼라를 어디서 구해오는지의 문제일 뿐이고, 그건 호출부(RPC 파이프라인, 운영툴 컨트롤러, RaidServer, 배치 잡)의 책임이다.

**원칙 B — 여러 Model에 걸친 로직은 항상 Service. Manager는 절대 자기 자신 밖으로 못 나간다.**
Manager(또는 뭐라 부르든)는 생성자에서 전체 Repo가 아니라 **자기 자신의 Component 하나만** 받는다. 구조적으로 다른 Component에 손을 댈 수 없게 만든다. "이 연산이 Model 하나로 끝나는가?"가 곧 "Manager 메서드인가 Service 메서드인가"의 답이 된다 — 판단 기준이 컨벤션이 아니라 **타입이 허용하는가**로 바뀐다. (완벽한 강제는 아니다 — Service가 여러 Manager를 순서대로 호출하는 것 자체는 여전히 사람이 설계해야 함. 다만 "Manager가 다른 Model을 몰래 건드리는" 경로 자체를 없애는 것.)

### 8.2 계층 구조

```
GlobalDbRepo (스칼라만 받음: shardId, ownerId — IGameContext 의존 없음)
  └── BeginUserRepo(int shardId, ulong playerId) → UserRepo   ← "본편"이든 "운영툴"이든 같은 메서드
        └── UserRepo(shardId, playerId, IRepository)
              └── Of<T>() → Component<T>          ← 등록만 되어 있으면 별도 클래스 없이 바로 사용
                    └── (선택) XxxComponent : Component<XxxModel>   ← 로직 있을 때만 서브클래스
```

`AuthRepo`/`CenterRepo`도 같은 모양(`ownerId` 대신 `accountId` 또는 owner 없음). **이 세 Repo는 순수 인프라 그루핑(어느 DB에 연결할지)이라고 명시적으로 규정** — 5.3.1의 애매함을 "이건 그냥 커넥션 그루핑이다"로 못박아서 해소.

### 8.3 핵심 타입 스케치 (설계 의도 전달용 — 실제 시그니처는 구현 시 조정)

```csharp
// Component<T>: 등록된 모델이면 서브클래스 없이 바로 이걸로 CRUD 끝. B1/B2 해결.
public class Component<T> where T : ModelBase, new()
{
    protected readonly int _shardId;
    protected readonly ulong _ownerId;      // 이 Repo가 대표하는 소유자(예: PlayerId)
    protected readonly IRepository _repository;

    public Component(int shardId, ulong ownerId, IRepository repository) { ... }

    public virtual Task<List<T>> GetListAsync(object conditions = null) => ...;
    public virtual Task<(bool Found, T? Value)> TryGetAsync(object pk) => ...;
    public virtual Task<T> CreateAsync(T entity) => ...;
    public virtual Task UpdateAsync(T entity) => ...;
    public virtual Task DeleteAsync(T entity) => ...;   // 5.6 — 엔진 계층에 Delete 추가
}

// 로직이 필요한 엔티티만 서브클래스 (Component+Manager 병합 — 5.1.1/5.1.2/7-①②)
public class ItemComponent : Component<ItemModel>
{
    public ItemComponent(int shardId, ulong ownerId, IRepository repository) : base(shardId, ownerId, repository) { }

    // 자기 자신(ItemModel)만 만짐 — 다른 Component에 접근할 방법 자체가 없음 (원칙 B)
    public async Task<double> DecAmountAsync(ItemModel item, double amount, string reason)
    {
        ReqHelper.ValidEnough(amount, item.Amount, $"ITEM_{item.Num}", reason);
        item.Amount -= amount;
        item.AccAmount -= amount;
        await UpdateAsync(item);
        return item.Amount;
    }
}
```

```csharp
// GlobalDbRepo — IGameContext 의존 제거. 스칼라만 받음.
public class GlobalDbRepo
{
    // "본편" 경로든 "운영툴" 경로든 완전히 같은 메서드 — GSA의 InitUserRepo(playerId)가 여기선 특별 취급이 아니라 기본값
    public UserRepo BeginUserRepo(int shardId, ulong playerId)
    {
        var repository = CreateRepository(GetUserDbConnectionStr(shardId));
        return new UserRepo(shardId, playerId, repository);
    }
}

// Server 프로젝트(DI 등록 시점) — RpcContext → 스칼라 변환은 여기서만 일어남
services.AddScoped(sp =>
{
    var ctx = sp.GetRequiredService<RpcContext>();
    return new GlobalDbRepo(sp.GetRequiredService<DbSessionManager>(), ...);
});
// RpcMethod 등에서: _dbRepo.BeginUserRepo(RpcContext.ShardId, RpcContext.PlayerId)
```

### 8.4 각 문제가 어떻게 해소되는지

| 불편함 | 해소 방식 |
|---|---|
| 5.1.1 Manager 유무 불균형 | Component/Manager 병합. 로직 없으면 서브클래스 자체가 없음 |
| 5.1.2 1:1 가정 vs 리스트/운영툴 | `GetListAsync()`가 기본적으로 `List<T>`(Model) 반환 — Manager 래핑은 선택 |
| 5.1.4 / 5.4 처리 주체 불명확 | 원칙 B로 Manager의 접근 범위를 타입으로 제한. 여러 Model 얽히면 Service행이 기계적으로 결정됨 |
| 5.2.1 등록 누락이 런타임에만 터짐 | PK 메타데이터를 모델에 attribute로 붙이고 어셈블리 스캔으로 자동 등록(`RegisterAll()`) — 사람이 빼먹을 수 있는 단계 자체를 제거 |
| 5.2.2 손으로 반복되는 CRUD | `Component<T>`가 기본 제공, 서브클래스는 필요한 것만 override |
| 5.3.1 인프라/도메인 그루핑 혼재 | Repo 3종은 "순수 인프라 그루핑"이라고 명시적으로 규정 |
| 5.5.1 / 5.5.2 앰비언트 컨텍스트 | DbModel이 `IGameContext`를 아예 모름 — `RaidGameContext`가 인터페이스를 구현할 필요 자체가 없어짐 |
| 5.5.4 모델 등록 중복 | attribute + 어셈블리 스캔이라 등록 자체가 프로세스마다 반복될 이유가 없음 |
| 5.6 Delete 없음 | `Component<T>.DeleteAsync` 추가 |
| 6장 운영툴 패턴 | `BeginUserRepo(shardId, playerId)`가 기본 진입점이라, GSA의 `InitUserRepo`가 "특수 케이스"가 아니라 "유일한 경로" |

### 8.5 트레이드오프 — 솔직히 다 풀리진 않음

- **원칙 B는 컴파일러가 아니라 타입 설계로 유도하는 것.** Service가 여러 Manager를 순서대로 잘못 호출하거나, 트랜잭션 원자성이 필요한 연산을 실수로 쪼개는 것 자체를 막지는 못함. 완벽한 강제(예: 도메인 이벤트, Unit of Work 패턴)는 이 프로젝트 규모에 과할 가능성이 커서 의도적으로 안 함.
- **5.1.3(Model 캡슐화 부재)은 그대로 둠.** `Component<T>.UpdateAsync(T entity)`도 여전히 필드를 밖에서 직접 바꾼 걸 그대로 저장할 수 있음. 완전한 캡슐화(private setter + 도메인 메서드로만 변경)는 게임 서버의 Model 대부분이 사실상 DTO라는 점을 감안하면 얻는 것보다 의식(ceremony) 비용이 커 보여서 비목표로 둠.
- **부분 필드 업데이트(JsonPutEntity 대응)는 엔진 계층 문제가 아니라 API 경계 문제라 이번 제안 범위 밖.** 실제 운영툴을 만들 때 GSA 패턴을 그대로 가져오면 됨 — `Component<T>.TryGetAsync`로 로드 → JSON 머지 → `UpdateAsync`.
- **PK attribute + 어셈블리 스캔 자동 등록은 `ClassGenerator`가 PK 필드 정보를 attribute로 같이 찍어내야 완성됨** — 지금 생성기가 그 정보를 갖고 있는지 확인 필요.

<!-- (구) 초안 끝 -->
