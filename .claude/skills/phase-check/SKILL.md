---
name: phase-check
description: 현재 개발 로드맵 진행 현황을 확인하고 다음 작업을 정리한다. 현재 어떤 페이즈에 있는지, 무엇이 완료되었는지, 다음에 무엇을 해야 하는지 물어볼 때 사용한다.
disable-model-invocation: true
---

## 현재 로드맵 상태
- 로드맵: [Docs/Content_List_And_Roadmap.md](Docs/Content_List_And_Roadmap.md)
- 포트폴리오 서버 로드맵: [Docs/Portfolio_Server_Roadmap.md](Docs/Portfolio_Server_Roadmap.md)

## 최근 커밋 이력
!`git log --oneline -15`

## 현재 변경 중인 파일
!`git status --short`

## 할 일

1. 로드맵 문서를 읽고 각 Phase(A0, A1, A2, A3, A4, A5, B, C)의 목표를 파악한다.
2. 최근 커밋과 변경 파일을 분석해 현재 어떤 페이즈 작업 중인지 추론한다.
3. 각 콘텐츠의 현재 L-레벨(서버/클라이언트)을 표로 정리한다.
4. 현재 진행 중인 페이즈에서 완료된 항목과 남은 항목을 체크리스트로 표시한다.
5. 다음에 집중해야 할 작업 TOP 3을 구체적으로 제시한다.

출력 형식:
- 현재 페이즈: Phase X - 콘텐츠명
- 콘텐츠별 L-레벨 현황표
- 현재 페이즈 체크리스트
- 다음 작업 TOP 3 (서버/클라이언트 구분)
