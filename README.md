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

---
본 README는 Claude Code를 사용해 작성되었습니다.
