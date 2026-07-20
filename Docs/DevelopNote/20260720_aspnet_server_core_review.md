# 2026-07-20 ASP.NET WebServer 코어 구조 리뷰

> 신규 프로젝트로 이관하기 전, 현재 `Code/Server` / `Code/ServerCore` / `Code/ServerFramework`의
> 코어 구조(정책/비즈니스 로직 제외)를 훑어보고 잘된 점과 구조적 이슈를 정리한 노트.

## 👍 잘 설계된 부분

### DB/캐시 계층

- `IDbSessionFactory`(싱글턴, 세션 생성만) → `DbSessionManager`(스코프드, connectionString별
  세션 추적 + commit/rollback 일괄 처리) → `IRepository`(Cache-aside 표준화: `GetList` /
  `Insert` / `Update`)로 책임이 명확히 나뉘어 있음. MySql/InMemory 구현체를 DI 등록 한 줄로
  완전히 교체 가능해서, 실제 DB 없이 InMemory로 통합 테스트가 돌아가는 구조
  (`Server.Tests` 프로젝트 존재 이유).
- Redis 캐시: `InMemoryCacheLayer`(요청 스코프, read-your-writes 보장) →
  `RedisCompositeCacheLayer`(공유) 체이닝. Redis에 대한 쓰기는 무조건 pending에 쌓아뒀다가
  **DB 커밋 성공 시에만** flush (`RedisCompositeCacheLayer.cs:41-67`). 캐시-DB 정합성을
  트랜잭션 생명주기에 정확히 묶어놓은 설계라 "DB는 성공, 캐시만 따로 놀아서 불일치"라는
  흔한 버그 클래스를 구조적으로 막음.
- `CacheKey.For<T>(ids)`로 문자열 키를 값 객체로 감싸서, 오타로 인한 캐시 키 충돌을
  컴파일 타임 수준으로 방지.
- `ModelRegistration.Init<T>` 한 줄이 `DapperExtension`(SQL 캐시) + `InMemoryPkRegistry`를
  동시 등록 — 새 모델 추가 시 등록 지점이 하나뿐.
- `GlobalDbRepo`의 `Lazy<AuthRepo/CenterRepo/AllUserRepo>` — 요청마다 모든 리포를 다 여는 게
  아니라 실제로 쓰는 것만 연결.

### RPC / Formatter

- `RpcMethod<TSvc,TReq,TRes>` + `IRpcMethod`가 실행뿐 아니라
  `CreateOpenApiRequestBody/Response`까지 같이 생성 — Swagger 문서가 실제 등록된 RPC와
  어긋날 수 없는 구조. RPC 하나 추가할 때 라우트/스웨거를 따로 안 챙겨도 됨.
- JSON/Protobuf를 같은 핸들러가 Content-Type 기반으로 갈아 끼워서 지원
  (`CustomInputFormatter` / `CustomOutputFormatter`), 요청 Content-Type을 응답에 echo하는
  것도 실용적.

### Config

- `YamlConfigurationProvider`가 ASP.NET Core의 `FileConfigurationProvider`를 정석대로 상속 —
  yaml을 쓰면서도 환경변수 오버라이드, 계층 병합 같은 `IConfiguration` 표준 파이프라인을
  그대로 활용. 직접 파서를 만들어 표준 파이프라인을 우회하는 것보다 훨씬 나은 선택.

## 🔧 코어 구조 이슈

1. **DB 계층 전체가 동기(sync) I/O**
   `IDbExecutor` / `IDbSession` / `DapperDbExecutor` / `DBSqlExecutor`에 Async 오버로드가
   하나도 없음. Dapper는 `QueryAsync`/`ExecuteAsync`를 지원하는데 여기선 전부 동기 호출.
   ASP.NET Core는 요청당 스레드풀 스레드를 빌려쓰는 모델이라, DB 대기 중에도 스레드를 계속
   붙잡으면 동시 요청이 늘어날 때 스레드풀 고갈로 이어질 수 있음. 인터페이스 자체는 이미
   깔끔하게 추상화되어 있어서 Async로 바꾸는 리팩토링 부담은 크지 않은 편.
   **→ 신규 프로젝트는 처음부터 Async(`ExecuteAsync`, `SelectByPkAsync` 등)로 설계 추천.**

2. **커넥션이 "쿼리 시점"이 아니라 "리포 참조 시점"에 열림**
   `DBSqlExecutor.StartTransaction`이 생성자에서 바로 `Open()`(커넥션 오픈 + 트랜잭션 시작)
   까지 해버림(`DBSqlExcutor.cs:9-14`). `GlobalDbRepo`가 `Lazy<T>`로 지연은 시키지만,
   `.Auth`를 한 번이라도 건드리는 순간 실제 DB 커넥션이 풀에서 뽑혀 나감. 한 요청에서
   Auth/User/Center를 다 건드리면 그만큼 동시에 커넥션이 열림. 서로 다른 DB라 트랜잭션
   공유가 불가능해서 불가피한 면도 있지만, 신규 프로젝트에서는 커넥션 풀 사이즈 산정 시
   이 특성을 염두에 둬야 함.

3. **MVC 컨트롤러 파이프라인이 사실상 죽어있음**
   `CommonController`는 health-check/hello-world 2개뿐, `DebugController`는 전부 주석.
   실제 RPC는 전부 `RpcService.MapAllPostRpc`(Minimal API `app.MapPost`)로 처리되고
   `HttpContext.Request.Body`를 직접 읽고 씀. `AddController`, `LogFilter`,
   `CustomInputFormatter`/`CustomOutputFormatter` 같은 MVC용 배관이 RPC 경로에서는 전혀
   안 탐 — 지금 프로젝트엔 사실상 두 개의 병렬 파이프라인(MVC용 / RPC Minimal API용)이
   공존하는데 메인은 RPC뿐.
   **→ 신규 프로젝트에서 "REST 컨트롤러는 아예 안 쓴다"고 확정하면 Controller/Filter/MVC
   Formatter 계층을 통째로 들어낼 수 있음.** (헬스체크 같은 순수 REST 엔드포인트를 계속
   병행할 계획이면 유지.)

4. **`APP`(WebStudyServer.GAME) vs `Core`(ServerCore) — static 진입점이 두 개**
   `APP.Init()`이 내부에서 `Core.Init()`을 감싸 부르고, `APP.Cfg`(GameConfig)와
   `Core.Cfg`(CoreConfig)가 별도로 존재. 인프라 설정과 게임 도메인 설정을 분리한 의도는
   이해되지만, 코드 곳곳에서 `Core.Cfg.DbType`(`StartUp.Resource.cs`)과
   `APP.Prt`(`StartUp.Proto.cs`)를 상황에 따라 섞어써서 신규 합류자가 "이 설정값은 어디
   있지?"를 헷갈리기 쉬움.
   **→ 진입점을 하나로 합치거나, 최소한 "인프라=Core / 게임 도메인=APP" 경계를 문서화.**

5. **ErrorHandler 진입점이 3개**
   `Handle` / `HandleWithException` / `HandleWithExceptionAsync`, 결국 다
   `HandleInternalAsync`로 합류. `UseExceptionHandler` 미들웨어, `ReqMiddleware`,
   `RpcService`가 각자 다르게 호출. `ReqMiddleware` 자체가 "Map.Post방식으로 바꿨기 때문에
   필요한지 검토"라는 주석을 스스로 남겨둔 상태(`ReqMiddleware.cs:3`) — RPC가 이미 자체
   try/catch(`RpcService.OnHttpBodyRequestAsync`)를 갖고 있는 지금, 이 미들웨어가 실제로
   잡는 예외가 남아있는지 확인해서 없으면 진입점을 줄일 수 있음.

6. **`RpcMethod<TSvc,TReq,TRes>`가 "라우팅/OpenAPI 생성"과 "인증+DB세션 시작 오케스트레이션"
   두 책임을 겸함**
   `RunAsync` 안에서 `switch(_type)`으로 `AUTHORIZED`/`AUTHORIZED_PLAYER`일 때 세션 검증과
   `dbRepo.BeginOwnUserRepo()`를 직접 호출 — 작성자 본인도
   `// 여기서 처리해야하는지는 의문임` 주석을 남겨둔 지점(`RpcMethod.cs:48`). 이 클래스가
   요청 실행 파이프라인의 유일한 확장점이라, 인증 없는 배치/관리자 API 같은 새 케이스가
   생길 때마다 이 switch를 계속 늘려야 함.
   **→ 인증/세션로드/DB세션시작을 파이프라인(필터 체인) 형태로 분리 권장.**
   (참고: RaidServer 세션 설계 때 정리한 "SocketService 특수 케이스 대신 패킷 핸들러
   파이프라인" 원칙과 같은 패턴의 문제.)

## 🔴 별도 트래킹 중인 보안/운영 이슈 (이전 리뷰, 세부 사항)

- 요청/응답 바디 전체를 Info 레벨로 로깅 (`RpcService.cs:66,76`) — 민감정보 노출 가능성.
- `ERpcMethodType.OPS`가 빈 껍데기 (`RpcMethod.cs:70-71`) — 실제 인가 검사 없음.
- `X-Forwarded-For` / `CloudFront-Viewer-Country` 헤더를 무조건 신뢰 (`RpcContext.cs`) —
  트러스티드 프록시 경유가 보장 안 되면 스푸핑 가능.
- `ErrorHandler`가 예외 종류와 무관하게 항상 HTTP 500 반환 — 비즈니스 에러까지 500 알람에
  잡혀 모니터링 노이즈 유발 가능.

## 다음 논의 후보

- ③ MVC vs RPC 파이프라인 이원화: 신규 프로젝트에서 뭘 남기고 뭘 버릴지 결정만 하면 되는
  문제라 빠르게 정리 가능.
- ①/② Async DB, 커넥션 오픈 시점: 신규 프로젝트 뼈대 설계에 직접 영향 — 먼저 방향을 정하고
  갈지 검토.
