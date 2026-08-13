@ECHO OFF
REM Applies the changelog.  Usage: Update.bat [Auth|User|Center|All]  (default All)
REM ASCII ONLY (see _Env.bat).
REM
REM The generated changelog is create-only (changeSet id == table name). If a CSV
REM change regenerated it, an already-applied changeSet now has different content
REM and liquibase fails checksum validation here. Use Recreate.bat in that case.
REM See README.md.txt for the full reasoning.

CALL "%~dp0_Env.bat"
IF ERRORLEVEL 1 EXIT /B 1

SET "TARGET=%~1"
IF "%TARGET%"=="" SET "TARGET=All"

IF /I NOT "%TARGET%"=="All" (
    REM Bare EXIT /B: inside a block %ERRORLEVEL% would freeze at parse time.
    CALL "%~dp0_Liquibase.bat" %TARGET% update
    EXIT /B
)

CALL "%~dp0_Liquibase.bat" Auth update
IF ERRORLEVEL 1 EXIT /B 1
CALL "%~dp0_Liquibase.bat" User update
IF ERRORLEVEL 1 EXIT /B 1
CALL "%~dp0_Liquibase.bat" Center update
IF ERRORLEVEL 1 EXIT /B 1

ECHO.
ECHO Done: 3 databases updated
EXIT /B 0
