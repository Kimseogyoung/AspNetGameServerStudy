# 길드 시스템 기획서

작성일: 2026-03-08  
연관 시스템: 킹덤 생산, 레이드, 시즌 랭킹, 쿠키 성장

## 1. 목표
- 다중 사용자 협업 도메인을 안정적으로 운영한다.
- 길드 가입/기여/버프/길드 레이드를 제공한다.
- 분산락, 트랜잭션 경계, 이벤트 알림, 감사 로그를 포함한다.

## 2. 콘텐츠 콘셉트
- 길드 규모: 최대 30명
- 핵심 루프:
1. 가입/활동
2. 길드 기여(재화/생산품/레이드 포인트)
3. 길드 버프 활성화
4. 길드 레이드 참여
5. 시즌 종료 보상

## 3. 도메인 설계

## 3.1 길드 엔티티
- `Guild`
1. `GuildId`
2. `Name`
3. `Level`
4. `Exp`
5. `Notice`
6. `State`
- `GuildMember`
1. `GuildId`
2. `PlayerId`
3. `Role` (Master/Officer/Member)
4. `JoinAt`
5. `Contribution`
6. `LastActiveAt`

## 3.2 권한 모델
- Master:
1. 공지 수정
2. 멤버 추방
3. 임원 임명
- Officer:
1. 가입 승인
2. 공지 보조 수정
- Member:
1. 기여
2. 길드 콘텐츠 참여

## 3.3 가입 방식
- 공개/승인제 선택
- 가입 요청 만료 시간(예: 24시간)
- 탈퇴/재가입 쿨다운(예: 12시간)

## 4. 길드 기여/버프

## 4.1 기여 소스
- 킹덤 생산품 기부
- 레이드 참여 포인트
- 일일 미션 완료

## 4.2 버프 종류
- 건설 시간 감소
- 제작 완료량 증가
- 레이드 공격력 증가
- 버프 발동 비용은 길드 재화에서 차감

## 4.3 버프 운영 규칙
- 버프 슬롯 제한
- 중복 버프 제한
- 활성화/종료 이벤트 로그 기록

## 5. 길드 레이드
- 주간 보스전으로 운영
- 길드 누적 데미지/개인 데미지 집계
- 보상:
1. 개인 참여 보상
2. 길드 등급 보상
3. 시즌 길드 랭킹 보상

## 6. 동시성/정합성 설계

## 6.1 분산락
- 대상:
1. 가입/탈퇴
2. 기여 반영
3. 버프 활성화
4. 길드 레이드 점수 갱신
- 키 예시:
1. `lock:guild:{guildId}`
2. `lock:guild:member:{playerId}`

## 6.2 트랜잭션 경계
- 원자 처리 단위:
1. 개인 자원 차감 + 길드 기여 증가
2. 길드 버프 비용 차감 + 버프 상태 활성화
3. 레이드 점수 반영 + 보상 상태 갱신
- 교차 도메인 처리:
1. 실패 시 보상/차감 롤백
2. 재시도 시 멱등 키 적용

## 6.3 멱등 처리
- 주요 요청:
1. 기여
2. 보상 수령
3. 가입 승인/거절
- `RequestId` 기반 중복 방지

## 7. 이벤트/알림 설계
- 이벤트 타입:
1. `GuildMemberJoined`
2. `GuildMemberLeft`
3. `GuildContributionAdded`
4. `GuildBuffActivated`
5. `GuildRaidRewardClaimed`
- 전달 채널:
1. 인게임 우편
2. 실시간 알림(접속자)
3. 로그 테이블(오프라인 확인)

## 8. 감사 로그(Audit)
- 로그 대상:
1. 멤버 강퇴/권한 변경
2. 길드 재화 증감
3. 버프 활성화/취소
4. 보상 지급/회수
- 필드:
1. `ActorPlayerId`
2. `ActionType`
3. `BeforeValue`
4. `AfterValue`
5. `RequestId`
6. `CreatedAt`

## 9. API/RPC 초안
- `guild/create`
- `guild/search`
- `guild/apply`
- `guild/apply/approve`
- `guild/leave`
- `guild/member/kick`
- `guild/contribution/add`
- `guild/buff/activate`
- `guild/raid/enter`
- `guild/raid/result`
- `guild/reward/claim`

## 10. 운영 지표
- 길드 생성 수/활성 길드 수
- 평균 길드 인원
- 일일 기여량
- 버프 사용률
- 길드 레이드 참여율
- 분산락 충돌률

## 11. 개발 단계
1. P1: 길드 생성/가입/탈퇴/권한
2. P2: 기여/버프/로그
3. P3: 길드 레이드 + 시즌 보상 + 알림 고도화

