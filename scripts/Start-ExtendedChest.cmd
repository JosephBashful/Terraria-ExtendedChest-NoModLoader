@echo off
setlocal
pushd "%~dp0"
if not exist "Terraria.exe" (
  echo Patched Terraria.exe was not found.
  echo Run scripts\Build-Client.ps1 from the repository first.
  pause
  exit /b 1
)
"%~dp0Terraria.exe" -savedirectory "%~dp0Saves"
set "exitCode=%errorlevel%"
popd
exit /b %exitCode%
