@ECHO OFF
REM Shows unapplied changeSets.  Usage: Status.bat [Auth|User|Center|All]  (default All)
REM ASCII ONLY (see _Env.bat).
REM
REM Note: liquibase validates the whole changelog before ANY command, so a
REM checksum mismatch blocks status too, not just update.

CALL "%~dp0_Env.bat"
IF ERRORLEVEL 1 EXIT /B 1

SET "TARGET=%~1"
IF "%TARGET%"=="" SET "TARGET=All"

IF /I NOT "%TARGET%"=="All" (
    REM Bare EXIT /B: inside a block %ERRORLEVEL% would freeze at parse time.
    CALL "%~dp0_Liquibase.bat" %TARGET% status
    EXIT /B
)

CALL "%~dp0_Liquibase.bat" Auth status
IF ERRORLEVEL 1 EXIT /B 1
CALL "%~dp0_Liquibase.bat" User status
IF ERRORLEVEL 1 EXIT /B 1
CALL "%~dp0_Liquibase.bat" Center status
EXIT /B %ERRORLEVEL%
