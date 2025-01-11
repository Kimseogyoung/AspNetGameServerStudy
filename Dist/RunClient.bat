@ECHO OFF
SET DIR_PATH=%~dp0
SET DIR_PATH=%DIR_PATH:~0,-1%
SET PROJ_PATH=%DIR_PATH%\..\Code\Client
SET DIST_PATH=%DIR_PATH%\..\Dist\Client
PUSHD %DIST_PATH%
CALL dotnet Client.dll "../../Data/Csv/Proto"
POPD