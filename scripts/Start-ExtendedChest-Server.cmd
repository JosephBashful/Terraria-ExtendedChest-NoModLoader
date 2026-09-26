@echo off
setlocal
pushd "%~dp0"
if not exist "TerrariaServer.exe" (
  echo Patched TerrariaServer.exe was not found.
  echo Run scripts\Build-Server.ps1 from the repository first.
  pause
  exit /b 1
)
if not exist "Saves" mkdir "Saves"
"%~dp0TerrariaServer.exe" -savedirectory "%~dp0Saves" %*
set "exitCode=%errorlevel%"
popd
exit /b %exitCode%
