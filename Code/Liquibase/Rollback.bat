@ECHO OFF
REM Rolls back recent changeSets.  Usage: Rollback.bat <Auth|User|Center> [count]  (default 1)
REM ASCII ONLY (see _Env.bat).
REM
REM The generated changelog is mostly createTable, so liquibase can auto-generate
REM the rollback. Rolling back drops those tables and their data with them.

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
