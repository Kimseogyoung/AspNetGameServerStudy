@ECHO OFF
REM 대상 이름 -> DB 이름 / changelog 파일 해석.
REM 사용법: CALL _Target.bat <Auth|User|Center>
REM 결과: DB_NAME, CHANGELOG

REM 직전 호출 값이 남아 오판하는 것을 막는다(이 배치들은 SETLOCAL 을 쓰지 않는다).
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
