@ECHO OFF
REM Drops the database(s) and rebuilds them from the current model.
REM Usage: Recreate.bat [Auth|User|Center|All] [-y]   (default All, -y skips the prompt)
REM ASCII ONLY (see _Env.bat).
REM
REM *** ALL DATA IN THE TARGET DATABASES IS LOST. ***
REM
REM Why this is the normal way to apply a model change: the generated changelog is
REM create-only (changeSet id == table name) and cannot express an incremental
REM ALTER. Regenerating after a CSV change rewrites an already-applied changeSet,
REM so liquibase refuses to update (stored MD5 differs). These are local dev
REM databases with nothing worth keeping, so we drop and rebuild.
REM This stops working the day a database holds data worth preserving.
REM See README.md.txt.

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
    REM Bare EXIT /B: inside a block %ERRORLEVEL% would freeze at parse time.
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
REM Password comes from MYSQL_PWD (set by _Env.bat) to avoid the command-line warning.
REM The changelog has no charset, so the database default below decides it.
"%MYSQL_EXE%" -h "%DB_HOST%" -P %DB_PORT% -u "%DB_USER%" -e "DROP DATABASE IF EXISTS %DB_NAME%; CREATE DATABASE %DB_NAME% DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;"
IF ERRORLEVEL 1 (
    ECHO [ERROR] %DB_NAME% : drop/create failed
    EXIT /B 1
)

CALL "%~dp0_Liquibase.bat" %1 update
EXIT /B %ERRORLEVEL%
