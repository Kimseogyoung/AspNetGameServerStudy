@ECHO OFF
REM DB 를 지우고 현재 모델로 다시 만든다.
REM 사용법: Recreate.bat [Auth|User|Center|All] [-y]   (기본 All, -y 는 확인 생략)
REM
REM *** 대상 DB 의 데이터가 전부 사라진다. ***
REM
REM 왜 이게 기본 반영 수단인가:
REM 생성 changelog 는 changeSet id 가 테이블명인 create-only 구조라 증분 ALTER 를
REM 표현하지 못한다. CSV 를 고쳐 재생성하면 이미 적용된 changeSet 의 내용이 바뀌고,
REM liquibase 는 저장된 MD5 와 달라 update 를 거부한다. 지킬 데이터가 없는 로컬
REM 개발 DB 뿐이므로 drop 후 재생성으로 반영한다.
REM 보존해야 할 데이터가 있는 DB 가 생기면 이 방식은 더 이상 쓸 수 없고,
REM 손으로 쓰는 증분 changeSet 수단이 필요해진다.

CALL "%~dp0_Env.bat"
IF ERRORLEVEL 1 EXIT /B 1

SET "TARGET=%~1"
SET "YES=%~2"
IF /I "%TARGET%"=="-y" ( SET "TARGET=All" & SET "YES=-y" )
IF "%TARGET%"=="" SET "TARGET=All"

IF /I "%TARGET%"=="All" (
    SET "TARGET_DESC=AuthDb, UserDb, CenterDb"
) ELSE (
    SET "TARGET_DESC=%TARGET%Db"
)

ECHO.
ECHO   target : %TARGET_DESC%   @ %DB_HOST%:%DB_PORT%
ECHO   DROP DATABASE and re-create. ALL DATA WILL BE LOST.
ECHO.

IF /I NOT "%YES%"=="-y" (
    SET "ANSWER="
    SET /P "ANSWER=Type yes to continue: "
    CALL :CONFIRM
    IF ERRORLEVEL 1 EXIT /B 1
)

IF /I NOT "%TARGET%"=="All" (
    REM 블록 안에서는 %ERRORLEVEL% 이 블록 파싱 시점 값으로 굳으므로 인자 없이 EXIT /B 한다.
    CALL :ONE %TARGET%
    EXIT /B
)

CALL :ONE Auth
IF ERRORLEVEL 1 EXIT /B 1
CALL :ONE User
IF ERRORLEVEL 1 EXIT /B 1
CALL :ONE Center
IF ERRORLEVEL 1 EXIT /B 1

ECHO.
ECHO Done: 3 databases re-created
EXIT /B 0


:CONFIRM
IF /I "%ANSWER%"=="yes" EXIT /B 0
ECHO Cancelled.
EXIT /B 1


:ONE
CALL "%~dp0_Target.bat" %1
IF ERRORLEVEL 1 EXIT /B 1

ECHO.
ECHO === %DB_NAME% : drop / create ===
REM 비밀번호는 _Env.bat 이 MYSQL_PWD 로 넘긴다(명령줄 노출 경고 회피).
"%MYSQL_EXE%" -h "%DB_HOST%" -P %DB_PORT% -u "%DB_USER%" -e "DROP DATABASE IF EXISTS %DB_NAME%; CREATE DATABASE %DB_NAME% DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;"
IF ERRORLEVEL 1 (
    ECHO [ERROR] %DB_NAME% : drop/create failed
    EXIT /B 1
)

CALL "%~dp0_Liquibase.bat" %1 update
EXIT /B %ERRORLEVEL%
