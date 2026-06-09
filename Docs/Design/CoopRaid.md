# 협동 레이드 기획서

작성일: 2026-06-09

---

## 1. 개요

2~4명이 실시간으로 연결된 아레나에서 협동으로 보스를 토벌하는 콘텐츠.  
방 입장 후 대기 중에는 자유롭게 이동하고, 전원 준비 완료 시 턴제 전투로 전환한다.

---

## 2. 서버 분리 아키텍처

### 2.1 별도 서버로 분리하는 이유

기존 게임 서버(이하 ApiServer)는 Stateless 구조다.  
요청이 들어올 때만 상태를 DB/Cache에서 읽고 처리한 뒤 응답하므로 인스턴스를 수평 확장할 수 있다.

레이드 서버(이하 RaidServer)는 Stateful 구조다.  
방 상태, 플레이어 위치, 전투 진행 상태, WebSocket 연결 객체가 서버 메모리에 상주한다.  
인스턴스를 여러 개 띄우면 같은 방의 플레이어들이 다른 인스턴스에 연결될 수 있어  
별도 외부 상태 저장소 없이는 수평 확장이 불가하다.  
따라서 RaidServer는 별도 프로세스/컨테이너로 분리해 단독 운영한다.

```
[Client]
  ├─ HTTP  →  ApiServer   (Stateless, 수평 확장 가능)
  └─ WS    →  RaidServer  (Stateful, 단일 인스턴스 or 외부 상태 저장 시 확장)
```

### 2.2 프로젝트 구조

```
Code/
├─ Server/          기존 ApiServer (HTTP REST)
├─ RaidServer/      신규 실시간 레이드 서버      ← 새로 생성
├─ Protocol/        공용 패킷 정의 (양쪽 공유)
├─ ServerFramework/ 공용 인프라 (DB, Cache 등, 양쪽 공유)
└─ Proto/           기획 데이터 (양쪽 공유)
```

### 2.3 서버 간 통신

RaidServer는 다음 두 가지 목적으로 ApiServer 또는 공유 인프라에 접근한다.

| 목적 | 방식 |
|------|------|
| 세션 검증 | Redis에서 직접 세션 조회 (ApiServer와 같은 Redis 공유) |
| 보상 지급 | RaidServer가 DB에 직접 Write (공유 DB 사용) |
| 쿠키/스킬 데이터 로드 | 공유 Proto CSV 로드 |

ApiServer를 HTTP로 호출하는 방식은 RaidServer에 ApiServer 의존성이 생기므로 사용하지 않는다.  
대신 DB와 Redis를 공유 인프라로 직접 접근한다.

### 2.4 클라이언트 연결 흐름

```
1. 클라이언트가 ApiServer에 로그인 → SessionKey 발급
2. 레이드 입장 요청 (ApiServer HTTP) → 입장권 소모, RaidServer 주소 + 방 코드 반환
3. 클라이언트가 RaidServer에 WebSocket 연결
   ws://raidserver/ws/raid?sessionkey={key}&roomCode={code}
4. RaidServer가 SessionKey를 Redis에서 검증 → PlayerId, 쿠키 데이터 로드
5. 방 입장 처리
```

---

## 3. 전체 흐름

```
방 생성/입장
    ↓
[대기 페이즈]
- 자유 이동
- 채팅
- Ready 토글
    ↓
전원 Ready → 3초 카운트다운
    ↓
[전투 페이즈]
- 턴제 전투
- 보스 AI
    ↓
승리(보스 HP 0) or 전멸
    ↓
결과 정산 및 보상
```

---

## 4. 방 시스템

### 3.1 방 생성 / 입장

- 방장이 보스 종류를 선택해 방을 생성하면 4자리 숫자 코드가 발급된다.
- 다른 플레이어는 코드 입력으로 입장한다.
- 최대 인원: 4명. 전투 시작 후 추가 입장 불가.

### 3.2 방 상태 (State Machine)

```
WAITING → COUNTDOWN → IN_BATTLE → RESULT → (종료)
```

| 상태 | 진입 조건 | 허용 액션 |
|------|-----------|-----------|
| WAITING | 방 생성 | 이동, 채팅, Ready 토글, 퇴장 |
| COUNTDOWN | 전원 Ready | 없음 (3초 대기) |
| IN_BATTLE | 카운트다운 완료 | 턴 행동 |
| RESULT | 보스 HP=0 or 전멸 | 퇴장 |

### 3.3 접속 끊김 처리

| 상황 | 처리 |
|------|------|
| 대기 중 접속 끊김 | 방에서 제거, 다른 인원에게 알림 |
| 전투 중 접속 끊김 | 해당 플레이어 턴 자동 스킵, 방에 잔류 |
| 전투 중 재접속 | 현재 게임 상태 스냅샷 전송 후 복귀 |
| 방장 퇴장 | 다음 입장 순서 플레이어가 방장 승계 |

---

## 5. 대기 페이즈

### 4.1 맵

- 크기: 20×15 타일 그리드
- 타일 종류

| 타입 | 설명 |
|------|------|
| FLOOR | 이동 가능 |
| WALL | 이동 불가 |
| PLAYER_SPAWN | 플레이어 초기 위치 (4개 고정) |
| BOSS_SPAWN | 맵 중앙, 전투 시작 시 보스 등장 위치 |

### 4.2 이동

- 4방향 입력 (UP / DOWN / LEFT / RIGHT)
- 이동 단위: 타일 1칸
- 서버가 목표 타일의 WALL 여부, 맵 경계를 검증한다.
- 검증 통과 시 위치를 갱신하고 방 전체에 브로드캐스트한다.
- 검증 실패 시 이동 거부, 요청 플레이어에게 현재 위치 재전송.

### 4.3 Ready 처리

- 플레이어가 READY 메시지를 보내면 본인의 ready 상태를 토글한다.
- 상태 변경 시 방 전체에 현재 ready 현황을 브로드캐스트한다.
- 전원 ready가 되면 서버가 COUNTDOWN 상태로 전환한다.
- Ready 상태에서 이동 입력 수신 시 ready 자동 해제.

---

## 6. 페이즈 전환 (대기 → 전투)

1. 전원 Ready 확인
2. 서버가 방 상태를 COUNTDOWN으로 변경
3. 3초 카운트다운 브로드캐스트 (1초 간격)
4. 카운트다운 종료 시:
   - 플레이어 위치 고정 (이동 입력 무시)
   - 보스 BOSS_SPAWN 타일에 등장
   - 턴 순서 계산 (플레이어 Speed 스탯 내림차순 정렬, 동점이면 입장 순서)
   - 방 상태 IN_BATTLE 전환
   - 첫 번째 플레이어 턴 시작 브로드캐스트

---

## 7. 전투 페이즈

### 6.1 턴 구조

한 라운드 = 모든 플레이어 턴 + 보스 턴 1회

```
[라운드 N 시작]
  플레이어1 턴 (30초 타임아웃)
  플레이어2 턴 (30초 타임아웃)
  ...
  보스 턴 (자동)
[라운드 N 종료, 종료 조건 체크]
[라운드 N+1 시작 or 게임 종료]
```

### 6.2 플레이어 행동

턴이 된 플레이어는 다음 중 하나를 선택한다.

| 행동 | 설명 |
|------|------|
| ATTACK | 기본 공격. 보스에게 ATK 기반 데미지 |
| SKILL | 쿠키 스킬 사용. 스킬별 효과 적용, 쿨다운 소모 |
| DEFEND | 다음 보스 공격에 DEF +50% 임시 증가 |
| ITEM | 소지 아이템 사용 (HP 회복 등) |

- 타임아웃(30초) 초과 시 자동으로 ATTACK 처리.
- 접속 끊김 상태의 플레이어 턴은 즉시 자동 ATTACK 처리.

### 6.3 스킬 시스템

- 쿠키별 스킬 2종 보유 (기존 `CookieSkill` 데이터 활용).
- 각 스킬은 쿨다운(라운드 수)을 가진다.
- 스킬 종류

| 타입 | 설명 |
|------|------|
| SINGLE_DAMAGE | 단일 대상 데미지 |
| ALL_DAMAGE | 전체 대상 데미지 (멀티 보스 대비) |
| HEAL | 아군 단일 or 전체 HP 회복 |
| BUFF | 아군 스탯 임시 증가 (1~2라운드) |
| DEBUFF | 보스 스탯 임시 감소 |

### 6.4 데미지 계산

```
기본 데미지 = 공격자 ATK × 스킬 배율 - 방어자 DEF
최소 데미지 = 1
크리티컬 여부 = Random(0~1) < 공격자 CRIT_RATE
최종 데미지 = 크리티컬 시 기본 데미지 × 1.5
```

- DEFEND 상태인 플레이어의 DEF는 1.5배 적용.
- 서버에서만 계산하며, 클라이언트는 결과를 수신한다.

### 6.5 보스 시스템

#### 보스 스탯 (예시: 1단계 보스)

| 스탯 | 값 |
|------|----|
| MAX_HP | 10000 |
| ATK | 150 |
| DEF | 30 |
| SPEED | 50 |
| PHASE2_HP | 5000 (MAX_HP의 50%) |

#### 보스 AI 상태머신

```
NORMAL → PHASE2 (HP ≤ PHASE2_HP 진입)
```

#### 보스 행동 선택 (턴마다 우선순위 순으로 평가)

| 조건 | 행동 |
|------|------|
| 특수기 쿨다운 = 0 | 특수기 사용 후 쿨다운 재설정 |
| PHASE2이고 연속 행동 카운터 = 1 | 추가 1회 행동 (PHASE2 전용) |
| 그 외 | 어그로 1위 플레이어 기본 공격 |

#### 보스 스킬 목록 (예시)

| 스킬명 | 효과 | 쿨다운 |
|--------|------|--------|
| 전체 강타 | 전체 플레이어에게 ATK × 0.8 데미지 | 4라운드 |
| 독 안개 | 전체 플레이어에게 매 라운드 HP 5% 감소 (3라운드) | 6라운드 |
| 분노 (PHASE2) | 본인 ATK +30% (2라운드), 다음 턴 추가 행동 | 5라운드 |

#### 예고 시스템

- 다음 턴에 전체 공격 또는 범위 스킬 사용 예정이면, 현재 턴 종료 시 `BOSS_WARNING` 메시지를 전송한다.
- 클라이언트는 이를 받아 예고 연출을 표시한다.

#### 어그로

- 누적 딜량 기반으로 어그로 순위 관리.
- 어그로 1위 플레이어가 기본 공격 대상.
- 힐/버프도 어그로 수치에 일부 반영.

### 6.6 상태이상

| 이상 | 효과 | 중첩 |
|------|------|------|
| 독 | 매 라운드 시작 시 최대 HP의 일정 % 데미지 | 가능 (수치 누적) |
| 방어 감소 | DEF 일정 % 감소 | 불가 (재적용 시 갱신) |
| 기절 | 해당 턴 행동 불가 | 불가 |

### 6.7 종료 조건

| 조건 | 결과 |
|------|------|
| 보스 HP = 0 | 승리 |
| 전체 플레이어 HP = 0 (전멸) | 패배 |

---

## 8. 결과 정산

### 7.1 표시 항목

- 승패 여부
- 총 소요 라운드
- 플레이어별 총 딜량 / 총 힐량 / 사망 횟수

### 7.2 보상

- 승리 시에만 보상 지급.
- 기본 보상: 고정 아이템 (보스 종류별 정의).
- 딜량 1위 추가 보상.
- 보상은 기존 아이템/포인트 시스템을 통해 지급.

---

## 9. 서버-클라이언트 프로토콜

### 8.1 기본 형식

```json
{ "type": "TYPE_NAME", "payload": { ... } }
```

### 8.2 공통 — 방 관리

| 방향 | type | payload |
|------|------|---------|
| C→S | `ROOM_CREATE` | `{ bossNum }` |
| C→S | `ROOM_JOIN` | `{ roomCode }` |
| C→S | `ROOM_LEAVE` | - |
| S→C | `ROOM_JOINED` | `{ roomCode, playerList, mapData }` |
| S→C | `ROOM_PLAYER_ENTER` | `{ player }` |
| S→C | `ROOM_PLAYER_LEAVE` | `{ playerId }` |
| S→C | `ROOM_ERROR` | `{ errorCode, message }` |

### 8.3 대기 페이즈

| 방향 | type | payload |
|------|------|---------|
| C→S | `MOVE` | `{ dir: UP\|DOWN\|LEFT\|RIGHT }` |
| C→S | `READY` | - |
| C→S | `CHAT` | `{ message }` |
| S→C | `PLAYER_MOVED` | `{ playerId, x, y }` |
| S→C | `PLAYER_MOVE_DENIED` | `{ x, y }` (현재 위치 재전송) |
| S→C | `PLAYER_READY` | `{ playerId, isReady }` |
| S→C | `CHAT_MESSAGE` | `{ playerId, playerName, message }` |
| S→C | `COUNTDOWN` | `{ count }` |

### 8.4 전투 페이즈

| 방향 | type | payload |
|------|------|---------|
| C→S | `TURN_ACTION` | `{ actionType, skillId?, targetId? }` |
| S→C | `BATTLE_START` | `{ turnOrder, bossData, playerDataList }` |
| S→C | `TURN_START` | `{ playerId, timeoutSec }` |
| S→C | `TURN_RESULT` | `{ actorId, actionType, targetId, damage, heal, effects, hpSnapshot }` |
| S→C | `BOSS_WARNING` | `{ skillId, targetType }` |
| S→C | `BOSS_TURN` | `{ actionType, skillId, targetId, results[] }` |
| S→C | `STATUS_EFFECT` | `{ targetId, effect, value, remainRound }` |
| S→C | `PLAYER_DEAD` | `{ playerId }` |
| S→C | `BOSS_PHASE2` | - |
| S→C | `GAME_END` | `{ result: WIN\|LOSE, stats[], rewards[] }` |

### 8.5 hpSnapshot 구조 (TURN_RESULT, BOSS_TURN 포함)

```json
{
  "bossHp": 7500,
  "players": [
    { "playerId": "...", "hp": 800 },
    ...
  ]
}
```
매 행동 후 전체 HP 상태를 함께 전송해 클라이언트가 동기화를 유지한다.

---

## 10. RaidServer 내부 구조

### 10.1 컴포넌트

```
WebSocketHandler          - 연결 수립, 메시지 수신/송신, 연결 해제 감지
RoomManager               - 방 목록 관리 (메모리 상주), 생성/입장/제거
  └─ RaidRoom             - 방 단위 상태머신
       ├─ WaitingPhase    - 위치 관리, Ready 관리, 채팅
       └─ BattlePhase     - 턴 관리, 보스 AI, 전투 계산
BossAI                    - 보스 행동 결정 로직
SessionValidator          - Redis에서 세션 직접 조회 (ApiServer와 Redis 공유)
RewardService             - 전투 종료 후 DB에 직접 보상 Write
```

### 10.2 연결 흐름

1. 클라이언트가 WebSocket 연결
   `ws://raidserver/ws/raid?sessionkey={key}&roomCode={code}`
2. `SessionValidator`가 공유 Redis에서 세션키 검증, PlayerId 로드
3. DB에서 해당 플레이어의 쿠키 데이터 로드
4. 검증 성공 시 WebSocket 연결 유지, 방 입장 처리

### 10.3 재접속

1. 클라이언트가 동일 세션키로 재연결
2. `RoomManager`에서 해당 PlayerId가 속한 방 확인
3. 방이 IN_BATTLE 상태이면 현재 게임 전체 스냅샷 전송
4. 플레이어 상태를 RECONNECTED로 변경, 이후 턴부터 정상 참여

### 10.4 스케일 전략

현재 설계는 단일 인스턴스를 전제로 한다.  
수평 확장이 필요한 경우, 방 상태 전체를 Redis에 직렬화해 외부로 이전하고  
클라이언트를 동일 방의 인스턴스로 라우팅하는 Sticky Session 또는 Gateway가 필요하다.  
이는 별도 과제로 남긴다.

---

## 11. 보스 프로토 데이터 구조 (CSV 기반)

기존 `ProtoSystem`에 보스 데이터를 추가한다.

| 필드 | 설명 |
|------|------|
| `Num` | 보스 식별 번호 |
| `Name` | 보스 이름 |
| `MaxHp` | 최대 HP |
| `Atk` | 공격력 |
| `Def` | 방어력 |
| `Speed` | 턴 순서 결정용 |
| `Phase2HpRate` | 페이즈2 전환 HP 비율 (0~1) |
| `SkillNumList` | 사용 스킬 번호 목록 |
| `RewardItemNum` | 클리어 보상 아이템 번호 |
| `RewardItemAmount` | 클리어 보상 수량 |

보스 스킬 테이블:

| 필드 | 설명 |
|------|------|
| `Num` | 스킬 번호 |
| `Type` | SINGLE / ALL / DOT / BUFF / DEBUFF |
| `DamageRate` | ATK 배율 |
| `EffectValue` | 상태이상 수치 |
| `EffectRound` | 상태이상 지속 라운드 |
| `Cooldown` | 재사용 대기 라운드 |
| `IsWarning` | 사전 예고 여부 |

---

## 12. 예외 / 엣지 케이스

| 상황 | 처리 |
|------|------|
| 카운트다운 중 Ready 해제 | COUNTDOWN 취소, WAITING 복귀 |
| 전투 중 전체 접속 끊김 | 일정 시간(60초) 대기 후 방 자동 종료 |
| 전투 중 1명만 남음 | 계속 진행 (1인 클리어 가능) |
| 중복 TURN_ACTION 수신 | 첫 번째만 처리, 이후 무시 |
| 보스 HP 음수 | 0으로 클램핑 후 즉시 종료 처리 |
