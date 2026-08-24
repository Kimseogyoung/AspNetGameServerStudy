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
  Data/         6   GameDb, Scope, OwnedSet<T>, EntityRegistry, EntityAttribute, RawQuery
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
| 데이터 접근 클래스 종류 | 4종 (Repo / ComponentBase / Component / Manager) | **1~3종** (OwnedSet<T> + 로드 단위가 다른 소수 / Model partial) — §S2-D |
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
    var cookie = await user.Owned<CookieModel>().TouchAsync(req.CookieNum);
    var detail = await user.Owned<PlayerDetailModel>().GetOneAsync();
    var point  = await user.Owned<PointModel>().TouchAsync((int)EObjType.POINT_COOKIE_LV);

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
> **주의 (2026-08-20)**: 이 예제의 `MarkDirty()` 는 §S2-H 에서 **철회된 모델**이다. 현재 형태는 `await user.Owned<CookieModel>().UpdateAsync(cookie);` 처럼 명시적 즉시 저장이다. 아래 예제도 같다 — S6~S13 은 §S2-I 대로 "가설"로 읽는다(S5 는 2026-08-23 에 재도출됐다).

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

> **실행 완료 (2026-08-14).** 선결 3건 `e07b98b`, PlacedKingdomItem 정리 `7c9837a`, 본체 `111389d`.
> 아래 체크리스트는 **실행 결과와 계획이 어긋난 곳**을 그대로 남겨 둔다. 계획대로 된 것보다 어긋난 쪽이 다음 스텝에 쓸모가 있다.

**실제로 건드린 파일**: `ModelGenerator.cs`(렌더링 + 가드), `EntityAttribute.cs`(신규), `EntityRegistry.cs`(신규), `ModelRegistration.cs`(충돌 검사), `Model/*.generated.cs` 19개(재생성), `Server`/`RaidServer`의 `StartUp.Resource.cs`(각 1줄)

**계획과 달랐던 것**
- `ModelTemplate.txt`는 **건드리지 않았다.** `EntityAttribute`를 `ModelBase`와 같은 `ServerCore.Model`에 두니 템플릿의 기존 `using`으로 해결됐다.
- 모델은 20개가 아니라 **19개**다. `PlacedKingdomItemModel`은 S1 전에 제거됐다(아래 "예상 걸림돌" 항목).
- `StartUp`은 2줄이 아니라 **각 1줄**이다. `AssertMatches`가 철회됐다(§S1-D).

```csharp
// Before — StartUp.Resource.cs 에만 존재. 모델 파일에는 PK 정보가 없다.
ModelRegistration.Init<CookieModel>("PlayerId", "Num");

// After — 지식이 모델로 이동
[Entity(Pk = ["PlayerId", "Num"], ScopeKey = "PlayerId")]
public partial class CookieModel : ModelBase { }

[Entity(Pk = ["Id"])]                        // Auth 계열 — ScopeKey 없음
public partial class AccountModel : ModelBase { }

[Entity(Pk = ["Id"], ScopeKey = "Id")]       // User 스코프 루트 — 자기 Id가 스코프 키
public partial class PlayerModel : ModelBase { }
```

#### S1-A 실행 체크리스트

| # | 작업 | 결과 |
|---|---|---|
| 1 | `launchSettings.json`의 `--mdlOutputPath`를 `..\Server\Model` → **`..\DbModel\Model`** | ✅ 실제로 stale이었다. 모르고 돌려서 `Code/Server/Model`에 19개가 생겼다 |
| 2 | `Data/Csv/Model/Auth/Session.csv` 2셀 수정 (§S1-C) | ✅ 다만 **판단 근거가 틀렸다**(§S1-E) |
| 3 | 생성기를 먼저 한 번 그대로 돌려 diff가 비어 있는지 확인 | ⚠️ **비어 있지 않았다. 이 게이트가 값을 했다** — 아래 |
| 4 | `[Entity]` 렌더링 + 가드 | ✅ 가드가 1종 → **4종**이 됐다(§S1-B) |
| 5 | 재생성 → diff가 attribute 추가만인지 확인 | ✅ 정확히 `19 files, 19 insertions, 19 deletions` |
| 6 | `EntityRegistry.ScanAndRegister` — 두 레지스트리 양쪽 등록(5.8) | ✅ **위험이 이미 없었다** — `ModelRegistration.Init`이 원래 둘을 묶어 부른다. 그것을 리플렉션으로 부르면 누락이 불가능하다. §5.8은 해소된 것으로 본다 |
| 7 | `StartUp`에 2줄 추가, 기존 목록 존치 | ✅ 단 **1줄**이다(§S1-D). `RaidServer`에도 같은 1줄을 넣었다 |
| 8 | 양쪽 부팅 + `ServerTest` 통과 | ✅ 단 **"동작 변화 0"은 거짓이다**(§S1-F) |

**3번 게이트에서 실제로 걸린 것 — 이 게이트를 넣은 값을 했다.** 생성기를 그대로 돌렸을 때 diff가 두 군데 나왔다.
- `KingdomMapPacket.generated.cs`가 되돌아갔다. `032f704`가 **생성 파일을 손으로 고쳐** 잡은 버그(빈 리스트가 `default`가 되어 클라 NRE)였고, 생성기를 돌릴 때마다 되살아나는 상태였다. 원인은 `KingdomMap.csv`의 `Value` 칸이 비어 있었던 것이고(`Player.csv`의 LIST 8개는 전부 `new()`), **입력을 고쳐** 드리프트를 끝냈다.
- `PlacedKingdomItemModel.generated.cs`가 새 출력에 없었다 → 아래.

**예상 걸림돌은 예상과 반대로 나타났다.** 문서는 "`AssertMatches`가 attribute는 있는데 등록이 없다로 잡을 것"이라고 적었으나, `GetModelFieldCnt == 0 → continue`(`ModelGenerator.cs:200`)라 **생성 자체가 안 되므로 attribute도 붙지 않는다.** 검증은 아무것도 못 잡는다.

진짜 문제는 그게 아니라 **CSV에서 재생산 불가능한 파일에 코드가 의존한다**는 것이었다. 조사해 보니 `PlacedKingdomItemComponent`는 파일 전체가 주석 처리돼 있었고 `PlacedKingdomItemManager`는 생성자 호출부가 0이었다. Packet만 살아 있었다(`KingdomMapPacket`이 리스트로 보유). 그래서 **Model/Manager/Component/CSV를 지우고 Packet은 수기 파일로 전환**했다(`7c9837a`). `ProtoMember` 번호는 보존해야 한다 — 이 타입은 와이어뿐 아니라 `KingdomMapSnapshotPacket`에 담겨 `KingdomMap.Snapshot` 컬럼에 **직렬화되어 저장**된다.

#### S1-B `[Entity]` 생성 규칙 (확정)

**`Pk`** — CSV `Key` 컬럼의 `pk` 토큰 (`ModelGenerator.cs:319`가 이미 쓰고 있다).

**`ScopeKey`** — **`User/` 폴더 한정.**

```
User/  : fk 토큰이 붙은 컬럼 → ScopeKey
User/Player : fk 없음(스코프 루트) → PK를 ScopeKey 로
Auth/  : ScopeKey 없음
Center/: ScopeKey 없음
```

CSV는 **한 줄도 새로 안 쓴다** — `fk` 토큰이 이미 있고 이미 소비되고 있다(`ModelGenerator.cs:376`이 Liquibase FK를 만든다). `scope` 같은 새 토큰을 넣으면 같은 사실이 두 군데 선언되어 드리프트한다.

**왜 User 한정인가 — 코드가 근거다.** 소유자 개념(ambient owner + 자동 `WHERE` + 소유자별 캐시 버킷)이 있는 건 User 계열뿐이다.

| 계열 | 소유자 | 근거 |
|---|---|---|
| User | **있음** | `UserComponentBase.LoadFromDb`가 **모든** 조회에 `WHERE PlayerId = RpcCtx.PlayerId`를 자동으로 건다 |
| Auth | 없음 | `AuthComponentBase`에 소유자 개념이 전무하다. `GetMdlAsync(dbFetch)`가 임의 람다를 받을 뿐 |
| Center | 없음 | 동일. Schedule은 전체 조회 |

Auth의 AccountId 조회는 전부 **인자 기반 명시 조회**이지 ambient 스코프가 아니다. 전수 조사 결과: `ChannelComponent.GetListAsync(accountId)`(비-PK 리스트 → **T2**, Channel의 PK는 `Key`), `SessionComponent.GetByAccountIdAsync`(AccountId가 PK + 자체 포인터 캐시, 5.3), `PlayerMapComponent`(AccountId가 PK 그 자체), `DeviceComponent`(**AccountId 조회 없음** — `Key`=idfv PK 조회만), `AccountComponent`(자기 Id가 PK). **소유자 축 리스트 조회는 Channel 하나뿐이고 그것은 T2다.**

즉 `fk = AccountId`는 지금 코드에서 **DB 참조 무결성 선언**으로만 쓰이고 스코프로는 쓰이지 않는다.

> **주의 (2026-08-14 추가)**: 위 표와 이 문단은 **"지금 코드가 소유자를 ambient로 거는가"** 에 대한 서술이다. **"데이터 모델에 소유자 축이 있는가"** 와는 다른 질문이고, 그쪽 답은 Auth도 **있다**이다 — `Account.Id`가 루트이고 나머지 넷이 전부 `AccountId`를 갖는, User와 같은 모양이다. 둘을 뭉뚱그리면 "Auth엔 소유자가 없다"는 잘못된 결론이 나온다. **§S1-G 참조.** S1이 `ScopeKey`를 User 한정으로 두는 근거는 "Auth에 소유자가 없어서"가 아니라 **"Auth의 스코프 밖 조회를 어디로 보낼지 아직 안 정해서"** 다.

**가드 (실행 시 1종 → 4종으로 늘었다).** 규칙이 애매한 경우는 추측하지 않고 **생성을 실패**시킨다. 조용히 틀린 컬럼을 고르면 그 값이 PK `WHERE` 절과 소유자 필터로 그대로 흘러가고, 증상은 예외 없이 "0행 매치"나 "남의 데이터 조회"로 나타나기 때문이다.

| 가드 | 조건 | 현재 해당 |
|---|---|---|
| `MISSING_PK` | `pk` 토큰이 없다 | 0건 |
| `MISSING_SCOPE_KEY` | User 폴더 / `Player` 아님 / `fk` 0개 | 0건 |
| `AMBIGUOUS_SCOPE_KEY` | User 폴더 / `fk` 2개 이상 | 0건 |
| `SCOPE_ROOT_COMPOSITE_PK` | `Player`의 PK가 복합키 | 0건 |

**`MISSING_SCOPE_KEY`가 왜 필요한가 — 위 표의 `User/Player : fk 없음 → PK를 ScopeKey 로`를 그대로 구현하면 안 된다.** "fk가 없으면 PK를 쓴다"로 일반화하면, 나중에 **fk를 빠뜨린 User 모델이 자기 PK를 ScopeKey로 갖게 된다.** S2 이후 `OwnedSet<T>`가 그 컬럼으로 필터하면 소유자 필터가 사실상 사라진다 — 플레이어 간 데이터가 새는 방향의 실수다. 그래서 `Player`는 **이름으로 특수 처리**하고, 나머지 User 모델의 `fk` 0개는 실패다.

**키 판정에서 `Packet` 전용 필드는 제외한다.** 테이블에 없는 컬럼이고, `GenerateLiquibaseChangeLog`도 같은 기준으로 컬럼을 고른다(`ModelGenerator.cs:282`).

**애트리뷰트 문자열은 템플릿이 아니라 C#에서 만든다.** `ModelTemplate.txt`에 문법을 넣는 대안이 있었으나 기각했다 — 이 생성기는 이미 `[ProtoMember(n)]`(`ModelGenerator.cs:444`)과 `[ProtoContract]`(`:459`)를 C#에서 만들고 있어 규칙이 갈린다. 그리고 `ScopeKey` 유무 판단과 위 가드는 어차피 C#에 있어야 하므로(Scriban으로는 예외를 못 던진다), **값을 고르는 곳과 값이 없을 때 터뜨리는 곳을 붙여 둔다.**

#### S1-C `Session.csv` 수정 — CSV만 틀렸다

```diff
  # Data/Csv/Model/Auth/Session.csv
- Key,VARCHAR(50),,,,pk
+ Key,VARCHAR(50),,,,index
- AccountId,BIGINT UNSIGNED,,,Model,fk
+ AccountId,BIGINT UNSIGNED,,,Model,"pk, fk"
```

| | Session PK | |
|---|---|---|
| ~~실제 DB — `Code/Liquibase/AuthDbChangeLog.yml`~~ | ~~**AccountId**, `Key`에는 `Session_Key_Index`~~ | **거짓. §S1-E 참조** |
| 런타임 — `StartUp.Resource.cs:46` | **AccountId** | 일치 |
| `Session.csv` | `Key` | **혼자 어긋남** |

`index` 토큰은 `ModelGenerator.cs:393-397`에서 `{className}_{FieldName}_Index` = **`Session_Key_Index`** 를 만든다.

> **한때 이렇게 판단했다가 뒤집은 기록**: CSV(`Key`)를 정답으로 보고 런타임을 거기 맞추려 했으나, 그러면 `SessionManager.cs:18-28`의 **세션 키 회전이 조용히 깨진다**. 회전은 `Model.Key`를 새 값으로 바꾼 뒤 UPDATE하는데, PK가 `Key`면 `WHERE Key = <새 키>`가 되어 0행 매치가 된다. **런타임이 정답이고 CSV를 맞춘다** — 이 결론은 유효하다.

#### S1-E 위 판단의 근거가 틀렸다 — 실DB를 직접 조회해 정정 (2026-08-14)

위 표의 "실제 DB = `AuthDbChangeLog.yml`" 이 **거짓이다.** 로컬 MySQL의 `DATABASECHANGELOG`를 조회하니 세 DB 모두 `FILENAME`이 **`CreateLog_*.json`**(생성물)이었다. 손으로 쓴 `*DbChangeLog.yml` 2개는 **한 번도 적용된 적이 없다.** `UpdateAuth/User/Center.bat`도 전부 json을 가리키고 있었고, yml은 README 예시에만 있었다. 두 파일은 삭제했다(`9caa468`).

실제 스키마는 이랬다.
```
session  PRIMARY             1  Key         <- PK는 Key였다
session  FK_Session_Account  1  AccountId   <- FK 제약이 만든 부산물
```
런타임은 `AccountId`를 PK로 등록하고 있었으므로 **런타임과 DB가 실제로 어긋나 있었다.** 조회 자체는 FK 인덱스를 타서 성능 사고는 아니었지만, **non-unique라 "계정당 세션 1개"를 DB가 강제하지 않았다.** 코드는 그것을 전제한다. 즉 S1-C의 수정은 "정합화"가 아니라 **코드의 전제를 DB에 반영하는 실제 스키마 변경**이었다.

그리고 **마이그레이션이 필요 없다는 것도 거짓이다.** 생성 changelog는 `changeSet id = 테이블명`인 create-only 구조라, CSV를 고쳐 재생성하면 **이미 적용된 changeSet의 내용이 바뀐다.** liquibase는 저장된 MD5와 달라 거부한다. 실제로 그렇게 터졌다.
```
CreateLog_Auth.json::Session::seogyoung
  was: 9:1c7ded9dbaf5f33f4c46713f142c5a8d
  now: 9:3898fc668f846ecd257ead7cdf4750db
```
지킬 데이터가 없는 로컬 개발 DB뿐이므로 **drop 후 재생성**으로 반영했다(`Recreate.bat`, `9caa468`). `clearCheckSums`는 쓰면 안 된다 — 체크섬만 지우고 DB는 옛 스키마 그대로인데 liquibase는 "적용됨"으로 믿게 되어 드리프트가 영구히 안 보이게 된다.

**남는 한계(S3 이후 스키마를 건드릴 때 다시 봐야 한다)**: 이 구조에는 증분 마이그레이션 수단이 없다. 보존해야 할 데이터가 있는 DB가 생기는 순간 drop-and-recreate는 쓸 수 없다. 손으로 쓰던 yml이 그 시도였으나 적용되지 않은 채 방치됐고(User 쪽은 모델 19개 중 5개 테이블에서 멈춰 있었다), 그 상태로 남아 있었기 때문에 이 세션에서 판단을 한 번 틀렸다.

**교훈**: 저장소 안의 파일만 보고 "무엇이 실제로 적용된 상태인가"를 판단하지 말 것. `DATABASECHANGELOG` 한 번 조회로 끝나는 문제였다.

**남는 사실(지금 조치 불필요)**: `Session_Key_Index`는 non-unique인데 `TryGetByKeyAsync`가 단건을 기대한다. 키가 GUID라 실질 충돌은 없지만 DB가 강제하지는 않는다.

**필드는 `Pk`(필수) + `ScopeKey`(선택) 둘뿐이다.** 확정 근거는 아래 두 항목:

**`Table`을 넣지 않는다.** `DapperExtension.cs:31-35`가 클래스명에서 `Model`을 떼어 테이블명을 만들고 **오버라이드 경로가 존재하지 않는다.** 규칙을 벗어나는 모델이 현재 0개이므로 지금 넣으면 순수 추측이다. 벗어나는 모델이 생기면 그때 추가한다(R7).

**`Cache`/`SlidingTtl`도 S1에는 넣지 않는다 — TODO 주석으로만 남긴다.** 5.4가 "나중에 넣으면 20개를 두 번 손댄다"고 적었으나 **그 논거는 약하다**: attribute 20줄에 필드 하나 추가하는 것은 기계적 편집인 반면, 일반화가 맞지 않는 enum을 20개 모델에 먼저 박는 비용이 훨씬 크다. 5.4에서 살아남는 것은 **발견 자체**(정책이 실제로 5종이고 `OwnedSet<T>`가 1종만 전제한다 / sliding TTL이 설계에 없었다)이며, 그것은 **S2에서 `OwnedSet<T>`를 설계할 때의 제약**으로 이월한다.

**`Owner` → `ScopeKey` 개명(2026-08-12).** `Owner`는 "무엇의 소유자인가"가 드러나지 않는다. 이 필드의 실제 의미는 **User 스코프 안에서 행을 소유자별로 가르는 컬럼**이고, `GameDb.User(playerId)` → 그 컬럼으로 필터라는 연결이 이름에 드러나야 한다. `Pk`와 같이 "컬럼명을 담는 필드"라는 명명 규칙도 일치한다. `PartitionKey`는 기각 — 이 저장소에는 이미 AccountId 기준 물리 샤딩(`GlobalDbRepo._shardMap`)이 따로 있어 서로 다른 축이 같은 이름을 쓰게 된다.

#### S1-D `AssertMatches` / `Snapshot()` 철회 — 검사를 `Init` 안으로 옮겼다

계획은 이랬다.
```csharp
EntityRegistry.ScanAndRegister(typeof(CookieModel).Assembly);
EntityRegistry.AssertMatches(ModelRegistration.Snapshot());   // 계획 — 철회됨
```

**두 가지가 틀렸다.**
1. `ModelRegistration.Snapshot()`은 **존재하지 않는 API**였다. `DapperExtension`은 읽기 경로가 전혀 없고 `InMemoryPkRegistry.GetKeyFields`는 미등록 시 예외를 던진다. 새로 만들어야 했다.
2. 더 중요한 것 — **집합 비교는 `RaidServer` 부팅을 깨뜨린다.** RaidServer는 `Session`/`Player` **2개만** 등록한다. 스캔은 19개를 찾으므로 "19 vs 2"로 **항상** 불일치다.

그래서 검사를 `ModelRegistration.Init` 안으로 옮겼다.
```csharp
if (_registeredDict.TryGetValue(type, out var prev) && !prev.SequenceEqual(keyFields))
    throw new InvalidOperationException($"PK_REGISTRATION_CONFLICT:{type.Name} [...] vs [...]");
```
- **타입 단위**라 부분 등록에 안전하다. 겹치는 것만 보고 나머지는 그냥 등록한다.
- 같은 값 재등록은 허용한다 — `ServerTest`가 `WebApplicationFactory`를 여러 번 만든다.
- 새 public API가 필요 없고, **S11에서 손목록이 사라져도 중복 등록 방어로 남는다.** `Snapshot()`/`AssertMatches`는 S11에 같이 버려야 하는 임시 API였다.

`ScanAndRegister`는 리플렉션 예외를 벗겨서 던진다. 감싼 채로 두면 부팅 로그에 `TargetInvocationException`만 보이고 `PK_REGISTRATION_CONFLICT`가 묻힌다.

**최종 형태** — 각 호스트에 1줄씩:
```csharp
EntityRegistry.ScanAndRegister(typeof(PlayerModel).Assembly);
```

#### S1-F 실제로 가능해진 것 / 못 하게 된 것

**가능해진 것**
- `Server`와 `RaidServer`의 등록 목록이 **어긋날 수 없다.** 계획은 "어긋나면 감지"였는데 실제로는 **구조적으로 불가능**해졌다 — 양쪽이 같은 어셈블리를 스캔하기 때문이다. 감지보다 낫다.
- 손목록과 attribute가 어긋나면 부팅이 즉시 실패한다. **네거티브 테스트로 확인함**: `CookieModel` 등록을 `("Num","PlayerId")`로 뒤집으니 부팅이 죽고 `ServerTest` 17/17이 실패했다. 메시지는 `PK_REGISTRATION_CONFLICT:CookieModel [Num, PlayerId] vs [PlayerId, Num]`. **순서까지 본다.**
- `PlayerModel`의 `ScopeKey`가 `Id`라는 사실이 **데이터로** 표현된다.

**"동작 변화 0"은 거짓이었다 — `RaidServer`의 등록 범위가 2개 → 19개가 된다.**
지금까지는 RaidServer가 등록하지 않은 모델을 건드리면 `InMemoryPkRegistry`의 미등록 예외가 막아 주었다. **그 런타임 가드는 사라진다.** 등록 자체는 메타데이터 캐시라 기능적으로 해롭지 않고, 두 호스트 목록이 어긋날 수 없게 되는 것과 맞바꾼 것이다. 다만 "RaidServer는 Session/Player만 만진다"는 사실이 코드로 강제되지 않게 되었으므로, **S12(RaidServer 전환) 때 이 축소된 표면을 다시 세울지 판단해야 한다.**

**아직 안 되는 것**: 아무것도 이 메타데이터를 소비하지 않는다.

**롤백**: `StartUp` 각 1줄 + `EntityRegistry.cs` + `EntityAttribute.cs` 삭제, `ModelRegistration` 충돌 검사 제거, 생성기 변경 되돌린 뒤 재생성. `Session.csv` / `KingdomMap.csv` / `KingdomDeco.xlsx` / `launchSettings.json` 수정과 Liquibase 배치 정리는 **되돌리지 않는다** — 전부 A안과 무관하게 stale을 고친 것이다.

#### S1-G 이월된 열린 질문 — Auth는 스코프인가 (2026-08-14 재정리)

> **먼저 적었던 것**: "소유자가 User에만 있으므로 `GameDb.Auth()`/`Center()`는 스코프가 아니라 **DB 선택**이다. 셋을 `DataScope`로 균일하게 묶으면 Auth/Center에 의미 없는 인자와 빈 규칙이 따라다닌다."
>
> **이 전제가 부정확하다.** 아래로 대체한다.

**(1) Auth의 데이터 모델에는 소유자 축이 있다. User와 같은 모양이다.**

```
User                          Auth
Player     Id (루트)          Account    Id (루트)
나머지 13  PlayerId (fk)      Channel    AccountId (fk)
                              Device     AccountId (fk)
                              Session    AccountId (pk+fk)
                              PlayerMap  AccountId (pk)
```

루트 하나 + 나머지 전부가 루트를 가리키는 컬럼. **구조가 동일하다.** 앞선 서술은 *데이터 모델에 소유자 축이 있는가*와 *지금 코드가 그것을 ambient로 거는가*를 구분하지 않았다. 후자만 보면 Auth는 소유자가 없지만(`AuthComponentBase`는 호출자가 준 람다를 돌릴 뿐이다), 전자는 User와 다르지 않다. §S1-B의 "왜 User 한정인가" 표는 **후자에 대한 서술로만 읽어야 한다.**

**(2) 진짜 비대칭은 "Auth는 신원을 알아내는 계층"이라는 것이다.**

Auth 조회 진입점은 두 부류로 갈린다.

| | 진입점 | AccountId |
|---|---|---|
| (a) 스코프 이전 | `DeviceComponent.TryGetAsync(idfv)` · `ChannelComponent.TryGetAsync(key)` · `SessionComponent.TryGetByKeyAsync(key)` · `AccountComponent.CreateAsync()` | **모른다.** 오히려 이것으로 계정을 찾거나 만든다 |
| (b) 스코프 안 | `ChannelComponent.GetListAsync(accountId)` · `SessionComponent.GetByAccountIdAsync` · `AccountComponent.TryGetAsync(id)` | 안다 |

**User 스코프에는 (a)가 없다.** 유저 데이터를 만질 시점엔 `PlayerId`가 확정돼 있다. 그래서 `GameDb.Auth(accountId)` 하나로는 부족하다 — 로그인 첫 쿼리를 보낼 곳이 없어진다. Auth를 User와 똑같이 만들 수 없는 이유는 "소유자 컬럼이 없어서"가 아니라 **"소유자를 알아내는 단계가 있어서"** 다.

**(3) 스코프는 `XxxRepo`의 후계자다.** 이름이 바뀌는 이유는, `UserRepo`가 "저장소"라 불리지만 실제로 하는 일이 **"이 플레이어의 데이터만 보이게 하는 경계"** 이기 때문이다(`LoadFromDb`의 자동 `WHERE PlayerId`, `ListKeyFor(playerId)`의 플레이어별 캐시 버킷). 그 경계를 이름에 담은 것이고 `ScopeKey`(경계를 긋는 컬럼)라는 이름도 여기서 나온다.

```
지금                                   A안
GlobalDbRepo                           GameDb (UoW)
├ OwnUser : UserRepo                   ├ User(shardId, playerId) → UserScope
├ Auth    : AuthRepo                   ├ Auth(...)               → AuthScope
├ Center  : CenterRepo                 ├ Center()                → CenterScope
└ AllUser : AllUserRepo                └ AllShards               (스코프 밖)

UserRepo : 컴포넌트 11개를 손으로 나열   UserScope : Set<T>() 하나
RpcCtx.PlayerId 를 암묵적으로 읽음       playerId 를 인자로 받음
```

따라서 **`AuthScope(accountId)`는 `AuthRepo`에 없던 경계를 새로 긋는 일**이다. 그래서 판단이 필요했다.

**(4) 갱신된 결론 — Q1은 지금 닫고, Q2만 S4로 넘긴다**

두 질문을 분리해야 한다. 앞서 이 둘을 뭉쳐 통째로 S4로 미뤘는데, **Q1은 지금 답할 수 있다.**

| | 질문 | 답 |
|---|---|---|
| **Q1** | `AuthScope(accountId)`가 존재하는가 | **그렇다. 지금 확정한다.** |
| **Q2** | `[Entity]`에 Auth `ScopeKey`를 붙이는가 | **S4에서 판단** |

**Q1을 지금 닫을 수 있는 근거 — "스코프 밖 진입점"은 이미 A안에 있는 패턴이다.** Auth의 (a)가 특별한 문제라고 봤던 것이 착각이었고, 같은 성격의 것이 이미 둘 있다.
- `GameDb.AllShards.FindPlayerByNameAsync(name)` — `PlayerId`를 모르는 조회 (§S11)
- `PlayerMap`으로 `shardId`를 찾아 `GameDb.User(shardId, playerId)`를 연다 (§S12, GSA `PlayerMapService.TryGetUserRepoByPlayerId` 패턴의 복원)

즉 **"스코프를 여는 데 필요한 조회는 스코프 밖에 둔다"**는 규칙이 이미 있다. Auth의 기기 키·채널 키·세션 키 조회와 계정 생성은 정확히 같은 성격이므로 같은 자리에 놓으면 된다. 새 규칙이 필요 없다.

- **Center** — 소유자 축이 실제로 없다(Schedule은 전체 조회). 스코프가 아니라 DB 선택이 맞다.
- **Auth** — `AuthScope(accountId)`가 존재하고, (a)는 스코프 밖 진입점으로 간다.

**(5) `IDataScope` 인터페이스는 지금 만들지 않는다.**
앞서 "공유는 인터페이스로"라고 적었으나, 그 전에 물었어야 할 것은 **"지금 공통 인터페이스가 필요한가"** 였다. 근거로 든 것은 "`GameDb.CommitAsync`가 스코프를 순회해야 한다"였는데, **`GameDb`가 dirty 엔티티를 직접 들고 있으면 그 요구가 사라진다.** 스코프는 조회 진입점이고 dirty 목록은 UoW 소유라고 보면 된다.

그러므로 `UserScope`/`AuthScope`/`CenterScope`는 각자 자기 키만 갖는 **독립 클래스**로 시작한다(`AuthScope`에 `PlayerId` 같은 것이 생기지 않는다는 목적은 이것으로 이미 달성된다). 공유되는 것은 `OwnedSet<T>` 하나다. **S2에서 실제로 공통 처리가 필요해지면 그때 인터페이스를 뽑는다**(R7).

**(6) S1은 손대지 않는다 — Q2를 S4로 미루는 이유.** `ScopeKey`를 Auth에 붙이는 것은 생성기 규칙 한 줄과 재생성이면 되지만, **`ScopeKey`는 선언이 아니라 동작을 만든다** — 자동 `WHERE`, 쓰기 시 소유자 검증, 캐시 버킷. 붙이는 순간 (a)의 조회들과 충돌한다(기기 키 조회에 `WHERE AccountId`가 붙으면 0행이 된다). Q1이 정해졌어도 **어떤 조회가 스코프 안이고 어떤 것이 밖인지는 실제로 옮겨 봐야** 안다. S4 파일럿이 바로 **Channel/Device/Account**이고 (a)/(b) 두 부류를 `OwnedSet<T>`로 다시 써보는 스텝이다.

---

### S2 — `GameDb` / `Scope` / `OwnedSet<T>` 신설 (미사용)

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
    public GameDb(DbSessionManager sessions, ICacheSession cache) { ... }   // ILogger 는 쓸 곳이 없어 뺐다

    public UserScope   User(int shardId, ulong playerId);
    public AuthScope   Auth(ulong accountId);   // Q1 확정 - 인자를 받는다 (§S1-G)
    public CenterScope Center();

    // 이관 기간에는 tx 를 건드리지 않는다 — GlobalDbRepo.CommitAsync 가 단일 커밋 주체 (§7.1-②).
    // dirty flush 는 없다. 쓰기는 즉시 반영이며 GlobalDbRepo 와의 연결선도 없다 (§S2-H).
}
```

**dirty 모델은 이 스텝에서 구현했다가 같은 세션에 철회했다. §S2-H 를 먼저 읽을 것.** `ModelBase` 는 손대지 않으며, 커밋 경로는 S2 이전과 동일한 두 줄로 남는다.

**직후 가능해지는 것**: 없음. 컴파일만 된다.

**아직 안 되는 것**: 전부. **이 스텝의 목적은 다음 스텝에서 되돌릴 게 없게 만드는 것**이다.

**롤백**: 신규 파일 삭제 + DI 1줄.

---

#### S2-A 실행 결과 (2026-08-19~20, branch `db-refactor`)

| | 계획 | 결과 |
|---|---|---|
| 1 | `GameDb` / 스코프 3 / `OwnedSet<T>` 신설 | ✅ `Code/DbModel/Data/` 6파일 |
| 2 | `ModelBase`에 dirty 3종 | ⛔ **구현했다가 철회** (§S2-H). 충돌은 §S2-C ① |
| 3 | DI 1줄 + `GlobalDbRepo.CommitAsync` 연결 | ✅ DI 만. 커밋 연결은 철회로 불필요해짐 (§S2-H) |
| 4 | 커넥션 지연 오픈 (5.11) | ✅ `OwnedSet` 첫 조회에서 연다 |
| 5 | `MarkDirty()`가 `UpdateTime` 스탬프 (5.6) | ⛔ 철회. `UpdateAsync` 가 찍는다 — 오늘과 동일 |
| 6 | 캐시 2종만 안다 (5.4.1) | ⚠ 실제로는 **1종**이다 — `ScopeKey`+`CacheTag` 둘 다 요구하며 "캐시 없음"은 `OwnedSet` 밖이다 (§S2-J) |
| 7 | `GameDb.Utility` (5.2) | ⏸ **S10.5로 미룸** — 소비자가 거기뿐이라 지금 만들면 미사용 멤버 |
| 8 | lazy BEGIN 판단 (5.2) | ⏸ **S11로 미룸** — S2 코드는 아무도 안 써서 실증 불가 |
| 9 | — | ➕ 엔진에 `SelectListByColumnAsync` 추가 (§S2-C ③) |
| 10 | — | ➕ `DbConnectionResolver` 추출 — 샤드 맵 2벌 방지 |

**검증**: 빌드 0에러/신규경고 0, `ServerTest` 17/17, 생성 SQL 직접 확인(§S2-C ①).
**검증 한계**: `SelectListByColumnAsync`의 MySQL 경로는 호출자가 없어 미검증. `GlobalDbRepo` 변경은 InMemory로만 지나갔다.

#### S2-B 이 문서의 S2 코드가 두 군데 낡아 있었다 — 정정

`f7f5607`이 §S1-G 서술만 고치고 코드 블록을 안 고쳤다. 위 블록은 정정된 것이고, 무엇이 틀렸었는지 남긴다.

| 틀렸던 것 | 왜 |
|---|---|
| `public AuthScope Auth();` | Q1이 "`AuthScope(accountId)`는 존재한다"로 닫혔는데 시그니처가 인자 없는 채였다 |
| `foreach (var scope in _scopes) foreach (var set in scope.LoadedSets)` | **`IDataScope`를 철회한 근거가 바로 이 순회였다.** "GameDb가 dirty를 직접 들면 순회 요구가 사라진다"고 적어놓고 코드는 순회하고 있었다 |

두 번째가 실행에서 확인됐다. 체인이 하나로 닫힌다:

```
0.6  모델은 DB 참조를 갖지 않는다
 ->  MarkDirty() 는 bool 만 세운다. 아무에게도 알릴 수 없다
 ->  누군가 커밋 직전에 훑어야 한다
 ->  S1-G  dirty 목록은 UoW 소유
 ->  GameDb 가 로드된 OwnedSet 을 직접 든다. 스코프는 순회 대상이 아니다
```

`IDataScope`는 만들지 않았지만 **`IDirtyFlush`(메서드 1개)는 필요했다** — `_sets` 딕셔너리의 값이 이종이라 제네릭이 아닌 진입점이 없으면 순회가 불가능하다.

> **이 절은 이제 이력이다 (§S2-H).** dirty 를 철회하면서 위 체인 전체가 사라졌다 — 훑을 것이 없으므로 `_sets` 도 `IDirtyFlush` 도 없앴다. 남겨두는 이유는, "모델이 DB 참조를 갖지 않는다"는 전제 하나가 어디까지 파급되는지가 여기 그대로 보이기 때문이다.

#### S2-C 코드를 쓰면서 나온 것 4건

**① 🔴 `IsDirty`를 프로퍼티로 넣으면 SQL이 깨진다.**

`DapperExtension.SetQueryParameter`가 `type.GetProperties(Public|Instance)`로 INSERT/UPDATE 필드 목록을 만든다. 즉 **`ModelBase`에 public 프로퍼티를 추가하는 것은 곧 DB 컬럼 선언**이다. 그냥 넣었으면 `INSERT INTO Point (..., IsDirty)`로 MySQL에서 즉시 실패한다. 설계 문서 어디에도 없던 충돌이다.

`ServerTest`는 InMemory라 이것을 잡지 못한다(InMemory는 conditions의 프로퍼티만 읽는다). 그래서 `SetQueryParameter`에서 이름으로 제외하고, 생성 SQL을 직접 뽑아 확인했다:

```
PointModel  FIELDS : PlayerId, Num, Amount, AccAmount, UpdateTime, CreateTime
PlayerModel FIELDS : Id, AccountId, ... KingdomExp, UpdateTime, CreateTime
```

**dirty 철회로 이 변경은 되돌렸다.** 다만 **발견은 유효하다**: `ModelBase`에 public 프로퍼티를 추가하는 것은 곧 DB 컬럼 선언이며, InMemory 테스트가 그것을 잡지 못한다. 다음에 `ModelBase`를 건드릴 때 같은 함정이 기다린다.

**② 🟠 인스턴스 동일성은 최적화가 아니라 dirty의 전제다.**

```csharp
// SqlRepository.GetListAsync
if (cached.Hit) { return [.. cached.Value]; }   // 캐시 히트마다 새 List
```

Redis면 원소까지 매번 새 객체다. `MarkDirty()`를 찍은 인스턴스를 flush 때 찾을 수 없다. `InMemoryRepository`는 매번 전체 스캔이라 또 다르게 동작한다. 그래서 `OwnedSet<T>`가 로드분(`_tracked`)을 요청 내내 붙들어야 하는데, **이것이 없으면 dirty 모델 자체가 성립하지 않는다.** 설계 문서는 이 사실을 "스코프 내 캐싱"이라고만 적어 성능 최적화처럼 읽히게 두었다.

**dirty 철회로 `_tracked` 도 없앴다** — 추적하지 않으면 인스턴스 동일성이 필요 없고, `OwnedSet` 은 무상태가 된다(§S2-H). 이 발견은 **dirty 가 왜 비싼지의 근거**로 남는다: 지연 쓰기를 하려면 읽기 의미까지 함께 바꿔야 한다.

**③ 🟠 컬럼명 기반 조회를 엔진에 추가해야 했다.**

`OwnedSet<T>`는 제네릭 하나라 `new { PlayerId = ... }` 같은 익명 타입을 만들 수 없다. 그리고 스코프 키 컬럼명이 엔티티마다 다르다 — User 13개는 `PlayerId`인데 `PlayerModel`만 `Id`라서, 지금도 `PlayerComponent`가 `LoadFromDb`를 **유일하게** override하고 있다.

`Dictionary<string,object>`를 조건으로 넘기는 것은 안 된다. Dapper는 받지만 `InMemoryDbExecutor.MatchAll`이 조건 객체의 `GetProperties()`를 읽으므로 `Comparer`/`Count`/`Keys`를 조건으로 착각한다.

→ `IDbExecutor.SelectListByColumnAsync<T>(string column, object value)`를 Dapper/InMemory 양쪽에 추가했다. 문서가 S3에 두었던 종류의 엔진 추가가 S2로 당겨진 것이다.

**④ 🟡 `AuthScope`/`CenterScope`는 `Owned<T>()`를 갖지 않는다.**

처음에는 던지는 `Owned<T>()`를 두었는데, §S2-E에서 Q2가 잠정 정리되면서 아예 없앴다. 여기서 조용히 동작하게 두면 **코드가 Q2를 먼저 정해버린다** — Auth에 스코프 키 없이 전체 테이블을 읽는 경로가 생기고 그것이 "정상 동작"으로 굳는다. S2에서 실제로 열리는 것은 `UserScope.Owned<T>()` 하나다.

#### S2-D 타입을 가르는 축은 캐시도 DB도 아니라 **로드 단위**다

`OwnedSet<T>`가 감당하는 것은 "한 소유자의 컬렉션" 하나이고, 그 전제는 **"소유자당 행 수가 유계이고 작다"** 이다. 이 문장이 없으면 통계성 엔티티가 `[Entity(ScopeKey=...)]`를 달고 조용히 들어온다 — 로그 2개가 지금 딱 그 직전 상태다.

| | 로드 단위 | 대상 | 쓰기 | 스텝 |
|---|---|---|---|---|
| **A** | 소유자 전체 | Point / Cookie / Item / Kingdom / World … (11) | 즉시(§S2-H 에서 dirty 철회) | S5~S10 |
| **B** | **안 함** (컬렉션 없음) | CashChangeLog / GachaLog (2) | 즉시 INSERT | S13 |
| **C** | 키 하나 | Account / Channel / Device / Session / PlayerMap (5) | 즉시 | S4 |
| **D** | 전역 전체 | Schedule (1) | 즉시 + 무효화 | S8 |
| **E** | 전 샤드 | (Player 검색) | 없음 | S11 |

**쓰기는 다섯 경우 모두 즉시다** (§S2-H 에서 dirty 를 철회했다). 로드 단위만 다르고 쓰기 규칙은 하나다 — 이것이 철회로 얻은 가장 큰 단순화다.

B는 별도 동사를 두지 않는다. 로그도 INSERT이므로 `CreateAsync`이고, 다른 것은 **경로**다 — `scope.Owned<T>().CreateAsync`는 추적되는 컬렉션에 넣고, `scope.CreateAsync(entity)`는 컬렉션이 없는 엔티티용이다. 가드: 캐시 태그가 있는 엔티티를 후자로 넣으면 예외(캐시된 리스트가 어긋난다).

**엔티티별 클래스는 어느 경우에도 생기지 않는다.** 최악이 제네릭 3종이고, 클래스 수가 엔티티 수에 비례하지 않는다는 A안의 본체는 유지된다. §1.1의 "2종"은 낙관이었다.

#### S2-E Auth 5개 — Q2 잠정 = 아니오, 그리고 그 형태 (2026-08-20)

S4로 넘겼던 Q2를 census로 좁혔다. **결론부터: Auth는 `OwnedSet<T>`를 쓰지 않는다.**

**census (전 호출부)**

| 모델 | 실제 조회 축 | 캐시 |
|---|---|---|
| Account | `SelectByPk(Id)` | 없음 |
| Channel | `SelectByPk(Key)` + `SelectList(AccountId)` | 없음 |
| Device | `SelectByPk(Key)` **만** — AccountId 조회 **0건** | 없음 |
| Session | 키→AccountId 포인터 + 값 캐시 | **있음**(전용) |
| PlayerMap | `SelectByPk(AccountId)` | 없음 |

**Session을 빼면 Auth에 캐시가 하나도 없다.** 그리고 소유자 컬렉션이 의미 있는 것은 **Channel 하나**뿐이다 — Device는 AccountId로 조회하는 코드가 없고, Session/PlayerMap은 AccountId가 곧 PK라 "목록"이 성립하지 않고, Account는 자기 Id다.

`ScopeKey`를 붙이면 `OwnedSet`의 핵심 둘(소유자 리스트 캐시 · dirty flush)이 통째로 놀면서, 기기 키/채널 키 조회는 자동 `WHERE AccountId`로 0행이 된다. **얻는 것이 없고 잃는 것이 확실하다.**

**그러면 `AuthScope(accountId)`는 무엇을 하는가 (Q1이 무의미해지지 않는다)**

경계를 **자동 WHERE가 아니라 인자 고정**으로 긋는다. accountId를 스코프 생성 시 묶어두고 조회 메서드가 그것을 넘기므로, **호출부가 다른 계정 것을 조회할 수 없다.** 경계의 실질은 이것이다.

```
GameDb
├ Identity                       <- (a) AccountId 를 모르는 진입점
│   ├ TryGetDeviceAsync(idfv)
│   ├ TryGetChannelAsync(key)
│   ├ TryGetSessionAsync(sessionKey)
│   └ CreateAccountAsync()
└ Auth(accountId) -> AuthScope   <- (b) AccountId 를 아는 것 전부
    ├ GetAccountAsync()
    ├ GetChannelListAsync()        (Active 필터는 T1 확장 메서드)
    ├ GetOrCreateSessionAsync()    <- 포인터 캐시는 여기 전용 코드로 유지 (5.3)
    ├ GetPlayerMapAsync()
    ├ CreateDeviceAsync(idfv)
    ├ CreateChannelAsync(type)
    └ UpdateAsync<T>(entity)       <- 잠정 즉시 쓰기
```

이름 `Identity`는 §S1-G (2)의 규정에서 나온다 — **"Auth는 신원을 알아내는 계층"**. 계정 생성도 신원을 만들어내는 일이므로 `Lookup`보다 맞다.

```csharp
// SignIn — Before
var (foundDevice, mgrDevice)   = await Auth.Device.TryGetAsync(idfv);
var (foundAccount, mgrAccount) = await Auth.Account.TryGetAsync(mgrDevice.Model.AccountId);
var (foundChannel, mgrChannel) = await Auth.Channel.TryGetActiveAsync(mgrAccount.Id);
var mgrSession = await Auth.Session.TouchAsync(mgrAccount.Id);

// SignIn — After
var (found, device) = await _db.Identity.TryGetDeviceAsync(idfv);   // (a) 스코프 밖
var auth    = _db.Auth(device.AccountId);                           // 여기서 경계가 열린다
var account = await auth.GetAccountAsync();
var channel = (await auth.GetChannelListAsync()).Active();          // T1
var session = await auth.GetOrCreateSessionAsync();
```

```csharp
// SignUp — Before : 데이터 계층이 RpcContext 에 쓰고, 두 줄 뒤 다른 컴포넌트가 그것을 읽는다
var mgrAccount = await Auth.Account.CreateAsync();   // _authRepo.RpcContext.SetAccountId(...)
_ = await Auth.Device.CreateAsync(idfv);             //  <- RpcContext.AccountId 를 읽음

// SignUp — After : accountId 가 스코프를 타고 흐른다
var account = await _db.Identity.CreateAccountAsync();
var auth = _db.Auth(account.Id);
await auth.GetOrCreateSessionAsync();
await auth.CreateDeviceAsync(idfv);
await auth.CreateChannelAsync(EChannelType.GUEST);
```

§1.3이 "특히 중요하다"고 지목한 것(데이터 계층의 `RpcContext.SetAccountId`)이 **여기서 저절로 사라진다.**

**클래스 수**: Auth 데이터 접근 파일 13개(Component 5 + Manager 5 + AuthRepo + ComponentBase + ManagerBase) → **2개**(`AuthScope`, `Identity`). 새 제네릭 타입 0개. `KeyedSet<T>` 같은 것을 따로 세우는 것보다 명명 메서드 11개가 싸다 — 엔티티 5개에 조회 축이 둘뿐이라 제네릭이 오히려 인자로 다 드러난다.

**감수하는 것 (명시)**: `AuthScope`는 **손으로 쓰는 표면**이다. Auth 조회가 늘면 메서드가 는다 — User에서 없앤 성질이다. 근거는 (1) Auth는 5개이고 안 늘어난다, (2) 조회가 이질적이다(키 3종 + PK + 목록), (3) 5.4.1의 경계 규칙.

**열린 것 — 언젠가 dirty 로 돌아갈 것인가.** 즉시 쓰기의 이유를 "identity map 이 없어서"라고 적었으나 정확하지 않다. **identity map 은 소유자 리스트 로드를 요구하지 않는다** — EF 는 컬렉션을 통째로 읽지 않고 엔티티 단위로 추적한다. 즉 Auth 든 User 든 기술적으로는 추적이 가능하다. 다만 §S2-H 에서 **dirty 자체를 철회**했으므로 지금은 논점이 아니다. 되돌린다면 User/Auth 를 같이 판단해야 하고, 그때 걸리는 것은 `SessionComponent.UpdateAsync(befSessionKey, ...)` 가 **UPDATE 와 캐시 포인터 무효화의 순서에 의존**한다는 점이다.

**S4가 검증할 것**: 위 형태로 Channel/Device/Account를 실제로 옮겨 (a)/(b) 분류가 맞는지, `Identity` 4개로 충분한지, `UpdateAsync`를 dirty로 돌릴지. Session/PlayerMap은 S7/S12이므로 S4에서는 자리만 잡는다.

#### S2-F 네이밍 규칙 (2026-08-20 확정)

기존 데이터 계층 메서드 이름을 세어보니 규칙이 없었다:

```
11  TouchAsync            <- 최다 동사인데 CRUD 어디에도 대응 안 됨. "없으면 INSERT" 가 이름에 없다
 6  TryGetInternalAsync   <- "Internal" 이 무엇인지 이름에 없음
 4  GetMdlListAsync       <- base 접두사. base 가 사라지면 소멸
 4  GetAsync / 2 TryGetAsync / 1 GetActiveAsync   <- 실패 시 동작이 이름으로 안 갈림
 2  GetAllListAsync       <- "All" 이 전 샤드인지 전체 테이블인지 불명
 1  UpdateAccountAsync    <- 다른 곳은 UpdateAsync
```

**이름은 축 3개로 조립한다: 동사(실패 시 동작) + 대상 + `By<컬럼>`(기본이 아닌 조회 축).**

| 동사 | 뜻 | 반환 | 없을 때 |
|---|---|---|---|
| `Get` | 있어야 한다 | `T` / `List<T>` | **예외** |
| `TryGet` | 없을 수 있다 | `(bool Found, T Value)` | `(false, null)` |
| `GetList` | 목록 | `List<T>` | 빈 목록 |
| `GetOne` | 소유자당 1행인 엔티티 | `T` | 예외 |
| `GetOrCreate` | 없으면 만든다 | `T` | — |
| `Create` | 새로 만든다 | 생성된 `T` | — |
| `Update` | **즉시** 쓴다 | — | — |
| `Delete` | 지운다 | — | — |

정리되는 것: `TouchAsync` → **`GetOrCreateAsync`**(11곳 — 읽기인 줄 알고 부르면 INSERT가 나가던 이름) · `TryGetInternalAsync`·`*Mdl*` → 소멸 · `GetAllListAsync` → `AllShards`로 이동 · `UpdateAccountAsync` → `UpdateAsync` · `Find*`는 쓰지 않는다(`TryGet*`과 중복).

**두 타입의 표면**

| | 읽기 | 생성 | 변경 |
|---|---|---|---|
| `OwnedSet<T>` | `GetListAsync` / `TryGetAsync` / `GetOneAsync` | `CreateAsync` | `UpdateAsync(entity)` — 즉시 |
| `AuthScope` | `GetAccountAsync` 등 | `CreateDeviceAsync` | `UpdateAsync<T>(entity)` — 즉시 |

**쓰기 규칙은 하나다 — 전부 `Update`, 전부 즉시.** 한때 "추적되는 것은 `MarkDirty`, 아닌 것은 `Update`"로 갈랐으나 dirty 를 철회하면서 그 분기가 사라졌다(§S2-H). 읽는 사람이 외울 규칙이 하나 줄었다.

**지역변수 (2026-08-20 추가 — S4 스케치에서 나왔다)**

메서드 이름 규칙만 정해뒀는데, S4 혼재 코드를 그려보니 호출부에서 **스코프인지 모델인지 이름으로 안 갈리는** 문제가 먼저 걸렸다. 스코프 객체가 데이터 프로퍼티를 노출하기 때문이다 — `authScope.AccountId` 와 `account.Id`, `userScope.PlayerId` 와 `player.Id` 가 호출부에서는 둘 다 "값 든 객체"로 읽힌다.

| 규칙 | 예 |
|---|---|
| 스코프 변수는 **`Scope` 접미사** | `authScope`, `userScope`, `centerScope` |
| 모델 변수는 접미사 없음 | `account`, `channel`, `device`, `player` |
| 옛 경로(Manager)는 기존 `mgr` 접두사 유지 | `mgrSession` |
| **접두사는 같은 종류가 한 메서드에 둘 이상일 때만** | `device` (1개라 없음) / `foundAccount`·`newAccount` (2개) |
| 구분 접두사는 `found` / `new` | `origin` 은 쓰지 않는다 |

`mgr` 을 남기는 것은 이관 기간 한정으로 값이 있다 — **`mgr` 이 붙어 있으면 옛 경로**라는 표시가 되어, 한 메서드 안에서 신/구가 섞인 것이 눈에 보인다. S11 에서 Manager 가 사라지면 같이 사라진다.

`origin` 을 버리는 이유: 지금 `AuthService.cs:27,30,33` 의 `originMgrAccount` 계열은 **의미가 아니라 C# 스코프 회피용**이다(안쪽 블록과 바깥 블록이 같은 이름을 못 쓴다 — CS0136). 이름이 아무것도 말하지 않으므로 `found`/`new` 로 바꾼다.

**정렬 금지**: `=` 나 값을 세로로 맞추려고 공백을 채우지 않는다. 이름 하나만 길어져도 주변 줄을 전부 건드리게 되어 diff 가 부풀고, 실제 변경과 정렬 변경이 한 커밋에 섞인다.

**남은 불편, 일부러 안 고쳤다**: `SignUpAsync` 는 한 메서드에 두 흐름이 있어 `foundAuthScope` / `newAuthScope` 가 된다. 스코프는 의미상 "찾은" 것도 "새" 것도 아니므로(`_db.Auth(id)` 는 항상 성공하는 경계 핸들이고 found/new 가 걸리는 것은 그것이 감싸는 **계정**이다) 접두사가 정확히는 거짓말을 한다. 메서드를 둘로 쪼개면 사라지지만 그것은 S4 범위 밖이다. **S4 를 끝내고 실물을 본 뒤 다시 본다** — 미리 정하지 않는다(§S2-I).

#### S2-G 타입 이름 — `DataSet`을 버렸다

`DataSet<T>` → `OwnedDataSet<T>` → **`OwnedSet<T>`**, 접근자 `scope.Set<T>()` → **`scope.Owned<T>()`**.

| 후보 | 왜 탈락 |
|---|---|
| `DataSet<T>` | `System.Data.DataSet`이 BCL에 있고 이 저장소에 `using System.Data;`가 이미 있다. 그리고 "모든 데이터 접근의 제네릭"으로 읽히는데 19개 중 11개에만 해당한다 |
| `UserDbScopedDataSet<T>` | 좁은 축이 DB가 아니라 로드 단위다. Q2가 "예"로 뒤집히면 거짓이 된다 |
| `OwnedList<T>` | `GetListAsync`와 헷갈린다 |
| `TrackedSet<T>` | 추적은 **보편화될 수 있다**(§S2-E의 열린 항목). 구분 이름으로 못 쓴다 |

`Owned`는 변하지 않는 축(로드 단위)을 말하고, 메서드가 명사라 `Set<T>()`처럼 "설정한다"로 오독되지 않는다.

---

#### S2-H dirty 모델을 철회한다 (2026-08-20) — S0-1 결정 번복

**S0-1은 "(c) dirty 플래그 + 커밋 시 flush"로 확정돼 있었고, S2에서 실제로 구현했다가 같은 세션에 걷어냈다.** 무엇이 판단을 뒤집었는지 남긴다.

**바뀐 것은 근거가 아니라 증거다.** S0-1을 정할 때의 논거("명시적 Save는 N개 호출부에 실수 표면을 만든다")는 지금도 맞다. 다만 S2를 실제로 짜보니 **파생 비용이 사용처보다 먼저 쌓였다.**

| | dirty가 만든 것 | 상태 |
|---|---|---|
| 1 | S0-4 슬라이스 전체 — 커밋 경계를 유저 락 안으로 (쓰기가 락 밖으로 나가 lost update) | 커밋됨 (`7aba510`) |
| 2 | `MySqlLockService` 전용 커넥션 — 1의 선결 조건 | 커밋됨 (`1bf5a39`) |
| 3 | `IsDirty`가 INSERT/UPDATE 필드 목록에 섞이는 충돌 (§S2-C ①) | S2에서 발견 |
| 4 | 요청 단위 identity map 강제 → 읽기 의미 변화 (§S2-C ②) | S2에서 발견 |
| 5 | INSERT 즉시 / UPDATE 지연이라는 비대칭 | 설계에 있었음 |
| 6 | T3 raw 쿼리 실행 전 flush 규칙 | 설계에 있었음 |
| 7 | Session 키 회전의 UPDATE-무효화 순서 문제 | 미해결 |

**7개가 나왔는데 dirty를 쓰는 코드는 한 줄도 없었다.** 이 비율은 시간이 지나도 좋아지지 않는다 — 앞으로 들어오는 기능마다 이 규칙을 학습해야 하므로 오히려 늘어난다.

**두 번째 근거: 이 저장소는 `889d398`에서 EF Core를 지웠다.** dirty 모델은 EF의 change tracking을 손으로 다시 만드는 것인데, EF가 함께 주는 것(스냅샷 비교, FK 순서 정렬, 관계 fixup, 동시성 토큰)은 가져오지 않는다. ORM에서 가장 미묘한 기능만 떼어다 직접 구현하는 모양이 된다.

**세 번째 근거 — 거래의 방향이 틀렸다.**

| | 실수 | 드러나는 방식 |
|---|---|---|
| dirty가 막으려는 것 | 저장을 깜빡한다 | **시끄럽다** — 값이 안 남으므로 테스트 한 번에 드러난다 |
| dirty가 만드는 것 | 쓰기가 락 밖에서 일어난다 / raw 쿼리가 옛 값을 본다 / 캐시 무효화 순서가 어긋난다 | **조용하다** — 동시성 조건에서만 나오고 재현이 어렵다 |

**시끄러운 버그를 조용한 버그로 바꾸는 거래는 나쁘다.** 이것이 핵심 근거다.

**철회하면 연쇄적으로 단순해진다.** 추적이 없으면 identity map이 필요 없고 → `OwnedSet`이 무상태가 되고 → `GameDb`의 `_sets`가 없어지고 → `IDirtyFlush`가 없어지고 → **`GlobalDbRepo`가 `GameDb`를 참조할 이유가 사라진다.** 커밋 경로는 원래의 두 줄로 돌아간다.

```csharp
// 철회 후 GlobalDbRepo.CommitAsync — S2 이전과 동일하다
_dbSessionManager.Commit();
await _cacheSession.FlushPendingWritesAsync();
```

**남는 것**: `OwnedSet<T>.UpdateAsync(entity)` — 오늘 `UserComponentBase.UpdateMdlAsync`와 같다. 새 개념이 0개다.

**잃는 것 (정직하게)**: 같은 행을 한 요청에서 여러 번 고치면 UPDATE가 여러 번 나간다(오늘도 그렇다). InMemory 원자성 개선(테스트 환경 한정). 그리고 §3.7이 지적한 문제 — 도메인 메서드가 모델을 고쳤는데 아무도 저장하지 않는 것 — 이 남는다. **이것은 오늘 코드가 이미 안고 사는 위험이고 새로 생기는 것이 아니다.**

**되돌리지 않는 것**: S0-4(커밋 경계 이동, `7aba510`)와 락 커넥션 분리(`1bf5a39`)는 유지한다. 동기는 dirty였지만 그 자체로 틀린 코드가 아니고 이미 커밋됐다. 이 둘을 나눠 커밋해둔 판단(§4.2)이 여기서 값을 했다.

**다시 넣는 조건**: S5에서 재화 4종을 옮길 때 명시적 저장 호출이 실제로 문제를 일으키면 그때 붙인다. 그때는 붙일 근거가 코드에 있을 것이다 — 지금은 없었다.

#### S2-I 계획 자체에 대한 판단 (2026-08-20)

설계 문서 두 개가 **151KB**인데 실행된 스텝은 S1, S2 둘이다. S3~S13이 코드 한 줄 없이 미리 설계돼 있다.

이번 세션이 그 비용을 보여줬다 — S2를 실제로 짜자마자 문서 두 군데가 낡은 것이 드러났고(§S2-B), `IsDirty`/SQL 충돌은 어떤 설계 문서에도 없었고(§S2-C ①), Auth 5개의 형태는 census를 다시 돌리고 나서야 정해졌고(§S2-E), S0-1은 통째로 뒤집혔다(§S2-H). **코드가 문서보다 빠르게 답을 준다.**

따라서:

- **S3~S13은 "예정"이 아니라 "가설"로 읽는다.** S4를 실행한 뒤 다시 쓴다.
- **다음 행동은 S4다.** S4는 원래부터 "여기서 아니면 A안 중단"으로 못 박아둔 게이트이고, dirty가 빠진 지금 S4는 `OwnedSet`과 Auth 형태만 검증하면 된다 — 검증 대상이 줄었다.
- S3(엔진 `DeleteAsync`)은 소비자가 생길 때 함께 한다.

---

#### S2-J 커밋 전 리뷰 — `OwnedSet<T>`는 캐시되는 소유자 리스트 전용이다 (2026-08-20)

S2를 커밋하기 전에 코드를 다시 읽다가 **주석과 코드가 서로 다른 말을 하고 있는 것**을 찾았다. 둘 중 무엇이 맞는지가 곧 결정이었으므로 남긴다.

`OwnedSet<T>` 주석은 리뷰 5.4.1을 받아 이렇게 적혀 있었다.

> 캐시 정책은 2종뿐이다: 소유자 리스트 캐시 / 캐시 없음. 둘 중 어디인지는 `[Entity].ScopeKey` 유무로 갈리며 따로 선언하지 않는다.

그런데 생성자는 `ScopeKey`와 `CacheTag`를 **둘 다** 요구하고 하나라도 없으면 던진다. 즉 코드에는 "캐시 없음" 모드가 없다. 그리고 이 차이에 걸리는 엔티티가 이미 존재한다.

| | 수 | 비고 |
|---|---|---|
| `[Entity].ScopeKey` 가 있는 모델 | 13 | User 폴더 전체 |
| `CacheKeyTags.ByModelType` 에 있는 User 모델 | 11 | |
| 차이 | **2** | `CashChangeLogModel`, `GachaLogModel` |

호출부가 0개라 오늘 터지지는 않지만, S5에서 `ChangeSet`과 함께 감사 로그 쓰기를 붙이는 순간 정면으로 부딪힌다.

**두 선택지와 판단:**

| | 내용 | 비용 |
|---|---|---|
| (a) 주석대로 | `CacheTag` 가 없으면 캐시를 건너뛰고 DB 직행 | `IRepository`에 캐시 없는 경로가 없다 → 엔진 변경(S3급) |
| (b) 코드대로 | `OwnedSet<T>`는 캐시되는 소유자 리스트 전용, 로그류는 밖 | 주석 수정만 |

**(b)를 택했다.** 근거는 §S2-I와 같다 — 소비자가 없는데 엔진에 분기를 먼저 넣지 않는다. S3(`DeleteAsync`)을 "소비자가 생길 때"로 미룬 것과 같은 판단이고, 감사 로그는 애초에 **로드 단위가 다르다**(append-only라 소유자 리스트로 읽지 않는다). §S2-D가 "타입을 가르는 축은 캐시도 DB도 아니라 로드 단위다"라고 적어놓고, 정작 이 두 개를 로드 단위가 아니라 캐시 유무로 밀어내려 했던 것이 앞뒤가 맞지 않았다.

**리뷰 5.4.1의 "캐시 2종"은 이제 이렇게 읽는다**: 캐시 없음은 `OwnedSet<T>`의 두 번째 모드가 아니라 **`OwnedSet<T>` 밖**이라는 뜻이다.

**남는 구멍 (S4에서 같이 본다)**: `EntityMeta.VerifyCacheTags`는 맵→엔티티 방향만 검사한다. 새 User 모델에 `[Entity]`만 붙이고 태그를 빠뜨리면 부팅은 통과하고 첫 `Owned<T>()`에서 터진다. S1이 세운 "어긋나면 부팅을 실패시킨다"와 어긋나지만, 역방향 검사는 "ScopeKey 는 있으나 캐시는 안 쓴다"를 선언할 자리가 있어야 짤 수 있다. 지금 그 자리를 만들면 (a)를 반쯤 도입하는 것이 되므로, **감사 로그의 실제 쓰기 경로가 생기는 S5까지 미룬다.**

**같이 고친 것**: 코드 주석 4곳이 `DataSet<T>` / `EntityMeta.Verify` 라는 옛 이름을 달고 있었다(`CacheKeyTags.cs` 3곳, `IDbExecutor.cs` 1곳). §S2-G의 리네임이 주석에 반영되지 않은 것으로, 이 문서가 §S2-B에서 겪은 드리프트와 같은 종류다.

---

### S3 — 엔진 계층 `DeleteAsync`

**건드리는 파일**: `ServerCore/Repo/Database/IDbExecutor.cs`, `DapperExtension.cs`, `OwnedSet.cs`, `Server.Tests/`(신규)

```csharp
// IDbExecutor — Create/Read/Update 만 있고 Delete 가 없던 상태 (5.6)
Task<int> DeleteAsync<T>(T entity) where T : ModelBase;
```

**직후 가능해지는 것**: 삭제 연산. 그리고 **비어 있던 `Server.Tests`에 첫 단위 테스트가 생긴다.**

**아직 안 되는 것**: 부분 필드 업데이트(5.6 나머지)는 API 경계 문제라 운영툴 작업 시.

---

### S4 — Channel / Device / Account ← **의사결정 게이트**

> **형태는 §S2-E 가 기준이다 (2026-08-20).** 이 절의 코드 예제는 Auth 가 `OwnedSet<T>` 를 쓴다는 전제로 쓰였는데, S2 에서 census 를 다시 돌린 결과 **Q2 잠정 = 아니오**로 정리됐다. Auth 는 `GameDb.Identity`(스코프 밖 진입점)와 `AuthScope`(명명 메서드)로 간다. 아래 예제는 그에 맞게 고쳤다.

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
// After : 파일 삭제. auth.GetChannelListAsync() 가 ChannelModel 을 그대로 반환한다.
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
var account = await _db.Identity.CreateAccountAsync();   // (a) AccountId 를 만들어내는 진입점
var auth = _db.Auth(account.Id);                         // 여기서 경계가 열린다
// RpcContext.SetAccountId 호출 자체가 사라진다 — accountId 가 반환값 → 스코프 인자로
// 흐르므로 컨텍스트를 경유할 이유가 없다 (§S2-E). §1.3 이 "특히 중요하다"고 지목한 것.
```

```csharp
// ── Before : AccountComponent.GetActiveAsync
var (found, mgrAccount) = await TryGetAsync(accountId);
ReqHelper.ValidContext(found, "NOT_FOUND_ACCOUNT", () => new { AccountId = accountId });
ReqHelper.ValidContext(mgrAccount.IsActive(), "NOT_ACTIVE_ACCOUNT", () => new { ... });

// ── After : AuthScope.GetAccountAsync() + 도메인 가드
// 조회는 스코프가 accountId 를 이미 들고 있으므로 인자가 없다.
// 활성 검증은 조회가 아니라 도메인 규칙이므로 모델 partial 로 간다.
var account = (await auth.GetAccountAsync()).EnsureActive();
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
- 세 엔티티에 한해 `_db.Identity` + `auth.*` 로 접근. Component/Manager 6개가 메서드 몇 개로 대체된다.
- 빈 Manager 2개가 실제로 사라진다(5.1.1).
- `AuthService`가 `GlobalDbRepo`와 `GameDb`를 **동시에** 쓰는 상태가 된다 — 같은 트랜잭션이므로 정상이다.

**아직 안 되는 것**: Session/PlayerMap은 그대로. User 계열 전부 그대로. 재화·로직 이관 없음.

**롤백 비용**: 클래스 3쌍 복원. **여기서 `OwnedSet<T>` 설계가 맞지 않으면 A안을 중단하고 B안을 재검토한다.**

#### S4-A 실행 결과 (2026-08-20, branch `db-refactor`)

| # | 계획 | 결과 |
|---|---|---|
| 1 | `Identity` / `AuthScope` 신설 | ✅ 새 제네릭 0개. 표면은 계획보다 2개 늘었다 (§S4-C) |
| 2 | `Domain/AccountModel.Logic.cs` | ✅ `IsActive()` + `EnsureActive()` |
| 3 | `Data/Queries/ChannelQueries.cs` | ✅ `Active()` 1개 (T1) |
| 4 | `AuthService` 두 흐름 이관 | ✅ 혼재 형태 (§S4-D) |
| 5 | Component/Manager 6개 삭제 | ✅ 217줄 |
| 6 | `AuthRepo.PrepareComp` 5줄 → 2줄 | ✅ 프로퍼티 3개도 같이 |
| 7 | — | ➕ `AuthScope.UpdateAsync<T>` 는 **안 만들었다** (§S4-E) |

14파일 +244/-261. **검증**: 리빌드 0에러/신규 경고 0, `ServerTest` 17/17. `TestBase.CreateDummyPlayerAsync` 가 SignUp 을 타므로 17개 전부가 이 경로를 지난다.

#### S4-B 이 게이트는 `OwnedSet<T>` 를 검증하지 못한다 — 게이트가 S5 로 옮겨간다

위에 "여기서 `OwnedSet<T>` 설계가 맞지 않으면 중단한다"고 적어뒀는데, **S4 는 `OwnedSet<T>` 를 한 번도 지나가지 않는다.** §S2-E 에서 Q2 를 "아니오"로 정하면서 Auth 가 `OwnedSet` 을 쓰지 않게 됐기 때문이다. 그 결정이 게이트의 검증 대상을 통째로 들어냈는데 게이트 문구는 그대로 남아 있었다.

| S4 가 확인한 것 | S4 가 확인 못 한 것 |
|---|---|
| `Identity`(스코프 밖) / `AuthScope`(스코프 안) 분류가 실제 호출부에서 성립한다 | `OwnedSet<T>` — 코드가 지나가지 않는다 |
| 손으로 쓰는 Auth 표면이 감당된다 (11메서드) | `[Entity].ScopeKey` 자동 WHERE — Auth 는 안 쓴다 |
| Component/Manager 삭제가 실제로 된다 | 소유자 리스트 캐시 — Auth 는 캐시가 0이다 |
| 데이터 계층의 `RpcContext` 결합이 풀린다 | `SelectListByColumnAsync` — 여전히 호출부 0 |
| 신/구 혼재가 한 서비스에서 성립한다 | |

**따라서 A안 중단 여부를 묻는 진짜 게이트는 S5(Point/Ticket/Item/Cookie)다.** S4 는 "Auth 형태가 맞는가"만 답했고, 그 답은 예다. S5 는 처음으로 User 계열 · `OwnedSet<T>` · 캐시 · `ScopeKey` 를 동시에 지나간다.

이것은 §S2-I 가 말한 것의 세 번째 사례다 — 앞선 결정(§S2-E)이 뒤 스텝의 전제를 조용히 바꿔놓았고, 코드를 써보고 나서야 드러났다.

#### S4-C census 는 조회 축만 봤고 "못 찾으면 어떻게 되는가"를 안 봤다

§S2-E 가 확정한 표면으로는 두 흐름이 컴파일되지 않는다. 같은 축에 **던지는 것과 안 던지는 것이 둘 다** 필요했다.

| 추가 | 왜 |
|---|---|
| `Identity.GetChannelAsync(key)` | `SignIn` — 채널이 없으면 `NOT_FOUND_CHANNEL` 로 실패한다 |
| `AuthScope.TryGetAccountAsync()` | `SignUp` — 기기만 남고 계정이 없는 상태를 신규 가입으로 흘려보낸다 |

census 가 "어느 컬럼으로 조회하는가"를 셌기 때문에 놓쳤다. §S2-F 가 동사를 **실패 시 동작**으로 갈라놨으므로 쌍이 필요한 것은 규칙대로다. 다음에 표면을 census 로 도출할 때는 축과 함께 **실패 처리**를 같이 세야 한다.

#### S4-D 혼재 형태 — `SetShardId` 는 저절로 안 사라진다

`AuthService` 는 `GlobalDbRepo` 와 `GameDb` 를 동시에 든다. Session(S7) 과 PlayerMap(S12) 이 남아 있기 때문이다. 5개 엔티티 중 **3 신 / 2 구**다.

```csharp
SessionKey = mgrSession.Model.Key,   // 구 - .Model 을 거친다
ChannelKey = channel.Key,            // 신 - 모델 직접
```

한 응답 조립부에 두 세대가 나란히 놓인다. 이것이 이관 기간 코드의 실물이고, S5~S10 에서 같은 모양이 반복된다. **`mgr` 접두사가 "옛 경로"의 표시로 기능한다** — 의도한 것은 아니지만 혼재 상태에서 값이 있어 §S2-F 에 규칙으로 넣었다.

**`RpcContext` 두 세터의 운명이 갈린다.** 옛 `AccountComponent.CreateAsync:45-46` 은 둘을 썼는데,

| | 읽던 곳 | S4 후 |
|---|---|---|
| `SetAccountId` | `DeviceComponent.CreateAsync:29` | **소멸** — Device 가 신 경로로 가면서 스코프가 accountId 를 든다 |
| `SetShardId` | **`SessionComponent.TouchAsync:85`** | **남는다** — Session 이 구 경로라 읽는 쪽이 살아있다 |

그래서 `AuthService` 에 `RpcContext.SetShardId(newAccount.ShardId)` 를 **명시적으로** 뒀다. 컨텍스트 쓰기는 원래 Transport 인접 계층의 일이므로(§S2-E) 데이터 계층에 숨어 있던 것을 드러낸 것이고, `// S7 에서 제거` 로 표시했다.

**오늘은 없어도 안 터진다** — `RpcContext.ShardId` 기본값이 `0`이고 `Identity.CreateAccountAsync` 도 `ShardId = 0 // TODO` 라 no-op 이다. 그래서 더 위험하다. 그 `TODO` 가 풀리는 날 새 계정의 세션이 잘못된 샤드로 들어가고, `ServerTest` 는 못 잡는다.

**`SetAccountId` 제거가 안전한지는 읽는 쪽을 전부 세어 확인했다**: `RpcService.cs:84` 의 `RunAtomicAsync(_rpcCtx.AccountId, ...)` 는 인자가 **핸들러 실행 전에** 평가되므로 SignUp 에서는 예나 지금이나 `0` 이고, `ResponseCacheService.MakeKey` 는 `SessionKey`+`Seq` 로만 키를 만들며, `AuthPolicy` 검사는 핸들러 앞이다. **요청 안에서 값이 달라지는 독자가 없다.**

#### S4-E 만들지 않은 것

`AuthScope.UpdateAsync<T>` 는 §S2-E 표면에 있었으나 **짓지 않았다.** 옛 `AccountComponent.UpdateAccountAsync` / `DeviceComponent.UpdateAsync` / `ChannelComponent.UpdateAsync` 셋 다 **호출부가 0개**였다. 소비자가 생길 때 만든다 — §S2-I · §S2-J · S3 와 같은 판단이다.

§S4 계획이 적어둔 `Data/Queries/ChannelQueries.cs` 는 만들었지만, "Auth 데이터 접근 13 → 2" 는 **S4 가 아니라 S7/S12 까지 끝난 뒤**의 숫자다. 지금은 `SessionComponent`(122) · `PlayerMapComponent`(29) 와 그것들이 쓰는 `AuthComponentBase`(64) · `AuthManagerBase`(15) · `AuthRepo`(29) 가 그대로 남아 있다.

---

### S5 — Point / Ticket / Item / Cookie ← **A안 의사결정 게이트**

> **2026-08-23 전면 재도출.** 옛 본문은 폐기했다. 코드와 어긋난 곳이 6개였고 그중 하나는 자기모순이었다 — "Component 4 삭제"와 "`PlayerDetailManager`가 여전히 `_userRepo.Point`를 찌른다"가 한 절 안에 같이 있었다. 나머지 5개는 `MarkDirty()`(§S2-H에서 철회), `TouchAsync`(§S2-F에서 `GetOrCreateAsync`로 개명), `TryGetAsync(new { Num = ... })`(실제 시그니처는 술어), `Domain/ChangeSet.cs`(아래에서 S6으로 미룬다), "`PlayerId`는 스코프가 채운다"(그 동작이 코드에 없었다 — 아래 신규 결정). §S2-I의 "S3~S13은 예정이 아니라 가설"이 여기서 네 번째로 확인됐다.

**census — Point/Ticket/Item의 소비자는 서비스가 아니다**

옛 본문이 소비자로 `CookieService`만 적은 것은, census를 조회 축이 아니라 **서비스 파일 이름**으로 돌렸기 때문이다. §S4-C와 같은 종류의 누락이다.

| 엔티티 | 서비스 소비자 | 실제 소비자 |
|---|---|---|
| Point | 0 | `PlayerDetailManager` POINT region (265~277) |
| Ticket | 0 | `PlayerDetailManager` TICKET region (281~293) |
| Item | 0 | `PlayerDetailManager` ITEM region (313~325) |
| Cookie | `CookieService` 2곳 | + `PlayerDetailManager` COOKIE region (297~309) |

따라서 **S5는 `PlayerDetailManager`를 피해갈 수 없다.** 다만 그 클래스의 *공개* 표면(`DecCostAsync` / `IncRewardAsync` / `IncRewardListAsync` / `DecCashAsync` / `GetCashPacket`)은 5개 서비스 12곳이 쓰고 있고 **S6에서 통째로 사라질 것**이다. 그래서 S5는 **안쪽 region 4개만 갈아끼우고 공개 표면은 건드리지 않는다** — 곧 지울 시그니처를 지금 바꾸지 않는다.

**건드리는 파일**
- 삭제: `Component/{Point,Ticket,Cookie,Item}Component.cs`, `Manager/{Point,Ticket,Cookie,Item}Manager.cs` (8개, 약 370줄)
- 추가: `Domain/{Point,Ticket,Item,Cookie}Model.Logic.cs`, `Data/Queries/`의 `GetOrCreateAsync` 확장
- 수정: `Repo/UserRepo.cs`(프로퍼티 4 + `PrepareComp()` 4줄), `Component/PlayerDetailComponent.cs`(`TouchAsync(UserScope)`), `Manager/PlayerDetailManager.cs`(region 4개), `Service/{Cookie,Cheat,Gacha,Kingdom,World}Service.cs`(스코프 생성 + 인자 전달)

```csharp
// ── Before : CookieManager.EnhanceStarAsync — 검증 + 변경 + 저장이 한 메서드에
public async Task EnhanceStarAsync(int aftStar, int usedSoulStone)
{
    _ = _model.Star;
    var befSoulStone = _model.SoulStone;
    ReqHelper.ValidEnough(usedSoulStone, befSoulStone, $"COOKIE_SOUL_STONE:{_prt.Num}", "ENHANCE_STAR");

    _model.Star = aftStar;
    _model.SoulStone -= usedSoulStone;
    await _userRepo.Cookie.UpdateMdlAsync(_model);      // ← 이것 때문에 _userRepo 가 필요했다
}

// ── After : Domain/CookieModel.Logic.cs — DB 참조가 없다
public partial class CookieModel
{
    public void EnhanceStar(int aftStar, int usedSoulStone, CookieProto prt)
    {
        ReqHelper.ValidEnough(usedSoulStone, SoulStone, $"COOKIE_SOUL_STONE:{prt.Num}", "ENHANCE_STAR");
        Star = aftStar;
        SoulStone -= usedSoulStone;
    }
}
```

바뀐 것은 넷이다: ① `_model.` 접두사 제거 ② **저장 호출 삭제** ③ Proto를 필드가 아니라 인자로(§3.4) ④ 의미 없는 `_ = _model.X;` discard 제거(4개 Manager 합계 14줄, 원래 의도는 불명). `_userRepo`가 필요했던 유일한 이유가 ②였으므로, **Model이 DB를 참조할 이유가 사라진다.** 이것이 §0.6의 유일한 신규 결정이 실제로 성립하는 지점이다.

**저장 규칙 — 도메인 메서드는 저장하지 않는다**

dirty를 철회했으므로(§S2-H) 저장은 명시 호출이다. 규칙은 하나다: **도메인 메서드를 부른 쪽이 바로 다음 줄에서 `UpdateAsync`를 한다.** 저장을 빠뜨리면 테스트가 즉시 잡는 "시끄러운 버그"가 되는 것이 §S2-H가 선택한 거래다.

```csharp
// ── Data/Queries/PointQueries.cs — Component.TouchAsync 의 자리
public static async Task<PointModel> GetOrCreateAsync(this OwnedSet<PointModel> set, int num)
{
    var (found, point) = await set.TryGetAsync(x => x.Num == num);
    return found ? point : await set.CreateAsync(new PointModel { Num = num });
}

// ── PlayerDetailManager POINT region — 공개 표면은 그대로, 안쪽만 바뀐다
private async Task<double> DecPointInternalAsync(int pointNum, double amount, string reason)
{
    var pointSet = _userScope.Owned<PointModel>();
    var point = await pointSet.GetOrCreateAsync(pointNum);
    var pointAmount = point.DecAmount(amount, reason);
    await pointSet.UpdateAsync(point);
    return pointAmount;
}
```

`Dec`는 `ReqHelper.ValidEnough`가 쓰므로 `reason`을 받고, `Inc`는 검증이 없으므로 받지 않는다 — 시그니처가 의존성을 문서화한다(§3.4).

**스코프 전달 — `PlayerDetail.TouchAsync(userScope)` (결정)**

`PlayerDetailManager`가 `UserScope`를 얻는 길은 셋이었다. ① 리워드 메서드 인자로 받기 — S6의 `RewardHelper.Pay(detail, loaded, cost)` 모양에 가깝지만 곧 지울 시그니처 12곳을 지금 바꾸는 것이다. ② `UserRepo`에 `GameDb` 주입 — 서비스는 무변경이지만 **구 경로가 신 경로를 참조하게 되어** §S2-H에서 끊어낸 `GlobalDbRepo → GameDb` 방향이 되살아난다. ③ `PlayerDetailComponent.TouchAsync(userScope)`가 생성 시 넘겨주고 Manager가 필드로 보관.

**③으로 간다.** 리워드 API 시그니처가 불변이고, ②의 역방향 참조를 만들지 않으며, 서비스가 스코프를 드는 것은 어차피 S6에서 필요한 형태다. 이관 기간 동안 `PlayerDetailManager`는 `_userRepo`와 `_userScope`를 동시에 든다 — §S4-E가 받아들인 "두 세대가 나란히 놓인 코드"의 반복이다.

```csharp
// ── Service — 스코프를 한 번 만들어 양쪽에 쓴다
var userScope = _db.User(RpcContext.ShardId, RpcContext.PlayerId);
var cookieSet = userScope.Owned<CookieModel>();
var cookie = await cookieSet.GetOrCreateAsync(req.CookieNum);
var mgrPlayerDetail = await OwnUser.PlayerDetail.TouchAsync(userScope);
```

**신규 결정 — ScopeKey 쓰기 규칙: 생성은 채우고, 수정은 확인한다**

`[Entity].ScopeKey`가 오늘 실제로 쓰이는 곳은 **읽기 한 군데뿐**이다(`OwnedSet.LoadFromDbAsync`의 자동 WHERE). `CreateAsync` / `UpdateAsync`는 ScopeKey를 보지 않는다. 그래서 5.5.1이 지목한 버그가 그대로 열려 있다:

```csharp
var userScope = _db.User(shardId, 100);
await userScope.Owned<PointModel>().CreateAsync(new PointModel { Num = 5 });   // PlayerId 를 안 넣었다
// DB   : INSERT ... (PlayerId = 0)          ← 0번 플레이어의 행
// Cache: "PointModel:100" 버킷에 append     ← 100번 플레이어의 리스트
```

**이 버그는 DB 종류에 따라 다르게 나타난다.** MySQL에서는 캐시가 살아 있는 동안 100번이 그 행을 자기 것으로 읽고 캐시가 만료되면 사라진다 — 예외도 로그도 없다. InMemory에서는 캐시를 안 지나가므로 다음 조회에서 못 찾고 다시 INSERT → PK(0,5) 충돌로 시끄럽게 터진다. **즉 ServerTest가 이 버그를 만나는 방식과 프로덕션에서 나타나는 방식이 다르다.**

따라서 S5에서 `OwnedSet`에 다음을 넣는다:

```csharp
public Task<T> CreateAsync(T entity)
{
    EntityMeta<T>.SetScopeKeyValue(entity, _scopeKeyValue);
    entity.CreateTime = entity.UpdateTime = DateTime.UtcNow;
    return _repository().InsertAsync(entity, _listKey);
}

public Task UpdateAsync(T entity)
{
    EnsureOwned(entity);   // GetScopeKeyValue 가 _scopeKeyValue 와 다르면 예외
    entity.UpdateTime = DateTime.UtcNow;
    return _repository().UpdateAsync(entity, _listKey, EntityMeta<T>.PkMatcher(entity));
}
```

추가 비용은 `EntityMeta<T>`에 컴파일 세터 하나(`CompileGetter` 옆에 `CompileSetter`)다. 규칙은 한 문장으로 말할 수 있다: **소유자는 스코프가 정한다. 생성은 스코프가 채우고, 수정은 스코프가 확인한다.**

생성 쪽을 검증이 아니라 자동 채움으로 한 이유는 둘이다. ① 호출부가 넣은 ScopeKey 값은 "스코프가 소유자를 정한다"는 규칙 아래에서 애초에 의미가 없다 — 말없이 덮어도 잃는 정보가 없다. ② 검증만 하면 `GetOrCreateAsync` 확장이 소유자 값을 알아야 하는데, `OwnedSet`이 `_scopeKeyValue`를 private으로 들고 있어 **`public object ScopeKeyValue`를 열거나 확장 시그니처를 `(this UserScope scope, ...)`로 바꿔야 한다.** 규칙 하나의 단순함을 그 대가로 사기에는 비싸다.

부수 효과: 오늘 그 자리에 있던 줄이 `PlayerId = _userRepo.RpcContext.PlayerId`(4곳)다. **§1.3이 없애려는 앰비언트 컨텍스트 참조가 같이 사라진다.**

**`ChangeSet`은 S6으로 미룬다**

옛 본문은 S5에서 `Domain/ChangeSet.cs`를 만들고 도메인 메서드가 그것을 반환하게 했다. 그러나 S5에서 그 반환값의 소비자는 `PlayerDetailManager` 하나이고, 그것은 즉시 `double`로 되돌린다 — **소비자가 사실상 0이다.** 소비자 없는 것은 짓지 않는다는 §S2-I · §S2-J · S3와 같은 판단이다. 실제 소비자는 S6의 `RewardHelper`이므로 거기서 만든다.

**S5에서 고치지 않고 기록만 하는 것 — `ChgObjPacket`의 두 필드가 의미가 섞여 있다**

`IncRewardAsync`는 `Amount`를 **요청값**에서, `TotalAmount`를 **모델**에서 가져온다. COOKIE 타입일 때 `IncCookieAsync`가 `soulStoneCnt -= prt.InitSoulStone`으로 내부 조정을 하는데 `Amount`는 요청한 쿠키 수 그대로이고, `TotalAmount`는 현재 소울스톤이 아니라 `AccSoulStone`(누적)이다. `ChangeSet`이 생기면 `Delta` / `After`로 정리될 자리지만 **와이어 동작 변경**이므로 S5 범위 밖이다. S6에서 다시 본다.

**검증 계획, 그리고 그 한계**

- 리빌드 0에러 / 신규 경고 0 (`-t:Rebuild` 기준. 저장소 기존 경고 35건은 §S2 기록 참조)
- ServerTest 17/17. 4개 엔티티를 실제로 지나가는 테스트: `CookieTest`(Cheat → SOUL_STONE → Cookie, POINT_COOKIE_LV → Point), `GachaTest`(DecCost, IncRewardList), `WorldTest`(IncRewardList), `KingdomTest`(DecCost)

**한계 2개 — 이대로면 게이트 판정이 반쪽이다.**

1. `InMemoryRepository.GetListAsync`는 `Db.ExecuteAsync(dbFetch)`로 직행한다. **캐시 경로를 통째로 건너뛴다.** ServerTest는 InMemory 전용이므로 `OwnedSet`의 리스트 캐시 · Insert 시 append · Update 시 교체가 **하나도 검증되지 않는다.** S5는 "`OwnedSet` · `ScopeKey` · 캐시를 처음 동시에 지나가는 스텝"인데 테스트로는 캐시를 못 지나간다.
2. `SelectListByColumnAsync`의 MySQL 경로는 §S2에서 "호출자가 없어 안 지나갔다"로 남았고 **S5가 그 첫 소비자**다.

→ **MySQL 1회 수동 실측을 검증 항목에 넣는다 (결정).** `Code/Server/appsettings.yaml` 은 이미 `Db.Type: MySql` + `Cache.Type: Redis` + `UseUserLock: true` 이므로 로컬 MySQL·Redis 를 띄우고 서버를 그대로 실행하면 된다. 볼 것은 네 가지다:
>
> 1. `SelectListByColumnAsync` 가 만드는 SQL 이 실제로 나가고 행을 가져오는가 (§S2 이후 첫 실행)
> 2. 리스트 캐시 키가 옛 경로와 같은 `"<Tag>:<playerId>"` 로 나오는가 — 달라지면 배포 직후 전체 캐시 미스가 된다
> 3. `InsertAsync` 의 리스트 append 와 `UpdateAsync` 의 항목 교체가 같은 요청 안에서 보이는가
> 4. `UseUserLock: true` 경로와 섞여도 커넥션이 깨지지 않는가 (§4.2 의 락 커넥션 분리가 유지되는지)
>
> ServerTest 는 이 네 가지를 하나도 못 본다. 그래서 실측 없이는 게이트를 "통과"로 적지 않는다.

**직후 가능해지는 것**
- 재화 · 쿠키 로직이 **DB 없이 단위 테스트 가능**해진다: `new CookieModel { SoulStone = 10 }.EnhanceStar(3, 5, prt)`
- `_userRepo`를 드는 클래스가 8개 줄어든다(Component 4 + Manager 4 소멸)
- 데이터 계층에서 `RpcContext.PlayerId`를 읽던 4곳이 사라진다(§1.3)
- `OwnedSet<T>` · 자동 WHERE · 소유자 리스트 캐시 · 쓰기 검증이 **처음으로 동시에 실동작한다** → 게이트 판정이 가능해진다

**아직 안 되는 것**
- `PlayerDetailManager` 존치. Gold/Exp/Cash는 여전히 `_userRepo.PlayerDetail` 경유 — **5.1.4 / 5.4는 S6까지 미해결**
- 감사 로그 쓰기 경로 없음. `CashChangeLogModel` / `GachaLogModel`은 여전히 소비자 0이다. 따라서 **§S2-J가 S5로 미룬 `EntityMeta.VerifyCacheTags` 역방향 검사는 여기서도 앵커가 없다 — S13(감사 로그)으로 다시 미룬다.** "ScopeKey는 있으나 캐시는 안 쓴다"를 선언할 자리는 실제 쓰기 경로가 생길 때 만든다
- `ChangeSet` 없음 → 위 `ChgObjPacket` 항목 그대로

**전제**: S0-1(저장 모델)은 §S2-H에서 "즉시 쓰기"로 확정됐다. 열린 전제는 없다.

#### S5-A 실행 결과 (2026-08-23, branch `db-refactor`)

사양대로 실행했다. 확정된 결정 4개는 그대로 성립했고 코드가 사양을 되받아친 곳은 아래 §S5-B 하나다.

- 삭제 8: `Component/{Point,Ticket,Cookie,Item}Component.cs`, `Manager/{Point,Ticket,Cookie,Item}Manager.cs`
- 추가 8: `Domain/{Point,Ticket,Item,Cookie}Model.Logic.cs`, `Data/Queries/{Point,Ticket,Item,Cookie}Queries.cs`
- 수정 9: `Data/EntityMeta.cs`(`CompileSetter`/`SetScopeKeyValue`), `Data/OwnedSet.cs`(생성 채움·수정 검증), `Repo/UserRepo.cs`, `Component/PlayerDetailComponent.cs`, `Manager/PlayerDetailManager.cs`, `Manager/PlayerManager.cs`, `Service/{Cookie,Cheat,Gacha,Kingdom,World,Game}Service.cs`

검증: `Code.sln` 리빌드 0에러 · unique warning **35건 = 기존 baseline 그대로**(신규 0) · ServerTest **17/17**.

#### S5-B census 가 또 틀렸다 — `PlayerManager` 를 놓쳤다

사양의 census 표는 Point/Ticket/Item 의 소비자를 `PlayerDetailManager` 하나로 적었으나, 빌드가 `PlayerManager` 에서 6개 에러를 냈다. 실제로는 두 곳이었다:

- `PreparePlayerAsync` — 기본 플레이어 생성 시 쿠키를 `_userRepo.Cookie.CreateMdlAsync` 로 만든다
- `LoadPlayerAsync` — Cookie/Point/Ticket/Item **4종 리스트를 전부 조회**한다

원인은 census 를 돌릴 때 `Code/DbModel/Manager` 디렉터리를 grep 결과에서 **제외**한 것이다. Manager 안에서 다른 Manager 의 Component 를 부르는 호출이 통째로 안 보였다. §S4-C 는 "조회 축만 세고 실패 처리를 안 셌다"였고, 여기서는 **탐색 범위 자체를 좁혀놓고 census 라고 불렀다** — census 실패가 두 스텝 연속이다. 규칙으로 적어둔다: **census 의 grep 에서 디렉터리를 제외하지 않는다. 좁히려면 제외가 아니라 결과를 분류한다.**

#### S5-C 스코프를 *언제* 읽느냐가 함정이다

`PlayerComponent.TouchAsync` 는 신규 플레이어일 때 그 안에서 `RpcContext.SetPlayerId(accountId * 10)` 를 호출한다. 즉 **`Player.TouchAsync()` 전에는 `RpcContext.PlayerId` 가 0** 이고, 그때 만든 스코프는 0번 플레이어를 가리킨다.

그래서 `PlayerManager` 는 `UserScope` 를 **필드로 들지 않고 `PreparePlayerAsync(mapper, userScope)` / `LoadPlayerAsync(mapper, userScope)` 인자로만 받는다.** 서비스 쪽 `OwnScope` 는 계산 프로퍼티라 호출 시점에 `RpcContext.PlayerId` 를 읽으므로, `Player.TouchAsync()` 뒤에 평가되면 올바른 값이 들어온다. `GameService` 에 그 순서 의존을 주석으로 명시했다.

`PlayerDetailManager` 는 반대로 필드로 든다 — 생성 시점(`PlayerDetail.TouchAsync(userScope)`)이 이미 PlayerId 확정 이후이기 때문이다. **두 Manager 의 처리가 다른 이유가 이것이고, 우연이 아니다.** `SetPlayerId` 이동(5.9, S7)이 끝나면 이 비대칭도 사라진다.

#### S5-D MySQL 실측 — **게이트 통과.** 막고 있던 기존 결함 2개를 잡았다

실측은 로컬 MySQL 8.0 + Redis 에 `ServerTest` 를 붙여 돌렸다(설정은 임시 변경 후 되돌림). 처음엔 3 통과 / 14 실패였고, S5 이전 커밋(`c888ec7`)을 별도 워크트리에 꺼내 같은 설정으로 돌려 **똑같이 3 / 14** 임을 확인했다 — **S5 의 회귀는 0이다.** 막고 있던 것은 S5 밖의 기존 결함 2개였고, 둘 다 원인을 잡아 고쳤다.

| 구성 | 결함 수정 전 | 수정 후 |
|---|---|---|
| InMemory DB + InMemory 캐시 (커밋된 테스트 구성) | 17 / 17 | 17 / 17 |
| MySQL + InMemory 캐시 + `UseUserLock: true` | 3 / 14 | **17 / 17** |
| **MySQL + Redis + `UseUserLock: true`** | 3 / 14 | **17 / 17** |

**결함 ① `RpcContext.Ip` 가 null 이 될 수 있었다.** `GetIp` 의 마지막 줄이 `httpCtx.Connection.RemoteIpAddress?.ToString()` 이라 원격 IP 가 없으면 null 을 돌려주는데, `Ip` 는 `= string.Empty` 로 선언된 non-nullable `string` 이었다. 그 값이 `SessionManager.StartAsync` → `Session.PublicIp`(NOT NULL)로 들어가 **SignUp 이 500 으로 죽었다.** InMemory 는 NOT NULL 을 강제하지 않아 안 걸린다. `?? string.Empty` 로 막았고, 같은 메서드에서 `X-Forwarded-For` 값이 빈 문자열일 때 `Split` 이 NRE 를 내는 경로도 함께 막았다.

**결함 ② 응답 캐시가 SignUp 응답으로 오염됐다.**

```
AuthService.SignUpAsync
  → SessionManager.StartAsync() 가 세션 키를 새로 뽑고 RpcContext.SetSessionKey(신규키)
  → RpcService: _responseCache.SetAsync(_rpcCtx, signUpRes)
     키 = RpcResponseCache:{신규키}:0            ← 클라이언트가 다음에 쓸 키다
클라이언트: 그 신규키로 첫 인증 요청(GameEnter, Seq=0)
  → _responseCache.TryGetAsync 가 HIT
  → 핸들러가 아예 실행되지 않고, SignUp 응답 JSON 이 GameEnterResponsePacket 으로 역직렬화된다
```

증거 셋: 새 세션의 `RpcResponseCache:{sessionKey}:0` 값을 Redis 에서 직접 꺼내면 `{"info":...,"result":{"sessionKey":...,"channelKey":...}}` — **SignUp 응답**이다. `GameService.EnterAsync` 첫 줄에 임시 `throw` 를 넣어도 터지지 않는다(핸들러 미실행). `Cache.Type` 만 InMemory 로 바꾸면 즉시 정상 동작한다. 결과는 **200 OK + 빈 Player 패킷 + UserDb 에 아무 행도 안 남음** — 조용한 데이터 유실이다.

도입 시점은 `d89d3ed`("Seq 재전송 시 재실행 없이 캐시된 응답을 반환하도록 구현")다. S5 와 무관한 기존 회귀다.

**터지는 조건은 둘 다 만족해야 한다.** ① `ResponseCacheService._enabled` 가 `CacheType == Redis` 일 때만 켜진다 — **ServerTest 는 InMemory 전용이라 이 결함을 구조적으로 못 잡는다.** ② 호출자가 `seq` 를 안 보내야 한다. Unity 클라이언트는 `RpcSystem` 이 `Seq = ++_seq` 로 단조 증가시켜 URL 에 실으므로 **오늘 프로덕션에서 실제로 터지지는 않는다 — 잠재 결함이었다.** 반면 `seq` 쿼리를 안 붙이는 호출자(테스트 클라이언트, 운영툴, curl)는 `RpcContext.SetSeq` 가 0 으로 채우므로 **모든 요청이 Seq=0** 이 되어 반드시 터진다.

**수정: `Seq == 0` 이면 응답 캐시를 쓰지 않는다.** `Seq` 는 재전송을 식별하는 토큰이고 0 은 "호출자가 안 보냈다"는 뜻이라 애초에 구분할 근거가 없다. 캐시하면 한 세션의 모든 요청이 같은 키를 공유하게 된다. `ResponseCacheService.IsUsable` 한 곳에 조건을 모았다.

> 남은 것(이번 범위 밖): 응답 캐시 키에 **프로토콜 이름이 없다.** 그래서 SignUp 응답이 GameEnter 응답 자리에 앉을 수 있었다. `Seq == 0` 가드로 관측된 경로는 막혔지만, 키가 요청을 식별하지 못한다는 성질 자체는 남아 있다. 그리고 `RpcContext.Seq` 는 **쿼리스트링에서만** 읽는다 — 요청 바디의 `Info.Seq` 는 무시된다.

**게이트 판정 — 통과.** MySQL + Redis + 유저락 전 구성에서 **17 / 17** 이고, 실측 후 실제 데이터가 남았다: MySQL 에 Player 33 · Cookie 33 · Point 6 · Item 6 행, Redis 에 `CookieModel:{playerId}` · `PointModel:{playerId}` 소유자 리스트 키. 캐시 값도 확인했다 — `[{"playerId":1050,"num":1010,...,"lv":5,...}]` 로 **강화 결과가 반영된 리스트**가 들어 있다(Update 시 항목 교체 동작 확인). 따라서 `OwnedSet<T>` 의 `SelectListByColumnAsync` MySQL SQL · 소유자 리스트 캐시(Redis 직렬화 왕복 포함) · Insert append · Update 교체 · 생성 시 ScopeKey 채움 · 수정 시 소유자 검증이 **전부 실동작으로 확인됐다.** §S2 가 "호출자가 없어 안 지나갔다"고 남긴 `SelectListByColumnAsync` 의 MySQL 경로도 여기서 처음 지나갔다. **A안의 형태는 실측으로 성립한다.**

부수 기록: `RaidServerLauncher.StopAsync:37` 이 테스트 종료 때마다 NRE 를 던진다(`[Test Collection Cleanup Failure]`). 결과에는 영향이 없지만 매 실행마다 찍힌다. 그리고 `Code/ServerTest/appsettings.yaml` 은 `IsShowErrorDetail` 을 루트에 두는데 코드는 `Game:` 아래에서 읽는다 — 테스트에서 서버 에러가 나면 원문 대신 6자리 해시만 보여서 진단이 한 번 막혔다.

#### S5-E `GetOrCreateAsync` 는 엔티티별 확장으로 유지한다 (2026-08-23 확정)

`PointQueries`/`TicketQueries` 가 타입 이름만 다른 동일 코드라 `OwnedSet<T>` 에 제네릭 `GetOrCreateAsync(predicate, factory)` 를 두는 안을 검토했다. **기각.** 근거는 호출부 수다: 엔티티당 호출부가 2곳(Dec/Inc)이라 제네릭으로 바꾸면 `new ItemModel { Num, Type = prt.Type }` 같은 **기본 행 정의가 2곳에 복사된다.** 확장 파일이 존재하는 이유가 정확히 그것 — **엔티티의 기본 행은 한 곳에만 정의된다.** `PlayerId + Num` 모양은 13개 User 모델 중 7개(Point/Ticket/Item/Cookie/World/WorldStage/KingdomDeco)라 S9·S10 에서 3개가 더 붙는다.

#### S5-F 식 트리를 걷어내고 생성기가 접근자를 찍게 했다 (2026-08-23)

`EntityMeta<T>` 가 `[Entity]` 의 문자열을 식 트리로 컴파일해 접근자를 만들고 있었다. 사용자가 "이게 꼭 필요한가, 인터페이스는 별로인가"를 물었고, **인터페이스가 맞다**로 결론냈다. 근거는 성능이 아니다.

**우리가 모델을 생성한다.** 식 트리는 "런타임에 문자열밖에 없다"를 메우는 도구인데, 그 문자열을 우리 생성기가 직접 찍고 있었다. `ScopeKey = "PlayerId"` 를 쓸 수 있으면 `public ulong GetScopeKey() => PlayerId;` 도 쓸 수 있다. **스스로 지운 정보를 리플렉션으로 되사오고 있었던 것이다.**

**놓는 자리는 둘로 갈린다.** PK 는 모든 모델이 갖고(Auth 의 `Session.Key` 도 PK 다), 소유자는 User 계열에만 있다. 그래서 축이 다르다.

- `ModelBase` (abstract) ← `PkEquals(ModelBase other)` — **보편**
- `IScopedModel` (신설, `ServerCore.Model`) ← `GetScopeKey()` / `SetScopeKey(ulong)` — **User 한정**

```csharp
// 생성물 — Point (PK 2컬럼 + 소유자)
public partial class PointModel : ModelBase, IScopedModel
{
    public override bool PkEquals(ModelBase other)
    {
        return other is PointModel otherModel
            && PlayerId == otherModel.PlayerId
            && Num == otherModel.Num;
    }

    public ulong GetScopeKey() => PlayerId;
    public void SetScopeKey(ulong value) => PlayerId = value;
}

// 생성물 — Session (PK 1컬럼, 소유자 없음)
public partial class SessionModel : ModelBase
{
    public override bool PkEquals(ModelBase other)
    {
        return other is SessionModel otherModel
            && AccountId == otherModel.AccountId;
    }
}
```

**프로퍼티가 아니라 메서드인 것이 핵심이다.** `DapperExtension` 과 `InMemoryDbExecutor` 가 `GetProperties(Public|Instance)` 로 INSERT/UPDATE 컬럼 목록을 만든다. `public ulong ScopeKey { get; set; }` 를 붙였으면 **없는 컬럼이 SQL 에 들어간다** — §S2 에 기록해 둔 "`ModelBase` 에 public 프로퍼티를 추가하는 것은 곧 DB 컬럼 선언이다" 함정에 그대로 걸린다. 메서드는 그 리플렉션에 안 걸린다.

**`abstract` 이지 `virtual` 이 아니다.** 기본 구현(참조 비교)을 두면 생성기가 빠뜨린 모델이 **조용히** 참조 비교로 떨어져 캐시 리스트에 중복 행이 쌓인다. 모델 20개가 전부 생성물이고 `new ModelBase()` 도 없어서 abstract 로 강제해도 손으로 채울 것이 없다.

**제네릭으로 만들지 않았다.** `IScopedModel<TKey>` 는 소비자 0 인 일반화다(§S1-G 의 `IDataScope` 철회, §S2-I 와 같은 판단). 13개 스코프 키가 전부 `ulong` 이라 `ulong` 으로 못 박았고, 생성기가 **다른 타입이면 `NOT_ULONG_SCOPE_KEY` 로 생성을 실패시킨다.**

**결과**
- `EntityMeta<T>` 145 → 74 줄. `Pk`(외부 소비자 0 이었다) · `_pkGetters` · `PkMatcher` · `CompileGetter` · `CompileSetter` · `GetScopeKeyValue` · `SetScopeKeyValue` 소멸. `System.Linq.Expressions` 의존 제거. 남은 것은 **코드로 표현할 수 없는 문자열뿐**이다 — 자동 WHERE 에 넣을 SQL 컬럼명(`ScopeKey`)과 캐시 태그.
- `OwnedSet<T>` 의 `_scopeKeyValue` 가 `object` → `ulong`. 박싱과 `Equals(object, object)` 비교가 사라졌다. 업데이트마다 만들던 `object[]` + 클로저도 사라지고 `x => x.PkEquals(entity)` 하나만 남는다.
- 제약이 `where T : ModelBase, IScopedModel, new()` 가 됐다.

**제약이 실제로 막는 범위를 정확히 적는다.** 소유자 축이 아예 없는 **Auth/Center 6종**(Account/Channel/Device/Session/PlayerMap/Schedule)은 이제 `Owned<T>()` 에 **넘기면 컴파일이 안 된다.** 그러나 **감사 로그 2종(`CashChangeLog`/`GachaLog`)은 `ScopeKey` 가 있어서 `IScopedModel` 을 구현하고, 여전히 컴파일된다** — 이들을 막는 것은 캐시 태그가 없다는 런타임 검사(`HasCacheTag`)다. 즉 §S2-J 가 지적한 두 구멍 중 **하나만** 컴파일 타임으로 옮겨졌다. 나머지 하나(태그 누락)는 여전히 첫 `Owned<T>()` 에서 터지고, §S13 의 역방향 검사 과제로 남는다.

**재생성 결과**: 모델 20개만 바뀌었다. CSV · Liquibase 체인지로그 · 패킷은 무변경 — 생성기 입력이 커밋된 상태와 일치한다는 뜻이라, §S1 이 겪은 stale 입력 문제는 지금 없다.

**검증**: `Code.sln` 리빌드 0 에러 · unique warning 35 = 기존 baseline · ServerTest 17/17 (InMemory) · **MySQL + Redis + `UseUserLock: true` 에서도 17/17**.

**안 한 것**: `IRepository.UpdateAsync<T>(entity, listKey, Func<T,bool> match)` 에서 `match` 인자를 없애고 리포지토리가 `x.PkEquals(entity)` 를 직접 부르게 하는 안. 그 3인자 오버로드의 호출부가 둘인데 — `OwnedSet.UpdateAsync`(PK 비교)와 `UserComponentBase.UpdateMdlAsync`(**캐시 키 문자열 비교**) — 옛 경로가 다른 방식으로 매칭 중이라 이관 도중에 합치면 의미 변경 위험이 붙는다. 게다가 `IRepository` 는 XPDProject 포팅 표면이다. **Component 가 전부 사라지는 S10~S13 이후에 다시 본다.**

---

#### S5-G 커밋 전 리뷰 — 개명이 버그를 드러냈다 (2026-08-23)

**`EnhanceCookieLv` 로 보유하지 않은 쿠키를 포인트로 만들어낼 수 있었다.**

```csharp
var cookie = await cookieSet.GetOrCreateAsync(req.CookieNum);   // 없으면 생성 (Lv=1, State=NONE)
ReqHelper.ValidContext(req.BefLv == cookie.Lv, ...);            // BefLv=1 이면 통과
var resultCostObj = await mgrPlayerDetail.DecCostAsync(valCostObj, reason);
cookie.EnhanceLv(req.AftLv);
await cookieSet.UpdateAsync(cookie);                            // 커밋된다
```

보유 여부를 아무도 검사하지 않았다. `ECookieState` 는 `NONE = 0` / `AVAILABLE = 1` 이고 `IncCookie` 가 획득 시 `AVAILABLE` 로 올린다 — **모델에 소유 신호가 이미 있는데 안 보고 있었다.** 게다가 이 경로는 `CookieProto` 를 한 번도 조회하지 않아 **프로토에 없는 번호까지 행이 생겼다**(`CookieQueries.GetOrCreateAsync` 에 번호 검증이 없다). `EnhanceCookieStar` 는 새로 만든 쿠키의 소울스톤이 0 이라 `ValidEnough` 에서 막히지만 그것은 우연히 막히는 것이지 의도된 검사가 아니다.

**S5 가 만든 버그가 아니다.** 옛 `CookieComponent.TouchAsync` 도 똑같이 생성했다. 그런데 §S2-F 가 `TouchAsync` → `GetOrCreateAsync` 로 개명한 이유가 정확히 *"읽기인 줄 알고 부르면 INSERT 가 나가는 것을 보이게 하자"* 였고, **이름이 바뀌자 커밋 전 리뷰에서 바로 보였다.** 개명의 값이 여기서 회수됐다.

수정: 강화 두 경로 모두 조회로 바꾸고 소유를 검사한다.

```csharp
// 강화는 보유한 쿠키에만 한다. GetOrCreate 로 열면 안 가진 쿠키가 강화 요청만으로 생긴다.
private static async Task<CookieModel> GetOwnedCookieAsync(OwnedSet<CookieModel> cookieSet, int cookieNum)
{
    var (found, cookie) = await cookieSet.TryGetAsync(x => x.Num == cookieNum);
    ReqHelper.ValidContext(found && cookie.State == ECookieState.AVAILABLE, "NOT_OWNED_COOKIE",
        () => new { CookieNum = cookieNum });
    return cookie;
}
```

`CookieTest.CookieEnhanceLv_Test` 에 회귀 케이스를 넣었다(프로토에는 있으나 DefaultPlayer 에 없는 쿠키 1020 강화 → 실패해야 한다). **수정을 일시 되돌리면 이 테스트가 실제로 실패하는 것을 확인했다** — 통과만 보고 넘어가면 아무것도 안 지키는 테스트를 넣게 된다.

#### S5-H `OwnScope` 중복을 `ServiceBase` 로 올렸다

S5 가 서비스 6개에 같은 3줄(`GameDb _db` 필드 + `OwnScope` 프로퍼티 + 주석)을 복사해 넣었다. `ServiceBase` 가 이미 `RpcContext` 를 들고 있으므로 거기로 올린다.

```csharp
public class ServiceBase
{
    protected GameDb Db { get; private set; }

    // 요청 주체의 스코프. 계산 프로퍼티인 이유는 PlayerId 가 요청 도중 정해지기 때문이다.
    protected UserScope OwnScope => Db.User(RpcContext.ShardId, RpcContext.PlayerId);
}
```

`AuthService` 도 자기 `_db` 필드를 버리고 `Db` 를 쓴다. `CommonService`(헬스체크)까지 `GameDb` 를 받게 되는 것이 유일한 비용인데, `GameDb` 는 커넥션을 첫 조회에서야 여는 지연 구조(§S2)라 실비용이 0 이다.

같이 고친 것: `EnhanceCookieLvAsync` 가 `OwnScope` 를 두 번 평가해 `UserScope` 인스턴스를 둘 만들던 것을 지역 변수 하나로 합쳤다.

#### S5-I 리뷰에서 나왔으나 **일부러 안 고친 것**

- **신규 행에 INSERT 직후 UPDATE 가 한 번 더 나간다.** 첫 재화 획득이면 `GetOrCreateAsync` 가 `{Num, Amount=0}` 을 INSERT 하고 이어서 `UpdateAsync` 가 UPDATE 한다. DB 2 왕복 + 캐시 2 회 쓰기다. 옛 경로도 같았으므로 회귀는 아니고, **저장 경로를 정리하는 S6 에서 `ChangeSet` 과 함께 본다.**
- **`reason` 인자가 Inc 계열 5곳에서 미사용이다**(`IncPoint`/`IncTicket`/`IncItem`/`IncCookie`/`IncSoulStone`). 도메인 메서드가 검증을 안 하므로 안 받는다. S13 감사 로그가 쓸 자리라 남겼다.
- **`PlayerDetailManager` 의 Dec/Inc × Point/Ticket/Item 6 블록이 구조적으로 동일하다.** 제네릭 헬퍼로 묶고 싶어지지만 **S6 에서 이 클래스가 통째로 사라진다.** 곧 지울 코드의 중복을 없애는 것은 손해다.
- **`Owned<T>()` 가 호출마다 `OwnedSet` + `CacheKey` 문자열을 새로 만든다.** `IncRewardListAsync` 루프에서 보상 개수만큼 반복된다. §S2-H 가 무상태를 택한 대가이고, 프로파일에 잡히면 그때 스코프별로 메모이즈한다.

#### S5-J `PlayerDetail.TouchAsync(userScope)` 는 이관 기간 전용 다리다

`PlayerDetailComponent.TouchAsync` 가 `UserScope` 를 인자로 받는 모양은 **S6 에서 통째로 사라진다.** S6 이 `PlayerDetailManager`(330줄)를 `Domain/PlayerDetailModel.Logic.cs` + `Domain/RewardHelper.cs` 로 분해하면서 `PlayerDetailComponent` 자체가 삭제되기 때문이다. 그때 App Service 는 자기가 든 스코프에서 직접 `PlayerDetailModel` 을 꺼내고, Manager 에 스코프를 넘겨주는 단계가 사라진다.

즉 이 인자는 **"구 경로 객체가 신 경로 데이터를 필요로 하는" 혼재 상태의 표시**이고, §S4-D 의 `SetShardId` 다리와 같은 종류다. 남은 수명은 S6 한 스텝이다.

---

### S6 — PlayerDetail 분해 + `RewardService`

> **2026-08-23 전면 재도출.** 옛 본문은 폐기했다. 코드와 어긋난 곳이 6군데였고 그중 하나는 그대로 옮기면 버그가 됐다. 그리고 **`LoadedObjects` 설계를 철회했다** — 근거였던 "벌크 로드" 이득이 실재하지 않는다(§S6-계획-C).

#### 위상 정정 — 이 스텝은 "A안 핵심 검증"이 아니다

옛 본문은 S6 을 A안 전체의 검증 지점으로 적고 그 근거로 **"`_userRepo` 를 드는 클래스가 하나도 남지 않는다"**를 들었다. **거짓이다.** S6 이 `PlayerDetailComponent`/`PlayerDetailManager` 를 지워도 `UserComponentBase` · `UserManagerBase` + Player/Kingdom×3/World×2 의 Component 6 + Manager 6, 합쳐 **14개가 그대로 `_userRepo` 를 든다.** 그것들이 사라지는 것은 S7 · S9 · S10 이다.

게다가 **의사결정 게이트는 이미 S5 에서 통과했다**(§S5-D, MySQL + Redis 실측 17/17). 그러니 S6 이 답하는 것은 하나다 — **재화 라우팅이 순수해지는가.** §S4-B · §S5-B 에 이어 **앞선 스텝의 결과가 뒷 스텝의 문구를 낡게 만든 네 번째 사례**다.

#### 옛 본문이 코드와 어긋난 곳

| # | 옛 본문 | 실제 |
|---|---|---|
| 1 | `MarkDirty()` | §S2-H 에서 철회. 저장은 명시 호출 |
| 2 | "`_userRepo` 를 드는 클래스가 하나도 안 남는다" | 14개 남는다 (위) |
| 3 | `loaded.Point(cost.Key.Num)` | **틀린 컬럼.** 현재 라우터는 `(int)objType` 을 쓴다 |
| 4 | `LoadedObjects` / `user.LoadObjectsAsync(keys)` | 존재하지 않고, 만들지 않기로 했다 |
| 5 | `Owned<PlayerDetailModel>().GetOneAsync()` | `GetOneAsync` 는 없다. 만들지 않는다 |
| 6 | `ChangeSet` | S5 에서 미뤘으므로 S6 이 만든다 |

**#3 이 가장 위험하다.** 포인트/티켓의 번호는 `ObjKey.Num` 이 아니라 **`(int)ObjKey.Type`**(enum 값 자체)이다. ITEM/COOKIE 만 `Num` 을 쓴다. 옛 예제대로 옮겼으면 **모든 포인트 차감이 0번 포인트로** 갔고, 예외 없이 조용히 틀렸을 것이다.

#### S6-계획-A census (디렉터리 제외 없이)

| 표면 | 호출부 |
|---|---|
| `PlayerDetail.TouchAsync` | 12 — 서비스 5개 + **`PlayerManager.LoadPlayerAsync`** |
| `DecCostAsync` | 4 (Cookie, Gacha, Kingdom×2) |
| `IncRewardListAsync` / `IncRewardAsync` | 5 (Cheat, Gacha, World×3) |
| `DecCashAsync` / `GetCashPacket` | 2 (Kingdom) |

**또 `PlayerManager` 가 끌려온다.** S7 클래스인데 S6 이 건드려야 한다 — §S5-B 와 같은 패턴이라 이번에는 착수 전에 셌다. `KingdomService` 의 `_ = await OwnUser.PlayerDetail.TouchAsync(OwnScope);` 는 로드해서 버리는 줄이라 같이 지운다.

#### S6-계획-B 건드리는 파일

- 삭제: `Manager/PlayerDetailManager.cs`(348줄), `Component/PlayerDetailComponent.cs`
- 추가: `Domain/PlayerDetailModel.Logic.cs`, `Domain/ChangeSet.cs`, `Service/RewardService.cs`, `Data/Queries/PlayerDetailQueries.cs`
- 수정: `Repo/UserRepo.cs`, `Manager/PlayerManager.cs`, 서비스 5개(24곳), `ClientCore/ContextSystem.Sync.cs`

#### S6-계획-C `LoadedObjects` 를 만들지 않는다 (사용자 지적으로 철회)

옛 본문은 App Service 가 필요한 모델을 미리 벌크 로드해 `LoadedObjects` 로 넘기고 `RewardHelper` 는 순수 static 이 되는 그림이었다. 사용자가 **"왜 한 번에 로드해야 하는지 모르겠다, 각자 필요할 때 로드하면 되는 것 아닌가"**라고 물었고, 확인해 보니 **철회가 맞다.**

- **벌크 로드 이득이 없다.** `OwnedSet.GetListAsync` 는 **소유자 리스트 전체**를 읽고 캐시에 넣는다. 보상 루프에서 두 번째부터의 `Owned<PointModel>()` 은 DB 가 아니라 **캐시 히트**다. 즉 SELECT 는 이미 엔티티 타입당 1회이고, UPDATE 는 바뀐 행 수만큼으로 `LoadedObjects` 를 써도 동일하다. **DB 왕복이 하나도 안 줄어든다.**
- **순수성이 잘못된 계층에 있다.** 테스트 가치가 있는 로직은 `DecCash` 의 RealCash 우선 소모 순서, `IncCookie` 의 첫 장 규칙 같은 **모델 메서드**이고 그건 어차피 순수하다. enum switch 하나를 순수하게 만들려고 새 타입과 `NOT_LOADED_OBJECT` 라는 **없던 실패 모드**를 들이는 것은 손해다.
- §S1-G 에서 `IDataScope` 를 철회할 때와 같은 실수였다 — **"공유가 필요한가"를 안 묻고 "어떻게 공유하나"에 답한 것.**

#### S6-계획-D `RewardService` — 상태 없음, 스코프는 인자, 타입별 개별 구현

```csharp
// Domain 이 아니다 - DB 를 지나간다. 상태가 없고 대상은 인자로 받는다.
public static class RewardService
{
    public static async Task<ChangeSet> PayAsync(UserScope userScope, ObjValue cost, string reason)
    {
        ReqHelper.ValidUnderFlowParam(cost.Value, reason);
        var amount = ReqHelper.ValidWithoutDecimal(cost.Value, reason);

        switch (cost.Key.Type.ToObjTyeCategory())
        {
            case EObjType.GOLD:         return await DecGoldAsync(userScope, amount, reason);
            case EObjType.EXP:          return await DecExpAsync(userScope, amount, reason);
            case EObjType.TOTAL_CASH:   return await DecCashAsync(userScope, amount, reason);
            // 포인트/티켓의 번호는 enum 값 자체다. cost.Key.Num 이 아니다.
            case EObjType.POINT_START:  return await DecPointAsync(userScope, (int)cost.Key.Type, amount, reason);
            case EObjType.TICKET_START: return await DecTicketAsync(userScope, (int)cost.Key.Type, amount, reason);
            case EObjType.ITEM:         return await DecItemAsync(userScope, cost.Key.Num, amount, reason);
            default: throw new GameException(EErrorCode.PARAM, "NO_HANDLING_COST_OBJ_TYPE", new { cost.Key.Type });
        }
    }

    public static async Task<ChangeSet> DecPointAsync(UserScope userScope, int num, double amount, string reason)
    {
        var pointSet = userScope.Owned<PointModel>();
        var point = await pointSet.GetOrCreateAsync(num);
        var change = point.DecAmount(amount, reason);
        await pointSet.UpdateAsync(point);
        return change;
    }

    public static async Task<ChangeSet> IncPointAsync(UserScope userScope, int num, double amount)
    {
        var pointSet = userScope.Owned<PointModel>();
        var point = await pointSet.GetOrCreateAsync(num);
        var change = point.IncAmount(amount);
        await pointSet.UpdateAsync(point);
        return change;
    }
}
```

**타입별로 개별 구현한다(사용자 결정).** `Func<PointModel, ChangeSet> apply` 로 일반화하지 않는다 — 라우터에서 중복 5줄보다 **어떤 타입이 어떤 모델을 어떻게 건드리는지 한눈에 보이는 것**이 값이 크다. 이름은 옛 표면과 같은 `Dec*` / `Inc*` 를 쓴다. 필요한 메서드는 Dec 6종(Gold/Exp/Cash/Point/Ticket/Item) · Inc 9종(Gold/Exp/FreeCash/RealCash/Point/Ticket/Item/Cookie/SoulStone)이다.

**스코프를 필드로 안 들고 인자로 받는 이유**: ① 인스턴스 상태가 사라져 DI 등록이 필요 없다 ② §S5-C 의 "PlayerId 가 요청 도중 정해지는" 함정을 피한다 ③ 나중에 우편·길드처럼 **다른 플레이어에게 지급**하는 경우에 그대로 쓴다.

#### S6-계획-E `PlayerDetailModel` 도 다른 엔티티와 같은 규칙

엔진에 `GetOneAsync` 를 넣지 않는다. **다른 4개와 똑같이 Queries 확장** 하나로 끝난다.

```csharp
public static class PlayerDetailQueries
{
    public static async Task<PlayerDetailModel> GetOrCreateAsync(this OwnedSet<PlayerDetailModel> set)
    {
        var list = await set.GetListAsync();
        return list.Count > 0 ? list[0] : await set.CreateAsync(new PlayerDetailModel());
    }
}
```

캐시 키가 `PlayerDetailModel:{playerId}` 로 **기존과 동일**해 호환되고, 무엇보다 **S7 의 `Single` 정책을 미리 정하지 않는다.** `PlayerDetail` 이 다른 모델과 얽혀 있던 이유는 `PlayerDetailManager` 가 라우터였기 때문이고, 그 얽힘은 `RewardService` 가 떼어간다. 없으면 만드는 동작(`GetOrCreate`)은 유지한다(사용자 확인).

#### S6-계획-F `ChangeSet` 의 의미를 확정한다 — 클라 코드가 이미 그 규칙이다

**`Amount` = 이번에 변화된 양, `TotalAmount` = 현재 값 (사용자 결정).**

```csharp
public readonly record struct ChangeSet(EObjType Type, int Num, double Before, double After)
{
    public double Delta => After - Before;
    public static ChangeSet Of(EObjType t, int n, double b, double a) => new(t, n, b, a);
}

public static ChgObjPacket ToPacket(this ChangeSet c)
    => new() { Type = c.Type, Num = c.Num, Amount = c.Delta, TotalAmount = c.After };
```

`ClientCore/ContextSystem.SyncChgObj` 를 확인한 결과 **대부분 이미 이 규칙으로 동작한다.**

| 타입 | 클라 동작 | 적용 시 |
|---|---|---|
| EXP / GOLD / FREE_CASH / REAL_CASH / POINT / TICKET / ITEM | `TotalAmount` 를 현재값으로 대입 | **무변경** |
| COOKIE / SOUL_STONE | `Amount`(획득 개수)로 **클라가 소울스톤을 다시 계산** | **클라도 같이 고친다** |
| TOTAL_CASH | `TotalAmount` 를 차감액처럼 사용 | **클라 버그**(아래) |

COOKIE/SOUL_STONE 분기는 `InitSoulStone` 환산과 "첫 획득이면 한 장은 쿠키" 규칙을 **서버 로직 그대로 복제**하고 있다. 서버가 `Amount` = 실제 증가한 소울스톤, `TotalAmount` = 현재 소울스톤을 보내면 클라 분기는 `pakCookie.SoulStone = TotalAmount` + State 갱신으로 줄어든다. `ClientCore` 가 `Code.sln` 안에 있어 양쪽을 같이 고칠 수 있다.

**덤으로 발견한 클라 버그**: `TOTAL_CASH` 분기가 `TotalAmount`(차감 후 총 캐시)를 비용으로 쓰고 `Player.FreeCash = freeCashCost` 로 대입한다. 서버는 지금도 현재값을 보내므로 **클라가 틀렸다.**

#### S6-계획-G 옮기면서 같이 고치는 기존 이상 2건

- **`DecGold` 에만 잔액 검사가 없다.** `DecExp` · `DecCash` 에는 `ValidEnough` 가 있는데 골드에만 없어서 **골드가 음수로 갈 수 있다.** 모델로 옮기면서 넣는다(사용자 결정).
- **`Acc*` 가 이름과 다르게 동작한다.** Dec 에서도 `Acc` 를 같이 줄여 `Acc == 현재값` 이 되어 "누적"이 아니었다. 진짜 누적인 것은 `AccSoulStone` 하나뿐이었다. **차감에서 `Acc` 를 건드리지 않도록 고친다(사용자 결정).** Point/Ticket/Item 은 S5 산출물이라 먼저 고쳤고, Gold/Exp/Cash 는 어차피 모델로 새로 쓰므로 여기서 같이 고친다.
  > **마이그레이션은 없다.** 기존 행의 `Acc` 는 과거분이 틀린 채로 남고, `pakPlayer.AccGold/AccRealCash/AccFreeCash` 와 `pakCookie.AccSoulStone` 은 클라에 노출되므로 **표시되는 숫자의 의미가 바뀐다.**

#### 직후 가능해지는 것

- `PlayerDetailManager`(348줄) 소멸 — 모델 하나의 이름을 달고 여섯을 라우팅하던 클래스가 사라진다
- 재화 로직이 `PlayerDetailModel` 위의 순수 메서드가 되어 **DB 없이 단위 테스트 가능**해진다
- 서비스가 재화를 쓸 때 **모델 매니저를 거치지 않는다** — `RewardService.DecCashAsync(OwnScope, ...)` 처럼 대상과 동작이 호출부에 다 보인다
- `_userRepo` 를 드는 클래스가 16 → 14

#### 아직 안 되는 것

- Player/Session/Schedule/World/Kingdom 미이관. `GlobalDbRepo` 건재. `_userRepo` 14개 잔존
- 감사 로그 쓰기 경로 없음 → §S2-J 의 `VerifyCacheTags` 역방향 검사는 여전히 S13
- 신규 행 INSERT 직후 UPDATE(§S5-I)는 여기서도 그대로다 — `GetOrCreateAsync` 뒤에 `UpdateAsync` 를 부르는 형태가 유지된다

---

#### S6-A 실행 결과 (2026-08-23, branch `db-refactor`)

사양대로 실행했다. 계획이 코드에 의해 뒤집힌 곳은 없다 — §S6-계획-C에서 `LoadedObjects`를 미리 걷어낸 것이 컸다.

- 삭제 2: `Manager/PlayerDetailManager.cs`(348줄), `Component/PlayerDetailComponent.cs`
- 추가 5: `Domain/ChangeSet.cs`, `Domain/PlayerDetailModel.Logic.cs`, `Data/RewardService.cs`, `Data/Queries/PlayerDetailQueries.cs`, `Server/Extension/ChangeSetExtension.cs`
- 수정 10: `Repo/UserRepo.cs`, `Manager/PlayerManager.cs`, `Domain/{Point,Ticket,Item,Cookie}Model.Logic.cs`, 서비스 6개, `ClientCore/ContextSystem.Sync.cs`

**`RewardService` 는 `Code/DbModel/Data/` 에 두었다**(계획서에는 `Service/` 로 적었다). `GameDb`/`UserScope`/`OwnedSet` 옆이 맞다 — 스코프를 받아 로드·적용·저장하는 것이 이 폴더가 하는 일이고, `Code/Server/Service` 는 RPC 단위 DI 서비스의 자리다.

**규칙 하나로 통일했다: 모델은 값을 바꾸고 바뀐 값을 반환하고, `ChangeSet` 은 `RewardService` 가 만든다.** 모델이 `ChangeSet` 을 만들게 하면 `SOUL_STONE` 에서 깨진다 — 쿠키 모델을 바꾸지만 응답에는 소울스톤 번호가 실려야 하는데 모델은 그 번호를 모른다. `Type`/`Num` 은 요청이 지목한 `ObjKey` 를 그대로 통과시킨다.

**검증**: `Code.sln` 리빌드 0에러 · unique warning **34**(S5 의 35에서 1 감소, `PlayerDetailComponent` 삭제분) · ServerTest **17/17** (InMemory) · **MySQL + Redis + `UseUserLock: true` 17/17**.

`_userRepo` 를 드는 클래스는 16 → **14**. §S6 위상 정정에서 예측한 수와 같다.

#### S6-B 커밋 전 자율 리뷰 3회 (2026-08-23)

**1회차 — 정합성/회귀.** S6 이 만든 죽은 의존성 3건을 찾아 걷어냈다: `CheatService` 의 `GlobalDbRepo`·`IMapper`(둘 다 무사용), `CookieService` 의 `OwnUser`/`GlobalDbRepo`, `GachaService` 의 `OwnUser`(`Center` 는 계속 쓴다). 재화 라우팅이 서비스에서 빠지자 서비스가 `GlobalDbRepo` 를 들 이유가 함께 사라진 것이다.

와이어 의미 변경도 여기서 확인했다 — **`ChgObjPacket.Amount` 가 요청값에서 부호 있는 증감량(`After - Before`)이 됐다.** 차감이면 음수가 나간다. `ClientCore` 와 `ServerTest` 를 전부 훑어 **`Amount` 를 읽는 소비자가 하나도 없음**을 확인했다(클라는 전부 `TotalAmount` 를 쓴다).

**2회차 — 설계/일관성.** 서비스 6개의 `using` 블록을 필요한 것만 남기고 알파벳 순으로 정리했다(제거한 것이 전부 실제 미사용임을 빌드로 확인). §S5-I 에서 "위치가 파일마다 다르다"고 적어두고 넘어갔던 것이 S6 에서 더 늘어서 이번에 정리했다.

`PlayerDetailModel.TotalCash()` 를 **프로퍼티가 아니라 메서드**로 둔 것도 여기서 재확인했다 — `.Logic.cs` 파샬에 public 프로퍼티를 추가하면 `DapperExtension` 의 `GetProperties` 가 그것을 DB 컬럼으로 본다(§S2 의 함정). **`.Logic.cs` 에는 프로퍼티를 만들지 않는다**가 규칙이다.

**3회차 — 경계/실패 경로. 여기서 실제 결함이 나왔다.**

`KingdomStructureDecTimeAsync` 가 클라이언트가 보낸 캐시 금액을 검증 없이 `DecCashAsync` 로 넘긴다. 음수를 넣으면:

```
DecCash(-100)  ->  ValidEnough(-100, total)          통과 (-100 <= total)
               ->  realCashCost = Math.Min(RealCash, -100) = -100
               ->  RealCash -= (-100)                RealCash 가 100 늘어난다
```

**S6 이 만든 것이 아니다** — 옛 `PlayerDetailManager.DecCashAsync` 직접 호출도 같은 형태였다. `PayAsync` 경로만 `ValidUnderFlowParam` 으로 막혀 있었고 **직접 호출 경로가 뚫려 있었다.**

**심각도 정정**: 조사 중에 `KingdomStructureDecTimeAsync` 가 **RPC 에 등록되어 있지 않다**는 것을 발견했다(`KingdomItemChangeAsync`, `GameService.ChangeNameFirstAsync` 도 같다). 따라서 **오늘 네트워크로 도달할 수 없는 잠재 결함**이다. 그래도 고친다 — 등록되는 날 살아나고, 라우터만 믿는 구조 자체가 문제다.

**고친 자리는 라우터가 아니라 모델이다.** `amount > 0` 은 증감의 불변식이므로 `PlayerDetailModel.Dec/Inc*`, `Point/Ticket/ItemModel.DecAmount/IncAmount`, `CookieModel.IncCookie/IncSoulStone` 에 `ValidUnderFlowParam` 을 넣었다. 라우터에 넣으면 다음에 직접 호출 경로가 생길 때 또 뚫린다.

> 회귀 테스트는 **넣지 못했다.** 엔드포인트가 등록돼 있지 않아 HTTP 로 도달할 수 없고, 이 저장소에는 모델 단위 테스트 자리가 없다(`Code/Server.Tests` 는 bin/obj 만 남은 빈 디렉터리다). 등록되는 날 테스트도 같이 넣어야 한다.

#### S6-C 리뷰에서 나왔으나 안 고친 것

- **등록되지 않은 엔드포인트 3개**(`KingdomStructureDecTimeAsync`, `KingdomItemChangeAsync`, `GameService.ChangeNameFirstAsync`). 지울지 등록할지는 게임 기획 판단이라 손대지 않았다. `KingdomStructureDecTimeAsync` 는 `// TODO: 남은 시간, 캐시 보유량 일치하는지 검증` 이 남아 있어 **등록 전에 그 검증부터 필요하다** — 지금 등록하면 클라가 보낸 금액을 그대로 받는다.
- **한 요청에서 같은 행을 두 번 UPDATE** 하는 경우가 있다(비용 차감 + 보상 지급이 모두 `PlayerDetail` 을 건드리면). 옛 경로도 같았다. §S5-I 의 "INSERT 직후 UPDATE" 와 같은 계열이라 저장 경로를 손보는 날 함께 본다.
- **`Acc*` 의미 변경에 마이그레이션이 없다.** 기존 행은 과거분이 틀린 채 남는다(§S6-계획-G).

---

### S7 — Player / Session (+ RaidServer) · T2 확정

**S7a(Player + RaidServer) / S7b(Session) 로 쪼갠다** (사양 재도출 2026-08-23). 두 작업은 성격이 다르다 — Player 는 `OwnedSet` 으로 옮기는 이관이고, Session 은 §5.3 이 경고한 포인터 캐시를 어떤 특화 클래스로 남기느냐는 설계 문제다. S5·S6 에서 한 스텝에 두 성격을 넣었다가 계획이 코드에 뒤집힌 전례를 반복하지 않는다.

#### S7-계획-A 착수 전 census 가 정정한 것

census 는 디렉터리를 제외하지 않고 전수로 세고 결과를 분류했다(§S5-B 규칙).

**① `PlayerComponent.TryGetByAccountIdAsync` 는 호출부가 0이다.** 옛 §S7 은 이 메서드를 위해 T2 보조 인덱스(`[SecondaryIndex("AccountId")]` + `ByIndexAsync`)를 설계해뒀는데, **아무도 안 쓰는 메서드를 위한 인프라**였다. 만들지 않고 메서드를 지운다. `[SecondaryIndex]` 는 실수요가 생기는 날 만든다.

**② T2 선언은 이미 끝나 있다.** `PlayerModel.generated.cs:7` 에 `[Entity(Pk = ["Id"], ScopeKey = "Id")]` 가 S1 에서 붙었다. 옛 §S7 이 "선언한다"고 적은 것은 완료된 작업이다.

**③ `PlayerManager`(150줄)는 S7 에서 사라지지 않는다.** `PreparePlayerAsync`/`LoadPlayerAsync` 가 `_userRepo.KingdomStructure`·`KingdomDeco`·`KingdomMap` 을 직접 부른다 — S10 영역이다. 옛 문서의 "Component 2 + Manager 2" 는 틀렸다. S6 의 `PlayerDetailManager` 는 통째로 사라졌지만 이쪽은 아니다. **지울 파일이 아니므로 구조 투자를 하지 않는다**(§S6-C 교훈): `_userRepo.Player` 참조만 스코프 경유로 바꾸고 나머지는 S10 까지 둔다.

**④ 옛 §S7 의 "After" 예제가 PlayerMap 을 거친다.** `SessionModel` 이 이미 `ShardId` 를 갖고 있고(`Data/Csv/Model/Auth/Session.csv`) 실제 코드 두 곳이 그렇게 읽는다(`RpcContext.cs:123`, `PlayerRaidSessionService.cs:55`). PlayerMap(S12)을 S7 으로 끌어오면서 왕복만 하나 늘어난다. (b)단계는 삭제한다.

**⑤ `Ip=""` 스텁 제거의 효과가 과장돼 있다.** `PublicIp` 를 쓰는 곳은 `SessionComponent.TouchAsync`(세션 **생성**) 하나인데 RaidServer 에는 생성 경로가 없다 — 읽기만 한다. S6 의 `KingdomStructureDecTimeAsync` 와 같은 계열로 **실재하지만 도달 불가**다. "PublicIp 수동 확인" 은 확인할 대상이 없다.

**⑥ `AuthService` 의 `// S7 에서 제거` 는 S7b 다.** `RpcContext.SetShardId` 가 남아 있는 이유는 `SessionComponent.TouchAsync:85` 가 그것을 읽기 때문이고, Session 은 S7b 이므로 S7a 에서는 뺄 수 없다.

**⑦ `ZERO_PLAYER_ID` 는 이중 검사다.** `PlayerAuthPolicy.Validate` 가 이미 `PlayerId != 0` 을 `CONTEXT_PLAYER` 로 막는다(`AuthPolicy.cs:50`). 등록된 엔드포인트는 전부 이 정책을 지나므로 데이터 계층의 재검사는 옮기지 않고 없앤다.

#### S7-계획-B S7a 범위

```
삭제   Component/PlayerComponent.cs (70줄)   TryGetByAccountIdAsync 는 호출부 0 이라 같이 소멸
신규   Data/Queries/PlayerQueries.cs         PlayerDetailQueries 와 같은 "행 하나" 모양
수정   Server/Service/GameService.cs         PlayerId 생성을 서비스로 · ChangeName 은 Get 으로
       Manager/PlayerManager.cs             _userRepo.Player -> 스코프 경유 (Kingdom 은 S10 까지 존치)
       Repo/UserRepo.cs                      Player 프로퍼티 제거
       RaidServer/Services/PlayerRaidSessionService.cs   앰비언트 제거
```

`_userRepo` 를 드는 클래스는 **14 → 12**(Base 2 + Component 5 + Manager 5). Player 만 빠지고 Kingdom×3 · World×2 는 S9·S10 이다.

#### S7-계획-C PlayerId 생성을 데이터 계층 밖으로 (§5.9)

지금은 `PlayerComponent.TouchAsync` 안에서 컨텍스트를 쓴다.

```csharp
// Before — 데이터 계층이 RpcContext 에 쓴다. ID 생성 규칙도 Component 안에 숨어 있다.
if (playerId == 0)
{
    _userRepo.RpcContext.SetPlayerId(accountId * 10);
    ...
}
```

```csharp
// After — 서비스가 정하고 컨텍스트에 반영한다. 데이터 계층은 읽지도 쓰지도 않는다.
var playerId = RpcContext.PlayerId;
if (playerId == 0)
{
    playerId = IdHelper.MakePlayerId(RpcContext.AccountId);   // 지금의 accountId * 10
    RpcContext.SetPlayerId(playerId);
}
```

`accountId * 10` 규칙은 의미를 바꾸지 않고 이름만 붙인다. 이것이 끝나면 `ServiceBase.OwnScope` 의 "요청 도중 PlayerId 가 정해진다"는 전제가 `EnterAsync` 한 곳으로 좁혀진다.

#### S7-계획-D ChangeNameFirstAsync 의 Touch 는 버그다

`ChangeNameFirstAsync` 가 `Player.TouchAsync()` 를 부른다 — **이름 변경 요청이 플레이어를 만들 수 있다.** 조회여야 한다(사용자 확인). RPC 미등록이라 오늘 도달 불가지만(§S6-C 의 3개 중 하나), S7a 에서 `Get` 으로 고친다.

#### S7-계획-E RaidServer

```csharp
// Before — 앰비언트에 묶여 "나"만 열 수 있다
dbRepo.BeginOwnUserRepo();
var playerModel = (await dbRepo.OwnUser.Player.GetAsync()).Model;

// After — 세션이 ShardId 를 이미 갖고 있으므로 PlayerMap 을 거치지 않는다(정정 ④)
var userScope = _db.User(mgrSession.Model.ShardId, mgrSession.Model.PlayerId);
var mdlPlayer = await userScope.Owned<PlayerModel>().GetAsync();
```

`AuthenticateAsync` 에 `CommitAsync()` 가 없는 것은 그대로 둔다 — 이 경로는 읽기 전용이고, 소켓 인증에서 세션을 연장하지 않는다는 결정이 이미 있다.

#### S7-계획-F S7a 가 안 하는 것

- **Session 전부**(S7b). `SessionComponent`(122) · `SessionManager`(118) · `AuthService.SetShardId` · `RpcContext` 의 세션 로드
- **`PlayerManager` 의 패킷 조립**(S10). Kingdom 3종을 직접 부르는 구조라 그때 같이 정리한다
- **`[SecondaryIndex]` / `ByIndexAsync`**(수요 발생 시). 정정 ①
- **`AllUserRepo.TryGetPlayerByNameAsync`**(S11). `ChangeNameFirstAsync` 가 유일 호출부이고 그것이 미등록이다

#### S7-A S7a 실행 결과 (2026-08-23, 커밋 7094407)

계획이 코드에 또 뒤집혔다. **③(`PlayerManager` 는 S10 까지 남는다)이 틀렸다** — 패킷 조립을 `GameService` 로 옮기면서 Kingdom 결합이 서비스로 따라갔고 `PlayerManager` 는 껍데기만 남아 통째로 사라졌다. 결합이 없어진 게 아니라 **이동**한 것이라 `GameService` 가 지금 `OwnUser.KingdomStructure`·`KingdomDeco`·`KingdomMap` 을 직접 든다. S10 에서 정리할 자리다.

**실행 중 잡은 버그 2건.**

첫째, **신규 플레이어의 `Id` 가 0 으로 덮였다.** `OwnedSet.CreateAsync` 는 `SetScopeKey(_scopeKeyValue)` 를 하는데 `PlayerModel` 의 ScopeKey 는 `Id` 자신이다. `PlayerId = 0` 인 스코프로 만들면 호출부가 넣은 Id 가 0 으로 덮인다. **Player 는 스코프에 속한 것이 아니라 스코프의 소유자 자신이라 다른 엔티티와 순서가 반대다** — Id 를 먼저 정하고 그 Id 로 스코프를 연다.

둘째, **`KingdomMapModel.Logic.cs` 가 `PointModel` 을 열고 있었다.** `Snapshot` 이라는 public 프로퍼티가 `PointModel` 에 생겨 Dapper 가 DB 컬럼으로 보고 던졌다. §S2 에서 발견하고 §S6-B 에서 재확인한 "`.Logic.cs` 파샬에는 프로퍼티를 만들지 않는다" 를 세 번째로 밟았다. **InMemory 는 Dapper 를 안 타서 17/17 로 통과했고 MySQL 에서만 드러났다.**

푼 스냅샷을 모델이 들고 있지 않기로 한 이유는 Dapper 만이 아니다. 들고 있으면 **고친 뒤 다시 직렬화하는 걸 잊어도 컴파일이 되고 조용히 저장되지 않는다.** 읽는 쪽은 그때그때 풀고(`ParseSnapshot()`), 고쳐 쓰는 쪽은 `KingdomMapManager` 로 남긴다(S10).

`OwnedSet.TryGetAsync` 에 `predicate = null` 기본값을 넣었다가 되돌렸다. 조건 없는 조회를 열면 소유자 리스트형(Cookie/Item/Point/Ticket)에서 "아무거나 하나"가 합법이 된다. 그리고 C# 은 확장 메서드보다 인스턴스 메서드를 먼저 고르므로 `PlayerQueries.TryGetAsync` 가 도달 불가 코드가 돼 있었다.

**검증**: 리빌드 0에러 · InMemory 17/17 · MySQL+Redis+유저락 17/17 · 임시 테스트로 `ChangeNameFirst`(변경→재입장 저장 확인→중복 차단)와 `ChangeName_DoesNotCreatePlayer`(`CONTEXT_PLAYER`) 확인. `_userRepo` 14 → **12**(예측과 일치).

**검증하지 못한 것**: RaidServer 소켓 인증 경로. `ServerTest` 는 HTTP 만 태운다. 빌드만 통과한 상태다.

---

### S7b — Session · 착수 전 census (2026-08-23)

#### S7b-계획-A census 가 정정한 것

**① `SessionComponent.LogoutAsync` 는 호출부가 0이다.** §5.3 이 "이미 완성된 T2 포인터 캐시"의 근거로 든 네 동작 중 하나가 실제로는 죽어 있다. 옮기지 말고 지운다. S7a 의 `TryGetByAccountIdAsync` 와 같은 패턴이라 **census 없이 문서만 보고 착수하면 또 안 쓰는 것을 이식한다.**

**② S7b 의 무게중심은 캐시가 아니라 컨텍스트다.** §5.3 은 포인터 캐시 유지만 경고했는데, 실제로 더 얽힌 것은 데이터 계층이 `RpcContext` 를 **읽기 7곳 + 쓰기 1곳**으로 쓰고 있다는 점이다.

```
SessionComponent   Ip · ShardId
SessionManager     ServerTime ×2 · Ip · DeviceKey ×2 · SetSessionKey(쓰기)
```

`AuthService` 의 `RpcContext.SetShardId` 가 남아 있는 이유는 `SessionComponent:85` **한 줄**이다. S7a 가 PlayerId 로 푼 것과 같은 문제인데 범위가 넓다.

**③ Auth 는 캐시가 없는 구조인데 Session 만 캐시를 쓴다.** `AuthScope`/`Identity` 는 `IDbSession` 직행이고 주석이 "Auth DB는 캐시를 안 쓰므로 IRepository의 캐시 경로를 안 지남" 이라고 못 박고 있다. Session 을 `AuthScope` 에 넣으면 캐시가 사라져 §5.3 이 경고한 후퇴가 일어나고, 안 넣으면 Auth 안에 캐시 있는 것과 없는 것이 공존한다. **어느 쪽이든 규칙을 하나 고쳐야 한다.**

**④ `Identity` 가 이미 자리를 비워두고 있다.** 주석에 `// 세션 키 조회는 아직 SessionComponent에 있음` 이 적혀 있다. 세션 키 조회는 accountId 를 모르는 상태의 조회라 §S1-G 의 "스코프를 여는 데 필요한 조회는 스코프 밖" 에 해당한다.

**⑤ `AuthComponentBase`(64) / `AuthManagerBase`(15) 는 S7b 에서 안 사라진다.** `PlayerMapComponent` 가 `AuthComponentBase` 를 상속한다. PlayerMap 은 S12 다.

#### S7b-계획-B 갈라야 하는 세 가지

`SessionManager`(118) 는 순수 판단과 IO 와 컨텍스트가 한 덩어리다.

| 무엇 | 지금 | 가야 할 곳 |
|---|---|---|
| 만료됐나 · 절반 지났나 · grace 안인가 · `IsExpire` | `SessionManager` | `SessionModel.Logic.cs` (순수, 시각은 인자로) |
| 2키 캐시 로드 · 키 로테이션 저장 | `SessionComponent` | 특화 store (엔티티 전용, §5.4.1) |
| ServerTime · Ip · DeviceKey 주입 · `SetSessionKey` | 데이터 계층이 직접 | 호출부(`AuthService`/`RpcContext`) |

#### S7b-계획-C 열린 결정

**Q1. 세션 store 의 자리.** `Identity` 에 합칠지(④의 주석이 그 의도), 아니면 `Data/SessionStore.cs` 로 따로 둘지. 캐시 정책이 `Identity`(무캐시)와 정반대라 **따로 두는 쪽**이 "Auth 는 캐시 없음" 규칙을 지킨다.

**Q2. `StartAsync` 의 키 로테이션을 누가 하는가.** 지금은 데이터 계층이 새 키를 만들고 `RpcContext.SetSessionKey` 까지 한다. 호출부로 올리면 `AuthService` 가 키를 만들어 넘기고 자기 컨텍스트를 갱신한다(§S2-E: 컨텍스트 쓰기는 Transport 인접 계층의 일).

**Q3. `ExtendAsync` 를 소켓 인증에도 붙일지.** 안 붙인다 — 이미 결정돼 있다. RaidServer 는 읽기만 한다.

---

#### S7b-A 실행 결과 (2026-08-23, 커밋 fd6ad88)

> **이 절과 아래 §S8-A · §S8-B · §S9-A · §S10-A 는 실행 시점이 아니라 2026-08-24 에 커밋 메시지와 diff 로 재구성했다.** S1~S7a 는 실행하면서 썼고 이 넷은 안 썼다. 그래서 여기에는 **결과와 그 근거만** 있고, 실행 중에만 알 수 있는 것 — 시도했다가 되돌린 것, 어디서 오래 막혔는지 — 은 아래 appsettings 건 하나를 빼면 남아 있지 않다. 되살릴 방법이 없으므로 없는 채로 둔다.

**계획-C 의 세 질문에 답이 다 나왔다.**

**Q1 → 따로 둔다.** `Data/SessionStore.cs`. 잠정이 그대로 확정됐다. 근거는 계획이 적은 그대로다 — `Identity`/`AuthScope` 는 "Auth DB 는 캐시를 안 쓴다"를 주석으로 못 박고 있고, 세션은 **매 요청의 인증 경로**라 DB 로 내려가면 전 요청이 느려진다. 합치면 둘 중 하나가 거짓말이 된다. 포인터(세션 키 → accountId)와 값(accountId → `SessionModel` + sliding TTL) 두 키를 그대로 유지했다.

**Q2 → 호출부로 올린다. 그리고 옛 입구를 닫았다.** 키 로테이션은 `StartAsync` 만 할 수 있고, 이전 키를 넘기는 저장은 **private 으로 닫았다.** 이 "닫았다"가 판단의 핵심이다 — 호출부가 모델을 바꾼 뒤에 이전 키를 잡으면 옛 포인터가 남아 **로테이션된 뒤에도 옛 키로 인증이 통과한다.** 순서를 지키라고 문서에 적는 대신 순서를 틀릴 수 있는 입구를 없앴다. §S6-B 가 "불변식은 모델이 든다"로 내린 것과 같은 결이다.

**Q3 → 변동 없음.** `ExtendAsync` 는 소켓 인증에 안 붙인다.

**계획-A ⑤ 가 틀렸다.** "`AuthComponentBase`(64) / `AuthManagerBase`(15) 는 S7b 에서 안 사라진다. PlayerMap 은 S12 다" 였는데 **두 겹으로 틀렸다** — `AuthManagerBase` 는 S7b 에서 바로 나갔고, `AuthComponentBase` 는 S8 에서 나갔다. PlayerMap 이 S12 가 아니라 S8 로 당겨졌기 때문이다. 어느 쪽도 S12 가 아니다.

**계획-B 표의 "컨텍스트 → 호출부" 는 `SessionStamp` 라는 값 타입이 됐다.**

```csharp
// Data/SessionStamp.cs — 데이터 계층이 IGameContext 를 직접 읽지 않게 값으로 끊었다
public readonly record struct SessionStamp(DateTime ServerTime, string Ip, string DeviceKey);
```

데이터 계층이 `RpcContext` 를 읽던 **8곳이 사라졌고**, 그래서 §S4-D 부터 다리로 끌고 온 `AuthService` 의 `SetShardId` 도 같이 없앴다. **§S4-D 가 "`SetShardId` 는 저절로 안 사라진다"고 적어둔 것이 여기서 닫혔다.**

**지운 쓰기 하나.** `SignIn`/`SignUp` 의 `ExpireAsync` 를 뺐다. 바로 뒤 `StartAsync` 가 상태를 ACTIVE 로 덮으므로 쓰기 한 번이 그냥 낭비였다. 옛 키가 막히는 것은 `ExpireAsync` 가 아니라 **포인터 무효화**가 하는 일이고, 임시 테스트로 확인했다.

**`LogoutAsync` 는 호출부 0인데 이번엔 안 지웠다.** `SessionStore` 에 옮겨두고 미사용으로 남겼다. S7a 의 `TryGetByAccountIdAsync`, S8 의 `TryGetPlayerMapAsync` 는 같은 조건에서 지웠으므로 **여기만 기준이 다르다.** 세션 수명 관리의 짝(시작이 있으면 종료가 있다)이라는 것 말고 기록된 근거가 없다. 지울지 살릴지는 열어둔다.

**부수 발견 — 진단 도구가 꺼져 있었다.** `ServerTest/appsettings.yaml` 의 `Game` 항목이 전부 루트에 있어 **통째로 무시되고 있었다**(`GameConfig` 는 `"Game"` 섹션에서 읽는다). 그 안의 `IsShowErrorDetail` 이 꺼진 탓에 MySQL 실패가 6자리 해시로만 보였고 이번 진단에서 한참 돌았다. **설정이 무시되고 있다는 것 자체가 조용하다** — 섹션 이름이 틀리면 예외 없이 기본값으로 돌아간다.

---

### S8 — Schedule / PlayerMap · T0 확정

#### S8-계획-A 착수 전 census 가 정정한 것

**① 옛 §S8 의 예제는 컴파일되지 않는다.**

```csharp
var schedules = await center.Owned<ScheduleModel>().GetListAsync();   // 옛 문서
```

`CenterScope` 에는 `Owned<T>` 가 없고, `OwnedSet<T>` 는 `where T : ModelBase, IScopedModel, new()` 다. `ScheduleModel` 은 `[Entity(Pk = ["Num"])]` 뿐이라 `IScopedModel` 이 아니고 `CacheKeyTags` 에도 없다 — 생성자에서 `NOT_FOUND_SCOPE_KEY` / `NOT_FOUND_CACHE_TAG` 로 던진다. **소유자 축이 없는 엔티티를 `OwnedSet` 으로 다루려던 것이 애초에 모순이다**(§5.4.1 이 "소유자 리스트 / 캐시 없음 2종만" 으로 좁힌 이유).

**② "매 요청마다 Schedule 전량 조회" 는 사실이 아니다.** 호출부는 둘뿐이다.

```
GachaService:26  centerRepo.Schedule.GetListAsync()   -> ScheduleLoad RPC 에서만. 전량 조회.
GachaService:36  centerRepo.Schedule.GetAsync(num)    -> 가챠. SelectByPkAsync 단건 조회.
```

가챠는 PK 단건이다. 전량 조회는 클라가 스케줄 목록을 받을 때뿐이다. `GlobalList` 캐시의 이득은 옛 문서가 적은 것보다 **작다** — 무효화 설계를 얹을 값이 있는지 먼저 따져야 한다.

**③ `PlayerMapComponent.TryGetPlayerMapAsync` 는 호출부가 0이다.** `CreateAsync` 만 `GameService:52` 에서 쓴다. S7a 의 `TryGetByAccountIdAsync`, S7b 의 `LogoutAsync` 에 이어 **세 번째**다. 옮기지 말고 지운다.

**④ `PlayerMapComponent` 는 29줄이고 실제 기능은 INSERT 하나다.** `AuthScope` 에 메서드 하나로 들어간다. 옛 문서가 S8 을 "Schedule / PlayerMap" 으로 묶은 것은 크기 때문이 아니라 둘 다 남은 찌꺼기여서다.

**⑤ `ScheduleManager` 는 Manager 지만 DB 를 안 탄다.** 생성자에서 Proto + Model 을 겹쳐 필드를 채우고 기간 판정 메서드만 갖는다. `CenterManagerBase` 를 상속하지만 `_centerRepo` 를 쓰지 않는다 — §3.4 가 말한 "읽기 전용 뷰" 가 맞다.

**⑥ `AutoMapperProfile` 이 `ScheduleManager -> SchedulePacket` 을 매핑한다.** 뷰로 바꾸면 이 매핑도 같이 바뀐다. record struct 로 만들면 AutoMapper 설정이 필요 없게 손으로 조립하는 편이 짧다(필드 6개).

#### S8-계획-B 열린 결정

**Q1. `GlobalList` 캐시를 이번에 넣는가.** ②를 보면 이득이 큰 자리는 `ScheduleLoad` 하나다. 넣는다면 무효화(운영툴이 DB 를 직접 고치는 경우)까지 같이 와야 하고, 지금 그 운영툴은 없다. **캐시 없이 옮기고 `GlobalList` 는 수요가 생길 때** 라는 선택지가 있다 — §S7a 의 `[SecondaryIndex]` 를 안 만든 것과 같은 판단이다.

**Q2. 스케줄 뷰의 자리.** `ScheduleView(ScheduleProto Prt, ScheduleModel Mdl)` 를 `Data/` 에 둘지 `Domain/` 에 둘지. Proto 를 아는 타입이라 모델 파샬은 아니다.

**Q3. `CenterScope` 의 모양.** 지금은 빈 껍데기다. Schedule 이 들어가면 `Identity`/`AuthScope` 처럼 `IDbSession` 직행이 되는데, 그러면 캐시가 없다는 점에서 Auth 와 같아진다.

---

#### S8-A 실행 결과 (2026-08-23, 커밋 932f58e)

**계획-B 의 세 질문에 답이 다 나왔다.**

**Q1 → `GlobalList` 캐시를 넣지 않는다.** 도입 근거였던 "매 요청마다 Schedule 전량 조회"가 **사실이 아니었기 때문이다**(계획-A ②). 전량 조회는 `ScheduleLoad` 한 곳이고 가챠는 PK 단건이다. 무효화 설계를 얹을 만큼 이득이 크지 않고, 그 무효화가 필요한 운영툴도 아직 없다. §S7a 가 `[SecondaryIndex]` 를 안 만든 것과 같은 판단이다 — **수요가 생길 때 만든다.**

**Q2 → `Data/ScheduleView.cs`.** `readonly record struct(ScheduleProto Prt, ScheduleModel Mdl)`. Proto 를 아는 타입이라 모델 파샬이 아니고, DB 를 안 타므로 값이다.

**Q3 → `CenterScope` 는 `IDbSession` 직행.** 캐시가 없다는 점에서 Auth 와 같아졌다. 클래스 주석이 그 이유를 든다 — "소유자 축이 없어서 경계가 아니라 **DB 선택**이다. 그래서 인자가 없고 `OwnedSet<T>` 도 못 쓴다."

**`ScheduleManager` 를 뷰로 바꾸면서 겹치기를 생성자에서 복사하지 않았다.**

```csharp
// 복사해두면 Mdl 을 고쳐도 안 따라온다 → 계산 프로퍼티로
public DateTime ActiveStartTime => Mdl?.ActiveStartTime ?? Prt.ActiveStartTime;
```

`Mdl` 이 null 이면 운영 중 바꾼 적이 없다는 뜻이고 `Prt` 의 일정이 그대로 유효하다. **이 결정은 옳았지만 대가가 있었고, 그 대가가 바로 다음 커밋에서 드러난다**(§S8-B ②).

**잡은 버그: `SchedulePacket.State` 가 항상 0 으로 나가고 있었다.** 옛 `ScheduleManager` 가 `State` 를 **선언만 하고 어디서도 대입하지 않았다.** 뷰는 `Mdl` 에서 읽는다. 겹치기를 손으로 조립하던 코드에서 필드 하나가 조용히 빠져 있던 것으로, **계획-A ⑥ 이 "AutoMapper 매핑도 같이 바뀐다"고만 적고 그 매핑이 무엇을 안 채우는지는 안 봤다.**

**`PlayerMapComponent`(29줄, 실기능 INSERT 하나) → `AuthScope` 의 메서드 하나.** `TryGetPlayerMapAsync` 는 호출부가 0이라 옮기지 않고 지웠다 — S7a 의 `TryGetByAccountIdAsync`, S7b 의 `LogoutAsync` 에 이어 **세 번째**다.

**기간 검증이 서버 시각을 인자로 받는다(§5.10 해소).** 이것이 **데이터 계층이 `RpcContext` 를 읽던 마지막 자리**였다.

**여기서 Auth/Center 계열이 통째로 나갔다.** `AuthRepo` · `CenterRepo` · `AuthComponentBase` · `CenterComponentBase` · `CenterManagerBase`. `GlobalDbRepo` 에는 User 계열만 남았다.

#### S8-B 커밋 후 리뷰 반영 (2026-08-23, 커밋 58e31ea)

**① 가챠가 아닌 스케줄 번호가 가챠 API 로 오면 NRE 로 죽었다.** `ScheduleNum` 은 **클라가 보내고** `EScheduleType` 에 `ATTENDANCE` 가 있으므로 도달 가능한 경로다. 가드를 **`GachaPrt` 프로퍼티 게터 안**에 넣어 `NOT_GACHA_SCHEDULE` 로 막았다. 호출부마다 검사하면 새 호출부가 또 뚫리므로 값을 꺼내는 자리 하나에서 막는다 — §S6-B 가 "라우터가 아니라 모델"로 내린 것과 같은 판단이다. 임시로 ATTENDANCE 스케줄을 넣어 실제로 걸리는 것을 확인했다.

**② `ProtoDb` 조회가 가챠 요청 하나에 6번 나가고 있었다. §S8-A 의 "복사하지 않는다"가 만든 대가다.** 옛 `ScheduleManager` 는 생성자에서 한 번만 했다. **결정을 되돌리지는 않았다** — 복사하면 `Mdl` 변경이 안 따라오는 문제가 돌아온다. 대신 메서드가 자기 안에서 두 번 뒤지지 않도록 지역으로 받았다. **정확성을 지키고 중복만 호출 단위로 줄인 것이지 트레이드오프를 없앤 게 아니다.**

**③ `GetScheduleListAsync` → `GetFilledScheduleListAsync`.** DB 행이 없어도 프로토 값으로 채워 돌려주는 것이라, **행이 있어야 찾은 것으로 치는 `TryGetScheduleAsync` 와 기준이 다르다.** 그 차이를 이름이 말하게 했다.

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

**직후**: 집계 쿼리가 `scope.Raw`라는 이름으로 드러난다. `OwnedSet<T>` 확장 메서드로 감싸지 않으므로 코드 리뷰에서 보인다.

**주의**: dirty 모델에서 변경은 커밋까지 DB에 없다. `Raw`가 실행 전 flush를 하지 않으면 방금 변경한 값이 집계에서 빠진다. **이 규칙을 `Raw` 도입과 동시에 넣는다** — 나중에 붙이면 이미 쓰인 호출부가 조용히 틀린 값을 본다.

---

#### S9-A 실행 결과 (2026-08-23, 커밋 ab4f045)

**위 계획의 핵심인 `scope.Raw` 를 만들지 않았다.** 이 스텝이 존재한 이유가 T3 진입점 확정이었으므로 계획이 통째로 뒤집힌 것이다. 근거는 둘이다.

**첫째, 집계를 메모리로 옮겼다.** 월드당 스테이지가 10개 남짓이고 소유자 리스트는 **이미 캐시에 있다.** DB 로 다시 갈 이유가 없다.

```csharp
// WorldStageQueries.GetTotalStarAsync — T3 가 아니라 T1 이면 충분했다
var list = await set.GetListAsync();
return list.Where(x => x.WorldNum == worldNum).Sum(x => x.Star);
```

**둘째, 계획이 단 주의는 이미 무의미해져 있었다.** "`Raw` 가 실행 전 flush 를 하지 않으면 방금 변경한 값이 집계에서 빠진다"는 dirty 모델 위의 이야기인데, **§S2-H 에서 dirty 를 철회한 순간 flush 할 pending 자체가 없어졌다.** 즉시 저장이라 그냥 보인다. 계획 문구가 **철회된 모델 위에 서 있었다** — §S2-I 의 "S3~S13 은 예정이 아니라 가설"이 또 확인됐다(§S2-B · §S2-J · §S4-B · §S6 · §S7-A · §S7b-A 에 이어서). 따라서 §5.13 표의 S9 행("`scope.Raw` 자동 flush 와 `GameDb.Utility` 무flush 경로 구분 확정")도 앵커가 사라졌다.

**T3 를 안 만든 것이 테스트 가능성을 되찾았다.** 집계 SQL 은 InMemory 모드에서 못 돌아 **이 경로가 테스트에서 통째로 빠져 있던 원인**이기도 했다. 메모리 집계로 바꾸자 `WorldStarTest` 가 HTTP 만으로 InMemory/MySQL 양쪽에서 돈다. §S5-D 가 "캐시·MySQL 경로는 ServerTest 가 못 지나간다"로 적어둔 벽을, 이번엔 **벽을 넘는 대신 벽 이쪽으로 코드를 옮겨서** 지나갔다.

**`WorldRewardStar` 는 지금까지 한 번도 성공한 적이 없었다.** 버그 둘이 서로를 가리고 있었다.

1. 집계 SQL 이 `SUM(RewardAmount)` 를 보는데 `WorldStage` 에 **그런 컬럼이 없다**(실제 컬럼은 `Star`).
2. `WorldNum` 을 **어디서도 대입하지 않아 늘 0** 이었다.

**하나만 고쳤으면 `WHERE WorldNum = @n` 이 한 행도 못 찾아 여전히 0 이 나왔을 것이다.** 결함 둘이 같은 증상(총별 0)으로 수렴할 때, 하나를 고치고 증상이 그대로면 "안 고쳐졌다"로 오판하기 쉽다.

**옛 행의 `WorldNum` 은 읽는 김에 메운다.**

```csharp
// GetOrCreateAsync — 신규만 채우면 기존 유저는 영영 0 이라 별 보상이 안 열린다
if (mdlWorldStage.WorldNum == 0) { mdlWorldStage.WorldNum = worldNum; await set.UpdateAsync(mdlWorldStage); }
```

§S6 의 `Acc*` 는 마이그레이션 없이 **과거분이 틀린 채 남기기로** 했는데(§S6-계획-G), 여기는 **읽으면서 메우는 쪽**을 골랐다. 차이는 영향이다 — `Acc*` 는 표시되는 숫자의 의미가 바뀌는 것이고, `WorldNum` 은 **보상이 영영 안 열리는 것**이다.

**`req.Star` 경계가 하나 넘쳤다.** 0성 보상까지 리스트에 있으므로 유효한 별은 `0 ~ Count-1` 인데 `<= Count` 로 두고 있어 마지막 인덱스를 넘겼다. **클라가 보내는 값이라 도달 가능한 500** 이었다.

**Manager 를 뷰로 만들지 않았다.** 바로 앞 §S8 이 `ScheduleManager` 를 `ScheduleView` 로 바꿨지만, `WorldManager.Prt` 는 **겹치기가 아니라 `ProtoDb.Get` 한 줄**이라 뷰를 만들 실질이 없다. 앞 스텝에서 만든 형태를 기계적으로 복사하지 않았다 — 뷰가 값이 있는 조건은 "Proto 위에 Model 을 덮는다"이지 "Manager 였다"가 아니다.

**`_userRepo` 를 드는 클래스 12 → 8.** 남은 것은 전부 Kingdom(S10)이다.

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

#### S10-A 실행 결과 (2026-08-24, 커밋 c6969fa)

**계획의 `KingdomBuilder` 하나가 둘로 갈렸다.** 계획은 "여러 Model 에 걸친 규칙"이라는 §3.6 판정만 보고 정적 도메인 서비스 하나를 뒀는데, 실제로 갈라야 했던 축은 **계산이냐 IO 냐**였다.

| 어디 | 무엇 | 왜 이 자리인가 |
|---|---|---|
| `Domain/KingdomTileMap.cs` (internal static) | 타일 판정·배치 시뮬레이션 | DB·스코프·컨텍스트를 모르고 스냅샷만 다룬다. **이 저장소에서 DB 없이 검증할 수 있는 유일한 덩어리**가 됐다 |
| `Data/KingdomMapService.cs` | 로드·저장 | 푼 것을 고친 뒤 다시 넣는 **세 단계를 한 메서드 안에 묶는다** |

**묶은 이유가 이 스텝의 핵심 판단이다** — 호출부가 마지막 단계(다시 직렬화해서 저장)를 잊어도 **컴파일이 되고 조용히 저장이 안 된다.** §S7-A 가 "모델이 푼 스냅샷을 들고 있지 않기로 한" 이유로 적은 바로 그 위험을, 이번에는 **API 모양으로** 막았다.

`SimulatePlaceItems` 는 **복제 맵에서 보관·이동·배치를 한꺼번에 돌려보고 전부 유효할 때만** 그 스냅샷을 돌려준다. 원본을 안 건드리므로 중간에 던져도 상태가 반쯤 바뀌지 않는다. `out` 대신 튜플로 돌려주는데, `out` 을 쓰면 `async` 를 못 써서 동기 블로킹이 되기 때문이다.

**계획대로 간 것**: 스냅샷(`KingdomMapSnapshotPacket`)은 `ObjKey` 로 주소 지정이 불가능하므로 `ChangeSet` 대상이 아니다. 자기 응답 패킷으로만 간다.

**기존 결함 6건을 잡았다. 전부 이관이 만든 게 아니라 원래 있던 것이다.**

1. **건설 완료가 뒤집혀 있었다.** `SetReady` 의 부등호가 반대라 **진행 중일 때 통과하고 시간이 지나면 오히려 막았다.** 건설 시간을 기다릴 이유가 없었고, 기다리면 완료가 안 되는 상태였다. **기존 테스트 둘이 이 동작을 `Assert.Equal(OK)` 로 굳혀두고 있어 같이 뒤집었다** — 테스트가 버그를 보존하고 있던 사례다. 완료 성공 경로는 API 로는 10초를 기다려야 해서 **모델을 직접 부르는 테스트**로 경계를 덮었다(위 `KingdomTileMap` 분리가 처음 값을 낸 자리).
2. **데코 수량 검증이 음수에서 무력했다.** `UnplacedCnt >= cnt` 인데 배치는 `cnt` 가 음수로 들어와 **늘 참**이었다. §S6-B 3회차의 음수 금액과 **같은 모양의 결함이 다른 엔티티에서 또 나왔다** — 부호 가정을 검사 없이 믿는 패턴은 한 번 고쳤다고 사라지지 않는다.
3. **배치가 시작 칸 하나만 마킹하고 저장됐다.** 나머지 칸이 빈 칸으로 남아 다음 요청이 겹쳐 놓을 수 있었다. TRASH 를 뺀 **모든 아이템이 3x3 또는 2x2 라 전부 해당된다.** **이미 저장된 스냅샷은 시작 칸만 찍혀 있으므로 운영 데이터가 있다면 재생성이 필요하다** — 이번 스텝에서 데이터 영향이 남는 유일한 항목이다.
4. **단건 배치가 타입을 `STRUCTURE` 로 박아 넣었다.** 데코를 놓아도 구조물로 기록되고 나중에 보관이 `NOT_EQUAL_KINGDOM_ITEM_LIST` 로 막혔다. 고친 자리가 판단이다 — 값을 검사하는 대신 **`MakePlacedObj` 에서 매개변수를 없애고 프로토에서 직접 읽게 해 틀린 값을 넘길 방법 자체를 없앴다.**
5. **`ValidEmptyTile` 만 `TileMap` 을 `[x][y]` 로 읽어 전치된 칸을 검사했다.** 실제 위치가 차 있어도 **거울 위치가 비어 있으면 배치가 통과**했다. `TileMap` 이 `[y][x]` 라는 규칙을 `KingdomTileMap` 주석에 못 박았다.
6. **`GetTilePosRanges` 의 루프 경계가 `y < sizeX`, `x < sizeY` 로 뒤바뀌어 있었다.** 지금 아이템이 전부 정사각이라 드러나지 않았을 뿐이고, **비정사각이 하나라도 들어오면 바로 터진다.**

**RPC 등록을 둘로 갈라 판단했다.**

- **`kingdom/change-item` 을 등록했다.** 서비스 메서드와 패킷이 다 있는데 라우트만 없어 도달 불가였고, **위 4번·5번 버그가 사는 경로라 붙이지 않으면 검증할 수가 없었다.**
- **`KingdomStructureDecTime` 은 일부러 등록하지 않았다.** 남은 시간과 캐시 보유량 검증이 TODO 로 남아 있다 — §S6-C 가 "등록 전에 그 검증부터 필요하다"고 적은 그대로다.

**그 결과 §S6-C 의 "등록되지 않은 엔드포인트 3개"와 §S7-계획-F 의 "`ChangeNameFirstAsync` 가 유일 호출부이고 그것이 미등록이다"는 낡았다.** `ChangeNameFirst` 는 S7a(7094407)가, `KingdomItemChange` 는 S10 이 등록했다. **미등록으로 남은 것은 `KingdomStructureDecTime` 하나다.** 특히 §S7-계획-F 는 **자기 커밋(7094407)이 등록한 것을 미등록이라고 적었다** — 계획 문구가 같은 커밋 안에서 이미 낡은 경우다. 이 낡음은 S11 에 직접 영향이 있다: `AllUserRepo` 는 **도달 불가 코드가 아니라 살아 있는 경로**이므로 삭제가 아니라 이관 대상이다.

**검증**: 회귀 테스트 둘은 **수정을 되돌려 실제로 실패하는지 확인했다.** §S6-B 가 "404 를 잡는 테스트는 아무것도 안 지킨다"로 테스트를 되돌린 뒤라, 이번엔 테스트가 실제로 무엇을 잡는지를 먼저 확인한 것이다.

**철거**: `UserComponentBase` · `UserManagerBase` · `UserRepo` · `GlobalDbRepo.OwnUser` · `IRpcAuthPolicy.RequiresUserRepo` 가 같이 나갔다. **`_userRepo` 8 → 0. `DbModel/Component` 와 `DbModel/Manager` 가 비었다.**

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
// S12 : StartUp.Resource.cs 최종 형태 — S1 에서 이미 이 한 줄이 들어가 있다
EntityRegistry.ScanAndRegister(typeof(PlayerModel).Assembly);
// 여기서 지우는 것은 손 등록 목록뿐이다 (Server 19줄 / RaidServer 2줄).
// ModelRegistration.Init 의 PK_REGISTRATION_CONFLICT 검사는 남긴다 — 손목록이
// 사라진 뒤에도 중복 등록 방어로 유효하다 (§S1-D)
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

### S11 — 철거 · 착수 전 census (2026-08-24)

#### S11-계획-A census 가 정정한 것

**① 위 "S11 삭제 대상" 4줄 중 3줄이 이미 무효다.**

| 계획이 적은 것 | 실제 |
|---|---|
| `DbModel/Base/*` (8) | **2개만 남았다** — `ManagerBase.cs`, `RepoBase.cs`. ComponentBase 3종·`AuthManagerBase`·`CenterManagerBase`·`UserManagerBase` 는 S7b·S8·S10 에서 나갔다 |
| `DbModel/Repo/` GlobalDbRepo, AuthRepo, CenterRepo, UserRepo | `AuthRepo`·`CenterRepo`·`UserRepo` 는 **이미 없다**. `GlobalDbRepo`·`AllUserRepo` 만 남았다 |
| `DbModel/Component/` (18) · `DbModel/Manager/` (17) | **빈 디렉터리**다. 파일 0 |

그리고 남은 `ManagerBase`/`RepoBase` 는 **서로만 참조하는 완전 사문**이다(참조 0). **S11 은 "18+17+8 개를 지우는 큰 스텝"이 아니라 파일 2개와 클래스 1개를 지우는 작은 스텝이다.** S10 이 Component/Manager 를 같이 걷어냈기 때문이다.

**② `AllUserRepo` 는 도달 불가 코드가 아니다.** §S10-A 에 적은 그대로다 — §S7-계획-F 의 "`ChangeNameFirstAsync` 가 유일 호출부이고 그것이 미등록이다"를 믿고 지우면 **살아 있는 기능이 사라진다.** 라우트는 S7a(7094407)가 등록했다.

**③ `GlobalDbRepo` 는 이미 껍데기다.** 실체는 `AllUser` 와 `Commit`/`Rollback`/`Close` 뿐이고, 나머지는 죽어 있다.

```
CreateRepository(95)   호출부 0 — 사문. GameDb.GetOrCreateRepository 가 같은 일을 한다
_rpcContext(123)       생성자에서 대입만 하고 안 쓴다
```

**④ `GlobalDbRepo` 를 주입받고 안 쓰는 서비스가 둘이다.** `AuthService`·`GachaService` 는 생성자 대입만 있고 사용 0이다. 실사용은 `GameService:75` 한 곳뿐이다. §S6-B 1회차의 "책임을 옮기면 그 책임 때문에 들고 있던 의존성도 같이 죽는지 매번 확인할 것" 이 여기서 또 확인된다.

**⑤ `IRpcMethod.RunAsync` 의 `dbRepo` 인자는 아무도 안 쓴다.** `RpcMethod.RunAsync`(42)가 받기만 하고 본문에서 안 쓴다 — 서비스는 DI 로 받는다. 이관 다리도 아니고 그냥 잔재다.

**⑥ `Close()` 는 사문이 아니다. 지우면 커넥션이 샌다.** 계획에 "커밋 주체 역전" 한 줄만 있어 `Close` 를 안 봤는데, **커밋도 롤백도 안 타는 경로가 실재한다.**

```
OnHttpBodyRequestAsync
  RpcContext.InitAsync → LoadSessionAsync → Auth 커넥션 오픈 (+ 세션 연장 쓰기)
  ParseRequestAsync    → 415/400 이면 여기서 return       ← 커밋도 롤백도 없다
  응답 캐시 히트         → HandleMethodAsync 를 통째로 건너뜀 ← 커밋도 롤백도 없다
```

지금은 `GlobalDbRepo : IDisposable` 이 DI 종료 시 `Close()` 를 불러 이 두 구멍을 메운다. **`DbSessionManager` 는 `IDisposable` 이 아니다.** 그래서 "`GlobalDbRepo` 를 지운다"는 **자동으로 "`DbSessionManager` 가 자기 세션을 닫게 만든다"를 포함한다.** 안 하면 두 경로에서 커넥션이 안 닫힌다.

**⑦ ⑥ 을 조사하다 별건을 찾았다 — 응답 캐시 히트면 세션 연장 쓰기가 유실된다.** `RpcContext.ExtendSessionAsync` 가 `db.Sessions.SaveAsync` 로 **쓰는데**, 캐시 히트 경로는 `HandleMethodAsync` 를 건너뛰므로 커밋이 없고 `Close()` → `_transaction.Dispose()` → **MySQL 이 롤백**한다. S11 이 만든 게 아니라 기존 동작이고, 영향은 "재전송 요청이 세션을 연장하지 못한다"다. **커밋 주인을 바꾸는 스텝이 이걸 만나는 것은 우연이 아니다** — 커밋 경계가 어디인지 다시 그리는 순간이라 지금이 판단할 자리다(Q4).

**⑧ lazy BEGIN 은 이제 "판단"이 아니라 실측이 끝났다.** §S2-A 항목 8 이 "S2 코드는 아무도 안 써서 실증 불가"로 S11 에 미뤘던 것이다.

```csharp
// MySqlDbSessionFactory.Create → DBSqlExecutor.StartTransaction → Open() : 커넥션 + BEGIN 을 즉시
// OwnedSet.GetListAsync : _repository() 를 캐시 조회보다 먼저 부른다
return _repository().GetListAsync<T>(_listKey, LoadFromDbAsync);
```

`SqlRepository.GetListAsync` 는 캐시를 먼저 보지만, **그 `SqlRepository` 를 얻는 것 자체가 커넥션 오픈 + BEGIN 이다.** 즉 **캐시 히트만으로 끝나는 요청도 MySQL 트랜잭션을 연다.** §5.11 이 "커넥션 오픈 시점이 이르다"고 적은 것의 진짜 자리가 여기다 — S2 가 고친 것은 **스코프 생성** 시점이었고, 남은 것은 **캐시 히트** 경로다.

**⑨ `AllUserRepo` 는 `DbConnectionResolver` 를 안 거친다.** `Config<CoreConfig>.Get().UserDbConnectionStrList` 를 직접 읽는다 — §S2-A 항목 10 이 "`DbConnectionResolver` 추출 — 샤드 맵 2벌 방지"로 만든 규칙에서 **이 하나만 빠져 있다.**

그 결과가 ⑩ 이다.

**⑩ InMemory 에서는 이름 중복 체크가 항상 통과한다.** `ServerTest/appsettings.yaml` 의 `UserDb.ConnectionStrList` 가 `[]` 이므로 `BeginAllUserRepo` 가 **세션을 0개** 만들고, `TryGetPlayerByNameAsync` 는 늘 `(false, null)` 을 돌려준다. **`ServerTest` 로는 이 가드를 검증할 수 없다.** 이관하면서 `DbConnectionResolver` 를 거치게 하면 InMemory 가 `__inmemory__` 단일 키로 풀려 **검증 가능해진다** — §S9-A 의 "T3 를 안 만든 것이 테스트 가능성을 되찾았다"와 같은 종류의 이득이다.

#### S11-계획-B 범위 — 9건 중 설계 결정은 2건

| # | 무엇 | 성격 |
|---|---|---|
| 1 | `Base/ManagerBase.cs` · `Base/RepoBase.cs` 삭제 | 기계적 (참조 0) |
| 2 | 빈 디렉터리 `Component/` · `Manager/` 정리 | 기계적 |
| 3 | `AuthService` · `GachaService` 의 죽은 `GlobalDbRepo` 주입 제거 | 기계적 (④) |
| 4 | `IRpcMethod.RunAsync` 에서 `dbRepo` 인자 제거 (인터페이스·구현·호출부 3곳) | 기계적 (⑤) |
| 5 | `AllUserRepo` → `GameDb.AllShards`, `DbConnectionResolver` 경유 | **설계 결정 (Q2)** |
| 6 | 커밋 주체 역전 — `RpcService:94/98` 한 곳 | **설계 결정 (Q1)** |
| 7 | `DbSessionManager` 가 자기 세션을 닫게 (`IDisposable`) | 필수 (⑥) |
| 8 | RaidServer `PlayerRaidSessionService:41` 의 `using var dbRepo` 대체 | 7의 결과 |
| 9 | `GlobalDbRepo` 삭제 + DI 등록 2곳 제거 | 위 전부의 결과 |

**8에 대한 판단**: 그 `using` 의 실제 목적은 스코프 종료 시 세션 정리이므로 7이 끝나면 `using var scope` 가 대신한다. `catch` 의 `RollbackAsync()` 는 **읽기 전용 경로라 지금은 의미가 없지만 남긴다** — 이 경로에 나중에 쓰기가 붙었을 때 조용히 커밋 없이 새는 것보다 낫다.

#### S11-계획-C 열린 결정

> **답 (2026-08-24, 사용자 확정): Q1 `GameDb` · Q2 위 형태대로 · Q3 별도 슬라이스 · Q4 범위 밖(현행 문제없음).**

**Q1. 커밋의 새 주인은 `GameDb` 인가 `DbSessionManager` 인가.** `CommitAsync` 의 본체는 "DB 커밋 → 캐시 flush, flush 실패 시 pending 폐기"인데 **캐시 flush 순서가 이 메서드의 존재 이유**다(§5.7 — 응답 캐시가 커밋보다 먼저 쓰여 pending 에 쌓였다가 같이 반영된다). `DbSessionManager` 는 캐시를 모르고 `GameDb` 는 이미 `ICacheSession` 을 든다. **`GameDb` 쪽으로 기운다.** 다만 그러면 `GameDb` 가 "데이터 접근 진입점 + 트랜잭션 주인" 둘을 겸한다.

**Q2. `AllShards` 의 자리와 지연 오픈 형태.** `GameDb.AllShards.FindPlayerByNameAsync(name)`(§5.5)로 두되, ⑨ 때문에 `DbConnectionResolver` 에 전 샤드 커넥션을 돌려주는 접근자를 하나 추가해야 한다. 지연 오픈은 **샤드를 순회하며 그때그때 여는** 형태가 자연스럽다 — 지금은 생성 시 전 샤드를 한꺼번에 열지만 대부분 첫 샤드에서 끝난다.

**Q3. lazy BEGIN(⑧)을 S11 에서 구현하는가.** 실측이 끝났으므로 할 수 있다. 형태는 `DbSessionManager.Open` 이 **첫 `ExecuteAsync` 까지 `StartTransaction` 을 미루는 래퍼**를 돌려주고 `Commit`/`Rollback`/`Close` 는 만들어진 적 없으면 no-op 하는 것이다. **다만 S11 은 철거 스텝이고 이것은 추가다.** 별도 슬라이스로 떼는 쪽을 권한다.

**Q4. ⑦(캐시 히트 시 세션 연장 유실)을 S11 에서 고치는가.** 기존 동작이고 영향이 작지만, 커밋 경계를 다시 그리는 스텝이라 지금 만났다. 고친다면 "세션 연장은 응답 캐시 히트 여부와 무관하게 커밋된다"를 어디서 보장할지가 같이 정해져야 한다.

---

#### S11-A 실행 결과 (2026-08-24)

**계획-B 9건을 그대로 실행했다. 범위가 바뀐 곳은 없고, 계획이 못 본 것이 둘 나왔다.**

**책임이 둘로 갈렸다 — 이것이 Q1 의 실제 결론이다.** "커밋 주인"을 `GameDb` 로 정하자 **세션을 닫는 일은 따라오지 않았다.**

| | 누가 | 왜 |
|---|---|---|
| 커밋 / 롤백 | `GameDb` | 캐시 flush 순서를 아는 쪽이다. `DbSessionManager` 는 캐시를 모른다 |
| 세션 닫기 | `DbSessionManager` (`IDisposable`) | 세션을 **소유한** 쪽이다. 커밋도 롤백도 안 타는 경로(계획-A ⑥)를 DI 가 메운다 |

옛 `GlobalDbRepo` 는 이 둘을 한 클래스에 갖고 있었고, **남의 정리를 대신 불러주는 껍데기**가 그래서 필요했다. 갈라놓으면 껍데기가 사라진다. `GameDb` 는 `IDisposable` 이 아니다 — 닫을 것을 갖고 있지 않기 때문이다.

**`Find*` 를 쓰지 않았다 — §5.5 와 §S2-F 가 충돌했다.** §5.5 는 `GameDb.AllShards.FindPlayerByNameAsync(name)` 라고 적었는데 §S2-F 의 네이밍 규칙은 **"`Find*` 는 쓰지 않는다(`TryGet*` 과 중복)"** 다. §5.5 가 §S2-F 보다 먼저 쓰였다. 규칙을 따라 `TryGetPlayerByNameAsync` 로 갔다 — 반환이 `(bool Found, T Value)` 라 `TryGet` 이 맞기도 하다.

**계획이 못 본 것 ① — 죽은 `using` 3개는 census 에 안 잡혔다.** census 는 `GlobalDbRepo`·`AllUserRepo` 같은 **타입 이름**으로 훑었는데, 지워진 것은 네임스페이스이기도 했다. `RpcContext.cs`·`AuthService.cs`·`CommonService.cs` 가 `using Server.Repo` / `using WebStudyServer.Repo` / `using WebStudyServer.Base` 를 들고 있어 **빌드가 CS0234 5개로 막혔다.** 타입 참조 0이어도 네임스페이스 참조는 남는다 — **다음 철거 스텝의 census 는 `using` 도 같이 센다.**

**계획이 못 본 것 ② — `GameDb` 가 로거를 안 들고 있었다.** 커밋을 옮기면 커밋 실패 로그가 따라온다. `ILogger<GameDb>` 를 생성자에 추가했다. 로그 문구는 옮기기만 하고 손대지 않았다 — 이동 커밋에 문구 변경을 섞지 않는다.

**계획-A ⑩ 이 약속한 이득을 실제로 받았다.** `AllUserRepo` 가 `Config` 를 직접 읽던 것을 `DbConnectionResolver.AllUsers()` 로 돌리자 InMemory 가 `__inmemory__` 단일 키로 풀려 **이름 중복 가드가 처음으로 테스트에서 돈다.** `ChangeNameTest` 를 남겼다.

**그 테스트가 실제로 무엇을 잡는지 확인했다.** `AllUsers()` 를 S11 이전 동작(`return connList;` — InMemory 에서 빈 목록)으로 되돌리자 중복 단정이 **실패**했다. §S10-A 가 "회귀 테스트 둘은 수정을 되돌려 실제로 실패하는지 확인했다"로 세운 절차를 그대로 따랐다 — §S6-B 의 "404 를 잡는 테스트는 아무것도 안 지킨다"가 그 절차의 이유다.

**지연 오픈도 같이 왔다.** 옛 `BeginAllUserRepo` 는 `UserDbConnectionStrList.Select(Open).ToList()` 로 **전 샤드를 한꺼번에** 열었다. 새 `AllShards` 는 순회하며 그때그때 연다 — 대부분 첫 샤드에서 끝난다. §5.5 가 "지연 오픈으로 개선할 기회다"로 적어둔 자리다.

**RaidServer 의 `RollbackAsync` 는 남겼다.** 읽기 전용 경로라 지금은 의미가 없지만, 이 경로에 나중에 쓰기가 붙었을 때 조용히 커밋 없이 새는 것보다 낫다(계획-B).

**검증**: 리빌드 **0에러 / 경고 11**(전부 기존 경고이고 손댄 파일에는 없다) · `ServerTest` InMemory **30/30**(S10 시점 23 + `ChangeNameTest` + 리뷰가 붙인 `DbSessionManagerTest` 3 · `DbSqlExecutorTest` 3) · 되돌리기 확인 3건 · **MySQL+Redis+유저락 30/30**. 실측이 확인한 것: `AllShards` 가 실제 샤드 커넥션을 타고 이름 중복을 잡았고(`Player_*` 행 저장 확인), 약 250건의 실요청 뒤 **Threads_connected 1 · 열린 InnoDB 트랜잭션 0** 으로 누수가 없었다.

**남은 것 — S12 가 두 개 다 풀렸다.**

- **`IGameContext` 의 데이터 계층 소비자가 0이 됐다.** `GlobalDbRepo` 의 안 쓰이던 `_rpcContext` 필드가 마지막이었다. 지금은 `RpcContext`/`RaidGameContext` 가 구현만 하고 **주입받아 쓰는 곳이 하나도 없다** — 두 호스트의 `services.AddScoped<IGameContext>(...)` 등록도 같이 죽었다. S12 의 "Transport 전용으로 축소"는 **축소가 아니라 폐지 판단**이 됐다.
- `DbModel` 에서 `Component`/`Manager`/`Base`/`Repo` 디렉터리가 전부 사라졌다. 남은 것은 `Data`/`Domain`/`Model`/`Extension`/`Helper`/`GAME` 이다.

#### S11-B 안 고친 것

- **Q4(응답 캐시 히트 시 세션 연장 유실)** — 범위 밖으로 확정했다. 사용자 판단은 "지금도 문제없음"이다. 계획-A ⑦ 에 현상과 원인이 남아 있다.
- **Q3(lazy BEGIN)** — 별도 슬라이스. 계획-A ⑧ 에 실측과 고칠 형태가 남아 있다. **철거 스텝에 추가를 섞지 않는다.**
- **`ChangeNameFirstAsync` 의 상태 가드가 무력하다.** `ChangeNameTest` 를 쓰다 발견했다 — `ValidState(CHANGED_NAME_FIRST)` 는 `State <= CHANGED_NAME_FIRST` 인데 **`State` 를 `CHANGED_NAME_FIRST` 로 올리는 코드가 어디에도 없다**(`GameService:137` 이 `PREPARED` 로 올리는 것이 전부). 즉 이름은 몇 번이든 바꿀 수 있다. 기획 판단이 필요한 자리라 S11 에서 손대지 않았다.

---

#### S11-C 커밋 전 자율 리뷰 3회 (2026-08-24)

회차별로 축을 다르게 잡았다(§S6-B 와 같은 방식). **실제 결함은 이번에도 3회차에서만 나왔다.**

| 회차 | 축 | 결과 |
|---|---|---|
| 1 | 정합성 / 회귀 — 옮긴 본문이 옛것과 같은가, 호출부가 다 옮겨졌는가 | 결함 없음. 대신 **편집 도구가 만든 변경 1건** |
| 2 | 설계 / 일관성 — 책임 분리가 실제로 갈렸는가, 진입 경로가 두 벌이 아닌가 | 결함 없음 |
| 3 | 경계 / 실패 경로 | **결함 1건 — 커밋이 실패하면 커넥션이 샌다** |

**3회차 결함: 커밋이 도중에 실패하면 커넥션이 새고, S11 이 방금 세운 안전망이 정확히 그때 무력하다.**

두 겹이었다.

```csharp
// ① DBSqlExecutor — CloseInternal 이 트랜잭션 연산 "뒤에" 있다. 던지면 도달하지 않는다
public void Commit()
{
    _transaction?.Commit();   // 여기서 던지면
    CloseInternal();          // 여기에 안 온다 → 실패한 트랜잭션이 커넥션을 잡은 채 남는다
}

// ② DbSessionManager — 실패해도 목록을 비운다
finally { _openSession.Clear(); }   // 차례가 오지 않은 세션까지 잊힌다
```

**②가 S11 의 계약을 깬다.** 이 스텝은 "커밋도 롤백도 안 타는 경로는 `Dispose` 가 메운다"를 설계로 선언했는데(§S11-계획-A ⑥), **`Clear()` 가 먼저 돌아 `Dispose` 가 그 세션을 찾지 못한다.** 세션이 하나뿐인 요청도 ①만으로 샜다.

**기존 코드다.** `GlobalDbRepo` 시절에도 같았고 S11 이 만든 게 아니다. 그러나 **그 시절에는 안전망을 약속한 적이 없다.** 약속을 새로 한 스텝이 그 약속이 거짓이 되는 자리를 같이 닫는다.

**고친 자리를 관리자가 아니라 원천으로 정한 것이 이번 판단의 핵심이다.**

- `DBSqlExecutor.Commit`/`Rollback` 을 `try`/`finally` 로 — **원천이다.** 세션 하나짜리 요청(대부분)은 이것만으로 닫힌다. `DbSessionManager` 에서만 고치면 `DBSqlExecutor` 를 직접 쓰는 다른 경로(`StartUp.ConnectionTest`)가 그대로 남는다. §S6-B 가 "라우터가 아니라 모델"로 내린 것과 같은 결이다.
- `DbSessionManager.Commit`/`Rollback` 의 `finally` 를 `Close()` 로 — 차례가 오지 않은 세션까지 닫는다.
- `CloseInternal` 에서 `_transaction = null` — 두 번 닫히는 경로가 생겼으므로 재진입 안전을 코드로 박는다.

**회귀 테스트는 DB 없이 돈다.** `DbSessionManagerTest` 3개. 가짜 `IDbSessionFactory` 로 "두 번째 세션이 커밋에서 던진다"를 만들고 셋 다 닫혔는지 본다. **고치기 전 2 실패 / 1 통과**였고, 통과한 하나가 `Dispose_ClosesOpenSessions` 다 — **S11 의 새 안전망은 정상 경로에서는 실제로 동작하고 실패 경로에서만 뚫려 있었다.** §S10-A 의 `KingdomTileMap` 에 이어 **DB 없이 검증되는 자리가 하나 늘었다.**

**1회차가 잡은 것 — `DbConnectionResolver.cs` 에 BOM 이 붙어 있었다.** 내용과 무관하고, 편집 스크립트가 그 파일만 `utf-8-sig` 로 쓴 탓이다. 되돌렸다. **편집 도구가 만든 diff 는 빌드도 테스트도 안 잡는다 — 리뷰에서만 잡힌다.**

**4회차 — 고치면서 만든 것을 되짚었고, 심각도를 스스로 낮췄다.** "문제없나"는 물음에 다시 보니 **위 수정이 성공 경로에 double-close 를 만들었다.** `Commit()` 이 커넥션을 닫고 `finally` 의 `Close()` 가 한 번 더 부른다 — 즉 **이미 `Dispose` 된 `MySqlConnection` 에 `Close`/`Dispose` 를 다시 부르는 경로가 커밋되는 모든 요청에 생겼다.** 처음에는 "실제로 위험하다"고 말했다가 **정정했다**: 열지 않은 커넥션으로 double-close 를 태워 보니 드라이버가 던지지 않았다(`DbSqlExecutorTest` 3개, 가드를 빼고도 통과). **버그가 아니라 드라이버 동작에 기대고 있던 것이다.**

그래도 `_closed` 플래그로 막았다. 근거는 두 가지다 — ① 커밋되는 **모든** 요청이 지나는 자리를 드라이버 구현에 기대게 두지 않는다 ② **테스트가 덮은 것은 "열지 않은 커넥션"뿐이고**, 실제로 열려서 풀에 반납된 커넥션을 두 번 닫는 경우는 MySQL 없이 확인할 수 없다. §S6-B 가 "조사 중에 심각도를 스스로 낮췄다"로 남긴 것과 같은 자리다 — **다만 이번엔 내가 만든 위험이었다.**

**안 고친 것**: `DbSessionManager.Close()` 안에서 한 세션의 `Close()` 가 던지면 뒤 세션이 안 닫힌다. 세션별로 삼키려면 로그가 필요한데 이 클래스는 로거를 안 든다. 원천을 고친 뒤로는 `CloseInternal` 이 멱등이라 던질 여지가 거의 없어 남겨둔다.

---

### S11.5 — lazy BEGIN (2026-08-24, 실행 완료)

#### S11.5-A census 가 §S11-계획-A ⑧ 을 정정했다

⑧ 은 "캐시 히트만으로 끝나는 요청도 MySQL 트랜잭션을 연다"고 뭉뚱그렸는데, **경로마다 다르다.**

| 경로 | 캐시 히트일 때 커넥션을 여나 |
|---|---|
| `OwnedSet` | **연다.** `return _repository().GetListAsync(...)` — `_repository()` 가 캐시 조회보다 먼저 평가된다 |
| `SessionStore` · `Identity` · `AuthScope` · `CenterScope` | **안 연다.** `Db` 가 계산 프로퍼티(`=> _db.AuthRepository().Db`)라 쓰이는 자리에서만 평가된다 |

**그래서 §S11 이 "세션 로드 때문에 매 요청 Auth 커넥션이 열린다"고 본 것은 틀렸다.** 세션이 캐시에 맞으면 그 전에도 Auth 는 안 열렸다. 실제로 남아 있던 자리는 `OwnedSet` 하나다.

#### S11.5-B 형태 — 세션 생성 지점이 하나라서 한 곳만 고쳤다

`IDbSessionFactory.Create` 를 부르는 곳이 저장소에 **`DbSessionManager.Open` 하나뿐**이라, 거기서 `LazyDbSession` 을 돌려주면 모든 경로가 덮인다.

```csharp
// DbSessionManager.Open
session = new LazyDbSession(() => _sessionFactory.Create(connectionString));

// LazyDbSession — 만들어진 적이 없으면 정리할 것도 없다
public void Commit() => _session?.Commit();
```

**`DBSqlExecutor` 를 lazy 로 만들지 않은 이유**: `StartUp.ConnectionTest` 가 그것을 직접 써서 **부팅 시 접속 확인**을 한다. 거기를 lazy 로 만들면 접속 확인이 아무것도 확인하지 않게 된다.

**동시성**: 요청 스코프 안에서 순차로만 쓰인다. `Task.WhenAll`/`Parallel` 로 DB 를 동시에 치는 곳이 없음을 확인하고 생성에 락을 두지 않았다.

#### S11.5-C 실측 — BEGIN 249 → 141

같은 테스트 묶음을 MySQL+Redis 로 돌리고 `SHOW GLOBAL STATUS` 델타를 비교했다.

| | 전 | 후 |
|---|---|---|
| `Com_begin` | 249 | **141** (−43%) |
| `Com_commit` | 187 | 131 |
| `Com_rollback` | 62 | **10** |

**롤백이 62 → 10 으로 준 것이 이 변경의 성격을 가장 잘 보여준다.** DB 를 건드리기 전에 검증에서 걸리는 요청이 **빈 트랜잭션을 열었다가 롤백하고 있었다.** 지금은 열지 않으므로 되돌릴 것도 없다.

쓰는 요청은 여전히 BEGIN 을 열지만 **여는 시점이 첫 리포지토리 접근에서 첫 실제 쿼리로 늦춰진다** — 트랜잭션이 열려 있는 구간이 그만큼 짧아진다.

누수 확인: 실행 뒤 `Threads_connected` 1 · 열린 InnoDB 트랜잭션 0.

#### S11.5-D 테스트가 깨진 것이 맞다

`DbSessionManagerTest` 3개가 깨졌다 — **실체화된 적 없는 세션은 닫을 것이 없다**는 새 동작이 옳으므로, 테스트에 실체화 단계를 넣어 고쳤다. 그리고 lazy 자체를 보는 둘을 새로 넣었다.

- `Open_WithoutQuery_CreatesNothing` — 열기만 하고 커밋해도 팩토리가 한 번도 안 불린다
- `Query_CreatesSessionOnce` — 두 번 조회해도 한 번만 만든다

#### S11.5-E 안 한 것

**`DbSessionManager.Open` 개명.** lazy 가 된 뒤로 이 이름은 **실제로 열지 않으므로** 더 어긋난다. §S2-F 의 동사표에 이미 `GetOrCreate` 가 있고 호출부는 둘뿐이다. 사용자 확인 대기 중.

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
        var account = await _db.Identity.CreateAccountAsync();                 // 신 경로
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
| **S0-1** | 저장 모델 | ⛔ **번복 — dirty 철회, 즉시 쓰기 유지** (§S2-H). 한때 "(c) dirty 플래그 + 커밋 시 flush"로 확정돼 있었다 | S5 에서 재판단 |
| **S0-2** | `ClassGenerator`가 `[Entity]`를 찍을 수 있는가 | **미확인** | S1 (불가하면 모델 20개 수작업 — 작업량만 영향) |
| **S0-3** | 감사·반환 타입 | **확정 — `ChangeSet` 존치(근거: 와이어 계약 분리), 감사는 싱크별 개별 조립** | S5 도메인 메서드 반환형, S13 |
| **S0-4** | 커밋 경계를 유저 락 안으로 | **완료 — 선행 커밋 2개로 반영** | 리뷰 5.1, 5.2 |

**남은 미확인은 S0-2 하나뿐이며 작업량에만 영향을 준다 — 실행을 막지 않는다.**

**S0-2 확인 결과(2026-08-11) — 가능하다.** `Template/ModelTemplate.txt`에 `{{ClassAttribute}}` 슬롯이 이미 있고 `ModelGenerator.cs:225`가 모델에는 빈 문자열을 넣고 있다(패킷은 같은 슬롯에 `[ProtoContract]`). PK도 이미 스펙에 있다 — `ModelGenerator.cs:319`의 `x.KeyList.Contains("pk")`, 인덱스는 `c_index`/`index`(334·393행). 따라서 `[Entity(Pk)]`와 `[SecondaryIndex]`는 자동 생성이 가능하다.
다만 `ScopeKey`는 엑셀 스펙에 대응 컬럼이 없다. 모델 20개는 전부 `*.generated.cs` 단독이고 수기 partial이 하나도 없으므로, **A안이 어차피 도메인 partial을 새로 만든다면 `[Entity]`는 수기 partial 쪽에 두는 것**이 스펙 포맷을 건드리지 않아 낫다. S1 착수 시 확정한다.

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

### 5.4 🟠 설계 누락 — 캐시 정책이 실제로는 5종인데 `OwnedSet<T>`는 1종만 전제한다

| 현재 위치 | 정책 |
|---|---|
| `UserComponentBase` | 소유자별 **리스트** 캐시 |
| `AuthComponentBase.GetMdlAsync` | **캐시 없음** (DB 직행) |
| `AuthComponentBase.GetMdlWithCacheAsync` | **단건** 캐시 + **sliding TTL** |
| `CenterComponentBase` | **캐시 없음** |
| `SessionComponent` | 단건 + **포인터** |

§3.3의 `OwnedSet<T>`는 리스트 캐시 하나만 전제한다. 5.3.2를 "메타데이터로 통일"한다고 했지만 **어떤 정책들이 존재해야 하는지 열거가 없다.**

**최초 수정안**: `[Entity]`에 캐시 정책을 명시적으로 열거한다.

```csharp
public enum ECachePolicy { None, Single, OwnerList, GlobalList }

[Entity(Pk = ["PlayerId","Num"], ScopeKey = "PlayerId", Cache = ECachePolicy.OwnerList)]
[Entity(Pk = ["Num"],                                   Cache = ECachePolicy.GlobalList)]
[Entity(Pk = ["Id"],                                    Cache = ECachePolicy.None)]
[Entity(Pk = ["AccountId"], Cache = ECachePolicy.Single, SlidingTtl = true)]
```

**sliding TTL이 설계에 아예 없었다** — `GetMdlWithCacheAsync(key, fetch, slidingTtl)`는 캐시 히트 시 TTL을 갱신하며, 세션 유지에 필수다.

**결론 정정 (2026-08-12)** — 원래 "S1의 attribute 설계에 `Cache`/`SlidingTtl`을 포함한다. 나중에 추가하면 20개 모델을 두 번 손댄다"였는데, **이 논거를 철회한다.** attribute 20줄에 필드 하나를 나중에 붙이는 것은 기계적 편집이다. 반면 위 4종 열거가 실제 구현에서 깔끔하게 일반화되지 않으면 **틀린 enum이 20개 모델에 박힌 채로 시작**하게 되고, 그 되돌리기 비용이 훨씬 크다. 특히 5.3에서 확인된 `Session`의 포인터 캐시는 `Single`+`SlidingTtl` 두 플래그로 표현되지 않는 **다섯 번째 형태**이고, 이건 이 enum이 아직 닫히지 않았다는 증거다.

→ **S1에는 넣지 않는다. `Cache`/`SlidingTtl`은 `[Entity]` 주석에 TODO로만 남긴다.**
→ **이 항목에서 살아남는 것은 발견이지 해법이 아니다**: "정책이 실제로 5종인데 `OwnedSet<T>`는 1종만 전제한다"와 "sliding TTL이 설계에 없다"는 **S2에서 `OwnedSet<T>`를 설계할 때 반드시 만족해야 할 제약**으로 이월한다. 형태가 코드로 확정된 뒤에 attribute로 올린다(R7).

#### 5.4.1 결론 축소 (2026-08-14) — 일반화하지 않고 특화로 간다

> **정정 (2026-08-20, §S2-J)**: 아래가 말하는 "캐시 2종(소유자 리스트 / 캐시 없음)"은 `OwnedSet<T>`의 두 모드가 아니다. `OwnedSet<T>`는 앞에 해당하는 한 종류만 다루고, 뒤는 `OwnedSet` 밖이다.

위에서 "S2가 5종을 수용하는 형태를 확정해야 한다"고 이월했는데, **그 숙제 자체가 과하다.** 코드를 다시 보면 **캐시 정책은 애초에 일반화된 적이 없다.**

```csharp
// IRepository 전부 — 정책 enum 이 없다. 전부 CacheKey 를 인자로 받을 뿐이다.
Task<List<T>> GetListAsync<T>(CacheKey listKey, Func<IDbExecutor, Task<List<T>>> dbFetch);
Task<T>       InsertAsync<T>(T entity, CacheKey listKey);
Task          UpdateAsync<T>(T entity, CacheKey listKey, Func<T, bool> match);
```

정책은 **어떤 키를 넘기는가 + 어떤 base 메서드를 부르는가**로 표현된다. 5.3이 "완성된 T2"라고 평가한 `SessionComponent`의 포인터 캐시도 새 추상이 아니라 **키 2개 + 기존 메서드 조합**이다.

```csharp
CacheKey.For(SessionModel, "AccountBySessionKey", key)   // 포인터
CacheKey.For(SessionModel, "AccountId", accountId)       // 값
GetMdlWithCacheAsync<SessionModel>(..., slidingTtl)
```

따라서 `OwnedSet<T>`가 알아야 할 것은 **2종이면 충분하다.**

| | 표현 |
|---|---|
| 기본 — 소유자별 리스트 | `[Entity].ScopeKey`로 키가 정해진다 |
| 캐시 없음 | 캐시 키를 넘기지 않는 경로 하나 |

나머지(Session 포인터, Schedule 전역)는 **그 엔티티 전용 코드로 남긴다.** 결과:
- `[Entity]`에 `Cache`/`SlidingTtl`을 넣지 않아도 된다 — S1의 보류 결정이 그대로 유효해진다
- S2 숙제가 "5종 수용 형태 확정" → **"2종만 알고 나머지는 특화"** 로 줄어든다
- 5.3의 "Session 포인터 캐시를 잃지 않는다"가 자동으로 만족된다. 건드릴 이유가 없어지기 때문이다

**경계 규칙**: **엔티티 하나에만 해당하는 캐시 동작은 일반화하지 않는다. 두 번째 엔티티가 같은 것을 요구할 때 올린다**(R7). 현재 포인터 캐시는 `Session` 하나, 전역 캐시는 `Schedule` 하나다.

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

> **해소 (S1 실행, 2026-08-14).** 위험이 처음부터 없었다. 위 코드에서 보듯 **`ModelRegistration.Init`이 이미 둘을 묶고 있으므로**, `ScanAndRegister`가 그것을 리플렉션으로 호출하면 한쪽만 등록되는 상태가 만들어질 수 없다. 두 레지스트리를 각각 부르는 구현을 택했다면 실재했을 위험이고, 그렇게 하지 않은 이유가 이것이다.

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
→ **`OwnedSet<T>`의 첫 조회 시점까지 지연**하도록 명시한다. 스코프를 만들기만 하고 안 쓰는 경로(예: 조건 분기)에서 커넥션 낭비가 사라진다. A안이 아니면 고치기 어려운 지점이므로 기회를 살린다.

### 5.12 🟢 미개선 — T1 확장 메서드는 전체 로드 후 메모리 필터다

`GetListAsync` 결과를 거르는 구조라 데이터가 커지면 비효율이다. **현재도 동일**하므로 후퇴는 아니지만 개선도 아니다. 스코프 컬렉션이 작다는 전제 위에 있으며, 깨지면 T3로 승격한다.

---

### 5.13 리뷰 결과 반영 — 스텝 조정

| 스텝 | 추가되는 작업 |
|---|---|
| **S0-4** (완료) | **커밋 경계를 유저 락 안으로 이동 (5.1)** · **락 커넥션 분리 (5.2 선결)** — §4.2 |
| **S1** (완료) | attribute는 `Pk` + `ScopeKey` 둘로 한정 · `Table` 미포함(규칙 이탈 0건) · `Cache`/`SlidingTtl`은 TODO (5.4 결론 정정) · **ScopeKey는 User 폴더 한정, `fk` 토큰에서 생성** · 가드 4종 · `AssertMatches` 철회, 검사를 `Init` 안으로 (§S1-D) · 5.8은 해소 |
| **S12** | RaidServer의 등록 표면이 2 → 19로 넓어진 것을 되돌릴지 판단 (§S1-F) |
| **S2** (완료) | `OwnedSet<T>`는 캐시되는 소유자 리스트 전용 (5.4.1 → §S2-J) · 스코프 3종은 독립 클래스 (§S1-G) · 커넥션 지연 오픈 (5.11) · **dirty 철회 (§S2-H)** · `GameDb.Utility` 는 S10.5 로, lazy BEGIN 은 S11 로 이월 · 네이밍 규칙 확정 (§S2-F) · Auth 형태 확정 (§S2-E) |
| **S4** (완료) | Auth 형태 확정 · `Identity`/`AuthScope` 2클래스 · Component/Manager 6개 삭제 · **게이트는 S5 로 옮겨간다** (§S4-B) · `SetShardId` 다리 (§S4-D) |
| **S4** | 캐시 없는 경로 실증 (Account/Channel/Device는 원래 캐시 없음) · **Auth의 스코프 밖 조회(기기 키·채널 키·세션 키·계정 생성)를 어디로 보낼지 확정 → `[Entity]`에 Auth `ScopeKey`를 붙일지 함께 결정 (§S1-G Q2)** |
| **S5** (완료, **게이트 통과**) | `PlayerManager` 도 소비자였다(§S5-B) · 스코프를 읽는 *시점*이 함정(§S5-C) · **MySQL+Redis 실측 17/17 — 막고 있던 기존 결함 2개(Ip null, 응답 캐시 오염)를 잡았다(§S5-D)** · `GetOrCreateAsync` 는 엔티티별 확장 유지(§S5-E) · **식 트리 제거, 생성기가 `PkEquals`/`IScopedModel` 접근자를 찍는다(§S5-F)** · 커밋 전 리뷰에서 미보유 쿠키 강화 버그를 잡았다(§S5-G) · `OwnScope` 를 `ServiceBase` 로(§S5-H) · `PlayerDetailManager` 의 region 4개를 같이 고친다(census 재도출) · 스코프는 `PlayerDetail.TouchAsync(userScope)` 로 전달 · **ScopeKey 쓰기 규칙 신설 — 생성은 채우고 수정은 확인한다** · `ChangeSet` 은 S6 으로 · `VerifyCacheTags` 역방향 검사는 S13 으로 재이월 · **캐시·MySQL 경로는 ServerTest 가 못 지나간다** |
| **S6** (완료) | 위상 정정 — 게이트가 아니고 `_userRepo` 도 14개 남는다 · **`LoadedObjects` 철회**(벌크 로드 이득이 실재하지 않음, §S6-계획-C) · `RewardService` 는 상태 없이 스코프를 인자로, 타입별 개별 구현 · `PlayerDetail` 도 Queries 확장 하나로(`GetOneAsync` 안 만듦) · ChangeSet 의미 확정 + ClientCore COOKIE/SOUL_STONE 같이 수정 · DecGold 잔액검사와 Acc 차감 제거 · **자율 리뷰 3회에서 음수 금액 캐시 증발/증식 결함을 잡아 불변식을 모델로 내렸다(§S6-B)** |
| **S7** (완료) | 포인터 캐시 유지 — `SessionStore` 로 분리, `Identity` 와 안 합침 (§S7b-A Q1) · 키 로테이션 입구를 private 으로 닫음 (§S7b-A Q2) · `SetPlayerId`/`SetShardId` 이동 완료, `SessionStamp` 로 컨텍스트 단절 (5.9 해소) · **계획-A ⑤ 가 틀렸다 — Auth base 는 S12 가 아니라 S7b·S8 에서 나갔다** |
| **S8** (완료) | **`GlobalList` 를 넣지 않았다** — 도입 근거("매 요청 전량 조회")가 사실이 아니었다 (§S8-A Q1) · `ScheduleView` 가 `ServerTime` 을 인자로 (5.10 해소, 데이터 계층의 마지막 `RpcContext` 자리) · `SchedulePacket.State` 가 항상 0 이던 것을 잡음 · 커밋 후 리뷰로 가챠 타입 가드와 `ProtoDb` 6회 조회를 고침 (§S8-B) |
| **S9** (완료) | **`scope.Raw` 를 만들지 않았다 — 이 행의 과제는 앵커가 사라졌다.** 집계를 메모리로 옮겼고, flush 주의 자체가 §S2-H 의 dirty 철회로 이미 무의미했다 (§S9-A) · `WorldRewardStar` 가 한 번도 성공한 적 없던 원인 2건 + 별 경계 1건 수정 |
| **S10** (완료) | 계획의 `KingdomBuilder` 하나가 `KingdomTileMap`(순수 계산) / `KingdomMapService`(로드·저장) 둘로 갈림 (§S10-A) · 기존 결함 6건 수정 · `kingdom/change-item` 등록, `KingdomStructureDecTime` 은 검증 TODO 라 보류 · Component/Manager 전부 철거 |
| **S10.5** (축소) | `MySqlLockService`가 쓰는 `DbUtilityConnection`을 `GameDb.Utility`로 감싸기 — 커넥션 분리 자체는 S0-4에서 완료 (5.2) |
| **S11** (완료) | **커밋 전 리뷰 3회에서 커밋 실패 시 커넥션 누수를 잡아 원천(`DBSqlExecutor`)에서 고쳤다(§S11-C)** · `AllUserRepo` → `GameDb.AllShards.TryGetPlayerByNameAsync` (§5.5 의 `Find*` 는 §S2-F 규칙과 충돌해 기각) · 순회 중 지연 오픈 · **커밋은 `GameDb`, 세션 닫기는 `DbSessionManager(IDisposable)` 로 책임이 갈렸다** (§S11-A) · census 가 죽은 `using` 을 못 봐 빌드가 한 번 막혔다 · InMemory 에서 이름 중복 가드가 처음 검증 가능해져 `ChangeNameTest` 를 남겼다 |
| **S11.5** (완료) | lazy BEGIN — `DbSessionManager.Open` 이 `LazyDbSession` 을 돌려준다. **MySQL 실측 `Com_begin` 249 → 141**(§S11.5-C) · ⑧ 이 뭉뚱그린 것을 census 가 정정했다 — 늦게 열리던 자리는 `OwnedSet` 하나였다 |
