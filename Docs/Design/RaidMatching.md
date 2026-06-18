# 레이드 매칭 시스템 설계

작성일: 2026-06-14

관련 문서: [CoopRaid.md](./CoopRaid.md) (방/전투 상세 기획), [RaidRoom.md](./RaidRoom.md) (게임방 로비/전투/채팅)

---

## 1. 개요

`AuthReq`로 인증된 RaidServer 세션이, 보스 종류를 지정해 매칭을 요청하면
서버가 같은 조건의 대기자를 모아 1~4인 그룹을 구성하고 [RaidRoom.md](./RaidRoom.md)의
"방"으로 입장시키는 기능이다.

CoopRaid.md의 "방 코드 입장"(지인 파티용 수동 입장)과는 별도 트랙이며,
본 문서는 자동 매칭 트랙만 다룬다. 두 트랙 모두 최종적으로 동일한
[RaidRoom.md](./RaidRoom.md) 의 방(Room)으로 합류한다.

---

## 2. 전체 흐름 (큰 그림)

```
[연결]
  ↓ AuthReq / AuthRes
[AUTHENTICATED] ── 평소 상태. PlayerState = IDLE
  ↓ MatchingStartReq(BossNum)
[MATCHING] ── 매칭 큐 대기. PlayerState = MATCHING
  ↓ (그룹 성립) MatchingCompleteNtf(RoomId)
[IN_ROOM: WAITING] ── RaidRoom.md 로비. PlayerState = IN_ROOM
  ↓ (전원 Ready) COUNTDOWN → IN_BATTLE
[IN_ROOM: BATTLE]
  ↓ 승리/패배
[IN_ROOM: RESULT]
  ↓ 일정 시간 후 방 정리
[AUTHENTICATED] ── PlayerState = IDLE (다시 매칭 가능)
```

- `NetworkSession.State`(`PENDING`/`AUTHENTICATED`/`CLOSED`)는 **연결/인증 레벨** 상태로 그대로 둔다.
- 위 다이어그램의 `IDLE`/`MATCHING`/`IN_ROOM`은 **게임 레벨 상태(PlayerState)** 로, `MatchingService`/`RoomService`가
  관리한다. (`NetworkSession`에 게임 로직을 끼워 넣지 않는다 — 인증/연결과 게임 상태를 분리하는 기존 원칙과 동일)

---

## 3. 아키텍처 원칙

### 3.1 단일 루프 = 락 프리 (Actor의 "한 번에 한 메시지" 보장을 그대로 사용)

`PacketProcessor.ExecuteAsync`는 채널에 들어온 패킷을 **단일 컨슈머 루프**로 순차 처리한다.
`MatchingService`/`RoomService`의 인메모리 자료구조(큐, 방 목록 등)는 이 루프 안에서만
변경되므로 락이 필요 없다. Actor 모델에서 "액터당 메일박스 하나, 한 번에 한 메시지"로
동시성을 통제하는 것과 동일한 효과를 단일 루프로 얻는 셈이다.

### 3.2 PacketProcessor를 "루프에서 임의 동작 실행"까지 일반화

매칭은 클라이언트 패킷뿐 아니라 **시간에 의한 트리거**(예: "10초 동안 4명이 안 모이면
현재 인원으로 시작")가 필요하다. 이런 타이머 기반 로직도 같은 루프에서 처리해야
락 없이 안전하므로, `PacketProcessor`에 다음 메서드를 추가한다.

```csharp
// PacketProcessor.cs
public Task RunOnLoop(Func<Task> action)
{
    var envelope = new PacketEnvelope { Action = action };
    if (!_receiveChannel.Writer.TryWrite(envelope))
    {
        _logger.LogError("FAILED_ADD_INTERNAL_EVENT");
    }
    return envelope.Tcs.Task;
}
```

`ExecuteAsync` 루프에서는 `envelope.Action`이 설정된 경우 핸들러 디스패치 대신
바로 `await envelope.Action()`을 실행한다. 이렇게 하면:

- `MatchingTicker`(`BackgroundService`)가 N초마다 `packetProcessor.RunOnLoop(() => matchingService.OnTick())`을
  호출 → 매칭 큐 평가가 클라이언트 패킷과 동일한 직렬 큐에서 처리됨.
- [RaidRoom.md](./RaidRoom.md)의 카운트다운/턴 타임아웃 타이머도 동일 메커니즘을 재사용한다.

### 3.3 향후 샤딩 경로 (지금은 구현하지 않음)

- **매칭 샤딩**: 매칭 큐는 "보스Num(매칭 조건)"별로 이미 분리된 자료구조이므로,
  필요 시 보스Num 단위로 별도 `PacketProcessor`/루프를 두고 `MatchingService`만 이전하면 된다.
- **룸 샤딩**: [RaidRoom.md](./RaidRoom.md) §7 참고. RoomId 해시로 여러 루프에 분산 가능.
- 지금은 둘 다 같은 단일 `PacketProcessor` 위에서 동작하되, **서비스 클래스를 분리**해두어
  나중에 라우팅 계층만 추가하면 분리되도록 한다.

---

## 4. 매칭 큐 설계

```csharp
public class MatchingEntry
{
    public string SessionId { get; init; }
    public ulong PlayerId { get; init; }
    public DateTime EnqueueTime { get; init; }
}

public class MatchingService
{
    // BossNum별 대기열
    private readonly Dictionary<int, List<MatchingEntry>> _queueByBoss = new();

    // 취소/연결종료 시 역방향 조회 (SessionId -> BossNum)
    private readonly Dictionary<string, int> _bossNumBySessionId = new();
}
```

- 그룹 크기: 최소 1명 ~ 최대 4명 (CoopRaid.md §4.2 "최대 인원 4명", §12 "1인 클리어 가능"과 동일 전제)
- 큐는 보스Num별로 독립적이다. (ShardId는 매칭 키에 포함하지 않음 — 레이드는 ShardId와 무관하게 매칭. 필요 시 `(ShardId, BossNum)`으로 확장)

---

## 5. 매칭 처리 흐름

### 5.1 매칭 시작

1. 클라이언트 → `MatchingStartReq { BossNum }` (RequireAuth = true)
2. `MatchingService.StartMatching(sessionId, bossNum)`
   - PlayerState가 `IDLE`이 아니면 (`MATCHING` 또는 `IN_ROOM`) → `AlreadyMatching`/`AlreadyInRoom` 응답
   - `BossNum` 유효성 검사 (Proto 데이터에 존재하는 보스인지)
   - `_queueByBoss[bossNum]`에 `MatchingEntry` 추가, PlayerState → `MATCHING`
3. → `MatchingStartRes { Result = Success }`

### 5.2 매칭 틱 (주기 평가)

`MatchingTicker : BackgroundService`가 `RaidConfig.MatchingTickIntervalSec`(예: 2초)마다
`packetProcessor.RunOnLoop(() => matchingService.OnTick())` 호출.

`OnTick()`은 각 BossNum 큐에 대해:

```
큐 인원 >= 4
    → 앞에서 4명 그룹 확정
큐 인원 >= 1 AND (now - 가장 오래 대기한 엔트리.EnqueueTime) >= MatchingTimeoutSec
    → 현재 인원 전체로 그룹 확정
그 외
    → 대기 유지
```

그룹이 확정되면:

1. 그룹 멤버를 큐에서 제거
2. `RoomService.CreateRoom(bossNum, members)` 호출 → `RoomId` 발급, 각 멤버를 방의 WAITING 상태로 등록
3. 그룹 전원에게 `MatchingCompleteNtf { RoomId, BossNum, Members }` 전송 (`SessionService.Broadcast`)
4. 각 멤버 PlayerState `MATCHING` → `IN_ROOM`

이후 흐름은 [RaidRoom.md](./RaidRoom.md) §2(로비 입장)로 이어진다.

### 5.3 매칭 취소

1. 클라이언트 → `MatchingCancelReq` (RequireAuth = true)
2. PlayerState가 `MATCHING`이 아니면 `NotMatching` 응답
3. 큐에서 제거, PlayerState → `IDLE`
4. → `MatchingCancelRes { Result = Success }`

### 5.4 연결 종료 시 정리

`SessionService.CloseNetworkSession`이 호출될 때, 등록된 정리 콜백 목록(`MatchingService.OnSessionClosed`,
`RoomService.OnSessionClosed`)을 순서대로 호출한다. `MatchingService.OnSessionClosed`는
해당 세션이 `MATCHING` 상태였다면 큐에서 제거한다. (이 콜백 역시 §3.2의 `RunOnLoop`을 통해
단일 루프에서 실행한다.)

---

## 6. 프로토콜

### 6.1 EPacketType 추가

```csharp
public enum EPacketType : ushort
{
    EchoReq, EchoRes,
    AuthReq, AuthRes,
    PingReq, PongRes,
    EchoAuthReq, EchoAuthRes,

    // --- Matching ---
    MatchingStartReq,
    MatchingStartRes,
    MatchingCancelReq,
    MatchingCancelRes,
    MatchingCompleteNtf,   // S -> C, 매칭 성립 알림
}
```

`*Ntf`는 클라이언트 요청 없이 서버가 보내는 푸시 패킷을 가리키는 새 네이밍 규칙이다
(`PongRes`처럼 요청에 대한 응답이 아니라, 비동기 이벤트 알림).

### 6.2 패킷 정의

| 방향 | 패킷 | Payload |
|------|------|---------|
| C→S | `MatchingStartReqPacket` | `BossNum: int` |
| S→C | `MatchingStartResPacket` | `Result: EMatchingResult` |
| C→S | `MatchingCancelReqPacket` | (없음) |
| S→C | `MatchingCancelResPacket` | `Result: EMatchingResult` |
| S→C | `MatchingCompleteNtfPacket` | `RoomId: string`, `BossNum: int`, `Members: List<RoomMemberInfo>` |

```csharp
public enum EMatchingResult
{
    Success,
    AlreadyMatching,
    AlreadyInRoom,
    NotMatching,
    InvalidBoss,
}

[ProtoContract]
public class RoomMemberInfo
{
    [ProtoMember(1)] public ulong PlayerId { get; set; }
    [ProtoMember(2)] public string ProfileName { get; set; } = string.Empty;
}
```

---

## 7. 흐름도 (시퀀스)

### 7.1 정상 매칭 (4인 그룹 즉시 성립)

```
P1            P2            P3            P4            MatchingService          RoomService
 |--StartReq(Boss=1)------------------------------------>|                         |
 |<--------------------------------------StartRes(OK)----|                         |
              |--StartReq(Boss=1)------------------------>|                        |
              |<-------------------------------StartRes(OK)|                       |
                            |--StartReq(Boss=1)----------->|                       |
                            |<------------------StartRes(OK)|                      |
                                          |--StartReq(Boss=1)->|                   |
                                          |<--------StartRes(OK)|                  |
                                                              | (틱: 큐 4명)         |
                                                              |--CreateRoom(4)----->|
                                                              |<--------RoomId------|
 |<--MatchingCompleteNtf(RoomId)---------------------------|                       |
              |<--MatchingCompleteNtf(RoomId)---------------|                      |
                            |<--MatchingCompleteNtf(RoomId)--|                     |
                                          |<--MatchingCompleteNtf(RoomId)|         |
```

### 7.2 타임아웃 성립 (1~3인)

```
P1            MatchingService                RoomService
 |--StartReq(Boss=1)-->|                       |
 |<--StartRes(OK)------|                       |
   ... MatchingTimeoutSec 경과, 큐 인원 1명 ...
                        | (틱: 타임아웃, 1명으로 확정)
                        |--CreateRoom(1)------->|
                        |<--------RoomId--------|
 |<--MatchingCompleteNtf(RoomId)|
```

### 7.3 매칭 취소

```
P1            MatchingService
 |--StartReq(Boss=1)-->|
 |<--StartRes(OK)------|
 |--CancelReq---------->|
 |<--CancelRes(OK)------|
```

---

## 8. 서비스/핸들러 구조

```
Network/
├─ Handler/
│  ├─ MatchingStartReqHandler.cs   (RequireAuth = true)
│  └─ MatchingCancelReqHandler.cs  (RequireAuth = true)
├─ MatchingService.cs              (싱글톤, 큐 + PlayerState 관리)
└─ MatchingTicker.cs               (BackgroundService, SessionTimeoutChecker와 동일 패턴)
```

- `MatchingService`는 `RoomService`를 생성자로 주입받아 그룹 성립 시 `CreateRoom` 호출.
- `MatchingService`/`RoomService` 모두 `SessionService.RegisterCloseListener(...)`(가칭)으로
  연결 종료 시 정리 콜백을 등록한다. (§5.4)
- `StartUp.Dependency.cs`에 `MatchingService`, `MatchingTicker`(`AddHostedService`) 등록 추가.

---

## 9. 예외 / 엣지 케이스

| 상황 | 처리 |
|------|------|
| `MATCHING` 상태에서 `MatchingStartReq` 재요청 | `AlreadyMatching` 응답, 큐 변경 없음 |
| `IN_ROOM` 상태에서 `MatchingStartReq` | `AlreadyInRoom` 응답 |
| `MATCHING`이 아닌 상태에서 `MatchingCancelReq` | `NotMatching` 응답 |
| 큐 대기 중 연결 끊김 | §5.4에 따라 큐에서 자동 제거 |
| 그룹 확정 직후(아직 `MatchingCompleteNtf` 전송 전) 멤버 연결 끊김 | 그룹 확정은 틱 시점 스냅샷 기준이므로 그대로 진행하고,
끊긴 멤버는 [RaidRoom.md](./RaidRoom.md) §2의 "입장 시 접속 끊김" 처리(빈 자리로 시작)를 따른다 |
| 존재하지 않는 `BossNum` | `InvalidBoss` 응답 |
