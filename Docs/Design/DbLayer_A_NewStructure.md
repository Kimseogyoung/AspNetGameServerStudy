# DbLayer 재설계 — A안: 신규 구조 (Unit of Work + 순수 도메인 + 애플리케이션 서비스)

작성일: 2026-08-09
관련 노트: `Docs/DevelopNote/20260809_dbmodel_repo_component_manager_review.md` (문제 정의)
대응 문서: `DbLayer_B_Incremental.md` (기존 구조 개선안)

> 기존 `Repo/Component/Manager` 구조를 유지하지 않고, 백지에서 다시 설계한 안.
> 지금 코드에서 가져오는 것은 **엔진 계층(`ServerCore/Repo/Database`, `Repo/Cache`)뿐**이고,
> `DbModel`의 Repo/Component/Manager 3단은 전부 대체한다.

---

## 0. 설계 근거 — 세대별 판단 흐름

> 이 장은 "무엇을 만드는가"가 아니라 **"왜 그 형태여야 하는가"**를 기록한다.
> A안의 각 요소가 어느 세대의 어떤 경험에서 도출됐고, 이전 세대에는 왜 없었는지를 추적한다.
> 다음 세대가 이 문서를 읽을 때 **결론이 아니라 판단 흐름을 물려받게 하는 것**이 목적이다.

### 0.1 세 세대

| 세대 | 구조 | 정의된 문제 |
|---|---|---|
| **Gen1 — GSA(서버참고2)** | Controller → Service → Repo(EF Core) | 노트 3.1~3.4 |
| **Gen2 — GameAsp(현재)** | GlobalDbRepo → Auth/Center/UserRepo → Component → Manager | 노트 5.1~5.6 |
| **Gen3 — A안** | GameDb(UoW) → Scope → `DataSet<T>` / 순수 Model / 도메인 서비스 / App Service | 이 문서 |

### 0.2 Gen2의 해법 4개가 각각 무엇을 가정했는가

Gen1의 불편 4개에 Gen2가 해법 4개를 냈다. 그런데 **각 해법이 새 전제를 도입했고, 그 전제가 정확히 5장 문제 목록이 됐다.**

| Gen1 문제 | Gen2 해법 | 이 해법이 깐 **새 전제** | 그 전제가 낳은 문제 |
|---|---|---|---|
| 3.2 손 CRUD 반복 | 리플렉션 제네릭 CRUD | *모델 메타데이터(PK)가 코드 밖에 산다* | **5.2.1** 등록이 수동 체크리스트 |
| 3.1 동사축 God 클래스 | 엔티티축 Component | *엔티티마다 클래스가 하나 필요하다* | **5.2.2** 4형식 반복 / **5.2.3** 탈출구 |
| 3.3 raw 파라미터 실어나름 | Manager로 Model 감싸기 | *Model 1개 = Manager 1개* | **5.1.1** 유무 불균형 / **5.1.2** 래핑 비용 / **5.1.4** 전체 Repo 보유 |
| 3.4 DbType 분기 반복 | DI 시점 Repository 스왑 | **없음** | **없음** |

**4번만 깨끗하고, 5장 전체에 4번 관련 불만이 한 줄도 없다.**

**이 규칙(R2)을 A안 자신에게 적용하면** — 설계 단계에서 이미 하나가 발견됐다:

| A안 해법 | 이 해법이 까는 **새 전제** | 그 전제가 깨지는 곳 | 대응 |
|---|---|---|---|
| `DataSet<T>` + ScopeKey 자동 스코핑 | *모든 조회는 스코프 컬렉션 안에서 끝난다* | 비-ScopeKey 컬럼 조회, 집계 쿼리, ScopeKey 없는 엔티티 | **3.9** 4티어 분류 |

Gen3가 다음 세대에 넘길 뻔한 문제를 R2로 미리 잡은 사례다. 이후에도 A안의 각 요소에 대해 같은 질문을 반복한다.

이유: 4번만 **새 클래스 종류를 도입하지 않았다.** 이미 존재하는 경계(DI 컨테이너)에 분기를 옮겨 얹었을 뿐이다. 1~3은 각각 새로운 "종류"(Component, Manager)를 만들었다.

### 0.3 새 클래스 종류가 동반하는 세 질문

새 종류를 만들면 반드시 세 질문이 따라온다 — **(a) 언제 만드는가 (b) 무엇을 아는가 (c) 몇 개인가.**
Gen2가 이 셋에 **코드로** 답을 주지 않은 것이 5장이다.

| Gen2 | (a) 언제 | (b) 무엇을 아는가 | (c) 몇 개 |
|---|---|---|---|
| `Component` | 엔티티마다 항상 | `UserRepo` 전체 + `IDbSession` + `ICacheSession` | 엔티티 수 |
| `Manager` | **답 없음** → 5.1.1 | `UserRepo` 전체 → 5.1.4 | Model 1개당 1개 → 5.1.2 |

(b)의 증거는 `UserComponentBase` 하단에 상시 개방된 탈출구다:

```csharp
protected IDbSession DbSession => _repo.Db;        // Base에 항상 열려 있음
protected ICacheSession CacheLayer => _repo.Cache;
```

**A안은 이 세 질문에 답하는 형태로 설계됐다:**

| A안 | (a) 언제 | (b) 무엇을 아는가 | (c) 몇 개 |
|---|---|---|---|
| `DataSet<T>` | 자동 (등록만 되면) | 스코프의 `shardId`/`ownerId` + 엔티티 메타데이터만 | **0개** (제네릭 1개) |
| Model partial | 로직이 있을 때만 | 자기 필드만 (DB 참조 없음) | 최대 1개 |
| 도메인 서비스 | 여러 Model 불변식이 있을 때만 | 주입받은 Model들만 | 개념 수만큼 |

### 0.4 Gen2가 Gen1보다 후퇴한 두 지점

**(1) `InitUserRepo(playerId)` → `BeginOwnUserRepo()`**

```csharp
// Gen1 — 대상이 인자
protected UserRepo InitUserRepo(ulong playerId)

// Gen2 — GlobalDbRepo.cs:32. 인자가 없다
public void BeginOwnUserRepo()
{
    var connStr  = GetUserDbConnectionStr(_rpcContext.ShardId);   // 앰비언트
    var userRepo = new UserRepo(_rpcContext, repository);         // 앰비언트
}
```

Gen1에는 운영툴 컨트롤러 수십 개가 있어서 "임의 대상"이 강제됐다. Gen2는 소비자가 RPC 본편 하나뿐이다. **소비자가 하나면 앰비언트 컨텍스트는 편할 뿐 아니라 더 안전해 보인다** — 실수로 남의 데이터를 건드릴 경로가 아예 없으니까.

청구서는 소비자 2개째에 도착했다. `RaidGameContext`의 `SetSessionKey` 빈 구현, `Ip=""` 스텁, 그리고 그것이 `SessionComponent`를 타면 빈 IP가 저장되는 실버그(5.5.2).

코드에는 이미 시한폭탄이 있다 — `UserComponentBase`:

```csharp
public Task<T> CreateMdlAsync(T entity)
{
    return _repo.InsertAsync<T>(entity, ListKeyFor(RpcCtx.PlayerId));
    //                          ↑ entity.PlayerId로 저장     ↑ 캐시는 RpcCtx.PlayerId 버킷
}
```

두 값이 다를 수 있는데 검증이 없다. 소비자가 하나일 땐 항상 같아서 터지지 않는다.

**(2) `_playerService.ValidDecCost(...)` → `PlayerDetailManager`**

Gen1에서 재화 차감은 **Service에 있었다**. Gen2가 3.3("Service가 raw Model+Proto를 실어나름")을 고치면서 이걸 Manager 안으로 옮겼고, 그 결과가 330줄짜리 `PlayerDetailManager` — `_userRepo.Point`/`.Ticket`/`.Cookie`/`.Item`을 전부 찌르는 5.1.4의 대표 사례다.

그런데 **3.3의 문제는 *위치*가 아니라 *파라미터 형태*였다.** 파라미터가 6~7개인 게 문제였지 PlayerService에 있는 게 문제가 아니었다. 파라미터 문제를 위치 이동으로 풀려다 새 문제를 얻었다.

### 0.5 EF → Dapper 전환의 미결산 — 5.4의 진짜 뿌리

Gen1은 EF Core였다. `DbContext`가 하던 일과 Gen2의 처분:

| EF 기능 | Gen2의 처분 |
|---|---|
| 커밋 경계 | **가져옴** — `GlobalDbRepo.CommitAsync` (DB 커밋 → 캐시 flush) |
| 아이덴티티 맵 | **대체** — `ICacheSession` + Component 리스트 캐시 |
| 마이그레이션 | **버림** (의도적, Liquibase로) |
| 지연 로딩 | **버림** (의도적) |
| **변경 추적** | **처분 결정 없이 사라짐** |

변경 추적 상실의 인과사슬:

```
변경 추적 없음
  → "저장은 각자 즉시 한다"
  → ItemManager.DecAmountAsync 가 UpdateMdlAsync 까지 호출
  → 저장하려면 Component/Repo 참조가 필요
  → Manager 가 _userRepo 를 든다               ← 5.1.4
  → Manager 가 다른 모든 Model 에 접근 가능
  → "이 로직 누구 소유?" 판정 불가              ← 5.4
```

**5.1.4와 5.4는 별개 문제가 아니라 "변경 추적을 뭘로 대체할지 결정하지 않음"의 두 얼굴이다.**

→ 그래서 **3.8(저장 누락 방지)이 A안 최대 미결 사항인 것은 논리적 필연이다.** A안은 Gen2가 미룬 이 결정을 처음 정면으로 마주치는 문서다. 3.8이 허술한 것은 A안이 부실해서가 아니라 그 결정이 원래 어렵고 아직 아무도 하지 않았기 때문이다. **3.8 확정이 A안 채택의 전제 조건이다.**

### 0.6 A안 요소별 계보 — 무엇이 발명이고 무엇이 계승인가

| A안 요소 | 출처 | 이전 세대엔 왜 없었나 | 판단 |
|---|---|---|---|
| 리플렉션 제네릭 CRUD | Gen1 3.2 | EF가 매핑해줘서 불필요 | Gen2 것 **계승** |
| DI 시점 Repository 스왑 | Gen1 3.4 | — | Gen2 것 **계승** (후속 문제 0) |
| `ICacheSession` + 커밋 시 flush | Gen2 신규 | EF 아이덴티티맵 대체물 | **계승** |
| `[Entity]` + 어셈블리 스캔 | Gen2 5.2.1 | 리플렉션 CRUD 도입으로 메타데이터가 코드 밖으로 나갔기 때문 | Gen2 해법의 **수정** |
| `DataSet<T>` | Gen2 5.2.2 / 5.2.3 | Component가 "엔티티마다 클래스 1개"를 전제 | Gen2 Component의 **일반화** |
| Model partial 메서드 | **Gen1 3.3의 목적** | Gen2는 같은 목적을 Manager 래퍼로 달성 → 1:1 부작용 | **목적 계승, 수단 교체** |
| `GameDb.User(shardId, playerId)` | Gen1 `InitUserRepo` | Gen2가 "특수 경로"로 오해하고 일반화하지 않음 | Gen1에서 **회수** |
| UoW 루트 | Gen1 (EF `DbContext`) | Gen2가 커밋 경계만 가져옴 | Gen1에서 **부분 회수** |
| `ChangeSet` | Gen2 `ChgObjPacket` + `reason` | 감사 로그를 실제로 만들 필요가 아직 없었음 | Gen2 것의 **승격** — 단 근거는 "세 싱크 단일 출처"가 아니라 **와이어 계약 분리**(3.5) |
| 재화 도메인 서비스 | Gen1 `PlayerService.ValidDecCost` | Gen2가 3.3을 고치며 위치까지 옮김 | Gen1 위치로 **회수 + 정명** |
| **Model에 DB 참조 없음** | Gen2 5.1.4 + 5.4 | EF 변경추적 상실의 미결산 | ← **유일한 신규 결정** |

**A안에서 진짜 새로운 결정은 마지막 한 줄뿐이다.** 나머지는 전부 계승·회수·승격·일반화다.

이것이 A안이 "전면 재작성"으로 보이면서도 실제 리스크가 낮은 이유다 — 검증되지 않은 아이디어의 개수가 1개다. 그리고 그 1개의 대가가 3.8(저장 모델 재설계)로 정확히 청구되고 있다. **신규 결정 1개 = 미결 사항 1개**, 대차가 맞는다.

특히 **"Manager를 버린다"가 아니다.** Gen1 3.3에서 배운 목적("Model+Proto를 파라미터로 실어나르지 말고 메서드를 객체에 붙여라")은 100% 유지된다. 바뀌는 것은 수단뿐이다:

| | 수단 | 부작용 |
|---|---|---|
| Gen2 | Model을 **감싸는 별도 객체** | 감쌀 객체가 있으므로 → 언제 만드나(5.1.1), N개 래핑 비용(5.1.2), 저장하려면 Repo 필요(5.1.4) |
| A안 | Model **자신의 partial 메서드** | 감쌀 객체가 없으므로 → 로직 없으면 파일 없음, 래핑 비용 0, 저장은 위층의 일 |

### 0.7 의도적으로 가져가지 않는 것

| 가져가지 않는 것 | 근거 |
|---|---|
| EF식 전체 변경 추적(스냅샷 비교) | 리플렉션 비용 + 스냅샷 메모리. 대체 수단은 **3.8의 dirty 플래그**로 확정 — `bool` 1개로 같은 목적을 달성한다 |
| private setter 완전 캡슐화 | Dapper 리플렉션 + JSON 직렬화 대상. **Gen1·Gen2 두 세대에 걸쳐 같은 결론** — 근거가 2회 확인됨 |
| 도메인 이벤트 / 사가 | **5장 목록에 대응 항목이 없음.** 원자성이 아직 트랜잭션으로 전부 커버됨 |
| CQRS / 읽기 모델 분리 | **읽기 부하 문제가 아직 관찰되지 않음** |

역으로 `AllUserRepo`(전 샤드 접근)처럼 **이미 있고 실제로 쓰이는 것은 그대로 가져간다.** 있는 것을 버리는 데에도 없는 것을 만드는 것만큼의 근거가 필요하다.

### 0.8 판단 규칙 — 다음 세대에도 적용할 것

| | 규칙 | 이번 세대의 증거 |
|---|---|---|
| **R1** | 새 클래스 종류를 도입하면 (a)언제 만드나 (b)무엇을 아나 (c)몇 개인가에 **코드로** 답하라 | Manager가 (a)에 답이 없어 14줄~412줄 스펙트럼 |
| **R2** | 모든 해법은 새 전제를 낳는다. 해법과 함께 **그 전제를 문서에 적어라** | "Model 1개=Manager 1개" 전제가 5.1.1/5.1.2/5.1.4를 낳음 |
| **R3** | **"무엇이 잘못됐나"와 "어디에 있나"를 분리하라.** 잘못된 것만 고쳐라 | 3.3(파라미터 형태)을 위치 이동으로 풀다가 5.1.4를 얻음 |
| **R4** | 앰비언트 컨텍스트의 비용은 **소비자 2개째부터** 청구된다. 소비자 1개일 때의 편안함을 검증으로 착각하지 마라 | `BeginOwnUserRepo()` → RaidServer 붙자마자 스텁 |
| **R5** | 이전 세대의 "특수 경로"는 일반 경로 실패의 신호다. **기본 승격 후보로 보라** | `InitUserRepo(playerId)` |
| **R6** | 프레임워크를 버릴 때 **그 기능 목록과 각각의 처분(가져온다/대체한다/버린다)을 기록하라.** 기록되지 않은 항목은 사라지는 게 아니라 흩어진다 | EF 변경 추적이 기록 없이 사라져 5.4가 됨 |
| **R7** | **관찰된 문제에만 구조로 답하라** | 5장에 없는 CQRS·도메인이벤트는 만들지 않음 |

---

## 1. 설계 목표

| # | 목표 | 근거 |
|---|---|---|
| G1 | 데이터 계층이 "누가 요청했는가"를 모르게 한다 | 5.5.1/5.5.2 — 앰비언트 컨텍스트가 모든 마찰의 뿌리 |
| G2 | 비즈니스 로직의 소속을 타입으로 판정 가능하게 한다 | 5.4 — 핵심 미해결 질문 |
| G3 | 비즈니스 로직을 DB 없이 단위 테스트 가능하게 한다 | 현재는 DI+DB 세팅 없이는 로직 하나도 테스트 불가 |
| G4 | 도메인은 로깅하지 않고 변경 사실을 반환하고, 감사/로그는 App Service가 싱크별로 조립한다 | `CashChangeLogModel`(유료 재화 전용)·`GachaLogModel`이 등록만 되고 미사용 — 감사 미구현. **새 타입은 만들지 않는다**(3.5) |
| G5 | 새 엔티티 추가 비용을 0에 수렴시킨다 | 5.2.1/5.2.2 |
| G6 | 운영툴·RaidServer·배치가 본편과 같은 진입점을 쓰게 한다 | 5.5.1, 6장 GSA `InitUserRepo` 패턴 |

---

## 2. 계층 구조

```
 Transport          RpcService / OpsController / RaidServer / 배치잡
                    → "어느 샤드, 누구" 를 스칼라로 결정해서 아래로 내려보냄
                            ↓ (int shardId, ulong playerId)
 Application        XxxAppService
                    · 트랜잭션 경계   · 여러 Model 조율   · 로깅/감사   · 응답 조립
                            ↓
 Domain             Model partial 메서드 (순수)  +  도메인 서비스 (여러 Model 불변식)
                    · DB/캐시/컨텍스트를 전혀 모름   · 변경 사실을 반환
                            ↓ (App Service가 저장을 지시)
 Data               GameDb (Unit of Work)  →  DataScope  →  DataSet<T>
                            ↓
 Engine             IDbSession / IDbExecutor / ICacheSession        ← 기존 코드 그대로 재사용
```

**계층 규칙 (단방향)**: Transport → Application → Domain → Data → Engine.
Domain은 Data를 모르고, Data는 Application을 모른다. `IGameContext`는 Transport 계층에서 소멸한다.

---

## 3. 구성 요소

### 3.1 엔티티 메타데이터 — attribute + 어셈블리 스캔

```csharp
[Entity(Pk = ["PlayerId", "Num"], ScopeKey = "PlayerId")]
public partial class ItemModel : ModelBase { /* 코드젠 */ }

[Entity(Pk = ["Id"])]              // ScopeKey 없음 = 소유자 스코프 밖
public partial class AccountModel : ModelBase { }
```

- 부팅 시 `EntityRegistry.ScanAndRegister(typeof(ItemModel).Assembly)` 한 줄이 전부. 프로세스마다 목록을 복붙하지 않는다.
- `ScopeKey`가 "이 엔티티는 스코프 안에서 누구 소유인가"를 **데이터로** 표현한다. 지금은 `UserComponentBase.LoadFromDb`가 `WHERE PlayerId = ...`를 코드에 하드코딩하고 있는데, 그게 메타데이터로 올라간다.
- 코드젠(`ClassGenerator`)이 PK 정보를 이미 알고 있으므로 이 attribute까지 함께 찍어내면 손으로 쓸 것도 없다.

**S1 시점의 확정 필드는 `Pk` + `ScopeKey` 둘뿐이다 (2026-08-12).**

| | 판단 |
|---|---|
| `Owner` → **`ScopeKey`** 개명 | `Owner`는 "무엇의 소유자인지"가 드러나지 않는다. 실제 의미는 *User 스코프 안에서 행을 소유자별로 가르는 컬럼*이고, `GameDb.User(playerId)` → 그 컬럼으로 필터라는 연결이 이름에 있어야 한다. `Pk`와 같은 "컬럼명을 담는 필드" 명명 규칙과도 일치. `PartitionKey`는 기각 — AccountId 기준 물리 샤딩(`GlobalDbRepo._shardMap`)과 축이 다른데 이름이 겹친다 |
| `ScopeKey`는 **User 폴더 한정** | 소유자 개념(ambient owner + 자동 `WHERE` + 소유자별 캐시 버킷)이 있는 건 `UserComponentBase`뿐이다. `AuthComponentBase`/`CenterComponentBase`에는 소유자 개념이 전무하고, Auth의 AccountId 조회는 전부 **인자 기반 명시 조회**다(소유자 축 리스트 조회는 `ChannelComponent.GetListAsync` 하나뿐 — **T2**). `fk = AccountId`는 참조 무결성 선언이지 스코프 선언이 아니다 |
| 출처는 **기존 `fk` 토큰** | CSV에 `fk`가 이미 있고 이미 소비된다(`ModelGenerator.cs:376` → Liquibase FK). `scope` 같은 새 토큰을 넣으면 같은 사실이 두 군데 선언되어 드리프트한다. `User/Player`만 `fk`가 없으므로(스코프 루트) PK를 ScopeKey로 쓴다. `fk` 2개 이상이면 생성 실패 |
| `Table` **제외** | `DapperExtension.cs:31-35`가 클래스명−`Model`로 테이블명을 만들고 오버라이드 경로가 없다. 규칙 이탈 모델 현재 0개 → 지금 넣으면 추측 |
| `Cache`/`SlidingTtl` **제외, TODO 주석만** | 3.9의 정책 열거가 아직 닫히지 않았다(아래 5종 표의 `SessionComponent` 행이 2플래그로 표현되지 않는다). 틀린 enum을 모델 20개에 먼저 박는 비용 > 나중에 필드 하나 붙이는 비용. **S2에서 `DataSet<T>` 형태가 코드로 확정된 뒤 올린다** |

> **이월된 열린 질문 (2026-08-14 재정리 — 상세는 StepByStep §S1-G)**: 한때 "소유자가 User에만 있으므로 `GameDb.Auth()`/`Center()`는 스코프가 아니라 DB 선택"이라 적었으나 **전제가 부정확했다.** Auth의 데이터 모델에는 소유자 축이 있다 — `Account.Id`가 루트이고 `Channel`/`Device`/`Session`/`PlayerMap`이 전부 `AccountId`를 갖는, User와 **같은 모양**이다. 소유자가 없는 것은 Center뿐이다.
>
> 진짜 비대칭은 **Auth가 신원을 알아내는 계층**이라는 것이다. 기기 키·채널 키·세션 키 조회와 계정 생성은 `AccountId`를 *모르는 상태*에서 일어나므로, `GameDb.Auth(accountId)` 하나로는 로그인 첫 쿼리를 보낼 곳이 없다. User 스코프에는 이런 단계가 없다.
>
> "균일하게 묶으면 빈 규칙이 따라다닌다"는 반론은 **구체 클래스를 하나로 만들 때만** 성립한다. `IDataScope { DataSet<T> Set<T>(); }` 인터페이스로 공통부만 뽑으면 각 스코프가 자기 키만 들고(`UserScope`: ShardId+PlayerId, `AuthScope`: AccountId, `CenterScope`: 없음) dirty flush에는 셋 다 참여한다.
>
> **판단은 S4(Channel/Device/Account 파일럿)에서 한다.** `ScopeKey`는 선언이 아니라 동작(자동 `WHERE`·쓰기 검증·캐시 버킷)을 만들기 때문에, 스코프 밖 조회를 어디로 보낼지 정해지기 전에 Auth에 붙이면 안 된다. S1은 어느 결론에도 영향받지 않는다.

실행 절차·체크리스트는 `DbLayer_A_StepByStep.md` §S1-A/B/C 참조.

### 3.2 `GameDb` — Unit of Work 루트

```csharp
public class GameDb : IAsyncDisposable
{
    // 어느 샤드의 누구든 열 수 있다. "본편"과 "운영툴"의 구분이 없다.
    public UserScope   User(int shardId, ulong playerId);
    public AuthScope   Auth();
    public CenterScope Center();

    public Task CommitAsync();     // DB 커밋 → 캐시 flush (기존 GlobalDbRepo 로직 유지)
    public Task RollbackAsync();
}
```

- **`IGameContext` 의존 없음.** 생성자는 `DbSessionManager` / `ICacheSession` / `ILogger`만 받는다.
- `User(shardId, playerId)`를 여러 번 다른 값으로 호출할 수 있다 → 길드, 우편 발송, 거래, 레이드 보상처럼 **다중 플레이어 연산이 자연스럽게 성립**한다. (현재 `BeginOwnUserRepo()`는 요청당 1명 고정.)
- 커넥션/트랜잭션 추적은 기존 `DbSessionManager`가 그대로 담당.

### 3.3 `DataScope` / `DataSet<T>` — 유일한 데이터 접근 타입

```csharp
public class UserScope          // AuthScope / CenterScope 도 동형
{
    public int   ShardId  { get; }
    public ulong PlayerId { get; }        // ScopeKey 값

    public DataSet<T> Set<T>() where T : ModelBase, new();   // 스코프 내 캐싱
}

public class DataSet<T> where T : ModelBase, new()
{
    public Task<List<T>>                GetListAsync(object conditions = null);
    public Task<(bool Found, T? Value)> TryGetAsync(object pk);
    public Task<T>                      CreateAsync(T entity);
    public Task                         UpdateAsync(T entity);
    public Task                         DeleteAsync(T entity);      // 5.6 해소
}
```

- **`XxxComponent` 클래스가 존재하지 않는다.** 등록된 엔티티면 `user.Set<ItemModel>()`로 바로 쓴다.
- `ScopeKey`가 지정된 엔티티는 `GetListAsync`가 **자동으로 `WHERE PlayerId = scope.PlayerId`를 붙인다.** 스코프 밖 데이터를 실수로 읽을 수 없다.
- `CreateAsync`/`UpdateAsync`는 `entity`의 ScopeKey 필드가 `scope.PlayerId`와 다르면 **즉시 예외**. → 5.5.1에서 지적한 "DB엔 정상 저장, 캐시는 엉뚱한 버킷"이라는 조용한 정합성 버그를 구조적으로 차단.
- 스코프 컬렉션 안에서 끝나는 조회는 클래스를 새로 만들지 않고 **확장 메서드**로 붙인다:

```csharp
public static class ChannelQueries
{
    public static async Task<ChannelModel?> FindActiveAsync(this DataSet<ChannelModel> set, ulong accountId)
        => (await set.GetListAsync(new { AccountId = accountId }))
           .FirstOrDefault(x => x.State == EChannelState.ACTIVE);
}
```

> **확장 메서드 규칙 — SQL을 새로 쓰지 않는다.**
> `DataSet<T>` 확장 메서드는 `GetListAsync` 결과를 **거르기만** 한다. 위 예제가 이 규칙을 지키고 있다(새 쿼리가 아니라 이미 로드된 컬렉션 필터).
> 새 SQL이 필요한 순간 그것은 확장 메서드가 아니라 **3.9의 T2 또는 T3**이다. 이 경계를 지켜야 캐시 사본이 하나로 유지된다.
> 표준 CRUD 밖 조회 전반의 분류와 캐시 규칙은 **3.9**를 따른다.

### 3.4 도메인 — Model partial 메서드 (순수)

```csharp
// ItemModel.cs — 손으로 작성. DB·캐시·컨텍스트·로거 전부 모름.
public partial class ItemModel
{
    public ChangeSet Decrease(double amount, string reason)
    {
        ReqHelper.ValidEnough(amount, Amount, $"ITEM_{Num}", reason);
        var before = Amount;
        Amount    -= amount;
        AccAmount -= amount;
        MarkDirty();
        return ChangeSet.Of(EObjType.ITEM, Num, before, Amount);
    }
}
```

- **저장하지 않는다.** 필드만 바꾸고 "뭐가 변했는지"를 반환한다.
- 따라서 `new ItemModel { Amount = 10 }.Decrease(3, "test")` 하나로 단위 테스트가 끝난다 (G3).
- Proto가 필요하면 **파라미터로 받는다**: `cookie.CanEnhance(prtCookie, playerLv)`. 시그니처가 의존성을 그대로 문서화한다. `Manager`처럼 Proto를 필드로 붙들고 있는 래퍼 클래스는 없앤다.
- Model+Proto를 항상 같이 다뤄야 하는 곳은 **읽기 전용 뷰**로만 만든다: `readonly record struct ScheduleView(ScheduleProto Prt, ScheduleModel Mdl)`. DB를 모르는 값 객체이므로 Manager와 다르다.

### 3.5 `ChangeSet` — 도메인의 변경 사실 반환 타입 (G4)

**원칙**: **도메인은 로깅하지 않고 "무엇이 변했는지"를 반환한다. 로깅·감사는 App Service가 한다.** 컨텍스트(AccountId/PlayerId/TraceId)는 Transport가 로거 스코프에 한 번 바인딩하므로 도메인까지 내려갈 필요가 없다.

```csharp
// 서버 런타임 전용 타입. 직렬화 대상이 아니다.
public readonly record struct ChangeSet(EObjType Type, int Num, double Before, double After)
{
    public double Delta => After - Before;
    public static ChangeSet Of(EObjType t, int n, double b, double a) => new(t, n, b, a);
}

public partial class PointModel
{
    public ChangeSet DecAmount(double amount)
    {
        ReqHelper.ValidEnough(amount, Amount, $"POINT_{Num}", nameof(DecAmount));
        var before = Amount;
        Amount -= amount;
        MarkDirty();
        return ChangeSet.Of(EObjType.POINT_START, Num, before, Amount);
    }
}
```

**존치 근거 — "세 싱크의 단일 출처"가 아니라 "와이어 계약으로부터의 분리"**

초안은 "한 값이 감사·로그·응답 세 곳을 동시에 만족시킨다"를 근거로 들었다. **그 주장은 철회한다** — 싱크마다 범위와 shape이 다르다(아래 표). 그러나 타입 자체는 다른 근거로 존치한다:

1. **`ChgObjPacket`은 `[ProtoContract]` + `[ProtoMember]` 번호가 붙은 직렬화 계약이다.** 도메인이 이것을 반환하면 와이어 포맷 변경이 도메인 코드까지 파급되고, 클라이언트에 노출하면 안 되는 내부 값을 담을 수 없다.
2. **이 프로젝트는 이미 "런타임 타입 → 패킷" 매핑을 서비스 경계에서 한다** — `_mapper.Map<CookiePacket>(mgrCookie.Model)`. 도메인이 패킷을 직접 반환하는 현재의 `DecCostAsync`가 오히려 예외이며, `ChangeSet`을 두면 **일관성이 회복된다.**
3. 서버 런타임 전용이므로 감사에 필요한 `Before`를 1급 필드로 갖는다. `ChgObjPacket`은 `Amount`(delta)/`TotalAmount`(after) 형태라 `Before`가 계산으로만 얻어진다.

**비용은 응답 경계의 매핑 한 줄**(`changes.ToPacket()`)이며, 위 2번에 따라 기존 스타일과 같다.

**감사·로그는 싱크별로 개별 처리한다 (ChangeSet을 그대로 넘기지 않는다).**

| 싱크 | 범위 | ChangeSet과의 관계 |
|---|---|---|
| 응답 패킷 | 전 축 | `ToPacket()` 매핑 — **1:1** |
| 구조화 로그 | 필요한 곳 | App Service가 선택적으로 — **1:1** |
| `CashChangeLogModel` | `FREE_CASH`/`REAL_CASH`/`TOTAL_CASH` **전용** | **부분집합 필터 + 액션당 1행 fold** |
| `GachaLogModel` | 가차 1회 | **다른 축** — 가차 App Service가 직접 조립 |

**들어가지 않는 것**
- **`Reason`** — 액션당 1개이므로 변경 건마다 복제하지 않는다. `ActionName`/`ActionNameHash`/`ActionDetail`이 목적지이며 App Service가 액션 컨텍스트로 넘긴다. `reason` 문자열은 지금처럼 인자로만 흐른다.
- **`Acc*`(누적)** — 모델의 파생 상태다. 감사 기록 시점에 모델에서 읽는다.

**G4는 타입으로 강제하지 않는다.** 감사 대상이 유료 재화 하나뿐(6장 비목표)이므로 강제 장치는 과하다. 대신 **Cash를 변경하는 도메인 메서드를 소수로 한정**하고(`PlayerDetailModel.DecCash`/`IncCash`) 호출 지점을 코드 리뷰로 커버한다.

> *네이밍 참고*: 단건을 담는데 이름이 `...Set`이라 컬렉션으로 오독될 수 있다. `ObjChange`가 더 정확하지만, 논의에서 계속 `ChangeSet`으로 불러왔으므로 그대로 둔다. 바꾸려면 S5 착수 전에.

### 3.6 도메인 서비스 — 여러 Model 불변식 (G2)

`PlayerDetailManager`가 `_userRepo.Point/.Ticket/.Cookie/.Item`을 전부 찌르고 있는 실제 사례가, 사실은 **"재화 원장"이라는 이름 없는 도메인 개념**이었다. A안에서는 그걸 명시적으로 만든다:

```csharp
// 여러 엔티티에 걸친 규칙 — 이름이 있는 도메인 개념이지 "PlayerDetail의 Manager"가 아니다
public class ObjectLedger
{
    private readonly PlayerDetailModel _detail;
    private readonly Func<int, PointModel>  _point;    // 이미 로드된 것만 주입 (DB 접근 아님)
    private readonly Func<int, TicketModel> _ticket;
    private readonly Func<int, ItemModel>   _item;

    public ChangeSet               Pay(ObjValue cost, string reason);
    public IReadOnlyList<ChangeSet> Grant(IEnumerable<ObjValue> rewards, string reason);
}
```

**판정 규칙 (G2)**
| 이 연산이... | 소속 |
|---|---|
| Model 인스턴스 **하나**의 필드만 바꾼다 | Model partial 메서드 |
| Model **여러 개**에 걸친 규칙/불변식이다 | 도메인 서비스 |
| 로드/저장/커밋/로깅/응답조립이 필요하다 | App Service |

Model은 DB 참조가 없으므로 **자기 밖으로 나가는 것 자체가 불가능**하다. 규칙을 컨벤션이 아니라 타입이 강제한다.

### 3.7 App Service — 조립 지점

```csharp
public async Task<GachaNormalResponsePacket> GachaNormalAsync(int shardId, ulong playerId, GachaNormalRequestPacket req)
{
    var user   = _db.User(shardId, playerId);
    var center = _db.Center();

    // 1) 로드
    var detail   = await user.Set<PlayerDetailModel>().GetOneAsync();
    var schedule = await center.Set<ScheduleModel>().FindAsync(req.ScheduleNum);
    var prt      = ProtoDb.Get<ScheduleProto>(req.ScheduleNum);

    // 2) 순수 도메인 — 여기서 DB 접근 없음
    var ledger  = user.Ledger(detail);
    var cost    = ledger.Pay(prt.CostOf(req.Cnt), reason);
    var rewards = ledger.Grant(GachaRandom.Roll(prt, req.Cnt), reason);

    // 3) 저장 + 감사 + 응답 — 한 곳에서
    await user.SaveAsync(detail);
    await _audit.WriteAsync(user, cost.Concat(rewards));
    return Response.From(cost, rewards);
}
```

로드 → 순수 계산 → 저장의 3단이 눈에 보인다. "이 요청이 무슨 일을 하는가"가 App Service 한 메서드에서 읽힌다 (5.4에서 지적한 "Manager로 로직을 몰면 Service에서 한눈에 안 보인다"는 문제의 대응).

### 3.8 저장 모델 — dirty 플래그 + 커밋 시 flush **(확정)**

0.5에서 정리했듯 Gen2는 EF의 변경 추적을 처분 결정 없이 잃었고, 그 결과가 5.1.4/5.4다. A안은 이 결정을 정면으로 한다. 검토한 세 안:

| | (a) 스냅샷 자동 flush | (b) App Service 명시 Save | **(c) dirty 플래그 ← 채택** |
|---|---|---|---|
| 저장 누락 | 불가능 | **가능** | 불가능 |
| 비용 | 스냅샷 메모리 + 비교 리플렉션 | 0 | **bool 1개** |
| 실수 표면적 | 0 | **호출부 N곳** | **도메인 메서드 1곳** |
| Model의 외부 참조 | 없음 | 없음 | 없음 |

(b)는 기각한다 — 3.7의 예제 코드 자체가 이미 `ledger`가 변경한 Point/Item을 저장하지 않고 있다. 설계자가 쇼케이스에서 놓치는 것은 실무 코드에서 반복된다. (a)는 0.7에서 "EF식 전체 변경 추적은 가져가지 않는다"고 한 결정과 충돌한다.

**(c) — 도메인 메서드가 자기 변경을 스스로 표시한다.**

```csharp
public partial class CookieModel
{
    public void EnhanceLv(int aftLv)
    {
        Lv = aftLv;
        MarkDirty();          // => IsDirty = true;  자기 필드. 외부 참조 0
    }
}
```

`MarkDirty()`는 `ModelBase`의 `bool IsDirty`를 세우는 **자기 자신에 대한 변경**이다. 따라서 **0.6의 "유일한 신규 결정 = Model에 DB 참조 없음"이 그대로 지켜지고**, 단위 테스트도 그대로다 (오히려 `Assert.True(cookie.IsDirty)`로 검증 지점이 하나 는다).

(b) 대비 핵심 이점은 **실수의 표면적**이다. (b)는 App Service를 새로 쓸 때마다 매번 기억해야 하고, (c)는 도메인 메서드를 작성할 때 한 번이다. 메서드는 1번 작성되고 N번 호출된다.

**커밋 시 flush — 엔진 계층을 재사용한다.**

`IRepository.UpdateAsync<T>(entity, listKey, match)`가 이미 **DB 쓰기 + 캐시 갱신을 한 묶음으로** 처리하므로, flush는 이것을 호출만 한다:

```csharp
public async Task CommitAsync()          // GameDb
{
    foreach (var scope in _scopes)
        foreach (var set in scope.LoadedSets)
            await set.FlushDirtyAsync();

    _sessions.Commit();                            // 현 GlobalDbRepo.CommitAsync 와 동일
    await _cache.FlushPendingWritesAsync();
}

internal async Task FlushDirtyAsync()    // DataSet<T> — T가 컴파일 타임 확정 → 리플렉션 불필요
{
    foreach (var e in _loaded.Where(x => x.IsDirty))
    {
        await _repository.UpdateAsync<T>(e, ListKey, x => PkEquals(x, e));
        e.ClearDirty();
    }
}
```

`listKey`/`match`는 지금 `UserComponentBase`가 엔티티마다 추상 메서드(`ListKeyFor`/`KeyFor`)로 만들던 것인데, **`[Entity(Pk=…, ScopeKey=…)]` 메타데이터 + 스코프의 `ownerId`로 생성**된다. 그 반복도 함께 사라진다. **엔진 계층은 한 줄도 바뀌지 않는다.**

**캐시 — 이미 지연 구조라 오히려 정합적이 된다.**

`ICacheSession`은 *"FlushPendingWrites: DB 커밋 후 지연된 쓰기(예: Redis) 일괄 반영"*이다. 즉 **캐시 쓰기는 원래부터 커밋까지 지연**되고 DB만 즉시였다 — 타이밍이 어긋나 있었다.

```
현재:  DB UPDATE 즉시  /  Cache.Set pending → 커밋 때 flush     ← 어긋남
(c):   DB UPDATE 커밋 때 /  Cache.Set pending → 커밋 때 flush   ← 일치
```

`SqlRepository.UpdateAsync`가 둘을 한 메서드에서 하므로 커밋 시점에 부르면 자동으로 맞는다. **캐시 계층은 구조 변경이 없다.**
같은 요청 내 읽기도 문제없다 — `DataSet<T>`가 스코프 내 로드 인스턴스를 캐싱해 돌려주므로(3.3) dirty 상태의 **같은 객체**를 본다.

**InMemory 모드 — flush 주체가 `GameDb`여야 하는 이유.**

`InMemoryRepository`는 캐시를 쓰지 않고 `InMemoryStore`에 직행하며, `InMemoryDbSession.Commit()`은 **no-op**이다. 따라서 flush를 `IDbSession.Commit()`에 걸면 InMemory에서 동작하지 않는다. 위처럼 **`GameDb.CommitAsync`가 직접 순회하며 `UpdateAsync`를 호출**하면 MySQL/InMemory가 동일하게 동작한다(no-op인 것은 tx commit뿐이고 쓰기는 이미 끝났다).

**부수 이득**: 현재 InMemory는 UPDATE가 즉시 반영되고 `Rollback()`도 no-op이라, 요청 중간에 예외가 나면 **부분 쓰기가 남는다**. `ServerTest`가 InMemory로 도므로 실패 케이스에서 상태가 오염될 수 있는 구조다. (c)에서는 커밋 전 예외 시 dirty 플래그만 버려지므로 아무것도 쓰이지 않는다.

**DEBUG 스냅샷의 재배치.**

기존 초안의 "DEBUG 빌드 스냅샷 검출"은 (b)와 결합하면 릴리스 무방비라 기각했지만, **(c)와 결합하면 성격이 달라진다**:

- **릴리스** — dirty 플래그가 주 방어선. 비용 0.
- **DEBUG** — 스냅샷으로 *"값은 바뀌었는데 `IsDirty == false`"* = **`MarkDirty()` 호출 누락**을 검출.

스냅샷이 저장 누락의 유일한 방어선이 아니라 **보조 검증**이 된다.

**⚠ 커밋 경계는 반드시 유저 락 안에 있어야 한다.**

현재 `RpcService.HandleMethodAsync`는 **쓰기를 `RunAtomicAsync` 안에서, 커밋을 락 밖에서** 한다. dirty 모델은 쓰기 전체를 `CommitAsync`로 옮기므로, 순서를 그대로 두면 **쓰기 전체가 락 밖으로 나가 lost update가 발생한다**:

```
A 락 획득 → Point 100 읽음 → 메모리 90 (dirty) → 락 해제    ※ DB는 아직 100
B 락 획득 → DB에서 100 읽음 → 메모리 90 (dirty) → 락 해제
A commit → 90 기록 / B commit → 90 기록
→ 20 차감돼야 하는데 10만 차감
```

따라서 `CommitAsync`(flush + tx commit + 캐시 flush)를 `RunAtomicAsync` **안으로** 옮긴다. 이는 dirty 모델 도입의 **필수 동반 변경**이며 StepByStep S2의 완료 조건이다. 응답 캐시 쓰기가 커밋보다 앞에 있어야 한다는 기존 불변식은 유지한다(롤백 시 pending 폐기에 의존).

**락·유틸리티 쿼리는 flush 대상이 아니다.** `MySqlLockService`의 `GET_LOCK`은 엔티티 상태와 무관하며, 3.9 T3의 자동 flush를 타면 **락이 걸리기 전에 쓰기가 나간다.** 이런 쿼리는 `GameDb.Utility`라는 **flush하지 않는 별도 경로**로 분리한다.

**남는 한계 (정직하게)**

| | 내용 |
|---|---|
| INSERT는 여전히 즉시 | `InsertAsync`가 *"DB Insert 후 auto PK 포함 entity 반환"*이라 지연하면 auto PK를 못 받는다. **원자성 개선은 UPDATE에 한정**되며, InMemory에서 `TouchAsync`가 만든 행은 예외 시에도 남는다(현재와 동일 수준) |
| setter 직접 대입 | `cookie.Lv = 5`는 dirty가 안 된다. 6장의 "완전 캡슐화 비목표"와 같은 한계이며 DEBUG 스냅샷이 잡는다 |
| `scope.Raw`(T3)가 dirty를 못 봄 | 3.9 참조 — 실행 전 스코프 flush로 대응 |
| flush 순서 | dirty가 여러 테이블에 걸치면 쓰기 순서가 로드 순서가 된다. FK 제약이 있으면 확인 필요 |

**이름**: `Update()`가 아니라 `MarkDirty()`를 쓴다. `Update`는 DB UPDATE로 읽히는데, 없애려는 것이 정확히 "도메인 메서드가 저장한다"는 인식이기 때문이다.

### 3.9 표준 CRUD 밖 조회 — 4티어 분류

`DataSet<T>`는 *"모든 조회는 스코프 컬렉션 안에서 끝난다"*를 전제한다(0.2 형식으로 말하면 A안이 까는 새 전제다). 이 전제가 깨지는 조회가 실제로 존재하므로, **문법(확장 메서드)이 아니라 캐시 사본 관리로 분류한다.**

**근본 제약**: 현재 `UserComponentBase`는 "플레이어당 T 전체 리스트 캐시 1개" 모델이고, 모든 조회가 그 리스트를 통과하므로 엔티티 사본이 하나다. 그래서 무효화가 단순하다. **특화 쿼리가 이 모델을 깨는 이유는 별도 SQL 결과가 그 리스트와 별개 사본이 되기 때문**이며, 무효화 조건을 시스템이 추론할 수 없다. 현 코드가 특수 쿼리마다 캐시를 포기한 이유가 이것이다(`ScheduleComponent` "일반화가 어려운 부분이라", `WorldStageComponent` "TODO: 캐시", `PlayerComponent` "컬렉션 밖의 조회 → DB 직접 접근").

| 티어 | 언제 | 캐시 | 진입점 | 선언 |
|---|---|---|---|---|
| **T0** 메타데이터 | ScopeKey 컬럼명이 다름 / ScopeKey 없음 | 기본 리스트 캐시 | 없음(자동) | `[Entity(ScopeKey=…)]` |
| **T1** 스코프 필터 | 로드된 컬렉션에서 고르기 | 기본 리스트 캐시 **재사용** | `DataSet<T>` 확장 메서드 | **새 SQL 금지**(3.3) |
| **T2** 보조 인덱스 | 비-ScopeKey 컬럼으로 **엔티티** 찾기 | **현 단계 캐시 없음** (아래) | `set.ByIndexAsync(...)` | `[SecondaryIndex("AccountId")]` |
| **T3** 원시 쿼리 | 집계 / 조인 / 스칼라 | **금지**가 기본 | `scope.Raw<T>(sql)` — 감추지 않음 | 호출부 주석 필수 |

**현 탈출구 4건의 실제 분류** — 절반은 특수 쿼리가 아니라 메타데이터 부족이었다:

| 현재 코드 | 실체 | 티어 |
|---|---|---|
| `PlayerComponent.LoadFromDb` override ("PlayerModel의 PK는 Id") | ScopeKey 컬럼명이 `Id`일 뿐 | **T0 — 소멸** |
| `ScheduleComponent.GetListAsync` (`conditions: null`) | ScopeKey 없는 엔티티의 전체 리스트 | **T0 — 흡수** |
| `PlayerComponent.TryGetByAccountIdAsync` | 비-ScopeKey 컬럼 → 엔티티 | **T2** |
| `WorldStageComponent.GetTotalStarAsync` (`SUM`) | 집계 스칼라 | **T3** |

→ 5.2.3이 "Component 상당수에서 반복"이라 했지만, 분류하면 `[Entity]` 도입만으로 절반이 사라지고 **진짜 특수한 것은 2건**이다.

**캐시 정책은 5종이며 `[Entity]`가 선언한다.**

현재 계열별 베이스 클래스가 서로 다른 정책을 갖고 있다(5.3.2). 이를 메타데이터로 통일하되, **정책 종류를 명시적으로 열거**한다 — `DataSet<T>`가 리스트 캐시 하나만 전제하면 Auth/Center 계열이 표현되지 않는다.

| 현재 위치 | 정책 | `[Entity]` 표현 |
|---|---|---|
| `UserComponentBase` | 소유자별 리스트 캐시 | `Cache = OwnerList` |
| `AuthComponentBase.GetMdlAsync` | 캐시 없음 | `Cache = None` |
| `AuthComponentBase.GetMdlWithCacheAsync` | 단건 캐시 + **sliding TTL** | `Cache = Single, SlidingTtl = true` |
| `CenterComponentBase` | 캐시 없음 (매 요청 전체 조회) | **`Cache = GlobalList` — 캐싱 도입 확정** |
| `SessionComponent` | 단건 + **포인터 캐시** | `Cache = Single` + `[SecondaryIndex("Key", Cached = true)]` |

```csharp
public enum ECachePolicy { None, Single, OwnerList, GlobalList }
```

**sliding TTL은 세션 유지에 필수**다(`GetMdlWithCacheAsync`의 `slidingTtl` 인자 — 캐시 히트 시 TTL 갱신).

**단, 이 5종을 attribute로 올리는 시점은 S1이 아니라 S2다 (2026-08-12 정정).** 위 표의 마지막 행(`SessionComponent` = 단건 + 포인터 캐시)이 `Cache`/`SlidingTtl` 두 플래그로 표현되지 않는다는 것이 **열거가 아직 닫히지 않았다는 증거**다. `DataSet<T>`가 5종을 실제로 수용하는 형태를 코드로 확정한 뒤 attribute로 올린다. 그때까지 `[Entity]`에는 TODO 주석만 둔다. 이 표 자체는 **S2 설계가 만족해야 할 제약 목록**으로 유효하다.

**T2 — 신규 도입은 하지 않되, 이미 구현된 것은 유지한다.**
`TryGetByAccountIdAsync`의 결과는 T1 리스트 캐시와 **같은 엔티티**다. 여기에 값을 캐싱하면 사본이 둘이 된다. 정석 해법은 값이 아니라 **키만 캐싱**하는 것이다:

```
Account:{accountId} → playerId        ← 포인터만 (관계가 불변이라 무효화 거의 불필요)
        ↓
Set<PlayerModel>() 리스트 캐시         ← 엔티티 사본은 여전히 하나
```

다만 이 포인터 캐시는 **쓰기 경로 자동화**(`CreateAsync`/`DeleteAsync`가 `[SecondaryIndex]`를 보고 포인터를 함께 쓰고 지움)까지 갖춰야 성립하고, `PlayerComponent.TryGetByAccountIdAsync`는 로그인 경로에서만 쓰여 캐시 이득이 작다. **R7(관찰된 문제에만 구조로 답하라)을 적용해 `[SecondaryIndex]` 메타데이터 선언만 먼저 도입하고, 캐시 동작은 새로 만들지 않는다.** 조회는 지금과 동일하게 DB 직접이며, 달라지는 것은 이름 있는 정식 경로가 된다는 점이다.

> **⚠ 단, `SessionComponent`에는 이 패턴이 이미 완성되어 있다** — `AccountIdBySessionKey(key) → accountId` 포인터, `SessionByAccountId(accountId) → SessionModel` 값(sliding TTL), 키 로테이션 시 이전 포인터 invalidate, 로그아웃 시 두 키 제거까지. 여기에 "캐시 없음"을 적용하면 **매 요청의 세션 조회가 DB로 가는 명백한 후퇴**다.
> 따라서 이 결정은 **"신규 도입하지 않는다"로 한정**되며, Session의 포인터 캐시는 그대로 유지·이관한다(`[SecondaryIndex("Key", Cached = true)]`). 포인터 캐시를 다른 엔티티로 확대하는 것은 실제 부하가 관찰될 때 한다.

> 관계가 **가변**인 보조 인덱스(예: 길드원 목록)는 무효화 훅이 필요하므로 T2가 아니라 T3으로 분류한다. attribute에 이 구분을 명시한다.

**T3 — 캐시 금지가 기본, 그리고 숨기지 않는다.**
`SUM(RewardAmount)`는 `WorldStage` 행 하나만 바뀌어도 무효인데 `DataSet<WorldStageModel>.UpdateAsync`가 그것을 알 방법이 없다. **무효화 조건을 시스템이 추론할 수 없으면 캐싱하지 않는다.** 정말 필요하면 무효화 태그를 손으로 선언하게 한다:

```csharp
scope.Raw<int>(sql, args).CachedBy(CacheKeyTags.WorldStageModel, playerId)
```

T3을 `DataSet<T>` 확장 메서드로 **감싸지 않는다.** 감추면 호출부가 평범한 조회로 착각한다. `scope.Raw`라는 별도 진입점으로 노출해 코드 리뷰에서 드러나게 하는 것이 목적이다.

> **T3 실행 전 스코프 flush (필수).** 3.8의 dirty 모델에서 변경은 커밋까지 DB에 반영되지 않는다. `DataSet<T>` 조회는 스코프가 캐싱한 같은 인스턴스를 돌려주므로 무관하지만, **`scope.Raw`는 DB로 직행하므로 dirty 상태를 보지 못한다**:
>
> ```csharp
> worldStage.AddStar(3);                                   // 메모리에서만 변경 (dirty)
> var total = await user.Raw<int>("SELECT SUM(RewardAmount) FROM WorldStage ...");
> //                                                       ← 방금 더한 3이 빠진다
> ```
>
> 따라서 `scope.Raw`는 **실행 직전에 해당 스코프의 dirty를 flush**한다. MySQL 모드에서 `IDbSession`을 직접 쓰는 예외 경로도 같은 규칙을 따른다.

→ 이는 5.2.3을 "없앤다"가 아니라 **"탈출구를 정식 문으로 만들고 문패를 단다"**이다. 완전 제거는 불가능하며, 시도하면 더 나쁜 우회로가 생긴다.

---

## 4. 현재 구조(DbModel) 불편함 — 항목별 해소

| # | 불편함 | A안에서 어떻게 되는가 |
|---|---|---|
| 5.1.1 | Manager 유무 불균형 (14줄~412줄) | **Manager 개념 자체가 없음.** 로직 없는 엔티티는 파일도 없고, 로직 있는 엔티티는 Model partial 파일만 생긴다. 판단할 게 없어짐 |
| 5.1.2 | 모든 로드가 Manager 경유 → 1:1 가정이 리스트/운영툴과 충돌 | `DataSet<T>.GetListAsync()`가 `List<T>`(Model)를 그대로 반환. 래핑 개념이 없으므로 N개 벌크 처리에 아무 마찰 없음 |
| 5.1.3 | `Model` public getter로 캡슐화 미강제 | **부분 해소.** 필드 직접 대입은 여전히 가능(Dapper 리플렉션·직렬화 때문에 public setter 필요). 다만 "저장"은 반드시 `DataSet.UpdateAsync`를 거치고, ScopeKey 가드가 걸리므로 잘못된 저장은 막힌다. 완전 캡슐화는 비목표(8.5 참고) |
| 5.1.4 | Manager가 전체 Repo를 들고 있음 | Model은 DB 참조가 아예 없어 다른 엔티티에 접근 불가. 다중 Model 규칙은 `ObjectLedger` 같은 **이름 있는 도메인 서비스**로 승격 |
| 5.2.1 | 등록 누락이 런타임에만 터짐 | `[Entity]` attribute + 어셈블리 스캔. 등록이라는 수동 단계가 사라짐 |
| 5.2.2 | Component 4형식을 모델마다 손으로 반복 | `DataSet<T>` 하나로 끝. 서브클래스 불필요 |
| 5.2.3 | 표준 CRUD 밖 쿼리마다 캡슐화가 뚫림 | **3.9의 4티어로 분류.** 현 탈출구 4건 중 2건(`PlayerComponent.LoadFromDb` override, `ScheduleComponent.GetListAsync`)은 특수 쿼리가 아니라 메타데이터 부족이라 `[Entity]` 도입만으로 소멸(T0). 남는 2건은 T2(`TryGetByAccountIdAsync` — 보조 인덱스, 현 단계 캐시 없음)와 T3(`GetTotalStarAsync` — 집계, 캐시 금지 기본 + `scope.Raw`로 노출). **탈출구를 없애는 게 아니라 티어별로 이름과 캐시 규칙을 부여하는 것** |
| 5.3.1 | 인프라 그루핑 vs 도메인 그루핑 혼재 | `UserScope`/`AuthScope`/`CenterScope`는 **"어느 DB + 누구 소유"만 표현**한다고 정의. 도메인 그루핑 역할을 명시적으로 제거 → 처리 주체 후보에서 빠짐 |
| 5.3.2 | Auth/User 캐싱 정책 불일치 | `DataSet<T>`가 캐시 정책을 엔티티 메타데이터(`[Entity(Cache = ...)]`)로 통일 처리. 계열별로 다른 베이스 클래스가 없으므로 불일치 자체가 성립 불가 |
| 5.4 | **비즈니스 로직 처리 주체 불명확** | 3.6의 3줄 판정표 + Model에 DB 참조가 없다는 타입 제약으로 기계적으로 결정 |
| 5.5.1 | Get/Create/Update가 `RpcCtx.PlayerId`에 암묵 결속 | `GameDb.User(shardId, playerId)` — 대상이 명시적 인자. 운영툴/레이드/배치가 본편과 **완전히 같은 API** 사용 |
| 5.5.2 | `IGameContext`가 Component/Manager 깊숙이 박힘 | 데이터 계층이 `IGameContext`를 아예 모름. `RaidGameContext`가 인터페이스를 구현할 필요 자체가 없어지고, `Ip=""` 같은 스텁 문제도 소멸 |
| 5.5.3 | 네임스페이스가 `Server.*`/`WebStudyServer.*` | 신규 어셈블리(`GameData` 등 중립 이름)로 재구성하며 자연 해소 |
| 5.5.4 | 모델 등록이 프로세스마다 중복 | 어셈블리 스캔 1회. 프로세스별 목록 자체가 없음 |
| 5.6 | Delete 없음 / 부분 업데이트 없음 | `DataSet<T>.DeleteAsync` 추가. 부분 업데이트는 API 경계 문제로 보고 GSA의 `JsonPutEntity.ApplyTo` 패턴을 운영툴 계층에서 채택 |
| 5.7 | mdl/mgr 접두사 의존 | `mgr` 개념이 사라져 접두사가 1종(`mdl`)으로 줄거나 불필요해짐. (원래 무시하기로 한 항목) |

## 5. GSA(서버참고2) 불편점 — 항목별 해소

| # | GSA 문제 | A안에서 어떻게 되는가 |
|---|---|---|
| 3.1 | Repo가 "동사 축"으로 쪼개진 God 클래스 (`UserRepo.Update.cs` 2,425줄) | 엔티티별 CRUD 코드가 **아예 존재하지 않음**(`DataSet<T>` 제네릭 1개). 커질 파일 자체가 없음 |
| 3.2 | 모델마다 손으로 쓴 CRUD 중복, 필드 추가 시 2곳 수동 동기화 | 필드 매핑 코드 없음. 코드젠 모델 + attribute 메타데이터만으로 CRUD 성립 |
| 3.3 | Service가 raw Model+Proto를 파라미터로 실어나름 (파라미터 6~7개) | 로직이 Model 메서드/도메인 서비스로 이동해 파라미터 뭉치가 사라짐. Proto는 필요한 메서드에만 인자로 등장 |
| 3.4 | DbType 분기가 메서드마다 반복 | DI 시점 1회 스왑(현재 구조에서 이미 해결됨). A안도 엔진 계층을 그대로 재사용하므로 유지 |
| (GSA 장점) | `InitUserRepo(playerId)` — 임의 대상 Repo 팩토리 | **기본 진입점으로 승격.** GSA에서는 운영툴 전용 특수 경로였지만 A안에서는 유일한 경로 |
| (GSA 장점) | `JsonPutEntity<T>.ApplyTo` — 부분 필드 패치 | 운영툴 API 계층에서 그대로 채택 |

---

## 6. 트레이드오프 / 비목표

- **전면 재작성이다.** `DbModel`의 Repo/Component/Manager 전부와 그걸 호출하는 Service 전부가 바뀐다. 엔진 계층과 코드젠 모델은 유지되므로 "바닥부터"는 아니다. 실측 범위는 Component 18파일 997줄 + Manager 17파일 1,691줄 + 호출부 `Server/Service` 978줄 = **약 3,700줄**이며, `ServerTest`의 HTTP 레벨 통합 테스트 1,218줄이 그대로 회귀 안전망이 된다(7장). B안보다 크지만 "비교 불가"한 규모는 아니다.
- **완전한 캡슐화는 비목표.** Model은 Dapper 리플렉션·JSON 직렬화 대상이라 public setter가 필요하다. 필드 직접 대입을 막지 않는다.
- **App Service의 오케스트레이션 실수는 여전히 사람 책임이다.** 순서를 잘못 짜거나 원자성이 필요한 연산을 쪼개는 것 자체는 타입이 막지 못한다. 도메인 이벤트/사가 같은 장치는 이 규모에 과하다고 보고 도입하지 않는다.
- **비-Cash 재화의 DB 감사 원장은 비목표다.** DB 원장은 유료 재화(`FREE_CASH`/`REAL_CASH`/`TOTAL_CASH`)에만 둔다 — 환불·차지백·CS 분쟁이 걸리는 축이기 때문이다. EXP/GOLD/POINT/TICKET/ITEM/COOKIE 변동은 `ChangeSet`으로 반환되고 구조화 로그까지만 간다. 재검토 조건은 S0-3에 기록.
- **로직 많은 엔티티의 partial 파일은 여전히 커진다.** `KingdomMapModel.cs`는 지금 `KingdomMapManager`(412줄)와 비슷한 크기가 될 것이다. 나빠지진 않지만 해결되지도 않는다.
- **`DataSet<T>` 제네릭 + attribute 메타데이터는 리플렉션 의존도를 더 높인다.** 지금도 `DapperExtension`이 리플렉션 기반이지만, A안은 ScopeKey 스코핑·쓰기 가드까지 메타데이터로 처리하므로 "컴파일 타임에 안 보이는 규칙"이 늘어난다. 부팅 시 전량 검증(등록 누락·ScopeKey 필드 부재 즉시 실패)으로 완화한다.

---

## 7. 마이그레이션 계획

> **실코드 before/after는 `DbLayer_A_StepByStep.md`에 있다.** 이 장은 "무엇을 하는가"이고, 그 문서는 "하고 나면 코드가 어떻게 생겼는가"다. 실행 판단은 그쪽을 보고 한다.
>
> A안의 실행 단위. **엔티티 축 strangler** — 구조를 한 번에 바꾸지 않고, 새 계층을 옆에 세운 뒤 엔티티를 하나씩 옮기고 마지막에 구 계층을 철거한다.
> B안이 "결합 종류"를 축으로 슬라이스했다면 A안은 "엔티티"를 축으로 슬라이스한다. 슬라이스 가능성은 A안도 동일하게 확보된다.

### 7.1 전제 — 왜 병존 이관이 가능한가

**① 트랜잭션이 자동으로 하나로 유지된다.**
`DbSessionManager`는 열린 세션을 `Dictionary<string, IDbSession>`(키 = connectionString)로 관리한다. 따라서 `GameDb`와 `GlobalDbRepo`가 **같은 `DbSessionManager` 인스턴스를 주입받으면 같은 커넥션에 대해 같은 `IDbSession`을 받는다.**
→ 이관 중 한 요청이 구 경로(Component)와 신 경로(`DataSet<T>`)를 **섞어 써도 원자성이 깨지지 않는다.** 이것이 엔티티 단위 이관을 가능하게 하는 핵심 조건이며, 이미 충족돼 있다.

**② 커밋 지점은 이관 기간 내내 하나다.**
`GlobalDbRepo.CommitAsync`(DB 커밋 → 캐시 flush)가 계속 단일 커밋 주체다. `GameDb`는 이 기간 동안 **자체 커밋을 하지 않고 위임**한다. 주체 역전은 마지막 철거 단계(S11)에서 한 번에 한다.

**③ 캐시 키 포맷을 바꾸지 않는다.**
`DataSet<T>`는 이관 기간 동안 기존 `ListKeyFor` 포맷을 그대로 사용한다. → 스텝을 되돌려도 캐시 무효화가 필요 없다.

**④ 안전망이 데이터 계층에 결합돼 있지 않다.**
`ServerTest` 7파일 1,218줄이 전부 `WebApplicationFactory` + `POST /rpc/{protocol}` 방식이다. Repo/Component/Manager를 통째로 들어내도 테스트 코드는 한 줄도 바뀌지 않는다.

| 테스트 | 줄수 | 커버 |
|---|---|---|
| `KingdomTest` | 338 | Kingdom 4종 |
| `WorldTest` | 214 | World / WorldStage |
| `CookieTest` | 185 | Cookie / 재화 |
| `GachaTest` | 115 | PlayerDetail / 재화 라우팅 |
| `AuthTest` | 92 | Account / Device / Channel / Session |
| `CheatTest` | 79 | 재화 직접 증감 |
| `GameEnterTest` | 64 | Player / PlayerDetail 생성 |

**⑤ 두 번째 소비자의 접점이 좁다.**
`RaidServer`는 `Session`과 `Player` 딱 둘만 쓴다(`PlayerRaidSessionService.cs:42,57,58`). 5.5.2(RaidGameContext 스텁) 대응이 **스텝 하나(S7)로 격리**된다.

### 7.2 이관 대상 전량

| 계열 | 엔티티 | Component | Manager |
|---|---|---|---|
| Auth | Account, Channel, Device, PlayerMap, Session | 5 | 4 (PlayerMap 없음) |
| Center | Schedule | 1 | 1 |
| User | Cookie, Item, KingdomDeco, KingdomMap, KingdomStructure, PlacedKingdomItem, Player, PlayerDetail, Point, Ticket, World, WorldStage | 12 | 12 |
| (미사용) | CashChangeLog, GachaLog | 0 | 0 |
| **합계** | **20 모델** | **18** | **17** |

### 7.3 스텝 요약

| Phase | Step | 내용 | 완료 조건 | 롤백 비용 |
|---|---|---|---|---|
| **0 선결** | S0-1 | ~~3.8 저장 모델 확정~~ → **(c) dirty 플래그로 확정** | 3.8 재작성 완료 | — |
| | S0-2 | `ClassGenerator`의 PK/`ScopeKey` attribute 생성 가능 여부 확인 | **완료 — 가능**(StepByStep §4) | — |
| | S0-3 | **확정** — `ChangeSet` 존치(근거 교체: 와이어 계약 분리), 감사는 싱크별 개별 처리, 이름 `RewardHelper` | 3.5 재작성 완료 | — |
| | S0-4 | **완료** — 락 커넥션 분리(1bf5a39) + 커밋 경계를 유저 락 안으로(7aba510) | 빌드 + 전체 통과 | 순서 복원 |
| **1 기반** | S1 | `[Entity(Pk, ScopeKey)]` + `EntityRegistry` (기존 등록과 **병존·비교**) | 양 프로세스 부팅 + 전체 통과 | attribute 삭제 |
| | S2 | `GameDb`/`Scope`/`DataSet<T>`/`Utility` 신설 · **캐시 정책 5종 수용 형태 확정** · `ModelBase` dirty | 빌드 + 전체 통과 | 신규 파일 삭제 |
| | S3 | 엔진 계층 `DeleteAsync` 추가 (5.6) | 신규 단위 테스트 | 메서드 삭제 |
| **2 파일럿** | S4 | **Channel / Device / Account** | `AuthTest`+`GameEnterTest`, 구 클래스 3쌍 삭제 | **클래스 3쌍** ← 게이트 |
| **3 재화** | S5 | Point / Ticket / Item / Cookie + `ChangeSet` | `CookieTest` | 클래스 4쌍 |
| | S6 | PlayerDetail + 재화 도메인 서비스 추출 | `GachaTest`+`CheatTest` | 330줄 |
| **4 잔여** | S7 | **Player / Session** (+ RaidServer 동시 수정) · **T2 확정** | 전체 + RaidServer 확인 | 2쌍 + RaidServer |
| | S8 | Schedule / PlayerMap · **T0 확정** | 전체 통과 | 2쌍 |
| | S9 | World / WorldStage · **T3 확정** | `WorldTest` | 2쌍 |
| | S10 | Kingdom 4종 (628줄, 최대) | `KingdomTest` | 4쌍 |
| | S10.5 | `DbUtilityConnection` → `GameDb.Utility`로 감싸기 (커넥션 분리는 S0-4에서 완료) | 락 동작 확인 | 1파일 |
| **5 철거** | S11 | 구 계층 전량 삭제 + 커밋 주체 역전 + `AllUserRepo` → `GameDb.AllShards` | 전체 통과 | 되돌리기 불가 |
| | S12 | `IGameContext` 축소 / 수동 등록 목록 삭제 / 네임스페이스 | 전체 통과 | 기계적 |
| | S13 | 감사 로그 기록 (신규 기능) | 신규 테스트 | 기능 제거 |

### 7.4 스텝별 상세

#### Phase 0 — 선결 결정 (코드 변경 없음)

**S0-1. 3.8 저장 모델 — 확정됨 (c) dirty 플래그 + 커밋 시 flush**
도메인 메서드가 `MarkDirty()`로 자기 변경을 표시하고, `GameDb.CommitAsync`가 스코프의 dirty 엔티티를 순회하며 기존 `IRepository.UpdateAsync`로 쓴 뒤 tx commit → 캐시 flush 한다. 상세·근거·한계는 3.8 참조.

이 확정이 S2와 S5의 형태를 결정한다:
- **S2** — `DataSet<T>`에 `_loaded` 추적과 `FlushDirtyAsync`가 처음부터 들어간다.
- **S5** — 도메인 메서드가 저장 대신 `MarkDirty()`를 호출한다. App Service에 저장 코드가 없다.
- **S9** — `scope.Raw` 도입 시 실행 전 flush 규칙을 함께 넣는다(3.9).
- `ModelBase`에 `IsDirty`/`MarkDirty()`/`ClearDirty()` 추가가 **S2의 선행 작업**으로 들어간다.

**S0-2. `ClassGenerator` 확인**
PK/ScopeKey를 attribute로 함께 찍을 수 있으면 S1이 자동화된다. 불가하면 모델 20개에 손으로 붙인다 — 감당 가능하지만 **알고 시작한다.**

**S0-3. 감사·반환 타입 — 확정됨**

**① `ChangeSet`은 존치한다 — 단 근거가 바뀐다.** 도메인은 서버 런타임 전용 `ChangeSet(Type, Num, Before, After)`을 반환하고, App Service가 응답 경계에서 `ToPacket()`으로 매핑한다.
- **철회된 근거**: "한 값이 감사·로그·응답 세 곳을 동시에 만족시킨다" — 싱크마다 범위·shape이 다르므로 성립하지 않는다.
- **존치 근거**: `ChgObjPacket`은 `[ProtoContract]` 직렬화 계약이므로 도메인이 반환하면 와이어 변경이 도메인까지 파급된다. 또한 이 프로젝트는 이미 `_mapper.Map<CookiePacket>(model)`처럼 런타임 타입→패킷 매핑을 서비스 경계에서 하므로, ChangeSet을 두는 쪽이 **일관성이 맞다**. 상세는 3.5.
- **감사·로그는 여전히 싱크별 개별 처리**다. ChangeSet을 그대로 넘기지 않는다.

**② 싱크별 범위·shape.**

| 싱크 | 대상 범위 | shape |
|---|---|---|
| 응답 패킷 | `EObjType` 전 축 | `ChangeSet.ToPacket()` — 1:1 매핑 |
| 구조화 로그 | 필요한 곳 | App Service가 선택적으로 |
| `CashChangeLogModel` | **`FREE_CASH`/`REAL_CASH`/`TOTAL_CASH` 3종만** | **액션당 1행**에 3종 Cash가 나란히(`Chg/Bef/Aft/Acc` × 3) + `IapActionId` |
| `GachaLogModel` | 가차 1회 | 1행에 `ScheduleNum`/`Cnt` + Cash 2종 + 단일 `ChgObjType`/`ChgObjAmount` + `ExtraData` |

`Acc*`(누적)는 반환값에 넣지 않는다 — 모델의 파생 상태이므로 감사 기록 시점에 모델에서 읽는다.
`reason`은 지금처럼 인자로만 흐른다. `ActionName`/`ActionDetail`/`IapActionId`는 액션당 1개이므로 App Service가 기록 시점에 넘긴다.
킹덤 스냅샷·프로필 변경 등 `ObjKey`로 주소 지정이 불가능한 변경은 재화 반환값 대상이 아니며 각자의 응답 패킷으로 간다.

**③ `CashChangeLogModel`은 이름과 달리 유료 재화 전용 원장이다** (EXP/GOLD/POINT/TICKET/ITEM/COOKIE 컬럼이 없다). 따라서:

- **비-Cash 재화에 DB 감사 테이블이 없는 것은 의도된 설계다 (확인됨).** 유료 재화만 환불·차지백·CS 분쟁 때문에 DB 원장이 필요하고, 비-Cash 재화(EXP/GOLD/POINT/TICKET/ITEM/COOKIE)는 구조화 로그로 충분하다. 가차 10연차 1회에 수십 행이 쌓이는 쓰기 부하를 피하는 선택이기도 하다. **누락이 아니므로 S13에서 테이블을 신설하지 않는다.**
  > *재검토 조건(R2)*: 비-Cash 재화가 ① 거래/양도 가능해지거나 ② 유료 재화와 교환 가능해지거나 ③ CS가 아이템 단위 분쟁 해결(지급 취소·복구)을 요구하게 되면, 그 재화에 한해 DB 원장이 필요해진다. 그때 `CashChangeLogModel`을 일반화하지 말고 **해당 재화 전용 테이블을 따로 만든다** — 이 테이블의 액션당 1행 shape은 Cash 3종이 항상 함께 움직인다는 전제 위에 있고, 그 전제는 다른 재화에 성립하지 않는다.

**④ 3.6의 이름**은 실체가 "ObjKey → 담당 Model 라우팅 + 증감"이므로 `ObjectLedger`가 아니라 **`RewardHelper`** 로 한다. "전량 검증 후 일괄 적용"이 실제로 들어가는 시점에 Ledger로 승격한다.

**S0-4 (코드 선행 작업). 커밋 경계를 유저 락 안으로 이동 — 확정**
`RpcService.HandleMethodAsync`가 쓰기를 `RunAtomicAsync` 안에서, 커밋을 락 밖에서 한다. dirty 모델(3.8)은 쓰기 전체를 커밋으로 옮기므로 그대로 두면 **lost update가 발생한다**(상세: 3.8, StepByStep 5.1).
**A안 착수 전 별도 커밋으로 선행한다.** 현재 코드도 커밋이 락 밖이라 잠재 위험이 있고, 이 수정은 A안과 무관하게 그 자체로 이득이다. dirty를 켜기 전에 안전망을 먼저 확보한다. 응답 캐시 쓰기가 커밋 flush보다 앞이어야 한다는 불변식(StepByStep 5.7)을 유지한다.

#### Phase 1 — 기반 신설 (동작 변화 0)

**S1. `[Entity]` + `EntityRegistry.ScanAndRegister`**
기존 `ModelRegistration.Init<T>` 목록을 **지우지 않고 병존**시킨다. 부팅 시 스캔 결과와 수동 목록을 비교해 불일치하면 즉시 실패시킨다.
→ 이 비교 자체가 **5.5.4(Server와 RaidServer의 목록이 조용히 어긋남)를 즉시 검출하는 장치**다. 스캔이 정답이라는 게 요점이 아니라, 두 목록이 다르다는 사실이 곧 버그 리포트다. 수동 목록은 S12에서 지운다.

**S2. `GameDb` / `Scope` / `DataSet<T>` / `Utility` 신설**
아무도 사용하지 않는 상태로 추가한다. `DbSessionManager`·`ICacheSession`을 `GlobalDbRepo`와 **같은 DI 스코프에서 공유**하도록 등록한다(7.1-①). tx commit은 `GlobalDbRepo`에 위임하고, dirty flush만 `GlobalDbRepo.CommitAsync` 직전에 연결한다.

함께 들어가는 것:
- `ModelBase`에 `IsDirty`/`MarkDirty()`/`ClearDirty()` — `MarkDirty()`가 `UpdateTime`도 찍는다(StepByStep 5.6)
- `GameDb.Utility` — 락 등 엔티티와 무관한 쿼리 전용, **flush하지 않는 경로**(StepByStep 5.2)
- **커넥션 지연 오픈 (결정됨)** — `GameDb.User(shardId, playerId)`는 스코프만 만들고, `DataSet<T>`의 **첫 조회 시점에** `DbSessionManager.Open`을 호출한다. 현재 `RepoBase` 생성자가 `PrepareComp()`를 부르며 즉시 여는 구조(`20260720` 이슈 ②)를 여기서 해소한다. 스코프를 만들고 쓰지 않는 분기에서 커넥션 낭비가 사라진다.

**S3. 엔진 계층 `DeleteAsync`**
5.6의 절반. `Server.Tests` 프로젝트가 현재 비어 있으므로 **여기서 첫 단위 테스트를 만든다.**

#### Phase 2 — 파일럿

**S4. Channel / Device / Account ← 의사결정 게이트**
가장 단순한 셋을 고른다: Manager가 각각 14/14/20줄로 본문이 사실상 없고, `AuthComponentBase`는 캐시를 건드리지 않아(DB only, 5.3.2) 캐시 호환 리스크가 0이다. 호출부는 `AuthService` 107줄뿐이다.

**이 스텝의 목적은 이관이 아니라 `DataSet<T>` 설계의 실전 검증이다.** 설계가 안 맞으면 여기서 A안을 중단하고 B안을 재검토한다 — 손실은 클래스 3쌍이다.

#### Phase 3 — 재화 축 (A안 핵심의 검증)

**S5. Point / Ticket / Item / Cookie + `ChangeSet` 도입**
Manager 43/42/38/83줄. 이들의 로직은 전부 "검증 → 필드 변경 → 저장" 형태라 **저장 호출을 `MarkDirty()`로 바꾸면 그대로 순수해진다.** 재화 메서드는 `ChangeSet`을 반환하고, App Service가 응답 경계에서 `ToPacket()`으로 매핑한다(3.5). S0-1에서 확정한 dirty 모델(3.8)이 여기서 처음 실전 적용되며, **App Service에는 저장 코드가 없다.**

**S6. PlayerDetail 분해 + 재화 도메인 서비스 추출**
`PlayerDetailManager` 330줄을 둘로 가른다:
- `EObjType` 라우팅 → **순수 함수**(3.6). 3.6 스케치의 `Func<int, PointModel>` 지연 로드는 쓰지 않는다. 대신 App Service가 순서를 지킨다:
  `Roll/Cost 계산(순수, DB 불필요)` → `필요 ObjKey 집합 확정` → `벌크 로드` → `순수 적용`
  이 순서면 도메인이 DB를 다시 알게 되는 경로가 생기지 않는다.
- EXP/GOLD/CASH 등 PlayerDetail 자신의 필드 로직 → `PlayerDetailModel` partial 메서드

**여기까지 통과하면 0.6의 "유일한 신규 결정"(Model에 DB 참조 없음)이 검증된 것이다.** 5.1.4와 5.4가 실제로 닫혔는지 확인하는 지점이며, A안 전체에서 가장 중요한 스텝이다.

#### Phase 4 — 잔여 이관

**S7. Player / Session (+ RaidServer)**
`RaidServer`가 쓰는 유일한 두 엔티티. `GameDb.User(shardId, playerId)`의 `shardId`는 `PlayerMap`에서 조회한다 — GSA `PlayerMapService.TryGetUserRepoByPlayerId` 패턴의 복원이다(0.6).
**5.5.2가 여기서 닫힌다.** `RaidGameContext`의 `Ip=""` 스텁이 제거되며, `SessionModel.PublicIp`에 실제 IP가 들어가는지 반드시 확인한다(현재 조용히 빈 값이 저장될 수 있는 경로).
**T2 확정 지점이기도 하다.** `PlayerComponent.TryGetByAccountIdAsync`가 `[SecondaryIndex("AccountId")]` + `set.ByIndexAsync(...)`로 바뀐다. **캐시 동작은 도입하지 않는다** — 조회는 지금과 동일하게 DB 직접이고, 달라지는 것은 이름 있는 정식 경로가 된다는 점뿐이다(3.9).

**S8. Schedule / PlayerMap — T0 + `GlobalList` 캐싱 확정**
`ScheduleComponent.GetListAsync`는 특수 쿼리가 아니라 **ScopeKey 없는 엔티티의 전체 리스트**다(3.9). `[Entity(Pk="Num", Cache = GlobalList)]`로 흡수되며, 현재의 `DbSession` 직접 사용(*"일반화가 어려운 부분이라"*)이 사라진다. **T0(메타데이터 흡수) 패턴이 여기서 확정된다.**

**캐싱 도입 확정 (결정됨).** 현재 Center 계열은 캐시가 전혀 없어 매 요청마다 Schedule 테이블을 전량 조회한다. 스케줄은 거의 변하지 않으므로 캐싱 이득이 크다. **대신 무효화 경로가 필요하다** — 스케줄 갱신은 운영툴/배치에서 일어나므로, 이 스텝에 다음을 포함한다:
- `GlobalList` 캐시 키를 **샤드/오너와 무관한 전역 키**로 정의
- 갱신 경로(`CreateAsync`/`UpdateAsync`/`DeleteAsync`)가 전역 리스트를 갱신하거나 무효화
- **운영툴처럼 다른 프로세스가 DB를 직접 고치면 캐시가 어긋난다.** TTL(`CacheDefaultTtl`)을 상한으로 두고, 즉시 반영이 필요하면 명시적 무효화 API를 노출한다. 이 한계를 문서에 남긴다.

**S9. World / WorldStage — T3 확정**
`WorldStageComponent.GetTotalStarAsync`(`SELECT SUM(...)`, 현재 `TODO: 캐시`)는 **T3(원시 쿼리)**의 대표 사례다. `scope.Raw<T>(sql)` 진입점을 여기서 도입하고 **캐시 금지를 기본값으로 확정**한다. `DataSet<T>` 확장 메서드로 감싸지 않는다(3.9).

**S10. Kingdom 4종 (628줄, 최대)**
`KingdomMapManager.ConstructStructureAsync(KingdomStructureManager, ...)`처럼 **다른 Manager를 인자로 받는 메서드**가 있다 — 여러 Model에 걸친 불변식이므로 도메인 서비스 후보다(3.6 판정표 적용 사례).
스냅샷(`KingdomMapSnapshotPacket`)은 `ObjKey`로 주소 지정이 불가능하므로 **재화 반환값 대상이 아니며** 자기 응답 패킷으로 간다(S0-3).

#### Phase 5 — 철거 및 마감

**S11. 구 계층 전량 삭제**
`GlobalDbRepo` / `UserRepo` / `AuthRepo` / `CenterRepo` / Component 18 / Manager 17 / `*ComponentBase` / `ManagerBase`.
`AllUserRepo`(전 샤드 접근)는 **삭제하지 않고 이관**한다 — 이미 있고 실제로 쓰이는 것이므로(0.7). 다만 `TryGetPlayerByNameAsync`는 전 샤드 순회라 단일 샤드 전제인 `UserScope`에 맞지 않으므로, **스코프 밖 진입점** `GameDb.AllShards.FindPlayerByNameAsync(name)`으로 정의한다(T3 성격, 캐시 없음 — 현재도 `TODO: 캐시`). 이 김에 전 샤드 커넥션 즉시 오픈(`UserDbConnectionStrList.Select(Open)`)을 **지연 오픈**으로 바꾼다(`20260720` 이슈 ②).
커밋 주체를 `GameDb.CommitAsync`로 역전한다.

**S12. 잔여 정리**
`IGameContext`를 Transport 전용으로 축소(데이터 계층의 `ShardId`/`PlayerId` 소비처가 사라진 상태) → 5.5.2 완결. S1의 부팅 비교 assert와 수동 `ModelRegistration.Init` 목록 삭제 → 5.5.4 완결. 네임스페이스 정리 → 5.5.3 완결.

**S13. 감사 로그 — 리팩터링이 아니라 신규 기능**
`ChangeSet`이 S5에서 이미 존재하므로 **소비처만 추가**한다. 단 싱크마다 범위와 shape이 다르므로 그대로 넘기는 게 아니라 **각 App Service가 개별 조립**한다(3.5, S0-3):

- **구조화 로그** — 필요한 곳에서 App Service가 찍는다.
- **`CashChangeLogModel`** — Cash를 변경하는 App Service가 **액션당 1행**을 조립한다(`Chg/Bef/Aft` × 3종). `Acc*`는 `PlayerDetailModel`에서 읽고, `ActionName`/`ActionDetail`/`IapActionId`는 액션 컨텍스트로 넘긴다. Cash 변경 지점이 `PlayerDetailModel.DecCash`/`IncCash` 소수로 한정되므로 호출부를 리뷰로 커버한다.
- **`GachaLogModel`** — 가차 1회 = 1행. 축이 다르므로 가차 App Service가 직접 조립한다.

**신설 테이블 없음.** 비-Cash 재화(EXP/GOLD/POINT/TICKET/ITEM/COOKIE)에 DB 감사 테이블이 없는 것은 의도된 설계이므로(S0-3) 이 스텝은 **기존 테이블 2개에 기록을 붙이는 작업만** 한다. 비-Cash 재화는 구조화 로그가 종착지다.

G4가 여기서 달성된다.

### 7.5 진행 원칙

- **한 스텝 = 한 커밋.** 스텝 중간 상태로 커밋하지 않는다.
- **매 스텝의 완료 조건에 `ServerTest` 전체 통과가 포함된다.** 실패하면 스텝을 되돌린다.
- **의사결정 게이트는 S4 하나다.** 그 이후는 되돌리는 비용이 급격히 커지므로 A안 지속 여부를 S4에서 판단한다.
- 스텝을 건너뛰지 않는다. 단 **S8과 S9는 순서 교환 가능**하다.
- 각 스텝 종료 시 4장 표에 해당 항목이 실제로 닫혔는지 체크를 남긴다.

### 7.6 스텝 ↔ 해소 항목

| 5장 항목 | 닫히는 스텝 |
|---|---|
| 5.2.1 등록 누락 런타임 폭발 | S1 (검출) → S12 (완결) |
| 5.6 Delete 없음 | S3 |
| 5.1.1 Manager 유무 불균형 | S4 (첫 삭제) → S11 (완결) |
| 5.1.2 1:1 래핑 가정 | S4 |
| 5.2.2 Component 4형식 반복 | S4 → S11 |
| 5.3.2 Auth/User 캐싱 정책 불일치 | S4 (Auth) + S5 (User) |
| **5.1.4 Manager가 전체 Repo 보유** | **S6** |
| **5.4 비즈니스 로직 처리 주체** | **S6** |
| G4 감사 로그 미구현 | S5 (`ChangeSet` 도입) → S13 (기록) |
| 5.5.1 `RpcCtx.PlayerId` 암묵 결속 | S7 |
| **5.5.2 `IGameContext` 앰비언트** | **S7** (스텁 제거) → S12 (완결) |
| 5.2.3 캡슐화 탈출구 | S1 (T0 절반 소멸) → S7 (T2) → S8 (T0) → S9 (T3) |
| 5.3.1 인프라/도메인 그루핑 혼재 | S11 |
| 5.5.3 네임스페이스 | S12 |
| 5.5.4 등록 프로세스마다 중복 | S12 |
