# 실시간 레이드 보스(코옵) 기획서

작성일: 2026-03-08  
연관 시스템: 쿠키 성장, 월드 진행, 길드/파티, 재화 보상

## 1. 목표
- 2~4인 협동 보스전을 서버 권위 모델로 운영한다.
- 틱 기반 전투 시뮬레이션으로 동시성/정합성 문제를 제어한다.
- 재접속 복구, 상태 스냅샷, 장애 복구를 포함한 실서비스형 구조를 만든다.

## 2. 콘텐츠 콘셉트
- 모드명: `월드 균열 레이드`
- 입장 조건:
1. 계정 레벨 10 이상
2. 쿠키 3개 이상 보유
3. 일일 입장권 또는 길드 입장권 소모
- 플레이 흐름:
1. 매칭/파티 구성
2. 레이드 룸 생성
3. 전투 시작(180초 제한)
4. 보스 HP 0 또는 시간 종료
5. 개인/공동 보상 정산

## 3. 서버 아키텍처

## 3.1 룸/인스턴스 관리
- `RaidRoomManager`
- 기능:
1. 룸 생성/파괴
2. 참가자 상태 관리(Ready/InBattle/Disconnected/Reconnected)
3. 룸 타임아웃 처리(전원 이탈 시 60초 후 종료)
- 키:
1. `RaidRoomId`
2. `SeasonId`
3. `BossNum`

## 3.2 틱 기반 시뮬레이션
- Tick Rate: `20Hz` (50ms)
- 서버만 전투 결과를 확정한다.
- 클라이언트는 입력 커맨드만 전송한다.
- 처리 순서(고정):
1. 입력 수집
2. 스킬/쿨타임 검증
3. 상태이상 계산
4. 데미지/회복 적용
5. 보스 AI 패턴 적용
6. 스냅샷 생성

## 3.3 권위 서버 모델
- 클라이언트 제출값은 의도(Intent)로만 사용한다.
- 최종 판정:
1. 데미지
2. 치명타
3. 버프/디버프 적용
4. 사망/부활
- 불일치 방지:
1. 서버 시드 기반 랜덤
2. 스킬 프레임 검증
3. 명령 시퀀스 번호 검증

## 3.4 재접속 복구
- `SessionKey + RaidParticipantToken`으로 복귀한다.
- 복귀 시 전송:
1. 최신 스냅샷 1개
2. 최근 이벤트 로그 N개
3. 남은 전투 시간
- 복귀 제한: 전투 종료 후에는 관전 모드만 허용한다.

## 3.5 상태 스냅샷
- 저장 주기: 1초(20틱)마다
- 저장 항목:
1. 보스 상태(HP, 페이즈, 상태이상, 패턴 쿨다운)
2. 플레이어 상태(HP, 에너지, 스킬 쿨다운, 생존 여부)
3. 룸 타이머, 점수, 누적 데미지
- 사용 목적:
1. 재접속 복구
2. 장애 시 최근 스냅샷 기반 복구
3. 리플레이 검증 기초 데이터

## 4. 전투/보상 규칙

## 4.1 보스 페이즈
- Phase 1: 단일 공격 위주
- Phase 2(HP 60%): 광역 공격 + 디버프
- Phase 3(HP 25%): 광폭화(공격속도 증가)

## 4.2 점수 산정
- 개인 점수:
1. 누적 데미지
2. 생존 시간
3. 팀 보조 기여(힐/버프)
- 팀 점수:
1. 클리어 시간 보너스
2. 무사망 보너스

## 4.3 보상
- 공통 보상: 골드, 성장 재화
- 개인 보상: 레이드 코인, 쿠키 강화 재료
- 시즌 보상 연동: 누적 점수 기반 랭킹 보상

## 5. 데이터 모델(초안)

## 5.1 MySQL
- `RaidRoom`
1. `RoomId`
2. `SeasonId`
3. `BossNum`
4. `State`
5. `CreatedAt`
6. `EndedAt`
- `RaidParticipant`
1. `RoomId`
2. `PlayerId`
3. `Slot`
4. `State`
5. `ReconnectToken`
6. `TotalDamage`
- `RaidResult`
1. `RoomId`
2. `PlayerId`
3. `Score`
4. `RewardState`
5. `CreatedAt`
- `RaidSnapshot`
1. `RoomId`
2. `Tick`
3. `SnapshotJson` (압축)
4. `CreatedAt`

## 5.2 Redis
- `raid:room:{roomId}:state` (현재 전투 상태 캐시)
- `raid:room:{roomId}:lock` (룸 처리 락)
- `raid:player:{playerId}:room` (플레이어-룸 역인덱스)

## 6. API/RPC 초안
- `raid/match/start`
- `raid/room/enter`
- `raid/command/input`
- `raid/room/reconnect`
- `raid/room/snapshot`
- `raid/room/result`
- `raid/reward/claim`

## 7. 장애/운영 설계
- 룸 워커 다운 시:
1. Redis 상태 조회
2. 최신 스냅샷 로드
3. 룸 재구동 또는 안전 종료
- 멱등 처리:
1. `RewardClaimIdempotencyKey`
2. 결과 테이블의 Unique Key로 중복 보상 차단
- 주요 지표:
1. Tick 지연
2. 룸 생성/종료 수
3. 재접속 성공률
4. 보상 중복 차단 횟수

## 8. 개발 단계
1. P1: 1인 보스전(틱/스냅샷/보상)
2. P2: 2~4인 룸 확장 + 재접속
3. P3: 시즌 연동 + 리플레이 검증 연계

