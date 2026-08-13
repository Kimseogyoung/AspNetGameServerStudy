@ECHO OFF
REM 미적용 changeSet 확인. 사용법: Status.bat [Auth|User|Center|All]   (기본 All)

CALL "%~dp0_Env.bat"
IF ERRORLEVEL 1 EXIT /B 1

SET "TARGET=%~1"
IF "%TARGET%"=="" SET "TARGET=All"

IF /I NOT "%TARGET%"=="All" (
    REM 블록 안에서는 %ERRORLEVEL% 이 블록 파싱 시점 값으로 굳으므로 인자 없이 EXIT /B 한다.
    CALL "%~dp0_Liquibase.bat" %TARGET% status
    EXIT /B
)

CALL "%~dp0_Liquibase.bat" Auth status
IF ERRORLEVEL 1 EXIT /B 1
CALL "%~dp0_Liquibase.bat" User status
IF ERRORLEVEL 1 EXIT /B 1
CALL "%~dp0_Liquibase.bat" Center status
EXIT /B %ERRORLEVEL%
