@echo off
setlocal
cd /d "%~dp0"
if not exist "Terraria.exe" (
  echo Patched Terraria.exe was not found.
  echo Run scripts\Build-Client.ps1 from the repository first.
  pause
  exit /b 1
)
start "" "%~dp0Terraria.exe" -savedirectory "%~dp0Saves"
