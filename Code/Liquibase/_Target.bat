@ECHO OFF
REM Resolves a target name to DB name / changelog file.  ASCII ONLY (see _Env.bat).
REM Usage: CALL _Target.bat <Auth|User|Center>
REM Sets:  DB_NAME, CHANGELOG

REM Clear first: these scripts do not use SETLOCAL, so a previous call's value
REM would otherwise survive and be mistaken for this call's result.
SET "DB_NAME="
SET "CHANGELOG="

IF /I "%~1"=="Auth"   ( SET "DB_NAME=AuthDb"   & SET "CHANGELOG=CreateLog_Auth.json"   )
IF /I "%~1"=="User"   ( SET "DB_NAME=UserDb"   & SET "CHANGELOG=CreateLog_User.json"   )
IF /I "%~1"=="Center" ( SET "DB_NAME=CenterDb" & SET "CHANGELOG=CreateLog_Center.json" )

IF NOT DEFINED DB_NAME (
    ECHO [ERROR] unknown target: "%~1"   ^(expected: Auth ^| User ^| Center^)
    EXIT /B 1
)

EXIT /B 0
