---
name: level-up
description: 특정 콘텐츠를 현재 L-레벨에서 목표 L-레벨로 올리기 위한 구체적인 작업 목록을 생성한다. 사용법: /level-up <콘텐츠명> <현재레벨> <목표레벨> [server|client|both]
argument-hint: <콘텐츠명> <현재레벨> <목표레벨> [server|client|both]
---

# 콘텐츠 레벨업 작업 목록

대상: $ARGUMENTS

## 레벨 정의 참조
- [Docs/Content_List_And_Roadmap.md](Docs/Content_List_And_Roadmap.md) (2.4절 개발 정도 평가 기준)

## L-레벨 기준
- L0: 미착수 (기획만 존재)
- L1: 프로토타입 (단일 경로 동작, 예외/검증/복구 부족)
- L2: 기능 구현 (핵심 기능 동작, 엣지케이스/운영성 부족)
- L3: 베타 가능 (핵심 루프 + 기본 검증 + 기본 오류 처리)
- L4: 라이브 준비 (운영 로그/회귀 테스트/복구 전략 확보)
- L5: 운영 안정 (성능/관측성/자동화까지 포함)

## 관련 기획 문서
- 가챠: [Docs/Game/Gacha.md](Docs/Game/Gacha.md)
- 왕국: [Docs/Game/Kingdom.md](Docs/Game/Kingdom.md)
- 스테이지: [Docs/Game/MainStage.md](Docs/Game/MainStage.md)
- 공방 대기열: [Docs/Design/AsyncKingdomQueue.md](Docs/Design/AsyncKingdomQueue.md)
- 실시간 레이드: [Docs/Design/RealTimeRaidBoss.md](Docs/Design/RealTimeRaidBoss.md)
- 시즌 랭킹: [Docs/Design/SeasonRankingReplay.md](Docs/Design/SeasonRankingReplay.md)
- 길드: [Docs/Design/GuildSystem.md](Docs/Design/GuildSystem.md)

## 관련 서버 코드
!`find Code/Server/Service -name "*.cs" | head -20`

## 관련 클라이언트 코드
!`find Client/Assets/Scripts -name "*.cs" 2>/dev/null | grep -i "$ARGUMENTS" | head -10`

## 작업 순서

1. `$ARGUMENTS`를 파싱한다: 콘텐츠명, 현재 레벨(Lx), 목표 레벨(Ly), 대상(server/client/both, 기본 both)
2. 콘텐츠 관련 기획 문서와 기존 서버/클라이언트 코드를 확인한다.
3. 현재 레벨에서 목표 레벨까지 올리기 위한 레벨별 체크리스트를 생성한다:
   - 각 레벨 구간(예: L2→L3, L3→L4, L4→L5)마다 달성 조건 명시
   - 서버 작업과 클라이언트 작업을 분리
4. 각 작업에 구체적인 코드 위치(파일명, 클래스명)를 명시한다.
5. 예상 리스크나 의존성(다른 콘텐츠/시스템)을 표시한다.
6. 이번 세션에서 바로 시작할 수 있는 작업을 최상위에 표시한다.

## 출력 형식
```
# [콘텐츠명] Lx → Ly 작업 목록

## 서버 작업
### L2 → L3
- [ ] ...
### L3 → L4
- [ ] ...

## 클라이언트 작업
### L2 → L3
- [ ] ...

## 지금 시작 가능한 작업 (우선 3개)
1. ...
```
