@ECHO OFF
REM liquibase 단일 실행. 사용자가 직접 부를 일은 없다.
REM 사용법: CALL _Liquibase.bat <Auth|User|Center> <command> [extraArg]
REM 전제: 호출 전에 _Env.bat 이 CALL 되어 있어야 한다.

CALL "%~dp0_Target.bat" %1
IF ERRORLEVEL 1 EXIT /B 1

IF "%~2"=="" (
    ECHO [ERROR] no liquibase command given
    EXIT /B 1
)

ECHO.
ECHO === %DB_NAME% : liquibase %~2 %~3 ===

REM changeLogFile 이 상대 경로라 이 폴더에서 실행해야 한다.
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
