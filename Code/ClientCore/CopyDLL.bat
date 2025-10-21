@echo off
setlocal

set "SRC_FOLDER=%~dp0bin\Debug\netstandard2.1"
set "DST_FOLDER=%~dp0..\..\Client\Assets\Plugins"

set FILES=ClientCore.dll Proto.dll Protocol.dll

echo Copying DLLs from "%SRC_FOLDER%" to "%DST_FOLDER%" ...

for %%F in (%FILES%) do (
    if exist "%SRC_FOLDER%\%%F" (
        echo Copying %%F ...
        robocopy "%SRC_FOLDER%" "%DST_FOLDER%" "%%F" /NFL /NDL /NJH /NJS /NC /NS /NP /R:0 /W:0 /XO
    ) else (
        echo File not found: "%SRC_FOLDER%\%%F"
    )
)

echo All done.
endlocal