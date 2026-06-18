# 레이드 방(로비/전투/채팅) 설계

작성일: 2026-06-14

관련 문서: [CoopRaid.md](./CoopRaid.md) (방/전투 기획 원본), [RaidMatching.md](./RaidMatching.md) (매칭 → 방 입장)

본 문서는 CoopRaid.md의 "방(Room)" 개념을 현재 RaidServer 아키텍처
(`NetworkSession` / `PacketProcessor` / `EPacketType` / `IPacketHandler`)에
매핑하고, CoopRaid.md에 없던 **전투 중 채팅**을 포함한 공용 채팅 프로토콜을 추가한다.
전투의 데미지 계산식·보스 AI·상태이상 상세는 CoopRaid.md "전투 페이즈" 장을
그대로 따르며, 본 문서에서는 패킷 매핑과 흐름만 다룬다.

---

## 1. 개요

방(Room)은 매칭(또는 향후 방 코드 입장)으로 모인 1~4명의 플레이어가
WAITING → COUNTDOWN → IN_BATTLE → RESULT 상태를 거치는 단위다.
CoopRaid.md §4.2의 상태 머신을 그대로 사용한다.

```
WAITING → COUNTDOWN → IN_BATTLE → RESULT → (방 정리, PlayerState = IDLE 복귀)
```

| 상태 | 진입 조건 | 허용 액션 (본 문서 패킷) |
|------|-----------|-----------|
| WAITING | 방 생성([RaidMatching.md](./RaidMatching.md) §5.2) | RoomMoveReq, RoomReadyReq, RoomChatReq, RoomLeaveReq |
| COUNTDOWN | 전원 Ready | RoomChatReq, RoomReadyReq(해제 시 WAITING 복귀) |
| IN_BATTLE | 카운트다운 완료 | TurnActionReq, RoomChatReq |
| RESULT | 보스 HP=0 또는 전멸 | RoomLeaveReq |

---

## 2. 방 입장 경로

```
[RaidMatching.md 매칭 성립]
  → RoomService.CreateRoom(bossNum, members) : RoomId 발급, 멤버를 WAITING으로 등록
  → MatchingCompleteNtf { RoomId, BossNum, Members }     (매칭 성립 알림)
  → RoomEnterNtf { RoomId, BossNum, MapData, Members }   (방 초기 상태 스냅샷 = CoopRaid의 ROOM_JOINED)
```

- `MatchingCompleteNtf`는 "매칭이 성립했다"는 게임-흐름 전환 알림이고,
  `RoomEnterNtf`는 방의 실제 초기 상태(맵, 멤버별 스폰 위치, ready=false)를 담은 스냅샷이다.
  둘은 매칭 틱에서 연속으로 전송된다.
- CoopRaid.md §4.1 "방 코드 입장"(지인 파티)을 나중에 추가할 때도 동일하게
  `RoomService.CreateRoom`/`JoinRoom` → `RoomEnterNtf`로 합류하므로, 이 시점부터는
  매칭 경로와 방 코드 경로가 동일한 코드를 탄다.

---

## 3. 아키텍처 개요

### 3.1 RoomService — PlayerId 기준 멤버 관리 (재접속 대응)

`NetworkSession.Id`는 TCP 연결마다 새로 발급되는 GUID이므로, 재접속 시 바뀐다.
방 멤버십은 **PlayerId 기준**으로 관리하고, SessionId는 "현재 연결"을 가리키는
포인터로만 둔다.

```csharp
public enum ERoomState { WAITING, COUNTDOWN, IN_BATTLE, RESULT }

public class RoomMember
{
    public ulong PlayerId { get; init; }
    public string ProfileName { get; init; } = string.Empty;
    public string SessionId { get; set; } = string.Empty; // 재접속 시 갱신
    public bool IsConnected { get; set; }
    public (int X, int Y) Position { get; set; }
    public bool IsReady { get; set; }
    // 전투 중 HP 등 전투 상태는 BattleState에서 관리 (CoopRaid §6 참고)
}

public class Room
{
    public string RoomId { get; init; } = string.Empty;
    public int BossNum { get; init; }
    public ERoomState State { get; set; }
    public List<RoomMember> Members { get; init; } = new();
    public int CountdownToken { get; set; } // 카운트다운 취소 감지용 (§6.2)
    public BattleState? Battle { get; set; }
}

public class RoomService
{
    private readonly Dictionary<string, Room> _roomsById = new();        // RoomId -> Room
    private readonly Dictionary<ulong, string> _roomIdByPlayerId = new(); // PlayerId -> RoomId
}
```

### 3.2 단일 루프 재사용

[RaidMatching.md](./RaidMatching.md) §3.1~3.2와 동일하게, `Room`/`RoomService`의
인메모리 상태는 `PacketProcessor`의 단일 루프 안에서만 변경된다. 클라이언트 패킷
(`TurnActionReq` 등)은 기존 핸들러 디스패치로 처리되고, 카운트다운/턴 타임아웃 같은
**시간 기반 전환**은 `PacketProcessor.RunOnLoop(Func<Task>)`([RaidMatching.md](./RaidMatching.md) §3.2)을
통해 같은 루프에서 처리한다.

### 3.3 타이머 취소: "토큰 비교" 패턴

`Task.Delay` 기반 타이머는 `CancellationToken` 없이도 다음 패턴으로 취소를 표현한다.

1. 타이머 시작 시 `room.CountdownToken`(또는 `TurnToken`)을 증가시키고 현재 값을 캡처.
2. `Task.Delay(...)` 후 `RunOnLoop(() => ...)`에서 **캡처한 값이 현재 `room.CountdownToken`과
   같은지 확인** — 다르면(취소됨) 아무 것도 하지 않고 종료.
3. 같으면 다음 단계(브로드캐스트/상태 전환) 진행, 필요 시 토큰 다시 증가시켜 다음 타이머 예약.

카운트다운(§6.2)과 턴 타임아웃(§7.2) 모두 이 패턴을 사용한다.

---

## 4. 로비(WAITING) — 이동

CoopRaid.md §5.1~5.2의 20×15 그리드/4방향 이동 규칙을 그대로 사용한다.

| 방향 | 패킷 | Payload |
|------|------|---------|
| C→S | `RoomMoveReqPacket` | `Dir: EMoveDir (UP/DOWN/LEFT/RIGHT)` |
| S→C (브로드캐스트, 본인 포함) | `RoomMoveNtfPacket` | `PlayerId, X, Y` |
| S→C (요청자에게만, 거부 시) | `RoomMoveDeniedNtfPacket` | `X, Y` (현재 위치 재전송) |

- 이동 성공 시 `RoomMoveNtf`를 본인에게도 브로드캐스트하므로, 별도 ACK(Res)는 두지 않는다.
- `RoomReadyReq`로 `IsReady = true`인 상태에서 `RoomMoveReq`가 오면, 이동 처리 전에
  `IsReady = false`로 되돌리고 `RoomReadyNtf { PlayerId, IsReady=false }`도 함께 브로드캐스트한다
  (CoopRaid §5.3 "Ready 상태에서 이동 입력 수신 시 ready 자동 해제").
- `WAITING` 상태가 아닐 때 수신되면 무시(로그만 남김). COUNTDOWN/IN_BATTLE 중 이동 불가는
  CoopRaid §6 "위치 고정"과 동일.

---

## 5. 로비(WAITING) — Ready

| 방향 | 패킷 | Payload |
|------|------|---------|
| C→S | `RoomReadyReqPacket` | (없음, 토글) |
| S→C (브로드캐스트, 본인 포함) | `RoomReadyNtfPacket` | `PlayerId, IsReady` |

- `RoomReadyReq`는 `WAITING`과 `COUNTDOWN` 양쪽에서 수신 가능 (COUNTDOWN 중 해제 시 §6.2 참고).
- 전원이 `IsReady = true`가 되는 순간 §6 카운트다운으로 전환한다.

---

## 6. 페이즈 전환 (WAITING → COUNTDOWN → IN_BATTLE)

### 6.1 트리거

`RoomReadyReqHandler`가 토글 처리 후, 모든 멤버의 `IsReady == true`이면
`room.State = WAITING → COUNTDOWN`으로 변경하고 카운트다운을 시작한다.

### 6.2 카운트다운 (3초, §3.3 토큰 패턴)

```csharp
async Task RunCountdownAsync(string roomId)
{
    var token = ++room.CountdownToken; // 시작 시 토큰 증가/캡처

    for (int count = 3; count >= 1; count--)
    {
        await packetProcessor.RunOnLoop(() =>
        {
            if (room.State != ERoomState.COUNTDOWN || room.CountdownToken != token)
                return Task.CompletedTask; // 취소됨

            sessionService.Broadcast(memberSessionIds, RoomCountdownNtf { Count = count });
            return Task.CompletedTask;
        });
        await Task.Delay(1000);
    }

    await packetProcessor.RunOnLoop(() =>
    {
        if (room.State != ERoomState.COUNTDOWN || room.CountdownToken != token)
            return Task.CompletedTask;

        StartBattle(room); // 위치 고정, 보스 등장, 턴 순서 계산, BattleStartNtf
        return Task.CompletedTask;
    });
}
```

- **취소 조건** (CoopRaid §12 "카운트다운 중 Ready 해제 → COUNTDOWN 취소, WAITING 복귀"):
  `RoomReadyReqHandler`에서 누군가 `IsReady = false`로 토글하면
  `room.State = WAITING`, `room.CountdownToken++` 으로 진행 중인 카운트다운을 무효화한다.

### 6.3 전투 시작 (`StartBattle`)

CoopRaid §6의 절차를 그대로 수행한다.

1. 멤버 위치 고정 (이후 `RoomMoveReq` 무시)
2. 보스를 `BOSS_SPAWN` 위치에 등장, `BattleState` 생성 (보스 HP/스탯은 Proto 데이터에서 로드)
3. 턴 순서 계산 (Speed 내림차순, 동점 시 입장 순서) → `BattleState.TurnOrder`
4. `room.State = IN_BATTLE`
5. `BattleStartNtf { TurnOrder, BossData, PlayerDataList }` 브로드캐스트
6. 첫 턴 시작 → §7.1

---

## 7. 전투 페이즈 (IN_BATTLE)

데미지 계산식, 보스 AI/페이즈, 스킬, 상태이상, 종료 조건은
**CoopRaid.md "전투 페이즈" 장을 그대로 따른다**. 본 절은 그 흐름이 RaidServer
패킷으로 어떻게 오가는지만 정리한다.

### 7.1 패킷 매핑 (CoopRaid §9 JSON type → EPacketType)

| CoopRaid type | EPacketType | 방향 | 비고 |
|---------------|-------------|------|------|
| `BATTLE_START` | `BattleStartNtf` | S→C | §6.3에서 전송 |
| `TURN_START` | `TurnStartNtf` | S→C | `PlayerId, TimeoutSec` |
| `TURN_ACTION` | `TurnActionReq` / `TurnActionRes` | C→S / S→C | §7.2 |
| `TURN_RESULT` | `TurnResultNtf` | S→C | `ActorId, ActionType, TargetId, Damage, Heal, Effects, HpSnapshot` |
| `BOSS_WARNING` | `BossWarningNtf` | S→C | 예고 시스템 (CoopRaid §6.5) |
| `BOSS_TURN` | `BossTurnNtf` | S→C | `ActionType, SkillId, TargetId, Results[]` |
| `STATUS_EFFECT` | `StatusEffectNtf` | S→C | |
| `PLAYER_DEAD` | `PlayerDeadNtf` | S→C | |
| `BOSS_PHASE2` | `BossPhase2Ntf` | S→C | |
| `GAME_END` | `BattleEndNtf` | S→C | §8 |

### 7.2 턴 진행 + 타임아웃 (§3.3 토큰 패턴)

1. 현재 턴 플레이어에게 `TurnStartNtf { PlayerId, TimeoutSec = 30 }` 전송, `room.Battle.TurnToken++` 캡처
2. `packetProcessor.RunOnLoop(... Task.Delay(30s) 후 토큰 비교 ...)`로 30초 타임아웃 예약
3. 둘 중 먼저 일어나는 일에 따라:
   - `TurnActionReqHandler` 수신 → `room.Battle.TurnToken++`로 타임아웃 무효화, 행동 처리
   - 타임아웃 콜백이 먼저 → 토큰 일치 확인 후 자동 `ATTACK` 처리 (CoopRaid §6.2)
4. **중복 `TurnActionReq` 처리** (CoopRaid §12): 처리 직후 `room.Battle.TurnToken++`로
   같은 턴의 추가 요청은 토큰 불일치로 자연히 무시된다 → `TurnActionRes { Result = AlreadyActed }`
5. 행동 결과를 `TurnResultNtf`로 브로드캐스트, 다음 턴 또는 보스 턴(§7.3)으로 진행
6. **접속 끊김 플레이어 턴**: `RoomMember.IsConnected == false`이면 `TurnStartNtf` 전송 없이
   즉시 자동 `ATTACK` 처리 후 다음 턴으로 (CoopRaid §6.2)

### 7.3 보스 턴

보스 AI 행동 선택(CoopRaid §6.5)을 수행하고 `BossTurnNtf`로 결과를 브로드캐스트한다.
다음 턴 예고가 필요하면 `BossWarningNtf`를 함께 보낸다. 라운드 종료 시 종료 조건(§8)을 체크한다.

---

## 8. 결과 (RESULT)

| 조건 | 결과 |
|------|------|
| 보스 HP ≤ 0 | `0`으로 클램핑 후 즉시 종료, `BattleEndNtf { Result = WIN }` |
| 전원 HP = 0 | `BattleEndNtf { Result = LOSE }` |

`BattleEndNtf`에는 CoopRaid §8의 표시 항목(라운드 수, 플레이어별 딜량/힐량/사망 횟수,
보상)을 담는다. 전송 후 `room.State = RESULT`.

- RESULT 진입 시 모든 멤버의 `RoomMember`는 유지하되, `RoomLeaveReq`만 허용한다.
- 멤버 전원이 `RoomLeaveReq`를 보내거나, RESULT 진입 후 일정 시간(예: 60초)이 지나면
  `RoomService`가 방을 정리(`_roomsById`/`_roomIdByPlayerId`에서 제거)하고 각 멤버의
  PlayerState를 `IN_ROOM → IDLE`로 되돌린다.

---

## 9. 채팅 (공용 — WAITING + IN_BATTLE)

CoopRaid §9.3은 대기 페이즈에만 `CHAT`/`CHAT_MESSAGE`를 정의한다. **전투 중에도
채팅이 필요하다는 점이 CoopRaid.md의 공백**이므로, 본 문서에서 방 상태와 무관하게
동작하는 공용 채팅 프로토콜을 새로 정의한다.

| 방향 | 패킷 | Payload |
|------|------|---------|
| C→S | `RoomChatReqPacket` | `Message: string` |
| S→C (브로드캐스트, 본인 포함) | `RoomChatNtfPacket` | `PlayerId, ProfileName, Message` |

- `RoomChatReqHandler`는 `room.State`를 검사하지 않는다 — WAITING/COUNTDOWN/IN_BATTLE/RESULT
  어느 상태에서든 방에 속해 있으면(§10.2 `RequireRoom`) 허용한다.
- 메시지 검증(빈 문자열/길이 제한)에 실패하면 조용히 무시한다 (별도 Res 없음).
- 본인에게도 브로드캐스트하므로 별도 ACK 불필요 — §4 이동/§5 Ready와 동일한 설계.

---

## 10. 프로토콜 / 서비스 구조

### 10.1 EPacketType 추가

```csharp
public enum EPacketType : ushort
{
    EchoReq, EchoRes,
    AuthReq, AuthRes,
    PingReq, PongRes,
    EchoAuthReq, EchoAuthRes,

    // --- Matching (RaidMatching.md) ---
    MatchingStartReq, MatchingStartRes,
    MatchingCancelReq, MatchingCancelRes,
    MatchingCompleteNtf,

    // --- Room 입장 ---
    RoomEnterNtf,

    // --- Room: 로비 (WAITING) ---
    RoomMoveReq,
    RoomMoveNtf,
    RoomMoveDeniedNtf,
    RoomReadyReq,
    RoomReadyNtf,
    RoomCountdownNtf,
    RoomLeaveReq, RoomLeaveRes,
    RoomPlayerLeaveNtf,

    // --- Room: 채팅 (WAITING + IN_BATTLE 공용) ---
    RoomChatReq,
    RoomChatNtf,

    // --- Room: 전투 (IN_BATTLE) ---
    BattleStartNtf,
    TurnStartNtf,
    TurnActionReq, TurnActionRes,
    TurnResultNtf,
    BossWarningNtf,
    BossTurnNtf,
    StatusEffectNtf,
    PlayerDeadNtf,
    BossPhase2Ntf,
    BattleEndNtf,
}
```

### 10.2 `IPacketHandler.RequireRoom`

`RequireAuth`(`Code/RaidServer/Network/IPacketHandler.cs`)와 동일한 방식으로
"방에 속해 있어야 하는 패킷"을 게이팅하는 플래그를 추가한다.

```csharp
public interface IPacketHandler
{
    ushort Opcode { get; }
    Type Req { get; }
    bool RequireAuth => false;
    bool RequireRoom => false; // 추가
    Task RunAsync(string sessionId, object req);
}
```

`PacketProcessor.ExecuteAsync`에 `RequireAuth` 체크와 같은 자리에 추가:

```csharp
if (handler.RequireRoom)
{
    if (!_roomService.TryGetRoomBySessionId(envelope.SessionId, out _))
    {
        _logger.LogWarning($"NOT_IN_ROOM Opcode({opcode}) SessionId({envelope.SessionId})");
        envelope.Tcs.SetResult();
        continue;
    }
}
```

- `RequireRoom = true`: `RoomMoveReq`, `RoomReadyReq`, `RoomChatReq`, `RoomLeaveReq`, `TurnActionReq`
- **방 상태별 허용 여부**(예: `TurnActionReq`는 `IN_BATTLE`에서만 유효)는 이 플래그로 막지 않고,
  각 핸들러/서비스 메서드 내부에서 `room.State`를 확인해 `*Res { Result = InvalidState }` 등으로
  응답한다. (방 소속 여부는 모든 핸들러가 공통으로 검사하지만, 상태별 유효성은 패킷마다
  달라 핸들러 책임으로 둔다.)

### 10.3 서비스/핸들러 구조

```
Network/
├─ Handler/
│  ├─ RoomMoveReqHandler.cs    (RequireAuth, RequireRoom)
│  ├─ RoomReadyReqHandler.cs   (RequireAuth, RequireRoom)
│  ├─ RoomChatReqHandler.cs    (RequireAuth, RequireRoom)
│  ├─ RoomLeaveReqHandler.cs   (RequireAuth, RequireRoom)
│  └─ TurnActionReqHandler.cs  (RequireAuth, RequireRoom)
├─ RoomService.cs               (싱글톤, 방 목록 + PlayerState 관리)
└─ BattleState.cs               (방별 전투 상태: HP, 턴 순서, 상태이상 — CoopRaid §6)
```

- `RoomService`는 [RaidMatching.md](./RaidMatching.md) §8의 `MatchingService`로부터
  `CreateRoom(bossNum, members)` 호출을 받는다.
- `AuthPacketHandler`(`Code/RaidServer/Network/Handler/AuthPacketHandler.cs`) 인증 성공 직후,
  `RoomService.TryRebindSession(playerId, newSessionId)`를 호출해 §11 재접속 매핑을 갱신한다.

---

## 11. 재접속 처리

CoopRaid §10.3을 현재 아키텍처에 매핑한다.

1. 클라이언트가 새 TCP 연결로 `AuthReq` 전송 → 새 `NetworkSession`(새 SessionId) 인증 성공
2. `AuthPacketHandler`가 `RoomService.TryRebindSession(playerId, newSessionId)` 호출
3. `RoomService`: `_roomIdByPlayerId[playerId]`로 기존 방을 찾고, 해당 `RoomMember.SessionId`를
   새 SessionId로 교체, `IsConnected = true`
4. 방이 `IN_BATTLE`이면 현재 `BattleState` 전체 스냅샷을 담은 `RoomEnterNtf`(재진입용 재사용)를
   재접속한 세션에만 전송
5. 이후 턴부터 정상 참여 (§7.2 "접속 끊김 플레이어 턴" 처리 대상에서 제외)

연결 종료 시([RaidMatching.md](./RaidMatching.md) §5.4와 동일한 콜백 메커니즘)
`RoomService.OnSessionClosed`는 `IsConnected = false`로만 표시하고 멤버를 제거하지 않는다
(CoopRaid §4.3 "전투 중 접속 끊김 → 방에 잔류").
단, **WAITING 중** 접속 끊김은 CoopRaid §4.3에 따라 멤버를 제거하고
`RoomPlayerLeaveNtf`를 브로드캐스트한다.

---

## 12. 흐름도 (시퀀스)

### 12.1 매칭 → 로비 → 전투 시작

```
P1..P4         MatchingService     RoomService          (전원)
  |--StartReq-->|                   |
  ...(RaidMatching.md §7.1과 동일, 4인 그룹 성립)...
  |<--MatchingCompleteNtf(RoomId)---|
                |--CreateRoom------->|
                |<-------RoomId------|
  |<-----------------------------RoomEnterNtf(맵/스폰위치)-------------|
  |--RoomReadyReq-->|                                    |
  |<--------------------RoomReadyNtf(P1,true)------------|  (브로드캐스트)
  ... P2,P3,P4도 Ready ...
  |<--------------------RoomCountdownNtf(3,2,1)-----------| (1초 간격)
  |<--------------------BattleStartNtf(turnOrder,...)-----|
```

### 12.2 전투 턴 진행

```
RoomService                    P1(현재 턴)        나머지
  |--TurnStartNtf(P1,30s)------>|
  |--TurnStartNtf(P1,30s)----------------------->| (전원 브로드캐스트)
  |<--TurnActionReq(ATTACK)-----|
  |--TurnResultNtf(dmg, hpSnapshot)-------------->| (전원 브로드캐스트)
  | (다음 플레이어 턴 또는 보스 턴)
```

### 12.3 전투 중 채팅 (CoopRaid 공백 보강)

```
P2                        RoomService              전원(P1,P3,P4 포함)
  |--RoomChatReq("힐 주세요")-->|
  |<---------------------RoomChatNtf(P2,"힐 주세요")---| (IN_BATTLE 중에도 동작)
```

### 12.4 카운트다운 취소 (Ready 해제)

```
P1,P2,P3,P4 전원 Ready → COUNTDOWN 시작 (count=3 브로드캐스트)
  P2가 RoomReadyReq(해제) 전송
    → room.State = WAITING, CountdownToken++
  count=2 콜백 도착 → 토큰 불일치 → 무시 (브로드캐스트 안 함)
  → 클라이언트들은 RoomReadyNtf(P2,false)만 수신, WAITING 유지
```

---

## 13. 룸 샤딩 경로 (향후, 지금은 구현하지 않음)

[RaidMatching.md](./RaidMatching.md) §3.3과 동일한 전제: 지금은 모든 방이 단일
`PacketProcessor` 루프에서 처리된다. 향후 부하 분산이 필요해지면:

- `RoomId`를 해시해 N개의 `PacketProcessor` 인스턴스(=N개의 독립 루프)에 분산
- 방 생성 시점에 샤드를 결정하고, 해당 방에 속한 모든 패킷/타이머는 같은 샤드의
  루프로 라우팅 (방은 생성 후 멤버가 고정되므로 재라우팅 불필요)
- `SessionService`(전역 싱글톤)는 그대로 공유하되, `RoomService`는 샤드별 인스턴스로 분리

---

## 14. 예외 / 엣지 케이스

CoopRaid §12 기준 + 매칭 통합 관련 항목.

| 상황 | 처리 |
|------|------|
| 카운트다운 중 Ready 해제 | COUNTDOWN 취소, WAITING 복귀 (§6.2) |
| WAITING 중 접속 끊김 | 멤버 제거, `RoomPlayerLeaveNtf` 브로드캐스트 (§11) |
| 전투 중 접속 끊김 | 멤버 잔류(`IsConnected=false`), 해당 턴 자동 ATTACK (§7.2) |
| 전투 중 재접속 | `RoomEnterNtf`로 스냅샷 재전송 후 정상 참여 (§11) |
| 전투 중 전체 접속 끊김 | 60초 대기 후 방 자동 종료 (RunOnLoop 타이머, §3.3 토큰 패턴) |
| 전투 중 1명만 남음 | 계속 진행 (1인 클리어 가능) |
| 중복 `TurnActionReq` 수신 | 첫 번째만 처리, 이후 `AlreadyActed` (§7.2) |
| 보스 HP 음수 | 0으로 클램핑 후 즉시 `BattleEndNtf(WIN)` |
| RESULT 상태에서 `RoomMoveReq`/`TurnActionReq` 등 수신 | `RequireRoom`은 통과하되 핸들러에서 `InvalidState`로 무시 |
| 매칭 직후(WAITING 진입 전) 멤버 연결 끊김 | [RaidMatching.md](./RaidMatching.md) §9 "입장 시 접속 끊김" → §11 "WAITING 중 접속 끊김"과 동일 처리 |
