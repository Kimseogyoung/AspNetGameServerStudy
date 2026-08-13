@ECHO OFF
REM 공통 환경 로더. 모든 배치가 첫 줄에서 CALL 한다.
REM 접속 정보는 .env 에서 읽는다(.env.example 참고). .env 는 커밋하지 않는다.
REM
REM 콘솔에 찍는 문자열은 ASCII 로만 쓴다. 배치 파일 인코딩과 콘솔 코드페이지가
REM 어긋나면(UTF-8 파일 + CP949 콘솔 등) 한글이 깨져서, 하필 DROP 경고처럼
REM 읽혀야 하는 메시지가 못 읽게 된다. 설명은 이 REM 과 README 에 한글로 적는다.

SET "ENV_FILE=%~dp0.env"
IF NOT EXIST "%ENV_FILE%" (
    ECHO [ERROR] .env not found: %ENV_FILE%
    ECHO         Copy the sample first:  copy .env.example .env
    EXIT /B 1
)

REM eol=# 로 주석 줄을 건너뛴다. 빈 줄은 for /f 가 알아서 건너뛴다.
REM
REM 지연 확장(SETLOCAL ENABLEDELAYEDEXPANSION)을 켜지 않는다.
REM 켜면 비밀번호 안의 ! 가 조용히 잘려나가고, 인증 실패 원인을 찾기 어렵다.
FOR /F "usebackq eol=# tokens=1,* delims==" %%A IN ("%ENV_FILE%") DO (
    IF NOT "%%A"=="" SET "%%A=%%B"
)

IF NOT DEFINED DB_HOST   SET "DB_HOST=localhost"
IF NOT DEFINED DB_PORT   SET "DB_PORT=3306"
IF NOT DEFINED MYSQL_EXE SET "MYSQL_EXE=C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"

IF NOT DEFINED DB_USER (
    ECHO [ERROR] DB_USER is missing in .env
    EXIT /B 1
)

REM DB_PASSWORD 는 비어 있어도 정상이므로 존재 검사를 하지 않는다.
REM (batch 에서 SET "K=" 는 변수를 정의하는 게 아니라 지우는 것이라 구분이 불가능하다.)
REM mysql.exe 에는 명령줄 노출 경고를 피하려고 MYSQL_PWD 로 넘긴다.
SET "MYSQL_PWD=%DB_PASSWORD%"

EXIT /B 0
