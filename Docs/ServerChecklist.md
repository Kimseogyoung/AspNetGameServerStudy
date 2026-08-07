# 게임 웹서버 구현 체크리스트

> 게임 웹서버 필수 구성요소 기준표. 프로젝트마다 구현 컬럼을 채워 사용.
> 마지막 갱신: 2026-06-09

**전체 진행률: 33 / 85 (39%)**

| 카테고리 | 완료 | 전체 | 진행률 |
|----------|:----:|:----:|:------:|
| 요청 파이프라인 | 5 | 18 | 28% |
| 인증/보안 | 5 | 11 | 45% |
| 에러 처리 | 3 | 5 | 60% |
| 로깅/모니터링 | 4 | 10 | 40% |
| 데이터 계층 | 6 | 13 | 46% |
| 설정 관리 | 3 | 4 | 75% |
| 게임 특화 | 3 | 6 | 50% |
| 실시간 통신 | 0 | 2 | 0% |
| 서버 아키텍처 | 0 | 2 | 0% |
| 테스트 | 2 | 3 | 67% |
| 배포 | 0 | 4 | 0% |
| 운영 | 2 | 6 | 33% |
| 개발 도구 | 0 | 1 | 0% |

---

| 카테고리 | 분류 | 항목 | 구현 (이 프로젝트) | 상태 |
|----------|------|------|--------------------|:----:|
| 요청 파이프라인 | 라우팅/직렬화 | API 라우팅 방식 | Custom RpcMethod (`/rpc/{name}` POST) | O |
| 요청 파이프라인 | 라우팅/직렬화 | 직렬화 포맷 (다중 포맷 유연 지원) | ProtoBuf 기본, JSON fallback (테스트용) | O |
| 요청 파이프라인 | 라우팅/직렬화 | HTTPS/TLS 설정 (암호화 전송, 인증서 관리) | `UseHttpsRedirection()` 적용 | O |
| 요청 파이프라인 | 라우팅/직렬화 | 요청 크기 제한 | | X |
| 요청 파이프라인 | 라우팅/직렬화 | 타임아웃 설정 | | X |
| 요청 파이프라인 | 미들웨어/필터 | 요청 컨텍스트 관리 | `ReqMiddleware` → `RpcContext.Init()` | O |
| 요청 파이프라인 | 미들웨어/필터 | 유저별 동시 요청 직렬화 | `UserLockService` (MySQL/InMemory Lock) | O |
| 요청 파이프라인 | 미들웨어/필터 | Graceful Shutdown (진행 중인 요청 완료 후 종료, 배포 중 데이터 유실 방지) | | X |
| 요청 파이프라인 | 미들웨어/필터 | Rate Limiting (단위 시간 내 요청 횟수 제한, 어뷰징·과부하 방지) | | X |
| 요청 파이프라인 | 미들웨어/필터 | 점검 모드 (배포·마이그레이션 중 전체 API 차단, 점검 에러 응답) | | X |
| 요청 파이프라인 | 미들웨어/필터 | 점검 중 접속 Whitelist (특정 IP·계정은 점검 중에도 접속 허용, QA·개발자 사전 검증용) | | X |
| 요청 파이프라인 | 요청 검증 | 중복/재요청 처리 (Seq 기반 중복 차단 + 재요청 시 이전 응답 캐싱) | Seq 파싱만 함, 검증·응답 캐싱 없음 | X |
| 요청 파이프라인 | 요청 검증 | Replay Attack 방지 (만료된 요청 재전송 차단, Timestamp ±N초 검증) | Timestamp 파싱만 함, 검증 없음 | X |
| 요청 파이프라인 | 요청 검증 | 요청 서명 검증 (HMAC, 패킷 변조 방지) | ApiHash 계산만 함, 클라이언트 값과 비교 없음 | X |
| 요청 파이프라인 | 요청 검증 | 클라이언트/API 버전 관리 (강제/선택 업데이트, 구버전 접속 차단, API 하위 호환 유지) | | X |
| 요청 파이프라인 | 최적화 | 요청 배치 처리 (여러 API를 단일 요청으로 묶어 전송, 클라이언트 왕복 절감) | | X |
| 요청 파이프라인 | 최적화 | 요청 압축 (gzip 등, 패킷 크기 절감) | | X |
| 요청 파이프라인 | 최적화 | 네트워크 재시도 처리 (타임아웃/단절 시 자동 재시도, 응답 캐싱과 연동해 중복 처리 방지) | | X |
| 인증/보안 | 세션 | 세션 생명주기 관리 (발급 / 인증 / 만료 / 자동 연장 / 만료 유예) | `SessionManager` (Sliding Expiration + GracePeriod) | O |
| 인증/보안 | 세션 | 자동 로그인 지원 (기기 식별자 or 토큰 기반, 모바일 게임 표준) | DeviceKey(IDFV) → `SignUp` | O |
| 인증/보안 | 세션 | 중복 로그인 처리 (다기기 동시 접속 정책 선택 필요 — ① 후입 우선: 새 로그인 시 기존 세션 만료 ② 선입 우선: 기존 로그인 유지·신규 차단 ③ 다중 허용: 기기별 독립 세션 ④ 타입별 제한: 모바일/PC 각 1개) | 후입 우선 — SignUp 시 기존 세션 만료 | O |
| 인증/보안 | 계정/채널 | Guest 계정 | `EChannelType.GUEST` 채널 생성 | O |
| 인증/보안 | 계정/채널 | 소셜 로그인 연동 (Google, Apple, Kakao 등) | | X |
| 인증/보안 | 계정/채널 | 계정 연결/이전 (Guest → 소셜 계정 머지) | | X |
| 인증/보안 | 계정/채널 | 계정 탈퇴 / 개인정보 삭제 (개인정보보호법·GDPR 대응, 탈퇴 시 데이터 파기 절차) | | X |
| 인증/보안 | API 권한 | API 레벨별 권한 분리 (비인증 / 인증 / 플레이어 필요) | `ERpcMethodType` (NONE / AUTHORIZED / AUTHORIZED_PLAYER) | O |
| 인증/보안 | API 권한 | 운영/Admin API 권한 관리 | `ERpcMethodType.OPS` 정의됨, 인증 없이 통과 | X |
| 인증/보안 | API 권한 | Admin IP Whitelist (OPS API를 허용된 IP에서만 접근 가능, 인증과 별개의 네트워크 레벨 차단) | | X |
| 인증/보안 | 시크릿 관리 | 민감정보 분리 관리 (DB 비밀번호 등, Vault·Secrets Manager 별도 저장소) | appsettings.yaml에 DB 비밀번호 평문 노출 | X |
| 에러 처리 | 에러 정의 | 에러코드 표준화 | `EErrorCode` enum | O |
| 에러 처리 | 에러 정의 | 클라이언트-서버 에러 처리 프로토콜 (에러 응답 구조 + 커스텀 예외 클래스) | `GameException`, `CancelReqException`, `UserLockException` | O |
| 에러 처리 | 에러 처리 | 전역 예외 핸들러 (미처리 예외가 서버를 다운시키지 않도록 에러 응답 변환) | `ErrorHandler` (예외 종류별 분기) | O |
| 에러 처리 | 에러 처리 | 환경별 에러 상세 노출 제어 (라이브: Hash만 노출, 개발: 상세 노출) | `IsShowErrorDetail` 설정·`ErrorHash` 존재하나 ErrorHandler에서 분기 미구현 | X |
| 에러 처리 | 에러 처리 | 외부 에러 리포팅 (Sentry 등, 실시간 Stack Trace 수집·알림) | ErrorHandler에 TODO 존재, 미연동 | X |
| 로깅/모니터링 | 로깅 | 파일 로그 | NLog (파일 + 콘솔) | O |
| 로깅/모니터링 | 로깅 | 원격 저장소 로그 (Fluent Bit 등으로 S3/Elasticsearch 전달·저장) | | X |
| 로깅/모니터링 | 로깅 | 로그 구조화 | 파라미터 바인딩 방식 (`{Key}`) | O |
| 로깅/모니터링 | 로깅 | 요청/응답 로그 (Body 내용 포함) | `RpcService`가 RPC 요청/응답 직접 로깅 (Method, Path, Body). MVC 경로용이던 `LogFilter`는 DI 등록만 되고 실제 미적용 상태라 삭제함 | O |
| 로깅/모니터링 | 로깅 | 요청 처리 시간 로그 | | X |
| 로깅/모니터링 | 로깅 | 슬로우 쿼리 로그 | | X |
| 로깅/모니터링 | 로깅 | 로그 수집/검색 인프라 (Athena, Elasticsearch 등) | | X |
| 로깅/모니터링 | 감사 로그 | 중요 행동 감사 로그 (결제·아이템 획득/사용 등 별도 기록, 이슈 추적·어뷰징 분석용) | `CashChangeLogModel`, `GachaLogModel` 부분 구현 | O |
| 로깅/모니터링 | 메트릭/알림 | 메트릭 수집 (Prometheus, API 응답시간·에러율·처리량) | | X |
| 로깅/모니터링 | 메트릭/알림 | 장애 알림 (Slack webhook, PagerDuty 등) | | X |
| 데이터 계층 | DB | 테스트용 DB | InMemoryDB | O |
| 데이터 계층 | DB | DB 도메인 분리 | Auth / User / Center DB 분리 | O |
| 데이터 계층 | DB | DB 수평 분산 (Hash/Range 샤딩, 샤딩 키 설계 필요) | ShardId 기반 UserDb 분산 | O |
| 데이터 계층 | DB | DB Read Replica (운영·통계 쿼리 전용, 게임 서버 메인 DB 부하 분리 목적) | | X |
| 데이터 계층 | DB | 시작 시 연결 검증 | `ConnectionTest()` | O |
| 데이터 계층 | DB | 트랜잭션 관리 | ReadCommitted 사용, Commit 흐름 일부 미정리 | X |
| 데이터 계층 | DB | 개발 환경 Schema Migration | Liquibase 폴더 있으나 미구성 | X |
| 데이터 계층 | DB | 라이브 환경 Schema Migration (무중단, 롤백 절차 포함) | | X |
| 데이터 계층 | DB | 커넥션 풀 설정 | | X |
| 데이터 계층 | DB | 데이터 백업/복구 (정기 백업 전략 + 장애 시 복구 절차) | | X |
| 데이터 계층 | Cache | 캐시 (DB 부하 분산, 자주 읽히는 데이터 메모리 저장) | `RedisCompositeCacheLayer` (Redis + InMemory) | O |
| 데이터 계층 | Cache | 캐시 장애 Fallback (Circuit Breaker, Redis 다운 시 InMemory로 전환) | | X |
| 데이터 계층 | ID 생성 | 분산 환경 고유 ID 생성 | Snowflake ID (IdGen, WorkerId = ServerNum) | O |
| 설정 관리 | 설정 파일 | 환경별 설정 분리 | `appsettings.{Env}.yaml` | O |
| 설정 관리 | 설정 파일 | 설정 우선순위 프로세스 (파일 → 환경변수 순 오버라이드) | `AddEnvironmentVariables()` | O |
| 설정 관리 | 설정 파일 | 인프라 타입 전환 (DB/Cache 구현체 교체) | DbType / CacheType (MySQL ↔ InMemory) | O |
| 설정 관리 | 설정 파일 | 서버 설정 동적 변경 (RemoteConfig — 재배포 없이 파라미터 변경) | | X |
| 게임 특화 | 시간 | 서버 시간 동기화 (클라이언트는 서버 시간 기준으로 동작, 로컬 시간 변경 치트 방지) | `ServerTime` 싱글턴 구현 | O |
| 게임 특화 | 시간 | 시간 조작 치트 — 개인 (특정 플레이어 시간 이동, 이벤트·스케줄 테스트용) | | X |
| 게임 특화 | 시간 | 시간 조작 치트 — 서버 전체 (서버 시간 일괄 이동, 시간 기반 컨텐츠 QA용) | | X |
| 게임 특화 | 이벤트/스케줄 | 이벤트·스케줄 시스템 (DB 데이터로 기간·조건 제어, 재배포 없이 변경 가능) | `ScheduleManager` 구현, DB 기반 | O |
| 게임 특화 | 이벤트/스케줄 | 클라이언트 시간 신뢰 금지 (이벤트 판정은 무조건 서버 시간·서버 값 기준) | 서버 시간 기준으로 스케줄 판정 | O |
| 게임 특화 | 알림/전달 | In-game 우편함 (서버에서 플레이어에게 아이템·메시지 전달, 보상 지급·공지용) | | X |
| 실시간 통신 | Push | 서버→클라이언트 실시간 알림 (WebSocket, MQTT 등) | | X |
| 실시간 통신 | Push | 외부 Push Notification (FCM/APNs, 앱 비활성 상태 알림) | | X |
| 서버 아키텍처 | Gateway | Gateway 서버 (게임 서버 앞단, 라우팅·인증·부하 분산·설정 배포 담당) | | X |
| 서버 아키텍처 | 서버 간 통신 | 서버 간 데이터 공유 (Redis Pub/Sub, gRPC, Message Queue 등) | | X |
| 테스트 | 단위/통합 | 테스트 프레임워크 | XUnit | O |
| 테스트 | 단위/통합 | 통합 테스트 환경 | `WebApplicationFactory` + InMemory DB/Cache | O |
| 테스트 | 부하/품질 | 부하 테스트 (동시 접속, 가챠·결제 폭주 등 주요 시나리오 재현) | | X |
| 배포 | 빌드 | 컨테이너화 (Docker) | | X |
| 배포 | CI/CD | CI/CD 파이프라인 (빌드 → 테스트 → 이미지 빌드 → 배포) | `.github/workflows` 폴더 있으나 비어있음 | X |
| 배포 | CI/CD | 무중단 배포 전략 (Rolling / Blue-Green) | | X |
| 배포 | CI/CD | 롤백 절차 | | X |
| 운영 | 상태 관리 | Health Check 엔드포인트 | `GET /health-check` | O |
| 운영 | 상태 관리 | API 문서화 | Swagger UI (개발 환경 한정) | O |
| 운영 | 상태 관리 | 모니터링 대시보드 (Prometheus 메트릭 → Grafana 시각화) | | X |
| 운영 | 플레이어 관리 | Kick 기능 (특정 플레이어 강제 세션 만료, 어뷰징·긴급 대응) | | X |
| 운영 | 플레이어 관리 | Block 기능 (계정 접속 차단, 밴) | | X |
| 운영 | 플레이어 관리 | Player export/import (개발·QA 환경에서 플레이어 데이터 추출·삽입, 시나리오 재현용) | | X |
| 개발 도구 | 디버깅 | Relay Server (클라이언트-서버 간 패킷 중계·캡처, 중간 디버깅 프록시) | | X |

---

## 우선순위 요약

| 우선순위 | 항목 | 카테고리 |
|:--------:|------|----------|
| 높음 | 운영/Admin API 인증 구현 | 인증/보안 |
| 높음 | 트랜잭션 Commit 흐름 정리 | 데이터 계층 |
| 높음 | 민감정보 분리 (Secrets Manager) | 설정 관리 |
| 중간 | 중복/재요청 처리 (Seq 검증 + 응답 캐싱) | 요청 파이프라인 |
| 중간 | Replay Attack 방지 (Timestamp 검증) | 요청 파이프라인 |
| 중간 | 점검 모드 구현 | 요청 파이프라인 / 운영 |
| 중간 | CI/CD 파이프라인 기본 구성 | 배포 |
| 낮음 | 외부 에러 리포팅 (Sentry) | 에러 처리 |
| 낮음 | Rate Limiting | 요청 파이프라인 |
| 낮음 | 캐시 장애 Fallback (Circuit Breaker) | 데이터 계층 |
