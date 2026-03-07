# 시즌 랭킹 + 리플레이 검증 기획서

작성일: 2026-03-08  
연관 시스템: 월드 스테이지, 레이드, 쿠키 성장, 시즌 보상

## 1. 목표
- 시즌 단위 경쟁 구조를 제공한다.
- 랭킹 집계와 리플레이 검증을 분리해 공정성을 높인다.
- 데이터 정합성/성능/보안 요구를 동시에 만족한다.

## 2. 시즌 콘셉트
- 시즌 길이: 14일
- 대상 모드:
1. 월드 챌린지 점수
2. 레이드 누적 점수
- 점수 정책:
1. 스테이지 클리어 점수
2. 클리어 시간 보너스
3. 무피해/특수 조건 보너스

## 3. 집계 파이프라인

## 3.1 이벤트 수집
- 이벤트 타입:
1. `StageClearEvent`
2. `RaidResultEvent`
- 필수 필드:
1. `SeasonId`
2. `PlayerId`
3. `Mode`
4. `Score`
5. `ReplayRef`
6. `OccurredAt`

## 3.2 검증 단계
- 1차: 포맷/범위 검증
- 2차: 서버 재계산(핵심 수치)
- 3차: 이상치 탐지(비정상 고득점, 프레임 불일치)

## 3.3 랭킹 반영
- 임시 반영 상태:
1. `PendingValidation`
2. `Validated`
3. `Rejected`
- 정책:
1. 검증 완료 후 최종 점수 반영
2. 기존 최고점보다 높은 경우에만 갱신

## 4. 저장소 설계

## 4.1 Redis Sorted Set
- 키:
1. `season:{seasonId}:rank:global:{mode}`
2. `season:{seasonId}:rank:shard:{shardId}:{mode}`
- 멤버: `PlayerId`
- 스코어: `BestScore`
- 용도:
1. Top N 조회
2. 내 순위 조회
3. 실시간 랭킹 페이지

## 4.2 MySQL
- `Season`
1. `SeasonId`
2. `StartAt`
3. `EndAt`
4. `State`
- `SeasonBestScore`
1. `SeasonId`
2. `PlayerId`
3. `Mode`
4. `BestScore`
5. `ReplayId`
6. `UpdatedAt`
- `SeasonRankSnapshot`
1. `SeasonId`
2. `Mode`
3. `Rank`
4. `PlayerId`
5. `Score`
6. `RewardState`
- `ReplayMeta`
1. `ReplayId`
2. `PlayerId`
3. `Mode`
4. `InputHash`
5. `ServerSeed`
6. `ValidationState`

## 5. 리플레이 검증

## 5.1 제출 방식
- 클라이언트는 전체 전투 결과 대신 입력 로그와 요약 정보만 제출한다.
- 필드:
1. 입력 시퀀스
2. 프레임 타임라인
3. 최종 결과 요약
4. 해시값

## 5.2 서버 재계산
- 동일 시드/규칙으로 핵심 전투를 재시뮬레이션한다.
- 비교 항목:
1. 총 점수
2. 클리어 시간
3. 주요 이벤트 프레임
- 허용 오차 초과 시 `Rejected`

## 5.3 부정행위 대응
- 플래그:
1. 비정상 입력 빈도
2. 비현실적 반응속도
3. 해시 불일치
- 처리:
1. 점수 보류
2. 운영 검토 큐 적재
3. 반복 시 제한 조치

## 6. 시즌 스냅샷/보상
- 시즌 종료 시:
1. Top N 스냅샷 고정
2. 보상 테이블 생성
3. 다음 시즌 초기화
- 보상 수령:
1. 멱등 키 사용
2. 중복 수령 방지 Unique Key

## 7. API/RPC 초안
- `season/info`
- `season/score/submit`
- `season/rank/top`
- `season/rank/me`
- `season/replay/upload`
- `season/reward/claim`

## 8. 운영 지표
- 검증 대기 시간
- 검증 실패율
- 랭킹 갱신 처리량
- 의심 이벤트 비율
- 보상 중복 차단 횟수

## 9. 개발 단계
1. P1: Redis 랭킹 + 기본 점수 제출
2. P2: ReplayMeta 저장 + 서버 재계산
3. P3: 시즌 스냅샷/보상 자동화 + 이상치 탐지 강화

