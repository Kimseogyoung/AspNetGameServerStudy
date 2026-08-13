@ECHO OFF
REM Loads connection settings from .env. Every script CALLs this first.
REM See .env.example. .env is git-ignored.
REM
REM ASCII ONLY in this file (comments included). A UTF-8 batch file read by a
REM CP949 console can have a Korean byte swallow the line break, which turns a
REM REM comment into an executed command. Korean notes live in README.md.txt.

SET "ENV_FILE=%~dp0.env"
IF NOT EXIST "%ENV_FILE%" (
    ECHO [ERROR] .env not found: %ENV_FILE%
    ECHO         Copy the sample first:  copy .env.example .env
    EXIT /B 1
)

REM eol=# skips comment lines; for/f skips blank lines by itself.
REM
REM Do NOT enable delayed expansion here. It would silently eat "!" inside the
REM password, and the resulting auth failure is hard to trace.
FOR /F "usebackq eol=# tokens=1,* delims==" %%A IN ("%ENV_FILE%") DO (
    IF NOT "%%A"=="" SET "%%A=%%B"
)

IF NOT DEFINED DB_HOST   SET "DB_HOST=localhost"
IF NOT DEFINED DB_PORT   SET "DB_PORT=3306"
IF NOT DEFINED MYSQL_EXE SET "MYSQL_EXE=C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"

IF NOT DEFINED DB_USER (
    ECHO [ERROR] DB_USER is missing in .env
    EXIT /B 1
)

REM DB_PASSWORD is not checked: an empty password is legitimate, and in batch
REM SET "K=" deletes the variable rather than defining it, so "empty" and
REM "unset" cannot be told apart.
REM mysql.exe gets it via MYSQL_PWD to avoid the command-line exposure warning.
SET "MYSQL_PWD=%DB_PASSWORD%"

EXIT /B 0
