@echo off
rem Publish iPhoto (see Publish.ps1). -DryRun lists what would be copied; -Target picks another folder.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Publish.ps1" %*
set rc=%errorlevel%
if "%~1"=="" pause
exit /b %rc%
