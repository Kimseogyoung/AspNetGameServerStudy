# 프로젝트 상태 점검 및 실행 플랜

작성일: 2026-03-08  
목적: ASP.NET 기반 게임 서버 프로젝트의 실행 우선순위와 운영 개선 항목 관리

## 1. 현재 상태 요약
- 빌드 상태: `Code/Code.sln` 기준 오류 0, 경고 존재
- 구조 상태: 서버/클라/데이터/문서/배포 산출물 폴더 체계 확립
- 리스크:
1. TODO 다수(서버/클라)
2. 테스트 자동화 부족
3. 운영 지표/관측성 부족
4. 설정/배포 관리 정리 필요

## 2. 기준 문서(SSOT)
콘텐츠 현황, 완성도 등급, 우선순위, 상세 로드맵은 아래 문서를 기준으로 관리한다.

- [Content_List_And_Roadmap.md](/d:/GitWorks/GameServerStudyAspNet/Docs/Content_List_And_Roadmap.md)

세부 기획 문서는 아래 링크를 사용한다.

- [RealTimeRaidBoss.md](/d:/GitWorks/GameServerStudyAspNet/Docs/Design/RealTimeRaidBoss.md)
- [AsyncKingdomQueue.md](/d:/GitWorks/GameServerStudyAspNet/Docs/Design/AsyncKingdomQueue.md)
- [SeasonRankingReplay.md](/d:/GitWorks/GameServerStudyAspNet/Docs/Design/SeasonRankingReplay.md)
- [GuildSystem.md](/d:/GitWorks/GameServerStudyAspNet/Docs/Design/GuildSystem.md)

클라이언트 구조/시나리오 문서는 아래 문서를 기준으로 관리한다.

- [ClientArchitecture.md](/d:/GitWorks/GameServerStudyAspNet/Docs/Client/ClientArchitecture.md)
- [ClientE2EScenarios.md](/d:/GitWorks/GameServerStudyAspNet/Docs/Client/ClientE2EScenarios.md)

## 3. 실행 플랜(중복 제거 버전)

## Phase 0. 기반 정리 (1주)
1. 민감정보 분리(`appsettings`/환경변수)
2. 산출물/저장소 정책 정리(`Dist`, `.gitignore`)
3. 아키텍처/에러 정책 문서 초안 작성

## Phase 1. 코어 안정화 (2~3주)
1. 예외/트랜잭션 경계 정리(`RpcService`, `DbRepo`, `ErrorHandler`)
2. 정합성 관련 TODO 우선 해소
3. 공통 에러코드/검증 규칙 정리

## Phase 2. 콘텐츠 완성도 강화 (3~4주)
1. SSOT 기준 `L2 -> L3` 콘텐츠 우선 강화
2. 서버-클라이언트 동기화 실패 케이스 보강
3. 치트/디버그 기능 운영 분리

## Phase 3. 테스트/운영성 강화 (3~4주)
1. 단위/통합/회귀 테스트 확장
2. 로그/메트릭/부하 시나리오 정착
3. 장애 복구 절차 문서화

## 4. 운영 규칙
1. 콘텐츠 상태/완성도/우선순위 변경은 SSOT 문서에만 반영한다.
2. 본 문서는 상위 계획과 진행 체크만 기록한다.
3. 상세 기획 변경은 `Docs/Design` 하위 문서에서 관리한다.

## 5. 이번 주 체크리스트
- [ ] SSOT 문서 기준으로 현재 스프린트 대상 확정
- [ ] 서버 정합성 TODO 5개 처리
- [ ] 클라이언트 E2E 시나리오 1개 고정
- [ ] 테스트 케이스 최소 5개 추가
