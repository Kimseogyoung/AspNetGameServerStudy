@ECHO OFF
SET DIR_PATH=%~dp0
SET DIR_PATH=%DIR_PATH:~0,-1%
SET PROJ_PATH=%DIR_PATH%\..\Code\Server
SET DIST_PATH=%DIR_PATH%\..\Dist\Server
CD %PROJ_PATH%
CALL dotnet publish ./Server.csproj -c Release -o %DIST_PATH%
POPD