@ECHO OFF
REM 스키마 적용. 사용법: Update.bat [Auth|User|Center|All]   (기본 All)
REM
REM 주의: 생성 changelog 는 changeSet id 가 테이블명인 create-only 구조다.
REM CSV 를 고쳐 changelog 를 재생성하면 이미 적용된 changeSet 의 내용이 바뀌므로
REM 여기서 checksum 검증에 걸린다. 그때는 Recreate.bat 을 쓴다.

CALL "%~dp0_Env.bat"
IF ERRORLEVEL 1 EXIT /B 1

SET "TARGET=%~1"
IF "%TARGET%"=="" SET "TARGET=All"

IF /I NOT "%TARGET%"=="All" (
    REM 블록 안에서는 %ERRORLEVEL% 이 블록 파싱 시점 값으로 굳으므로 인자 없이 EXIT /B 한다.
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
