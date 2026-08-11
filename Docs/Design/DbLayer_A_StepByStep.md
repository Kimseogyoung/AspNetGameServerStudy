# DbLayer A안 — 스텝별 실코드 before/after

작성일: 2026-08-11
상위 문서: `DbLayer_A_NewStructure.md` (설계 근거 §0 / 구성 요소 §3 / 마이그레이션 계획 §7)

> §7이 "무엇을 하는가"라면 이 문서는 **"하고 나면 코드가 어떻게 생겼는가"**다.
> 저장소의 실제 파일을 인용하고, 각 스텝 직후의 코드를 그대로 적는다.
> **1장(최종 형태)을 먼저 읽는다.** 목적지를 모르면 경로가 이해되지 않는다.

---

## 1. 최종 형태 — 전부 적용하면 이렇게 된다

### 1.1 파일 구조

**Before (현재 `Code/DbModel`, 79파일)**

```
DbModel/
  Base/         8   RepoBase, UserComponentBase, AuthComponentBase, CenterComponentBase,
                    ManagerBase, UserManagerBase, AuthManagerBase, ...
  Repo/         5   GlobalDbRepo, AuthRepo, CenterRepo, UserRepo, AllUserRepo
  Component/   18   AccountComponent, CookieComponent, ItemComponent, ...
  Manager/     17   AccountManager, CookieManager, KingdomMapManager, ...
  Model/       20   (generated)
  Helper/       6
  Extension/    2
```

**After**

```
DbModel/
  Data/         6   GameDb, Scope, DataSet<T>, EntityRegistry, EntityAttribute, RawQuery
    Queries/    ~5  T1 확장 메서드 — 특수 필터가 있는 엔티티만
  Domain/      ~16  CookieModel.Logic.cs 등 partial — 로직 있는 엔티티만
                    + RewardHelper.cs, ChangeSet.cs   ← ChangeSet은 서버 런타임 전용 (§3.5)
  Model/       20   (generated, [Entity] attribute 포함)
  Helper/       6
  Extension/    2
```

**핵심은 파일 수가 아니라 클래스 *종류*의 감소다.**

| | Before | After |
|---|---|---|
| 데이터 접근 클래스 종류 | 4종 (Repo / ComponentBase / Component / Manager) | **2종** (DataSet\<T\> / Model partial) |
| 엔티티별 CRUD 클래스 | **18개** | **0개** (제네릭 1개) |
| 엔티티별 로직 클래스 | 17개 (빈 것 포함) | ~15개 (로직 있는 것만) |
| 계열별 베이스 클래스 | 3종 (User/Auth/Center) | **0개** (메타데이터로 통일) |

### 1.2 대표 코드 — `CookieService.EnhanceCookieLvAsync`

Cookie 하나 + 재화 차감 + 변동 응답. 이 저장소에서 가장 대표적인 형태다.

**Before — `Code/Server/Service/CookieService.cs` 현재 코드**

```csharp
public async Task<CookieEnhanceLvResponsePacket> EnhanceCookieLvAsync(CookieEnhanceLvRequestPacket req)
{
    var mgrCookie = await OwnUser.Cookie.TouchAsync(req.CookieNum);
    var mgrPlayerDetail = await OwnUser.PlayerDetail.TouchAsync();
    var cfgLvCost = 10;

    var reason = $"ENHANCE_COOKIE_LV:{req.BefLv}~{req.AftLv}";
    var deltaLv = req.AftLv - req.BefLv;
    ReqHelper.ValidUnderFlowParam(deltaLv, "REQ_COOKIE_ENHANCE_DELTA_LV");
    ReqHelper.ValidContext(req.BefLv == mgrCookie.Model.Lv, "NOT_EQUAL_COOKIE_Lv", () => new { ... });
    var valCostObj = ReqHelper.ValidCost(req.CostObj, EObjType.POINT_COOKIE_LV, 0, deltaLv * cfgLvCost, reason);

    var resultCostObj = await mgrPlayerDetail.DecCostAsync(valCostObj, reason);
    await mgrCookie.EnhanceLvAsync(req.AftLv);

    return new CookieEnhanceLvResponsePacket
    {
        Cookie = _mapper.Map<CookiePacket>(mgrCookie.Model),
        ChgObj = resultCostObj,
    };
}

private UserRepo OwnUser => _dbRepo.OwnUser;
```

**이 코드에서 보이지 않는 것 4가지**

1. `OwnUser`가 어느 플레이어인지 — `RpcContext.PlayerId`에 암묵 결속(5.5.1)
2. `DecCostAsync` 안에서 **`PointModel`을 로드하고 저장까지** 한다 — 호출부에 `Point`라는 단어가 없다
3. `EnhanceLvAsync` 안에서도 저장한다 — 저장이 두 군데로 흩어져 있다
4. 총 DB 쓰기가 몇 번인지 이 메서드만 봐서는 알 수 없다

**After**

```csharp
public async Task<CookieEnhanceLvResponsePacket> EnhanceCookieLvAsync(
    int shardId, ulong playerId, CookieEnhanceLvRequestPacket req)
{
    var user = _db.User(shardId, playerId);          // 대상이 인자로 명시된다
    var cfgLvCost = 10;

    // ── 1) 로드 : 무엇을 읽는지 전부 드러난다
    var cookie = await user.Set<CookieModel>().TouchAsync(req.CookieNum);
    var detail = await user.Set<PlayerDetailModel>().GetOneAsync();
    var point  = await user.Set<PointModel>().TouchAsync((int)EObjType.POINT_COOKIE_LV);

    // ── 2) 순수 계산 : 여기서 DB 접근이 일어나지 않는다
    var deltaLv = req.AftLv - req.BefLv;
    ReqHelper.ValidUnderFlowParam(deltaLv, "REQ_COOKIE_ENHANCE_DELTA_LV");
    var reason = $"ENHANCE_COOKIE_LV:{req.BefLv}~{req.AftLv}";
    var cost   = ReqHelper.ValidCost(req.CostObj, EObjType.POINT_COOKIE_LV, 0, deltaLv * cfgLvCost, reason);

    var change  = RewardHelper.Pay(detail, point, cost);   // 순수 → ChangeSet 반환 + MarkDirty
    cookie.EnhanceLv(req.BefLv, req.AftLv);                // 순수 (검증 포함) + MarkDirty

    // ── 3) 로그만. 저장 코드는 없다 — 커밋 시 dirty flush (§3.8)
    _logger.LogChange(reason, change);

    return new CookieEnhanceLvResponsePacket
    {
        Cookie = _mapper.Map<CookiePacket>(cookie),   // 런타임 → 패킷 매핑 (기존 스타일)
        ChgObj = change.ToPacket(),                   // 동일. 도메인은 와이어 타입을 모른다 (§3.5)
    };
}
```

**무엇이 달라졌나**

| | Before | After |
|---|---|---|
| 대상 플레이어 | 앰비언트 (`RpcContext`) | **인자** — 운영툴·레이드가 같은 메서드를 쓴다 |
| `PointModel` 접근 | `DecCostAsync` 안에 숨음 | **로드 단계에 명시** |
| DB 쓰기 지점 | 메서드 2곳 + 그 내부 | **커밋 1곳** (dirty flush) |
| 저장 누락 | App Service가 기억해야 함 | **구조적으로 불가능** |
| 감사 로그 | 없음 | `_audit.Write` 한 줄 |
| 로직 테스트 | DI + DB 필요 | `cookie.EnhanceLv(1, 5)` 한 줄 |

> **저장 코드가 왜 없나 — §3.8 (c) dirty 플래그.**
> `cookie.EnhanceLv(...)` 내부의 `MarkDirty()`가 `IsDirty = true`를 세우고, `GameDb.CommitAsync`가 스코프의 dirty 엔티티를 순회하며 기존 `IRepository.UpdateAsync`(DB 쓰기 + 캐시 갱신)로 쓴다. App Service가 저장을 기억할 필요가 없다.
> `MarkDirty()`는 자기 필드를 세우는 것뿐이므로 **Model에 DB 참조가 없다는 원칙(§0.6)은 유지**된다.

`RewardHelper.Pay`가 `PointModel`을 인자로 받는 이유는 §3.6의 `Func<int, PointModel>` 지연 로드를 쓰지 않기 위해서다. 가차처럼 **보상 종류가 런타임에 정해지는 경우**는 4단으로 푼다:

```csharp
var rolled  = GachaRandom.Roll(prt, req.Cnt);                    // ① 순수 — DB 불필요
var keys    = rolled.Select(x => x.Key).Distinct();              // ② 필요 대상 확정
var loaded  = await user.LoadObjectsAsync(keys);                 // ③ 벌크 로드 1회
var changes = RewardHelper.Grant(loaded, rolled);                // ④ 순수 적용
```

### 1.3 소멸하는 것

```csharp
// ── UserRepo.PrepareComp()  : 11줄 수동 등록 → 전부 소멸
Player = new PlayerComponent(this, Repository);
PlayerDetail = new PlayerDetailComponent(this, Repository);
Point = new PointComponent(this, Repository);
// ... 8줄 더

// ── StartUp.Resource.cs : 19줄 목록 → 1줄
ModelRegistration.Init<AccountModel>("Id");
ModelRegistration.Init<CookieModel>("PlayerId", "Num");
// ... 17줄 더
                    ↓
EntityRegistry.ScanAndRegister(typeof(CookieModel).Assembly);

// ── ChannelManager.cs / DeviceManager.cs : 본문 없는 클래스 → 파일 삭제
public class ChannelManager : AuthManagerBase<ChannelModel>
{
    public ChannelManager(AuthRepo authRepo, ChannelModel model) : base(authRepo, model) { }
}

// ── UserComponentBase 의 상시 개방 탈출구 → 소멸
protected IDbSession DbSession => _repo.Db;
protected ICacheSession CacheLayer => _repo.Cache;

// ── 데이터 계층이 요청 컨텍스트를 쓰던 것 → 소멸 (AccountComponent.CreateAsync)
_authRepo.RpcContext.SetAccountId(mgrAccount.Id);
_authRepo.RpcContext.SetShardId(mgrAccount.Model.ShardId);
```

마지막 것이 특히 중요하다. 현재 `AccountComponent`는 컨텍스트를 **읽기만 하는 게 아니라 쓰기까지** 한다. 데이터 계층이 요청 상태를 변경하는 것이며, A안에서는 이 값이 반환값으로 올라가고 Transport가 자기 컨텍스트를 갱신한다.

---

## 2. 스텝별 before / after

각 스텝은 **건드리는 파일 → 실코드 → 직후 가능해지는 것 → 아직 안 되는 것** 순으로 적는다.

---

### S1 — `[Entity]` + `EntityRegistry` (병존 검증)

**건드리는 파일**: `Model/*.generated.cs` 20개(attribute 추가), `EntityRegistry.cs`(신규), `StartUp.Resource.cs`(검증 1줄 추가)

```csharp
// Before — StartUp.Resource.cs 에만 존재. 모델 파일에는 PK 정보가 없다.
ModelRegistration.Init<CookieModel>("PlayerId", "Num");

// After — 지식이 모델로 이동
[Entity(Table = "Cookie", Pk = new[] { "PlayerId", "Num" }, Owner = "PlayerId")]
public partial class CookieModel : ModelBase { }

[Entity(Table = "Account", Pk = new[] { "Id" })]              // Owner 없음
public partial class AccountModel : ModelBase { }

[Entity(Table = "Player", Pk = new[] { "Id" }, Owner = "Id")]  // Owner 컬럼명이 Id
public partial class PlayerModel : ModelBase { }
```

```csharp
// StartUp.Resource.cs — 기존 19줄은 그대로 두고 아래를 추가한다
EntityRegistry.ScanAndRegister(typeof(CookieModel).Assembly);
EntityRegistry.AssertMatches(ModelRegistration.Snapshot());   // 불일치 시 부팅 실패
```

**직후 가능해지는 것**
- `Server`와 `RaidServer`의 등록 목록이 어긋나면 **부팅이 즉시 실패**한다. 지금은 조용히 어긋난 뒤 해당 요청에서 `NOT_FOUND_QUERY_PARAM`으로 터진다(5.5.4).
- `PlayerModel`의 Owner가 `Id`라는 사실이 **데이터로** 표현된다 → `PlayerComponent.LoadFromDb` override의 존재 이유가 메타데이터로 올라간다.

**아직 안 되는 것**: 아무것도 이 메타데이터를 소비하지 않는다. 동작 변화 0.

**롤백**: attribute와 2줄 삭제.

---

### S2 — `GameDb` / `Scope` / `DataSet<T>` 신설 (미사용)

**건드리는 파일**: `Data/*`(신규 5), `StartUp.Resource.cs`(DI 1줄)

```csharp
services.AddScoped<DbSessionManager>();
services.AddScoped<GlobalDbRepo>();
services.AddScoped<GameDb>();          // ← 추가. 같은 DbSessionManager 를 받는다
```

```csharp
public class GameDb
{
    // GlobalDbRepo 와 동일한 DbSessionManager 인스턴스를 받으므로
    // connectionString 이 같으면 같은 IDbSession = 같은 트랜잭션 (§7.1-①)
    public GameDb(DbSessionManager sessions, ICacheSession cache, ILogger<GameDb> logger) { ... }

    public UserScope   User(int shardId, ulong playerId);
    public AuthScope   Auth();
    public CenterScope Center();

    // 이관 기간에는 tx commit 을 하지 않는다 — GlobalDbRepo.CommitAsync 가 단일 커밋 주체 (§7.1-②).
    // 단 dirty flush 는 이 시점에 이미 필요하므로, GlobalDbRepo.CommitAsync 가
    // tx commit 직전에 GameDb.FlushDirtyAsync() 를 먼저 호출하도록 한 줄 연결한다.
    internal async Task FlushDirtyAsync()
    {
        foreach (var scope in _scopes)
            foreach (var set in scope.LoadedSets)
                await set.FlushDirtyAsync();     // 기존 IRepository.UpdateAsync 재사용
    }
}
```

`ModelBase`에 `IsDirty`/`MarkDirty()`/`ClearDirty()`를 추가하는 것이 이 스텝의 선행 작업이다(§3.8).

**직후 가능해지는 것**: 없음. 컴파일만 된다.

**아직 안 되는 것**: 전부. **이 스텝의 목적은 다음 스텝에서 되돌릴 게 없게 만드는 것**이다.

**롤백**: 신규 파일 삭제 + DI 1줄.

---

### S3 — 엔진 계층 `DeleteAsync`

**건드리는 파일**: `ServerCore/Repo/Database/IDbExecutor.cs`, `DapperExtension.cs`, `DataSet.cs`, `Server.Tests/`(신규)

```csharp
// IDbExecutor — Create/Read/Update 만 있고 Delete 가 없던 상태 (5.6)
Task<int> DeleteAsync<T>(T entity) where T : ModelBase;
```

**직후 가능해지는 것**: 삭제 연산. 그리고 **비어 있던 `Server.Tests`에 첫 단위 테스트가 생긴다.**

**아직 안 되는 것**: 부분 필드 업데이트(5.6 나머지)는 API 경계 문제라 운영툴 작업 시.

---

### S4 — Channel / Device / Account ← **의사결정 게이트**

**건드리는 파일**
- 삭제: `Component/{Channel,Device,Account}Component.cs`, `Manager/{Channel,Device,Account}Manager.cs` (6)
- 수정: `Repo/AuthRepo.cs`(PrepareComp 3줄 제거), `Server/Service/AuthService.cs`
- 추가: `Domain/AccountModel.Logic.cs`, `Data/Queries/ChannelQueries.cs`

```csharp
// ── Before : ChannelManager.cs 전체 (14줄, 본문 없음)
public class ChannelManager : AuthManagerBase<ChannelModel>
{
    public ChannelManager(AuthRepo authRepo, ChannelModel model) : base(authRepo, model) { }
}
// After : 파일 삭제. Set<ChannelModel>() 이 ChannelModel 을 그대로 반환한다.
```

```csharp
// ── Before : AccountManager.cs (20줄) — 로직은 IsActive 하나뿐인데 Repo 를 들고 있다
public class AccountManager : AuthManagerBase<AccountModel>
{
    public AccountManager(AuthRepo authRepo, AccountModel model) : base(authRepo, model) { }
    public bool IsActive() => Model.State >= EAccountState.NONE;
}

// ── After : Domain/AccountModel.Logic.cs — DB 참조가 없다
public partial class AccountModel
{
    public bool IsActive() => State >= EAccountState.NONE;
}
```

```csharp
// ── Before : AccountComponent.CreateAsync — 데이터 계층이 요청 컨텍스트를 쓴다
public async Task<AccountManager> CreateAsync()
{
    var newAccount = new AccountModel { ShardId = 0, State = EAccountState.ACTIVE, ... };
    var repoAccount = await CreateMdlAsync(newAccount);
    var mgrAccount = new AccountManager(_authRepo, repoAccount);

    _authRepo.RpcContext.SetAccountId(mgrAccount.Id);        // ← 데이터 계층이 컨텍스트를 변경
    _authRepo.RpcContext.SetShardId(mgrAccount.Model.ShardId);
    return mgrAccount;
}

// ── After : AuthService (Transport 인접) 로 이동
var account = await auth.Set<AccountModel>().CreateAsync(new AccountModel
{
    ShardId = 0, State = EAccountState.ACTIVE, AdditionalPlayerCnt = 0, ClientSecret = ""
});
RpcContext.SetAccountId(account.Id);          // 컨텍스트 변경은 컨텍스트 소유자가 한다
RpcContext.SetShardId(account.ShardId);
```

```csharp
// ── Before : AccountComponent.GetActiveAsync
var (found, mgrAccount) = await TryGetAsync(accountId);
ReqHelper.ValidContext(found, "NOT_FOUND_ACCOUNT", () => new { AccountId = accountId });
ReqHelper.ValidContext(mgrAccount.IsActive(), "NOT_ACTIVE_ACCOUNT", () => new { ... });

// ── After : Data/Queries/AccountQueries.cs — T1 확장 메서드 (새 SQL 없음, §3.3)
public static async Task<AccountModel> GetActiveAsync(this DataSet<AccountModel> set, ulong accountId)
{
    var (found, account) = await set.TryGetAsync(new { Id = accountId });
    ReqHelper.ValidContext(found, "NOT_FOUND_ACCOUNT", () => new { AccountId = accountId });
    ReqHelper.ValidContext(account.IsActive(), "NOT_ACTIVE_ACCOUNT", () => new { AccountId = accountId, account.State });
    return account;
}
```

```csharp
// ── AuthRepo.PrepareComp() : 5줄 → 2줄
Account = new AccountComponent(this, Repository);   // 삭제
Session = new SessionComponent(this, Repository);   // 유지 (S7)
Device  = new DeviceComponent(this, Repository);    // 삭제
Channel = new ChannelComponent(this, Repository);   // 삭제
PlayerMap = new PlayerMapComponent(this, Repository); // 유지 (S8)
```

**직후 가능해지는 것**
- 세 엔티티에 한해 `auth.Set<AccountModel>()`로 접근. **클래스를 만들지 않고** 새 엔티티를 쓸 수 있다는 것이 실증된다.
- 빈 Manager 2개가 실제로 사라진다(5.1.1).
- `AuthService`가 `GlobalDbRepo`와 `GameDb`를 **동시에** 쓰는 상태가 된다 — 같은 트랜잭션이므로 정상이다.

**아직 안 되는 것**: Session/PlayerMap은 그대로. User 계열 전부 그대로. 재화·로직 이관 없음.

**롤백 비용**: 클래스 3쌍 복원. **여기서 `DataSet<T>` 설계가 맞지 않으면 A안을 중단하고 B안을 재검토한다.**

---

### S5 — Point / Ticket / Item / Cookie + `ChangeSet`

**건드리는 파일**: Component 4 + Manager 4 삭제, `Domain/{Point,Ticket,Item,Cookie}Model.Logic.cs` 추가, `Domain/ChangeSet.cs` 추가, `UserRepo.PrepareComp()` 4줄 제거, `CookieService`

```csharp
// ── Before : CookieManager.EnhanceStarAsync — 검증 + 변경 + 저장이 한 메서드에
public async Task EnhanceStarAsync(int aftStar, int usedSoulStone)
{
    _ = _model.Star;
    var befSoulStone = _model.SoulStone;
    ReqHelper.ValidEnough(usedSoulStone, befSoulStone, $"COOKIE_SOUL_STONE:{_prt.Num}", "ENHANCE_STAR");

    _model.Star = aftStar;
    _model.SoulStone -= usedSoulStone;
    await _userRepo.Cookie.UpdateMdlAsync(_model);      // ← 저장. 이것 때문에 _userRepo 가 필요하다
}

// ── After : Domain/CookieModel.Logic.cs — 저장이 MarkDirty 로 바뀌면 그대로 순수해진다
public partial class CookieModel
{
    public void EnhanceStar(int aftStar, int usedSoulStone, CookieProto prt)
    {
        ReqHelper.ValidEnough(usedSoulStone, SoulStone, $"COOKIE_SOUL_STONE:{prt.Num}", "ENHANCE_STAR");
        Star = aftStar;
        SoulStone -= usedSoulStone;
        MarkDirty();                       // ← await _userRepo.Cookie.UpdateMdlAsync(_model); 의 자리
    }
}
```

바뀐 것은 **세 가지뿐**이다: ① `_model.` 접두사 제거 ② 저장 호출 → `MarkDirty()` ③ Proto를 필드가 아니라 **인자로** 받음(시그니처가 의존성을 문서화한다, §3.4). 의미가 없어 보이는 `_ = _model.Star;` 같은 discard 줄도 함께 정리된다(원래 의도는 불명).

`_userRepo`가 필요했던 유일한 이유가 저장이었으므로, ②만으로 **Model이 DB를 참조할 이유가 사라진다.** 이것이 §0.6의 유일한 신규 결정이 실제로 성립하는 지점이다.

```csharp
// ── 재화 메서드는 ChangeSet 을 반환한다
public partial class CookieModel
{
    public ChangeSet IncSoulStone(int amount)
    {
        var before = SoulStone;
        SoulStone    += amount;
        AccSoulStone += amount;
        MarkDirty();
        return ChangeSet.Of(EObjType.SOUL_STONE, Num, before, SoulStone);
    }
}
```

```csharp
// ── ModelBase 에 추가 (S2 선행 작업) — 외부 참조 0
public abstract class ModelBase
{
    public bool IsDirty { get; private set; }
    public void MarkDirty()  => IsDirty = true;
    public void ClearDirty() => IsDirty = false;
}
```

```csharp
// ── Domain/ChangeSet.cs — 서버 런타임 전용. 직렬화 대상이 아니다.
public readonly record struct ChangeSet(EObjType Type, int Num, double Before, double After)
{
    public double Delta => After - Before;
    public static ChangeSet Of(EObjType t, int n, double b, double a) => new(t, n, b, a);
}

// 응답 경계 매핑 — 도메인은 ChgObjPacket 을 모른다
public static ChgObjPacket ToPacket(this ChangeSet c)
    => new() { Type = c.Type, Num = c.Num, Amount = c.Delta, TotalAmount = c.After };
```

> **`ChangeSet`의 근거는 "세 싱크의 단일 출처"가 아니다 (S0-3에서 교체).**
> 그 주장은 철회됐다 — 싱크마다 범위·shape이 다르다. 존치 근거는 **와이어 계약 분리**다: `ChgObjPacket`은 `[ProtoContract]` 직렬화 계약이므로 도메인이 반환하면 와이어 변경이 도메인까지 파급된다. 그리고 이 프로젝트는 이미 `_mapper.Map<CookiePacket>(model)`로 런타임→패킷 매핑을 서비스 경계에서 하므로, ChangeSet을 두는 쪽이 **기존 스타일과 일관**된다. `Reason`과 `Acc*`는 넣지 않는다(액션당 1개 / 파생 상태). 상세는 §3.5.

```csharp
// ── CookieComponent.TouchAsync (없으면 생성) 는 T1 확장 메서드로
public static async Task<CookieModel> TouchAsync(this DataSet<CookieModel> set, int cookieNum)
{
    var (found, cookie) = await set.TryGetAsync(new { Num = cookieNum });
    return found ? cookie : await set.CreateAsync(new CookieModel
    {
        Num = cookieNum, Lv = DEF.DEFAULT_LV, SkillLv = DEF.DEFAULT_LV,
        // PlayerId 는 스코프가 채운다 — 손으로 RpcContext.PlayerId 를 넣지 않는다
    });
}
```

**직후 가능해지는 것**
- 재화 로직이 **DB 없이 단위 테스트 가능**해진다: `new CookieModel{SoulStone=10}.EnhanceStar(3, 5, prt)`
- `ChangeSet`이 존재한다 → S13의 감사 로그가 소비만 하면 되는 상태
- `PlayerId`를 손으로 채우던 코드가 사라진다 → 5.5.1의 캐시 버킷 불일치 버그 경로가 4개 엔티티에서 닫힌다

**아직 안 되는 것**
- `PlayerDetailManager`가 여전히 `_userRepo.Point/.Ticket/.Cookie/.Item`을 찌른다 — **5.1.4/5.4는 S6까지 미해결**
- 감사 로그 기록 없음

**전제**: **S0-1(저장 모델)이 확정되어 있어야 한다.** 저장을 걷어낸 로직이 어디에 착지하는지가 여기서 처음 결정된다.

---

### S6 — PlayerDetail 분해 + `RewardHelper` ← **A안 핵심 검증**

**건드리는 파일**: `Manager/PlayerDetailManager.cs`(330줄) 삭제, `Domain/PlayerDetailModel.Logic.cs` + `Domain/RewardHelper.cs` 추가

```csharp
// ── Before : PlayerDetailManager 는 이름과 달리 EObjType 라우터다
public async Task<double> DecCostAsync(EObjType objType, int objNum, double objAmount, string reason)
{
    ReqHelper.ValidUnderFlowParam(objAmount, reason);
    var valObjAmount = ReqHelper.ValidWithoutDecimal(objAmount, reason);

    switch (objType.ToObjTyeCategory())
    {
        case EObjType.EXP:         return await DecExpInternalAsync(valObjAmount, reason);
        case EObjType.GOLD:        return await DecGoldInternalAsync(valObjAmount, reason);
        case EObjType.TOTAL_CASH:  return await DecCashInternalAsync(valObjAmount, reason);
        case EObjType.POINT_START: return await DecPointInternalAsync((int)objType, valObjAmount, reason);
        case EObjType.TICKET_START:return await DecTicketInternalAsync((int)objType, valObjAmount, reason);
        case EObjType.ITEM:        return await DecItemInternalAsync(objNum, valObjAmount, reason);
        default: throw new GameException(EErrorCode.PARAM, "NO_HANDLING_COST_OBJ_TYPE", new { ObjType = objType });
    }
}

private async Task<double> DecPointInternalAsync(int pointNum, double amount, string reason)
{
    var mgrPoint = await _userRepo.Point.TouchAsync((EObjType)pointNum);   // ← DB 접근
    return await mgrPoint.DecAmountAsync(amount, reason);                  // ← 저장
}
```

**분해 결과 — 둘로 갈라진다**

```csharp
// ── After ①: Domain/RewardHelper.cs — 라우팅. 순수. static.
//    "이미 로드된 모델을 받아 ObjKey 로 골라 적용한다"
public static class RewardHelper
{
    public static ChangeSet Pay(PlayerDetailModel detail, LoadedObjects loaded, ObjValue cost)
    {
        ReqHelper.ValidUnderFlowParam(cost.Value, nameof(Pay));
        var amount = ReqHelper.ValidWithoutDecimal(cost.Value, nameof(Pay));

        return cost.Key.Type.ToObjTyeCategory() switch
        {
            EObjType.EXP          => detail.DecExp(amount),
            EObjType.GOLD         => detail.DecGold(amount),
            EObjType.TOTAL_CASH   => detail.DecCash(amount),
            EObjType.POINT_START  => loaded.Point(cost.Key.Num).DecAmount(amount),
            EObjType.TICKET_START => loaded.Ticket(cost.Key.Num).DecAmount(amount),
            EObjType.ITEM         => loaded.Item(cost.Key.Num).DecAmount(amount),
            _ => throw new GameException(EErrorCode.PARAM, "NO_HANDLING_COST_OBJ_TYPE", new { cost.Key.Type }),
        };
    }

    public static IReadOnlyList<ChangeSet> Grant(PlayerDetailModel detail, LoadedObjects loaded,
                                                 IEnumerable<ObjValue> rewards) => ...;
}

// ── After ②: Domain/PlayerDetailModel.Logic.cs — 자기 필드(EXP/GOLD/CASH)만
public partial class PlayerDetailModel
{
    public ChangeSet DecGold(double amount)
    {
        ReqHelper.ValidEnough(amount, Gold, "GOLD", nameof(DecGold));
        var before = Gold;
        Gold -= amount;
        MarkDirty();
        return ChangeSet.Of(EObjType.GOLD, 0, before, Gold);
    }
}
```

`LoadedObjects`는 App Service가 미리 로드해 넘기는 읽기 전용 묶음이다. **`Func<int, PointModel>` 지연 로드를 쓰지 않는다**(§3.6 정정). 필요 대상은 App Service가 1.2의 4단 순서로 확정한다.

**직후 가능해지는 것 — 이 스텝이 A안 전체의 검증 지점이다**
- `_userRepo`를 드는 클래스가 **하나도 남지 않는다** → 5.1.4 해소
- "이 로직은 어디 소속인가"가 타입으로 결정된다 → **5.4 해소**. `PlayerDetailModel`에 DB 참조가 없으므로 다른 Model을 건드리는 코드는 컴파일이 안 된다
- 재화 라우팅 전체가 DB 없이 테스트 가능해진다

**아직 안 되는 것**: Player/Session/Schedule/World/Kingdom 미이관. `GlobalDbRepo` 건재.

**여기까지 통과하면 §0.6의 "유일한 신규 결정"이 실증된 것이다.**

---

### S7 — Player / Session (+ RaidServer) · T2 확정

**건드리는 파일**: Component 2 + Manager 2, `RaidServer/Services/PlayerRaidSessionService.cs`, `AuthService`

```csharp
// ── Before : RaidServer — 앰비언트에 묶여 "나"만 열 수 있다
var mgrSession  = await dbRepo.Auth.Session.TryGetByKeyAsync(req.SessionKey);
dbRepo.BeginOwnUserRepo();                                   // 인자 없음
var playerModel = (await dbRepo.OwnUser.Player.GetAsync()).Model;

// ── After : 대상을 직접 연다 (GSA InitUserRepo 패턴 복원)
var session = await _db.Auth().Set<SessionModel>().ByKeyAsync(req.SessionKey);
var shardId = await _db.Auth().Set<PlayerMapModel>().ShardOfAsync(session.PlayerId);
var player  = await _db.User(shardId, session.PlayerId).Set<PlayerModel>().GetOneAsync();
```

```csharp
// ── T2 : 보조 인덱스. 선언만 하고 캐시는 넣지 않는다 (§3.9 결정)
[Entity(Table = "Player", Pk = new[] { "Id" }, Owner = "Id")]
[SecondaryIndex("AccountId")]
public partial class PlayerModel : ModelBase { }

// Before : PlayerComponent.TryGetByAccountIdAsync — DbSession 직접 접근 + 캐시 없음
// After  : 이름 있는 정식 경로. 동작은 동일(DB 직접), 포인터 캐시는 미도입
var (found, player) = await user.Set<PlayerModel>().ByIndexAsync(nameof(PlayerModel.AccountId), accountId);
```

**직후 가능해지는 것**
- **`RaidGameContext`의 `Ip=""` 스텁이 제거된다** → `SessionModel.PublicIp`에 빈 값이 저장될 수 있던 잠재 버그 소멸(5.5.2)
- 임의 플레이어를 여는 경로가 **본편과 동일한 API**가 된다(5.5.1)

**확인 필수**: `SessionModel.PublicIp`에 실제 IP가 들어가는지 RaidServer 경로로 수동 확인.

---

### S8 — Schedule / PlayerMap · T0 확정

```csharp
// ── Before : ScheduleComponent.GetListAsync
// 주석: "전체 조회 — 캐시 -> DB조회 일반화가 어려운 부분이라 DbSession 직접 사용"
var mdlList = await DbSession.ExecuteAsync(async db =>
    (await db.SelectListByConditionsAsync<ScheduleModel>(null)).ToList());

// ── After : 특수 쿼리가 아니었다. Owner 없는 엔티티일 뿐 (§3.9 T0)
[Entity(Table = "Schedule", Pk = new[] { "Num" }, Cache = ECachePolicy.GlobalList)]
public partial class ScheduleModel : ModelBase { }        // Owner 없음 → WHERE 없음

var schedules = await center.Set<ScheduleModel>().GetListAsync();   // 전역 리스트 캐시
```

**`GlobalList` 캐싱 도입 확정.** 현재 Center 계열은 캐시가 전혀 없어 **매 요청마다 Schedule 전량 조회**다. 스케줄은 거의 변하지 않으므로 이득이 크다. 대신 이 스텝에 무효화를 함께 넣는다:
- 전역 키(샤드·오너 무관) 정의
- `Create/Update/Delete`가 전역 리스트를 갱신 또는 무효화
- **한계**: 운영툴/배치가 DB를 직접 고치면 캐시가 어긋난다. `CacheDefaultTtl`이 상한이며, 즉시 반영이 필요하면 명시적 무효화 API를 노출한다. 문서에 남긴다.

`ScheduleManager`가 Model+Proto를 묶던 부분은 **읽기 전용 뷰**로 바뀐다(§3.4):

```csharp
public readonly record struct ScheduleView(ScheduleProto Prt, ScheduleModel Mdl)
{
    public bool IsActivePeriod(DateTime now) => ...;
}
```

**직후**: `DbSession` 직접 사용 1건 소멸 + 캐시가 적용된다(현재는 매 요청 DB 전체 조회).

---

### S9 — World / WorldStage · T3 확정

```csharp
// ── Before : WorldStageComponent.GetTotalStarAsync — "TODO: 캐시" 상태
public Task<int> GetTotalStarAsync(int worldNum)
{
    var sql = "SELECT SUM(RewardAmount) FROM WorldStage WHERE PlayerId = @PlayerId AND WorldNum = @WorldNum";
    return DbSession.ExecuteAsync(db => db.QuerySingleAsync<int>(sql, new { RpcCtx.PlayerId, WorldNum = worldNum }));
}

// ── After : T3 — 정식 진입점. 감추지 않는다. 캐시 금지가 기본 (§3.9)
//    Raw 는 실행 직전에 스코프의 dirty 를 flush 한다 (§3.8 dirty 모델 + §3.9 T3 규칙)
var totalStar = await user.Raw<int>(
    "SELECT SUM(RewardAmount) FROM WorldStage WHERE PlayerId = @PlayerId AND WorldNum = @WorldNum",
    new { WorldNum = worldNum });      // PlayerId 는 스코프가 채운다
```

**직후**: 집계 쿼리가 `scope.Raw`라는 이름으로 드러난다. `DataSet<T>` 확장 메서드로 감싸지 않으므로 코드 리뷰에서 보인다.

**주의**: dirty 모델에서 변경은 커밋까지 DB에 없다. `Raw`가 실행 전 flush를 하지 않으면 방금 변경한 값이 집계에서 빠진다. **이 규칙을 `Raw` 도입과 동시에 넣는다** — 나중에 붙이면 이미 쓰인 호출부가 조용히 틀린 값을 본다.

---

### S10 — Kingdom 4종 (628줄, 최대)

```csharp
// ── Before : KingdomMapManager 가 다른 Manager 를 인자로 받는다 → 여러 Model 불변식
public Task ConstructStructureAsync(KingdomStructureManager mgrStructManager, TilePosPacket valStartTilePos);
public Task ConstructDecoAsync(KingdomDecoManager mgrDecoManager, TilePosPacket valStartTilePos);

// ── After : 이름 있는 도메인 서비스 (§3.6 판정표 — Model 여러 개에 걸친 규칙)
public static class KingdomBuilder
{
    public static void ConstructStructure(KingdomMapModel map, KingdomStructureModel structure,
                                          TilePosPacket startPos, KingdomItemProto prt);
}
```

스냅샷(`KingdomMapSnapshotPacket`)은 `ObjKey`로 주소 지정이 불가능하므로 **`ChangeSet` 대상이 아니다**. 자기 응답 패킷으로만 간다(S0-3).

**직후**: 최대 덩어리 완료. 이 시점에 Component/Manager가 전부 비어 있다.

---

### S11~S13 — 철거 및 마감

```csharp
// S11 삭제 대상
DbModel/Base/*        (8)   ← ComponentBase 3종 + ManagerBase 3종 + RepoBase
DbModel/Repo/         GlobalDbRepo, AuthRepo, CenterRepo, UserRepo
DbModel/Component/    (18)
DbModel/Manager/      (17)
// AllUserRepo 는 삭제하지 않고 GameDb 로 이관 (§0.7 — 있고 쓰이는 것은 가져간다)

// 커밋 주체 역전
await _dbRepo.CommitAsync();   →   await _db.CommitAsync();
```

```csharp
// S12 : StartUp.Resource.cs 최종 형태
EntityRegistry.ScanAndRegister(typeof(CookieModel).Assembly);
// ModelRegistration.Init 19줄 + AssertMatches 삭제
// IGameContext 는 Transport 전용으로 축소 — 데이터 계층 소비처가 0이 되었으므로
```

```csharp
// S13 : 감사 — 싱크마다 shape 이 다르다 (S0-3)
_logger.LogChanges(reason, changes);                              // 전 축, 1:1
await _audit.WriteCashChangeAsync(user, action, changes);         // Cash 3종만 필터 + 액션당 1행 fold
await _audit.WriteGachaLogAsync(user, scheduleNum, cnt, changes); // 가차 축, 별도 조립
// 비-Cash 재화 전용 테이블은 만들지 않는다 (의도된 설계, §6)
```

---

## 3. 이관 기간 중 코드는 어떻게 보이는가

S4~S10 사이에는 **구 경로와 신 경로가 한 파일 안에 공존**한다. 이것이 정상 상태다.

```csharp
// 예: S4 직후의 AuthService — 두 진입점이 같이 보인다
public class AuthService : ServiceBase
{
    public AuthService(GlobalDbRepo dbRepo, GameDb db, ...)   // 둘 다 주입
    {
        _dbRepo = dbRepo;   // 아직 이관 안 된 엔티티용 (Session, PlayerMap)
        _db = db;           // 이관 완료된 엔티티용 (Account, Channel, Device)
    }

    public async Task<AuthSignUpResponsePacket> SignUpAsync(...)
    {
        var account = await _db.Auth().Set<AccountModel>().CreateAsync(...);   // 신 경로
        var session = await _dbRepo.Auth.Session.CreateAsync(...);             // 구 경로
        // 같은 DbSessionManager → 같은 IDbSession → 같은 트랜잭션. 원자성 유지.
        await _dbRepo.CommitAsync();
    }
}
```

**판단 기준**: 지저분해 보이는 것이 정상이며, 각 스텝이 끝날 때마다 구 경로 참조가 단조 감소한다. 진행도는 **`GlobalDbRepo`를 주입받는 클래스 수**로 측정한다 — S11에서 0이 된다.

| 시점 | `GlobalDbRepo` 주입 | `Component` 파일 | `Manager` 파일 |
|---|---|---|---|
| 현재 | 11 (Service 전체) | 18 | 17 |
| S4 후 | 11 | 15 | 14 |
| S6 후 | 11 | 10 | 9 |
| S10 후 | 1 (커밋용만) | 0 | 0 |
| S11 후 | **0** | **0** | **0** |

---

## 4. 실행 전 확정해야 하는 것

| | 항목 | 상태 | 이 문서에서 영향받는 곳 |
|---|---|---|---|
| **S0-1** | 저장 모델 | **확정 — (c) dirty 플래그 + 커밋 시 flush** (§3.8) | 1.2 3)블록, S2 `ModelBase`, S5 전체, S9 Raw flush |
| **S0-2** | `ClassGenerator`가 `[Entity]`를 찍을 수 있는가 | **미확인** | S1 (불가하면 모델 20개 수작업 — 작업량만 영향) |
| **S0-3** | 감사·반환 타입 | **확정 — `ChangeSet` 존치(근거: 와이어 계약 분리), 감사는 싱크별 개별 조립** | S5 도메인 메서드 반환형, S13 |
| **S0-4** | 커밋 경계를 유저 락 안으로 | **완료 — 선행 커밋 2개로 반영** | 리뷰 5.1, 5.2 |

**남은 미확인은 S0-2 하나뿐이며 작업량에만 영향을 준다 — 실행을 막지 않는다.**

**S0-2 확인 결과(2026-08-11) — 가능하다.** `Template/ModelTemplate.txt`에 `{{ClassAttribute}}` 슬롯이 이미 있고 `ModelGenerator.cs:225`가 모델에는 빈 문자열을 넣고 있다(패킷은 같은 슬롯에 `[ProtoContract]`). PK도 이미 스펙에 있다 — `ModelGenerator.cs:319`의 `x.KeyList.Contains("pk")`, 인덱스는 `c_index`/`index`(334·393행). 따라서 `[Entity(Pk)]`와 `[SecondaryIndex]`는 자동 생성이 가능하다.
다만 `Owner`/`Cache`/`SlidingTtl`은 엑셀 스펙에 대응 컬럼이 없다. 모델 20개는 전부 `*.generated.cs` 단독이고 수기 partial이 하나도 없으므로, **A안이 어차피 도메인 partial을 새로 만든다면 `[Entity]`는 수기 partial 쪽에 두는 것**이 스펙 포맷을 건드리지 않아 낫다. S1 착수 시 확정한다.

### 4.2 S0-4 반영 내역 (완료)

| 커밋 | 내용 |
|---|---|
| 1 | `DbUtilityConnection` 신설 + `MySqlLockService` 전용 커넥션 전환. **실행 순서 변화 없음** — 5.2의 선결 조건만 제거 |
| 2 | `RpcService.HandleMethodAsync`의 `SetAsync`/`CommitAsync`/`RollbackAsync`를 `RunAtomicAsync` 안으로 이동 |

두 커밋으로 나눈 이유: 커넥션 분리는 커밋 경계와 무관하게 그 자체로 옳은 수정이므로, 경계 이동을 되돌려야 할 때 같이 딸려 나가면 안 된다.

### 4.1 dirty 모델에서 확인이 필요한 것 (S2 착수 시)

| | 내용 |
|---|---|
| INSERT 즉시 유지 | `InsertAsync`가 auto PK를 반환하므로 지연 불가. 원자성 개선은 UPDATE 한정 |
| FK 제약 유무 | dirty flush 순서 = 로드 순서가 된다. FK가 있으면 순서 제어 필요 |
| `RedisCompositeCacheLayer`의 pending 읽기 | `Cache.TryGetAsync`가 pending 쓰기를 보는지 확인 (보지 못하면 같은 요청 내 캐시 조회가 옛 값을 볼 수 있음) |

---

## 5. 자체 리뷰 — 모의 실행 결과

각 스텝을 실제 코드에 대입해 실행해본 결과. **심각도 순.**

### 5.1 🔴 치명 — dirty flush가 유저 락 밖으로 나간다 (lost update)

`RpcService.HandleMethodAsync`의 현재 순서:

```csharp
await _userLockSvc.RunAtomicAsync(_rpcCtx.AccountId, async () =>
{
    rpcResObj = await rpcMethod.RunAsync(_rpcCtx, httpCtx, _dbRepo, rpcReqObj);   // ← DB 쓰기가 여기서 일어난다
});

await _responseCache.SetAsync(_rpcCtx, rpcResObj);
await _dbRepo.CommitAsync();                                                       // ← 락 해제 후
```

현재는 **쓰기가 락 안**이고 커밋만 락 밖이다. 그런데 3.8의 dirty 모델을 도입하면 **쓰기 전체가 `CommitAsync`로 이동 = 락 밖으로 나간다.**

같은 `accountId`의 동시 요청 A/B:

```
1. A 락 획득 → Point 100 읽음 → 메모리에서 90 (dirty) → 락 해제   ※ DB에는 아직 100
2. B 락 획득 → DB/캐시에서 100 읽음 → 메모리에서 90 (dirty) → 락 해제
3. A CommitAsync → 90 기록
4. B CommitAsync → 90 기록
→ 20 차감돼야 하는데 10만 차감. Lost update.
```

**dirty 모델이 만들어내는 신규 버그다. 반드시 같이 고쳐야 한다.**

**수정**: flush + commit을 **락 안으로** 옮긴다.

```csharp
await _userLockSvc.RunAtomicAsync(_rpcCtx.AccountId, async () =>
{
    rpcResObj = await rpcMethod.RunAsync(...);
    await _responseCache.SetAsync(_rpcCtx, rpcResObj);
    await _db.CommitAsync();              // dirty flush → tx commit → cache flush
});
```

→ **S0-4로 분리해 A안 착수 전에 선행 처리한다.** dirty flush를 실제로 켜는 S5보다 먼저 해두는 것이 안전하다. (**완료** — §4.2)

> 참고: 현재 코드도 커밋이 락 밖이라 미묘하지만, 쓰기가 락 안이라 DB row lock이 잡혀 실질적으로 직렬화된다. (c)는 그 우연한 보호마저 제거한다.

롤백도 함께 락 안으로 넣었다. 롤백만 밖에 두면 "락 해제 후 롤백" 구간이 생겨, 그 사이에 들어온 같은 계정 요청이 아직 되돌려지지 않은 값을 읽는다.

### 5.2 🔴 치명 — `MySqlLockService`가 `GlobalDbRepo`에 의존한다 (S11에서 컴파일 에러)

```csharp
public MySqlLockService(GlobalDbRepo dbRepo) { _dbRepo = dbRepo; }

await _dbRepo.Auth.Repository.Db.ExecuteAsync<long>(
    db => db.QuerySingleAsync<long>("SELECT GET_LOCK(@id, @timeout)", ...));
```

§7 어디에도 락 서비스 이관이 없다. S11이 `GlobalDbRepo`를 지우면 **깨진다.**

**그리고 이건 S11까지 갈 문제가 아니었다 — 5.1을 고치는 순간(S0-4) 이미 런타임에서 깨진다.** 처음엔 "S11 컴파일 에러"로만 적었는데, 실제 파손 시점이 훨씬 앞이다:

```
DbSessionManager.Commit() → 세션마다 Commit() → DBSqlExecutor.CloseInternal()  // 커넥션 Dispose
```

커밋을 락 안으로 옮기면 순서가 `GET_LOCK → 쓰기 → COMMIT(커넥션 Dispose) → finally: RELEASE_LOCK`이 되어, `UserLockService.cs:44`의 `finally`가 Dispose된 커넥션을 건드린다. **MySql + `UseUserLock: true`(운영 설정)에서 매 요청 터진다.** `ServerTest`는 `UseUserLock: false` + InMemory + `InMemoryLockService`(no-op)라 이 경로를 전혀 타지 않아 테스트로는 잡히지 않는다.

**진단 정정 — 원인은 트랜잭션이 아니라 수명이다.** `DBSqlExecutor`가 트랜잭션을 강제해서 깨진 게 아니다. 락을 무트랜잭션 세션으로 열었어도 그게 `DbSessionManager`에 등록되는 한 커밋이 똑같이 닫는다. 구분 축은 **"요청 작업 단위에 참여하는가"**다.

| | `DBSqlExecutor` / `IDbSession` | `DbUtilityConnection` |
|---|---|---|
| `DbSessionManager` 추적 | O | X |
| 수명 | 요청 커밋/롤백까지 | 호출자 소유, **커밋보다 오래 삶** |
| 트랜잭션 | 현재는 항상. **내부 사정** | 없음 |

**수정(S0-4에서 완료)**: `ServerCore/Repo/Database/DbUtilityConnection.cs`를 신설하고 `MySqlLockService`를 거기로 옮겼다. `IDbSession`을 구현하지 않으므로 `DbSessionManager.Open()`에 넣을 수 없다 — 경계를 타입이 강제한다. `DBSqlExecutor`는 손대지 않았다.

**열린 질문 (S2/S11로 이월)**: 읽기 전용 요청이 매 요청 진짜 트랜잭션을 여는 문제(최초 리뷰 지적)의 답은 **lazy BEGIN**이다 — 커넥션은 열되 첫 쓰기에서 `BeginTransaction`. 이건 `DBSqlExecutor` + `DbSessionManager` **내부** 변경이고 위 구분과 충돌하지 않는다(그 세션은 여전히 매니저가 추적하고 커밋 시 닫힌다). 소비처가 생기는 S2/S11에서 판단한다. 그때 무트랜잭션 상태의 `Commit()`이 no-op이 되는 것은 오용이 아니라 "쓸 게 없었다"는 정당한 결과다.

**T3 auto-flush와의 충돌은 그대로 남아 있다** — `GET_LOCK`을 부르면서 dirty flush가 트리거되면 락도 걸리기 전에 쓰기가 나간다. 락 쿼리가 엔티티 세션을 아예 쓰지 않게 되어 구조적으로는 해소됐지만, `GameDb.Utility`가 이 무flush 성질을 명시적으로 보장해야 한다.

```csharp
GameDb.Utility.ExecuteScalarAsync<long>(sql, args)   // flush 없음. 엔티티와 무관한 쿼리 전용
```

→ **S2에 `GameDb.Utility` 추가.** S10.5는 축소됐다 — 커넥션 분리는 S0-4에서 끝났고, `DbUtilityConnection`을 `GameDb.Utility`로 감싸는 일만 남는다.

**남은 비용(의도적으로 감수)**: 커넥션을 소유하는 타입이 둘이 됐다. 최초 리뷰가 지적한 쿼리 로깅/재시도를 넣을 때 양쪽 다 손봐야 한다. 공통 베이스 추출은 세 번째 소비처가 생기면 한다(R7). 세 번째 후보는 이미 있다 — `StartUp.Resource.cs:78`의 `ConnectionTest()`가 핑 하나에 `StartTransaction` + `Commit`을 하고 있고 이것도 매니저 밖 사용이다(시작 시점 동기 코드라 S0-4 범위에서 제외).
그리고 락이 걸린 요청은 커넥션을 하나 더 쓴다. 커넥션 풀 크기 확인이 필요하다.

### 5.3 🟠 기능 후퇴 — `SessionComponent`는 이미 완성된 T2 포인터 캐시다

3.9에서 "T2는 포인터 캐시를 도입하지 않는다"고 결정했는데, `SessionComponent`에는 **그 패턴이 이미 완전히 구현되어 있다**:

```csharp
AccountIdBySessionKey(key) → accountId      // 포인터 캐시
SessionByAccountId(accountId) → SessionModel // 값 캐시 + sliding TTL
UpdateAsync(befKey, mdl)                     // 키 로테이션 시 이전 포인터 invalidate + 신규 등록
LogoutAsync(mdl)                             // 두 키 즉시 제거
```

이대로 "T2 = 캐시 없음"을 적용하면 **매 요청의 세션 조회가 DB로 간다.** 인증 경로라 전 요청에 영향한다. 명백한 후퇴다.

**수정**: 3.9의 T2 결정을 **"신규 도입하지 않는다"로 한정**하고, **이미 구현된 Session의 포인터 캐시는 그대로 유지**한다고 명시. `TryGetByAccountIdAsync`(Player)만 캐시 없이 간다.

### 5.4 🟠 설계 누락 — 캐시 정책이 실제로는 5종인데 `DataSet<T>`는 1종만 전제한다

| 현재 위치 | 정책 |
|---|---|
| `UserComponentBase` | Owner별 **리스트** 캐시 |
| `AuthComponentBase.GetMdlAsync` | **캐시 없음** (DB 직행) |
| `AuthComponentBase.GetMdlWithCacheAsync` | **단건** 캐시 + **sliding TTL** |
| `CenterComponentBase` | **캐시 없음** |
| `SessionComponent` | 단건 + **포인터** |

§3.3의 `DataSet<T>`는 리스트 캐시 하나만 전제한다. 5.3.2를 "메타데이터로 통일"한다고 했지만 **어떤 정책들이 존재해야 하는지 열거가 없다.**

**수정**: `[Entity]`에 캐시 정책을 명시적으로 열거한다.

```csharp
public enum ECachePolicy { None, Single, OwnerList, GlobalList }

[Entity(Table = "Cookie",  Pk = ["PlayerId","Num"], Owner = "PlayerId", Cache = ECachePolicy.OwnerList)]
[Entity(Table = "Schedule", Pk = ["Num"],                                Cache = ECachePolicy.GlobalList)]
[Entity(Table = "Account",  Pk = ["Id"],                                 Cache = ECachePolicy.None)]
[Entity(Table = "Session",  Pk = ["AccountId"], Cache = ECachePolicy.Single, SlidingTtl = true)]
```

**sliding TTL이 설계에 아예 없었다** — `GetMdlWithCacheAsync(key, fetch, slidingTtl)`는 캐시 히트 시 TTL을 갱신하며, 세션 유지에 필수다. `[Entity]`에 `SlidingTtl`을 넣어야 한다.

→ **S1의 attribute 설계에 `Cache`/`SlidingTtl`을 포함한다.** 나중에 추가하면 20개 모델을 두 번 손댄다.

### 5.5 🟠 설계 누락 — `AllUserRepo`(전 샤드 검색)가 스코프 모델에 맞지 않는다

```csharp
public async Task<(bool Found, PlayerModel? Value)> TryGetPlayerByNameAsync(string name)
{
    foreach (var factory in _factories)    // 샤드 전체 순회
        ... SelectByConditionsAsync<PlayerModel>(new { ProfileName = name })
}
```

`UserScope`는 단일 샤드 전제다. §7 S11에 "`GameDb`로 이관"이라고만 적혀 있고 **형태가 없다.**

**수정**: 스코프 밖 진입점으로 정의한다.

```csharp
GameDb.AllShards.FindPlayerByNameAsync(name)    // T3 성격. 캐시 없음(현재도 "TODO: 캐시")
```

추가로 `AllUserRepo` 생성이 **모든 샤드 커넥션을 즉시 연다**(`UserDbConnectionStrList.Select(Open)`). `20260720` 노트 이슈 ②와 같은 문제이며 A안에서도 그대로다 — 지연 오픈으로 개선할 기회다.

### 5.6 🟡 동작 변화 — `UpdateTime` 스탬프 시점이 이동한다

현재 `UpdateMdlAsync`가 `entity.UpdateTime = DateTime.UtcNow`를 찍는다 = **변경 시점**.
dirty 모델에서는 flush 시점 = **커밋 시점**이 된다. 요청 처리가 길면 차이가 난다.

**수정**: `MarkDirty()`가 `UpdateTime`을 찍게 한다. 변경 시점 의미가 유지되고, 오히려 지금보다 정확하다.

```csharp
public void MarkDirty() { IsDirty = true; UpdateTime = DateTime.UtcNow; }
```

### 5.7 🟡 깨뜨리면 안 되는 불변식 — 응답 캐시가 커밋보다 먼저 쓰인다

```csharp
await _responseCache.SetAsync(_rpcCtx, rpcResObj);   // 커밋 전
await _dbRepo.CommitAsync();
```

역전처럼 보이지만 안전하다 — `ResponseCacheService`가 `ICacheSession.SetAsync`를 쓰고, 그것은 **pending에 쌓였다가 `FlushPendingWritesAsync`에서 반영**되며 롤백 시 `DiscardPendingWrites`로 버려지기 때문이다.

**5.1의 순서 변경 시 이 의존성을 깨뜨리면 안 된다.** 응답 캐시 쓰기는 반드시 커밋 flush보다 **앞**이거나 같은 pending 안에 있어야 한다. 커밋 후로 옮기면 롤백된 요청의 성공 응답이 캐시에 남는다.

### 5.8 🟡 누락 — `[Entity]` 스캔이 등록해야 할 곳이 두 군데다

```csharp
public static void Init<T>(params string[] keyFields)
{
    DapperExtension.Init<T>(keyFields);      // SQL 생성용
    InMemoryPkRegistry.Init<T>(keyFields);   // InMemory PK 계산용
}
```

S1의 `EntityRegistry.ScanAndRegister`는 **둘 다** 등록해야 한다. `InMemoryPkRegistry`를 빠뜨리면 InMemory 모드(= `ServerTest` 전체)가 `"미등록. Init<T>()를 먼저 호출하세요"`로 죽는다.

### 5.9 🟡 누락 — 데이터 계층이 `RpcContext`를 쓰는 곳이 Account 말고 더 있다

§1.3에서 `AccountComponent.CreateAsync`만 언급했는데, 하나 더 있다:

```csharp
// PlayerComponent.TouchAsync
_userRepo.RpcContext.SetPlayerId(accountId * 10);
```

**S7에서 함께 처리**한다. 반환값으로 올리고 Transport가 자기 컨텍스트를 갱신한다.

### 5.10 🟡 누락 — `ScheduleView`가 `ServerTime`을 어디서 받는가

```csharp
// ScheduleComponent.GetAsync — RpcCtx.ServerTime 을 쓴다
mgrSchedule.IsActivePeriod(RpcCtx.ServerTime)
```

`ScheduleView`는 컨텍스트를 모르므로 **`ServerTime`을 인자로 받아야 한다**(§3.4의 "Proto는 파라미터로 받는다"와 같은 원칙). S8에서 명시.

### 5.11 🟢 미개선 — 커넥션 오픈 시점이 여전히 이르다

`RepoBase` 생성자가 `PrepareComp()`를 호출하고, `GlobalDbRepo.CreateRepository`가 `_dbSessionManager.Open(connStr)`을 부른다 → **Repo 참조 시점에 커넥션이 열린다**(`20260720` 이슈 ②, 여전히 열려 있음).

A안에서 `GameDb.User(shardId, playerId)`가 즉시 여는지 지연하는지 **문서에 없다.**
→ **`DataSet<T>`의 첫 조회 시점까지 지연**하도록 명시한다. 스코프를 만들기만 하고 안 쓰는 경로(예: 조건 분기)에서 커넥션 낭비가 사라진다. A안이 아니면 고치기 어려운 지점이므로 기회를 살린다.

### 5.12 🟢 미개선 — T1 확장 메서드는 전체 로드 후 메모리 필터다

`GetListAsync` 결과를 거르는 구조라 데이터가 커지면 비효율이다. **현재도 동일**하므로 후퇴는 아니지만 개선도 아니다. Owner 스코프 컬렉션이 작다는 전제 위에 있으며, 깨지면 T3로 승격한다.

---

### 5.13 리뷰 결과 반영 — 스텝 조정

| 스텝 | 추가되는 작업 |
|---|---|
| **S0-4** (완료) | **커밋 경계를 유저 락 안으로 이동 (5.1)** · **락 커넥션 분리 (5.2 선결)** — §4.2 |
| **S1** | `[Entity]`에 `Cache`/`SlidingTtl` 포함 (5.4) · 스캔이 `DapperExtension` + `InMemoryPkRegistry` 양쪽 등록 (5.8) |
| **S2** | `GameDb.Utility` 추가 (5.2) · `MarkDirty()`가 `UpdateTime` 스탬프 (5.6) · 커넥션 지연 오픈 (5.11) · lazy BEGIN 판단 (5.2 열린 질문) |
| **S4** | `ECachePolicy.None` 경로 실증 (Account/Channel/Device는 원래 캐시 없음) |
| **S7** | Session 포인터 캐시 **유지** (5.3) · `Single`+`SlidingTtl` 정책 실증 · `PlayerComponent`의 `SetPlayerId` 이동 (5.9) |
| **S8** | `GlobalList` 정책 실증 · `ScheduleView`가 `ServerTime`을 인자로 (5.10) |
| **S9** | `scope.Raw` 자동 flush **와** `GameDb.Utility` 무flush 경로 구분 확정 (5.2) |
| **S10.5** (축소) | `MySqlLockService`가 쓰는 `DbUtilityConnection`을 `GameDb.Utility`로 감싸기 — 커넥션 분리 자체는 S0-4에서 완료 (5.2) |
| **S11** | `AllUserRepo` → `GameDb.AllShards` 형태 확정 (5.5) · 지연 오픈 적용 |
