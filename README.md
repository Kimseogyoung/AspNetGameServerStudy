# GameServerStudyAspNet

ASP.NET + Unity 기반 게임 서버/클라이언트 학습 및 구현 프로젝트입니다.  
현재는 `쿠키런 킹덤` 스타일의 코어 루프(인증, 왕국, 월드, 가챠, 쿠키 성장)를 중심으로 서버 구조와 콘텐츠를 확장하고 있습니다.

## 프로젝트 목표
- 게임 서버 핵심 구조를 직접 설계/구현/운영 가능한 수준으로 고도화
- 콘텐츠 로직과 데이터 파이프라인(기획 데이터 -> 코드/DB 반영) 정립
- 테스트/로그/운영 관점까지 포함한 실무형 개발 흐름 구축

## 저장소 구조
```txt
Client/   Unity 프로젝트
Code/     서버/공용 라이브러리/생성기 소스
Data/     기획 데이터(Excel/CSV)
Dist/     실행 산출물 및 실행 배치
Docs/     기획/설계/로드맵/개발 노트
Tool/     개발 도구
```

## 기술 스택
- Server: .NET 8, ASP.NET Core, Dapper, EF Core, Protobuf-net, NLog
- Client: Unity
- DB: MySQL, SQLite(로컬/테스트)
- Schema: Liquibase
- Data: Excel/CSV 기반 코드 생성(ClassGenerator)

## 빠른 시작

## 1) 서버 빌드
```bash
dotnet build Code/Code.sln
```

## 2) 서버 실행
```bash
Dist/RunServer.bat
```

또는:
```bash
dotnet run --project Code/Server
```

## 3) 클라이언트 실행(간이 콘솔/실행 산출물)
```bash
Dist/RunClient.bat
```

## 설정
- 서버 설정 파일:
1. `Code/Server/appsettings.yaml`
2. `Code/Server/appsettings.Development.yaml`
- 로컬 환경에서는 민감정보를 직접 커밋하지 않도록 분리해서 사용하세요.

## 현재 콘텐츠 범위

| 콘텐츠 | 서버 | 클라이언트 | 상태 |
|---|---|---|---|
| 인증/입장 루프 | `L3` 베타 가능 | `L2` 기능 구현 | 구현됨 |
| 왕국 건설/배치 | `L3` 베타 가능 | `L2` 기능 구현 | 구현됨 |
| 월드 스테이지 진행/보상 | `L3` 베타 가능 | `L2` 기능 구현 | 구현됨 |
| 가챠(기초) | `L3` 베타 가능 | `L2` 기능 구현 | 구현됨 |
| 쿠키 성장(레벨/승급) | `L2` 기능 구현 | `L2` 기능 구현 | 구현됨 |
| 공방 제작/건설 대기열 | `L0` 미착수 | `L0` 미착수 | 개발 예정 (Phase B) |
| 실시간 레이드 보스 | `L0` 미착수 | `L0` 미착수 | 개발 예정 (Phase C) |
| 시즌 랭킹 + 리플레이 검증 | `L0` 미착수 | `L0` 미착수 | 개발 예정 (Phase C) |
| 길드 | `L0` 미착수 | `L0` 미착수 | 개발 예정 (Phase C) |

- L0 미착수 / L1 프로토타입 / L2 기능 구현 / L3 베타 가능 / L4 라이브 준비 / L5 운영 안정

상세 상태/완성도는 아래 SSOT 문서를 기준으로 관리합니다.
- `Docs/Content_List_And_Roadmap.md`

## Docs 가이드
```txt
Docs/
ㄴ (root): 프로젝트 공통 로드맵/기준 문서 보관, 현재 상태와 우선순위 관리
ㄴ Design/: 신규/확장 콘텐츠 상세 서버 설계(규칙, 데이터 모델, API 초안, 단계별 계획)
ㄴ Client/: 클라이언트 아키텍처/검증 시나리오 문서
ㄴ Game/: 실제 게임 화면/플로우 기준 기능 기획 문서
ㄴ Title/: 타이틀/로그인 구간 기획 자료
ㄴ DevelopNote/: 날짜별 개발 기록 및 작업 로그
   ㄴ img/: 개발 스크린샷 보관
```
- 서버 구현 로드맵: `Docs/Portfolio_Server_Roadmap.md`
- 콘텐츠/완성도 기준 로드맵(SSOT): `Docs/Content_List_And_Roadmap.md`
- 서버 구현 체크리스트: `Docs/ServerChecklist.md`

---

## 게임 웹서버 구현 체크리스트

> 게임 웹서버 필수 구성요소 기준으로 이 프로젝트의 구현 현황을 정리합니다.

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

| 카테고리 | 분류 | 항목 | 구현 | 상태 |
|----------|------|------|------|:----:|
| 요청 파이프라인 | 라우팅/직렬화 | API 라우팅 방식 | Custom RpcMethod (`/rpc/{name}` POST) | O |
| 요청 파이프라인 | 라우팅/직렬화 | 직렬화 포맷 (다중 포맷 유연 지원) | ProtoBuf 기본, JSON fallback (테스트용) | O |
| 요청 파이프라인 | 라우팅/직렬화 | HTTPS/TLS 설정 | `UseHttpsRedirection()` 적용 | O |
| 요청 파이프라인 | 라우팅/직렬화 | 요청 크기 제한 | | X |
| 요청 파이프라인 | 라우팅/직렬화 | 타임아웃 설정 | | X |
| 요청 파이프라인 | 미들웨어/필터 | 요청 컨텍스트 관리 | `ReqMiddleware` → `RpcContext.Init()` | O |
| 요청 파이프라인 | 미들웨어/필터 | 유저별 동시 요청 직렬화 | `UserLockService` (MySQL/InMemory Lock) | O |
| 요청 파이프라인 | 미들웨어/필터 | Graceful Shutdown | | X |
| 요청 파이프라인 | 미들웨어/필터 | Rate Limiting | | X |
| 요청 파이프라인 | 미들웨어/필터 | 점검 모드 | | X |
| 요청 파이프라인 | 미들웨어/필터 | 점검 중 접속 Whitelist | | X |
| 요청 파이프라인 | 요청 검증 | 중복/재요청 처리 (Seq + 응답 캐싱) | Seq 파싱만 함, 검증·캐싱 없음 | X |
| 요청 파이프라인 | 요청 검증 | Replay Attack 방지 (Timestamp 검증) | Timestamp 파싱만 함, 검증 없음 | X |
| 요청 파이프라인 | 요청 검증 | 요청 서명 검증 (HMAC) | ApiHash 계산만 함, 비교 없음 | X |
| 요청 파이프라인 | 요청 검증 | 클라이언트/API 버전 관리 | | X |
| 요청 파이프라인 | 최적화 | 요청 배치 처리 | | X |
| 요청 파이프라인 | 최적화 | 요청 압축 | | X |
| 요청 파이프라인 | 최적화 | 네트워크 재시도 처리 | | X |
| 인증/보안 | 세션 | 세션 생명주기 관리 (발급/인증/만료/연장/유예) | `SessionManager` (Sliding Expiration + GracePeriod) | O |
| 인증/보안 | 세션 | 자동 로그인 지원 (기기 식별자 기반) | DeviceKey(IDFV) → `SignUp` | O |
| 인증/보안 | 세션 | 중복 로그인 처리 (후입우선/선입우선/다중허용/타입별) | 후입 우선 — SignUp 시 기존 세션 만료 | O |
| 인증/보안 | 계정/채널 | Guest 계정 | `EChannelType.GUEST` 채널 생성 | O |
| 인증/보안 | 계정/채널 | 소셜 로그인 연동 | | X |
| 인증/보안 | 계정/채널 | 계정 연결/이전 (Guest → 소셜 머지) | | X |
| 인증/보안 | 계정/채널 | 계정 탈퇴 / 개인정보 삭제 | | X |
| 인증/보안 | API 권한 | API 레벨별 권한 분리 | `ERpcMethodType` (NONE / AUTHORIZED / AUTHORIZED_PLAYER) | O |
| 인증/보안 | API 권한 | 운영/Admin API 권한 관리 | `ERpcMethodType.OPS` 정의됨, 인증 미구현 | X |
| 인증/보안 | API 권한 | Admin IP Whitelist | | X |
| 인증/보안 | 시크릿 관리 | 민감정보 분리 관리 | appsettings.yaml에 DB 비밀번호 평문 노출 | X |
| 에러 처리 | 에러 정의 | 에러코드 표준화 | `EErrorCode` enum | O |
| 에러 처리 | 에러 정의 | 클라이언트-서버 에러 처리 프로토콜 | `GameException`, `CancelReqException`, `UserLockException` | O |
| 에러 처리 | 에러 처리 | 전역 예외 핸들러 | `ErrorHandler` (예외 종류별 분기) | O |
| 에러 처리 | 에러 처리 | 환경별 에러 상세 노출 제어 | `IsShowErrorDetail` 설정 존재, ErrorHandler 분기 미구현 | X |
| 에러 처리 | 에러 처리 | 외부 에러 리포팅 (Sentry 등) | ErrorHandler에 TODO 존재, 미연동 | X |
| 로깅/모니터링 | 로깅 | 파일 로그 | NLog (파일 + 콘솔) | O |
| 로깅/모니터링 | 로깅 | 원격 저장소 로그 (Fluent Bit 등) | | X |
| 로깅/모니터링 | 로깅 | 로그 구조화 | 파라미터 바인딩 방식 (`{Key}`) | O |
| 로깅/모니터링 | 로깅 | 요청/응답 로그 (Body 포함) | `RpcService`가 RPC 요청/응답 직접 로깅 (Method, Path, Body). MVC 경로용이던 `LogFilter`는 DI 등록만 되고 실제 미적용 상태라 삭제함 | O |
| 로깅/모니터링 | 로깅 | 요청 처리 시간 로그 | | X |
| 로깅/모니터링 | 로깅 | 슬로우 쿼리 로그 | | X |
| 로깅/모니터링 | 로깅 | 로그 수집/검색 인프라 | | X |
| 로깅/모니터링 | 감사 로그 | 중요 행동 감사 로그 (결제·아이템 등) | `CashChangeLogModel`, `GachaLogModel` 부분 구현 | O |
| 로깅/모니터링 | 메트릭/알림 | 메트릭 수집 (Prometheus 등) | | X |
| 로깅/모니터링 | 메트릭/알림 | 장애 알림 | | X |
| 데이터 계층 | DB | 테스트용 DB | InMemoryDB | O |
| 데이터 계층 | DB | DB 도메인 분리 | Auth / User / Center DB 분리 | O |
| 데이터 계층 | DB | DB 수평 분산 (샤딩) | ShardId 기반 UserDb 분산 | O |
| 데이터 계층 | DB | DB Read Replica (운영·통계 전용) | | X |
| 데이터 계층 | DB | 시작 시 연결 검증 | `ConnectionTest()` | O |
| 데이터 계층 | DB | 트랜잭션 관리 | ReadCommitted 사용, Commit 흐름 일부 미정리 | X |
| 데이터 계층 | DB | 개발 환경 Schema Migration | Liquibase 폴더 있으나 미구성 | X |
| 데이터 계층 | DB | 라이브 환경 Schema Migration | | X |
| 데이터 계층 | DB | 커넥션 풀 설정 | | X |
| 데이터 계층 | DB | 데이터 백업/복구 | | X |
| 데이터 계층 | Cache | 캐시 (DB 부하 분산) | `RedisCompositeCacheLayer` (Redis + InMemory) | O |
| 데이터 계층 | Cache | 캐시 장애 Fallback (Circuit Breaker) | | X |
| 데이터 계층 | ID 생성 | 분산 환경 고유 ID 생성 | Snowflake ID (IdGen, WorkerId = ServerNum) | O |
| 설정 관리 | 설정 파일 | 환경별 설정 분리 | `appsettings.{Env}.yaml` | O |
| 설정 관리 | 설정 파일 | 설정 우선순위 프로세스 (파일 → 환경변수) | `AddEnvironmentVariables()` | O |
| 설정 관리 | 설정 파일 | 인프라 타입 전환 (DB/Cache 구현체 교체) | DbType / CacheType (MySQL ↔ InMemory) | O |
| 설정 관리 | 설정 파일 | 서버 설정 동적 변경 (RemoteConfig) | | X |
| 게임 특화 | 시간 | 서버 시간 동기화 | `ServerTime` 싱글턴 구현 | O |
| 게임 특화 | 시간 | 시간 조작 치트 — 개인 | | X |
| 게임 특화 | 시간 | 시간 조작 치트 — 서버 전체 | | X |
| 게임 특화 | 이벤트/스케줄 | 이벤트·스케줄 시스템 (DB 기반 제어) | `ScheduleManager` 구현, DB 기반 | O |
| 게임 특화 | 이벤트/스케줄 | 클라이언트 시간 신뢰 금지 (서버 시간 기준) | 서버 시간 기준으로 스케줄 판정 | O |
| 게임 특화 | 알림/전달 | In-game 우편함 | | X |
| 실시간 통신 | Push | 서버→클라이언트 실시간 알림 (WebSocket, MQTT 등) | | X |
| 실시간 통신 | Push | 외부 Push Notification (FCM/APNs) | | X |
| 서버 아키텍처 | Gateway | Gateway 서버 | | X |
| 서버 아키텍처 | 서버 간 통신 | 서버 간 데이터 공유 (Redis Pub/Sub, gRPC 등) | | X |
| 테스트 | 단위/통합 | 테스트 프레임워크 | XUnit | O |
| 테스트 | 단위/통합 | 통합 테스트 환경 | `WebApplicationFactory` + InMemory DB/Cache | O |
| 테스트 | 부하/품질 | 부하 테스트 (주요 시나리오 재현) | | X |
| 배포 | 빌드 | 컨테이너화 (Docker) | | X |
| 배포 | CI/CD | CI/CD 파이프라인 | `.github/workflows` 폴더 있으나 비어있음 | X |
| 배포 | CI/CD | 무중단 배포 전략 (Rolling / Blue-Green) | | X |
| 배포 | CI/CD | 롤백 절차 | | X |
| 운영 | 상태 관리 | Health Check 엔드포인트 | `GET /health-check` | O |
| 운영 | 상태 관리 | API 문서화 | Swagger UI (개발 환경 한정) | O |
| 운영 | 상태 관리 | 모니터링 대시보드 (Prometheus + Grafana) | | X |
| 운영 | 플레이어 관리 | Kick 기능 | | X |
| 운영 | 플레이어 관리 | Block 기능 | | X |
| 운영 | 플레이어 관리 | Player export/import (개발·QA용) | | X |
| 개발 도구 | 디버깅 | Relay Server | | X |

> 세부 설명 및 비고: [`Docs/ServerChecklist.md`](Docs/ServerChecklist.md)
