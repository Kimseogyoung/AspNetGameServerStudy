## Liquibase 사용법

### 준비

1. `.env.example` 을 `.env` 로 복사하고 접속 정보를 채운다. `.env` 는 커밋하지 않는다.

```
copy .env.example .env
```

2. JDBC 드라이버 설정
   MySQL 의 경우 mysql-connector-java JAR 을 liquibase 설치 디렉터리의 `lib` 폴더에 넣어야 한다.
   https://dev.mysql.com/downloads/connector/j/ 에서 받은 `mysql-connector-j-9.0.0` 을
   liquibase 설치 폴더의 `lib` 로 이동.

3. `liquibase` 가 PATH 에 있어야 한다. `mysql.exe` 는 `.env` 의 `MYSQL_EXE` 로 지정한다
   (DROP/CREATE DATABASE 에만 쓴다).

### 명령

대상은 `Auth` / `User` / `Center` / `All` 이며 생략하면 `All` 이다.

```
Update.bat   [대상]              스키마 적용
Status.bat   [대상]              미적용 changeSet 확인
Recreate.bat [대상] [-y]         DROP -> CREATE -> 적용 (데이터 전부 삭제)
Rollback.bat <대상> [개수]       최근 changeSet 되돌리기 (기본 1개)
```

적용 시 대상 DB 의 `DATABASECHANGELOG` 테이블에 이력이 기록된다.

### changelog 는 어느 것이 적용되는가

`CreateLog_Auth.json` / `CreateLog_User.json` / `CreateLog_Center.json` 이며,
전부 `ClassGenerator` 의 `ModelGenerater` 프로필이 CSV 로부터 **생성**한다.
직접 편집하지 말고 `Data/Excel/Model/**` 을 고친 뒤 생성기를 돌린다.

### 모델을 바꾼 뒤에는 Recreate 를 쓴다

생성 changelog 는 changeSet id 가 테이블명인 create-only 구조라 증분 ALTER 를
표현하지 못한다. CSV 를 고쳐 재생성하면 **이미 적용된 changeSet 의 내용이 바뀌고**,
liquibase 는 저장된 MD5 와 달라 `update` 를 거부한다. 그래서 반영 수단은
`Recreate.bat` 이다.

지킬 데이터가 없는 로컬 개발 DB 라서 성립하는 방식이다. 보존해야 할 데이터가 있는
DB 가 생기면 이 방식은 쓸 수 없고, 손으로 쓰는 증분 changeSet 수단이 필요해진다.

한때 `AuthDbChangeLog.yml` / `UserDbChangeLog.yml` 이 그 시도였으나 세 DB 어디에도
적용된 적이 없었고(`DATABASECHANGELOG.FILENAME` 이 전부 `CreateLog_*.json`),
모델 19개 중 User 쪽은 5개에서 멈춰 있어 삭제했다.

### 배치 파일 수정 시 지켜야 할 것

**1. 줄바꿈은 CRLF.** LF 로 저장하면 cmd 가 각 줄을 개별 명령으로 실행해 버린다.

**2. 배치 파일 안에는 ASCII 만 쓴다. 주석(REM)도 포함이다.**
UTF-8 로 저장된 배치를 CP949 콘솔이 읽으면 한글 바이트가 줄바꿈을 삼켜서
REM 주석의 일부가 명령으로 튀어나온다. 실제로 이렇게 터졌다:

```
'?ъ깮?깊븯硫??대?'은(는) 내부 또는 외부 명령 ... 이 아닙니다.
```

콘솔 코드페이지는 환경마다 다르므로(949 / 65001) 어느 한쪽에 맞출 수 없다.
그래서 배치는 ASCII 로 고정하고, 한글 설명은 이 README 에 적는다.
특히 Recreate 의 DROP 경고처럼 **반드시 읽혀야 하는 메시지**가 깨지면 안 된다.

**3. 괄호 블록 안에서는 `EXIT /B %ERRORLEVEL%` 를 쓰지 않는다.**
블록 전체가 한 번에 파싱되면서 `%ERRORLEVEL%` 이 블록 진입 시점 값으로 굳는다.
인자 없이 `EXIT /B` 하면 직전 명령의 종료 코드가 그대로 전달된다.

**4. 지연 확장(`SETLOCAL ENABLEDELAYEDEXPANSION`)을 켜지 않는다.**
비밀번호에 `!` 가 들어가면 조용히 잘려서 인증 실패 원인을 찾기 어려워진다.

### TODO

- 자동화 (팀시티, 젠킨스)
- 생성 changelog 의 `runningAs: root` 하드코딩 (`ModelGenerator.cs:273`).
  `.env` 로 `DB_USER` 를 바꿀 수 있는데 root 가 아니면 precondition 에 걸린다.
  이 precondition 이 실제로 막아 주는 것이 없으므로 제거하거나 인자로 받게 한다.
