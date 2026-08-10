# DbLayer 재설계 — B안: 기존 구조 개선 (Repo / Component / Manager 유지)

작성일: 2026-08-09
관련 노트: `Docs/DevelopNote/20260809_dbmodel_repo_component_manager_review.md` (문제 정의)
대응 문서: `DbLayer_A_NewStructure.md` (신규 구조안)

> 지금의 `GlobalDbRepo → AuthRepo/CenterRepo/UserRepo → Component → Manager` 3단 구조와
> 이름·계층을 **그대로 유지**하면서, 확인된 불편함을 하나씩 제거하는 안.
> 호출부(Service) 코드가 거의 그대로 남는 것이 A안 대비 최대 장점.

---

## 0. 설계 방침

- **계층 이름과 개수를 바꾸지 않는다.** Repo/Component/Manager는 그대로 남는다.
- **바꾸는 것은 세 가지 결합뿐이다.**
  1. Component/Manager ↔ `IGameContext` 결합을 끊는다 (→ 스칼라 인자)
  2. Manager ↔ 전체 Repo 결합을 끊는다 (→ 자기 Component만)
  3. 엔티티 ↔ 손으로 쓴 Component 클래스 결합을 끊는다 (→ 제네릭 기본 클래스)
- 나머지(캐시 계층, 트랜잭션 경계, `IRepository`, 엔진 계층)는 손대지 않는다.

---

## 1. 변경 항목

### 1.1 `IGameContext` 제거 → 스칼라 주입 (5.5.1 / 5.5.2)

```csharp
// Before
public UserRepo(IGameContext rpcContext, IRepository repository) : base(rpcContext.ShardId, repository)

// After — DbModel은 "누가 요청했는지" 모른다
public UserRepo(int shardId, ulong playerId, IRepository repository) : base(shardId, repository)
```

`GlobalDbRepo`도 동일하게 `IGameContext` 의존을 버리고, 대상을 인자로 받는다:

```csharp
public UserRepo BeginUserRepo(int shardId, ulong playerId);   // 운영툴·레이드·배치·본편 공용
public AuthRepo Auth  { get; }                                 // 기존 Lazy 유지
public CenterRepo Center { get; }
```

호출부(RPC 파이프라인)가 `_dbRepo.BeginUserRepo(RpcContext.ShardId, RpcContext.PlayerId)`로 부른다.
→ GSA의 `InitUserRepo(playerId)` 패턴이 **별도 장치 없이 그대로 성립**한다.

`UserComponentBase.LoadFromDb`의 `WHERE PlayerId = RpcCtx.PlayerId`도 `_userRepo.PlayerId`를 쓰도록 바꾼다.

### 1.2 제네릭 `Component<T>` — 서브클래스는 필요할 때만 (5.2.2)

```csharp
public class Component<T> where T : ModelBase, new()
{
    protected readonly UserRepo _repo;          // 소속 Repo (ShardId/PlayerId 보유)
    protected readonly IRepository _repository;

    public Task<List<T>>                GetMdlListAsync(Func<T, bool> predicate = null);
    public Task<(bool Found, T? Value)> TryGetMdlAsync(Func<T, bool> predicate);
    public Task<T>                      CreateMdlAsync(T entity);
    public Task                         UpdateMdlAsync(T entity);
    public Task                         DeleteMdlAsync(T entity);       // 5.6
}
```

- `UserRepo.Of<T>()`가 스코프 내에서 인스턴스를 캐싱해 반환한다.
- **특수 쿼리나 도메인 로직이 있는 엔티티만** `class ItemComponent : Component<ItemModel>`로 서브클래스를 만든다. `AccountComponent`/`DeviceComponent`/`PlayerMapComponent`처럼 4형식뿐인 것들은 파일이 사라진다.
- `PrepareComp()`는 서브클래스가 있는 것만 등록하면 되므로 줄어들고, 없는 엔티티는 `Of<T>()`로 자동 처리된다.

### 1.3 Manager는 자기 Component만 받는다 (5.1.4 / 5.4)

```csharp
// Before — 전체 Repo. 다른 모든 엔티티에 접근 가능
public ItemManager(UserRepo userRepo, ItemModel model)

// After — 자기 Component만. 구조적으로 자기 밖으로 못 나감
public ItemManager(ItemComponent component, ItemModel model)
```

**여러 Model에 걸친 로직은 Manager에서 뺀다.** 현재 `PlayerDetailManager`가 `_userRepo.Point`/`.Ticket`/`.Cookie`/`.Item`을 전부 찌르고 있는데, 이건 사실 "PlayerDetail의 Manager"가 아니라 **재화 원장**이라는 별개 개념이다. `DbModel/Service/ObjectLedgerService.cs`로 승격한다:

```csharp
// 여러 Component를 조율하는 유일한 자리 — Manager가 아니다
public class ObjectLedgerService
{
    public ObjectLedgerService(UserRepo userRepo) { ... }   // 여기서만 전체 Repo 접근 허용

    public Task<List<ChgObjPacket>> PayAsync(ObjValue cost, string reason);
    public Task<List<ChgObjPacket>> GrantAsync(List<ObjValue> rewards, string reason);
}
```

**판정 규칙**
| 이 연산이... | 소속 |
|---|---|
| Model 하나의 필드만 바꾼다 | Manager |
| Model 여러 개를 조율한다 | `DbModel/Service`의 도메인 서비스 |
| 요청 흐름·응답 조립·로깅이다 | `Server/Service`의 App Service |

Manager 생성자가 `Component<T>` 하나만 받으므로, "여러 Model을 만지고 싶으면 애초에 컴파일이 안 된다".

### 1.4 Manager는 로직이 있을 때만 만든다 (5.1.1 / 5.1.2)

- `ChannelManager`/`DeviceManager`(본문 없음)는 삭제. `Component<T>`가 `ChannelModel`을 직접 반환한다.
- 리스트 조회는 `GetMdlListAsync()`가 `List<T>`(Model)를 그대로 반환한다. **N개를 전부 Manager로 감싸지 않는다.** 필요할 때만 호출부가 `comp.Wrap(model)`로 감싼다.
  → "모든 로드가 Manager를 거쳐야 한다"는 1:1 가정이 깨지고, 운영툴·벌크 처리와의 충돌이 사라진다.

### 1.5 등록 자동화 (5.2.1 / 5.5.4)

```csharp
[Entity(Table = "Item", Pk = new[] { "PlayerId", "Num" }, Owner = "PlayerId")]
public partial class ItemModel : ModelBase { }
```

부팅 시 `ModelRegistration.ScanAndRegister(typeof(ItemModel).Assembly)` 한 줄로 대체한다.
`StartUp.Resource.cs`의 19줄짜리 `Init<T>()` 목록과 `RaidServer` 쪽 복붙본이 함께 사라진다.
(코드젠이 PK를 이미 알고 있으므로 attribute까지 생성하게 하면 손으로 쓸 것도 없다.)

### 1.6 Owner 가드 (5.5.1의 잠재 버그)

`Component<T>.CreateMdlAsync`/`UpdateMdlAsync`에서 `entity`의 Owner 필드(`PlayerId`)가 `_repo.PlayerId`와 다르면 즉시 예외. 지금은 어긋나도 DB엔 정상 저장되고 캐시만 엉뚱한 버킷에 쓰이는데, 이를 구조적으로 차단한다.

### 1.7 로깅/감사 — 변경 내역 반환 (미구현 기능 채우기)

현재 `DbModel`에는 비즈니스 로깅이 전혀 없고, `CashChangeLogModel`/`GachaLogModel`은 등록만 되고 아무도 쓰지 않는다. Manager/도메인 서비스가 이미 반환하고 있는 `ChgObjPacket`(Type/Num/Amount/TotalAmount)과 `reason` 인자를 **감사 기록의 정식 입력**으로 규정하고, App Service가 한 곳에서 처리한다:

```csharp
var changes = await ledger.PayAsync(cost, reason);
await _audit.WriteCashChangeAsync(userRepo, changes);   // CashChangeLogModel 기록
_logger.LogChanges(changes);                            // 구조화 로그
return changes;                                         // 응답 패킷
```

Manager 안에서 로깅하지 않는다(컨텍스트를 모르므로). **"뭐가 변했는지"를 반환하고 로깅은 위에서** — A안과 동일한 원칙이며, B안에서도 그대로 적용 가능하다.

### 1.8 네임스페이스 정리 (5.5.3)

`namespace Server.Repo` / `WebStudyServer.*` → `namespace DbModel.*` 등 중립 이름으로. `RaidServer`가 `using Server.Repo;`를 적을 이유가 없어진다. 기계적 치환이라 리스크가 낮다.

---

## 2. 현재 구조(DbModel) 불편함 — 항목별 해소

| # | 불편함 | B안에서 어떻게 되는가 | 해소도 |
|---|---|---|---|
| 5.1.1 | Manager 유무 불균형 | 1.4 — 로직 없으면 Manager를 안 만든다. 빈 Manager 파일 삭제 | ○ |
| 5.1.2 | 모든 로드가 Manager 경유 → 1:1 가정 충돌 | 1.4 — `GetMdlListAsync()`가 `List<T>`(Model) 반환. 래핑은 선택 | ○ |
| 5.1.3 | `Model` public getter로 캡슐화 미강제 | 변화 없음. Dapper 리플렉션·직렬화 때문에 public setter 유지 필요 | △ |
| 5.1.4 | Manager가 전체 Repo를 들고 있음 | 1.3 — 생성자가 자기 Component만 받음. 다중 Model은 도메인 서비스로 | ○ |
| 5.2.1 | 등록 누락이 런타임에만 터짐 | 1.5 — attribute + 어셈블리 스캔 | ○ |
| 5.2.2 | Component 4형식을 모델마다 손으로 반복 | 1.2 — `Component<T>` 제네릭. 서브클래스는 필요할 때만 | ○ |
| 5.2.3 | 표준 CRUD 밖 쿼리마다 캡슐화가 뚫림 | 서브클래스에서 `DbSession` 직접 사용은 유지. **"서브클래스가 있다는 것 자체가 특수 케이스 표시"**가 되어 의도는 명확해지지만, 탈출구 자체는 남음 | △ |
| 5.3.1 | 인프라 그루핑 vs 도메인 그루핑 혼재 | Repo 3종을 "커넥션 그루핑"으로 문서상 규정하고, 조율 로직은 도메인 서비스로 보내 Repo가 처리 주체 후보에서 빠지게 함. 다만 클래스 자체는 남음 | △ |
| 5.3.2 | Auth/User 캐싱 정책 불일치 | `Component<T>` 하나로 합치면서 캐시 정책을 엔티티 메타데이터로 통일 (별도 작업) | ○ |
| 5.4 | **비즈니스 로직 처리 주체 불명확** | 1.3의 판정표 + Manager 생성자 타입 제약으로 결정. 단, Manager가 여전히 저장을 수행하므로 "순수 로직/저장"의 경계는 A안만큼 선명하지 않음 | ○ |
| 5.5.1 | `RpcCtx.PlayerId` 암묵 결속 | 1.1 — `BeginUserRepo(shardId, playerId)`. 운영툴·레이드가 본편과 같은 API 사용 | ◎ |
| 5.5.2 | `IGameContext`가 깊숙이 박힘 | 1.1 — DbModel이 `IGameContext`를 모름. `RaidGameContext` 스텁 문제 소멸 | ◎ |
| 5.5.3 | 네임스페이스가 `Server.*` | 1.8 — 기계적 치환 | ◎ |
| 5.5.4 | 모델 등록 프로세스마다 중복 | 1.5 — 스캔 1회 | ◎ |
| 5.6 | Delete 없음 / 부분 업데이트 없음 | `DeleteMdlAsync` 추가. 부분 업데이트는 운영툴 API 계층에서 GSA `JsonPutEntity.ApplyTo` 채택 | ○ |
| 5.7 | mdl/mgr 접두사 의존 | 유지 (원래 무시하기로 한 항목) | — |

범례: ◎ 완전 해소 / ○ 해소 / △ 부분 해소

## 3. GSA(서버참고2) 불편점 — 항목별 해소

| # | GSA 문제 | B안에서 어떻게 되는가 |
|---|---|---|
| 3.1 | Repo가 "동사 축" God 클래스 (`UserRepo.Update.cs` 2,425줄) | 이미 해결됨(엔티티 축 Component). B안은 `Component<T>` 도입으로 파일 수 자체를 더 줄인다 |
| 3.2 | 모델마다 손으로 쓴 CRUD 중복 | 이미 해결됨(`DapperExtension` 리플렉션). B안은 Component 계층의 잔여 반복까지 제거 |
| 3.3 | Service가 raw Model+Proto를 파라미터로 실어나름 | Manager가 Model을 감싸 완화된 상태 유지. 다중 Model 조율이 도메인 서비스로 이름을 얻으면서 파라미터 뭉치가 더 줄어듦 |
| 3.4 | DbType 분기가 메서드마다 반복 | 이미 해결됨(DI 시점 스왑). 변화 없음 |
| (GSA 장점) | `InitUserRepo(playerId)` 임의 대상 팩토리 | 1.1의 `BeginUserRepo(shardId, playerId)`로 **기본 진입점 승격** |
| (GSA 장점) | `JsonPutEntity<T>.ApplyTo` 부분 패치 | 운영툴 API 계층에서 그대로 채택 |

---

## 4. 트레이드오프 / 남는 것

- **Manager가 여전히 저장을 수행한다.** `ItemManager.DecAmountAsync`는 검증+필드변경+`UpdateMdlAsync`를 한 메서드에서 계속 한다. 따라서 **비즈니스 로직 단위 테스트에는 여전히 DI/DB(InMemory) 세팅이 필요**하다. A안이 얻는 "순수 도메인 = DB 없이 테스트" 이점은 B안에는 없다.
- **계층이 하나 더 많은 상태가 유지된다.** Repo/Component/Manager/도메인서비스/AppService — A안(Scope/DataSet/Model/도메인서비스/AppService)과 개수는 비슷하지만, Component와 Manager가 둘 다 Model을 감싸는 성격이라 "왜 둘인가"에 대한 답은 계속 필요하다.
- **5.2.3(탈출구)·5.3.1(Repo 성격)은 부분 해소에 그친다.** 구조를 유지하는 대가.
- **대신 마이그레이션 비용이 압도적으로 낮다.** 호출부(Service) 코드 대부분이 그대로 살고, 1.1~1.8을 독립된 슬라이스로 나눠 하나씩 커밋할 수 있다. 각 단계마다 빌드·테스트가 통과하는 상태를 유지할 수 있다.

## 5. 슬라이스 제안 (순서)

각 단계가 독립적으로 빌드·테스트 통과 가능하도록 배열.

| 순서 | 슬라이스 | 비고 |
|---|---|---|
| 1 | 1.8 네임스페이스 정리 | 기계적, 리스크 최소 |
| 2 | 1.5 등록 자동화 (attribute + 스캔) | 코드젠 수정 동반 |
| 3 | 1.1 `IGameContext` 제거 → 스칼라 | 호출부(RPC/Raid) 동시 수정 |
| 4 | 1.2 `Component<T>` 도입 + 1.6 Owner 가드 + 1.4 빈 Manager 삭제 | 가장 큰 덩어리 |
| 5 | 1.3 Manager 생성자 축소 + `ObjectLedgerService` 분리 | `PlayerDetailManager`가 주 대상 |
| 6 | 1.7 감사 로그/`CashChangeLog` 기록 | 신규 기능 |
