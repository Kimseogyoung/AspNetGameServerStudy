@ECHO OFF
REM 최근 changeSet 되돌리기. 사용법: Rollback.bat <Auth|User|Center> [개수]   (기본 1)
REM
REM 생성 changelog 는 createTable 위주라 liquibase 가 자동 롤백을 만들어 준다.
REM 다만 되돌리면 그 테이블의 데이터도 같이 사라진다.

CALL "%~dp0_Env.bat"
IF ERRORLEVEL 1 EXIT /B 1

IF "%~1"=="" (
    ECHO Usage: Rollback.bat ^<Auth^|User^|Center^> [count]
    EXIT /B 1
)

SET "COUNT=%~2"
IF "%COUNT%"=="" SET "COUNT=1"

CALL "%~dp0_Liquibase.bat" %~1 rollbackCount %COUNT%
EXIT /B %ERRORLEVEL%
