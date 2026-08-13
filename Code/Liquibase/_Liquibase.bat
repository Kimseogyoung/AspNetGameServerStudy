@ECHO OFF
REM Runs one liquibase command. Not meant to be called directly.
REM ASCII ONLY (see _Env.bat).
REM Usage: CALL _Liquibase.bat <Auth|User|Center> <command> [extraArg]
REM Requires _Env.bat to have been CALLed first.

CALL "%~dp0_Target.bat" %1
IF ERRORLEVEL 1 EXIT /B 1

IF "%~2"=="" (
    ECHO [ERROR] no liquibase command given
    EXIT /B 1
)

ECHO.
ECHO === %DB_NAME% : liquibase %~2 %~3 ===

REM changeLogFile is relative, so run from this folder.
PUSHD "%~dp0"
liquibase ^
  --url="jdbc:mysql://%DB_HOST%:%DB_PORT%/%DB_NAME%" ^
  --username="%DB_USER%" ^
  --password="%DB_PASSWORD%" ^
  --driver="com.mysql.cj.jdbc.Driver" ^
  --changeLogFile="%CHANGELOG%" ^
  %~2 %~3
SET "RC=%ERRORLEVEL%"
POPD

IF NOT "%RC%"=="0" (
    ECHO [ERROR] %DB_NAME% : liquibase %~2 failed ^(exit %RC%^)
    EXIT /B %RC%
)
EXIT /B 0
