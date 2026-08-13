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

### TODO

- 자동화 (팀시티, 젠킨스)
