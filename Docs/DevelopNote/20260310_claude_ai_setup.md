# 2026-03-10 개발 노트

> 💬 AI를 적극 사용하기로 결심함!!!!!

## 1. Serena MCP 서버 연동 — Claude가 코드를 직접 이해하도록 연결

Claude Code가 단순히 텍스트를 읽는 수준을 넘어 코드 구조를 파악할 수 있도록 Serena MCP 서버를 연동했다. Serena는 언어 서버(LSP)를 활용해 심볼 검색·참조 추적 등 코드 탐색 기능을 AI에게 제공하는 도구다.

`.mcp.json`에 `serena` 서버 항목을 추가해 `uvx`로 실행하도록 구성했고, `.serena/project.yml`에 프로젝트 이름(`GameServerStudyAspNet`)·언어(`csharp`)·인코딩(`utf-8`) 등 기본 메타데이터를 작성했다. Serena가 로컬에서 생성하는 캐시(`/cache`)와 로컬 전용 설정(`project.local.yml`)은 `.serena/.gitignore`로 저장소에서 제외했다.

> **핵심 포인트**: `--context=ide` 옵션으로 IDE 보조 모드로 실행하며, 웹 대시보드는 비활성화해 불필요한 프로세스를 줄였다.

---

## 2. Claude Skills 추가 — 자주 쓰는 작업을 슬래시 명령어로 자동화

Claude Code에서 `/commit`, `/dev-note`, `/level-up`, `/phase-check` 네 가지 커스텀 슬래시 명령어(Skills)를 추가했다. 각 명령어는 `.claude/skills/` 하위 디렉터리의 `SKILL.md` 파일로 정의되며, AI가 git 이력이나 문서를 읽고 자동으로 작업을 수행한다.

- `/commit` — 스테이지 내용을 분석해 한국어 커밋 메시지 자동 생성
- `/dev-note` — 오늘 커밋을 바탕으로 `Docs/DevelopNote`에 개발 노트 작성
- `/level-up` — 특정 콘텐츠를 목표 L-레벨로 올리기 위한 작업 목록 생성
- `/phase-check` — 로드맵 기준 현재 페이즈 진행 현황과 다음 작업 TOP 3 정리

> **핵심 포인트**: `disable-model-invocation: true` 옵션으로 Skills는 AI가 직접 실행하며, 셸 명령(`!` 접두사)으로 git 이력·파일 목록 등을 런타임에 주입한다.

---

*이 노트는 아래 커밋을 바탕으로 AI(Claude)가 작성했습니다.*

| 커밋 | 메시지 |
|------|--------|
| `6e5bfab` | claude skills 추가 (commit / dev-note / level-up / phase-check) |
| `0fbe6a1` | claude mcp 설정 추가 / serena project yml |
| `6b2101c` | serena gitignore 추가 |
