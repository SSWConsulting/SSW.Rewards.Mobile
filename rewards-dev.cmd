@echo off
rem rewards-dev — wrapper so you can type `rewards-dev <cmd>` from the repo root instead
rem of the long `dotnet run --project tools\RewardsDev -- <cmd>`. Run `rewards-dev help`.
setlocal
dotnet run --project "%~dp0tools\RewardsDev" --verbosity quiet -- %*
exit /b %ERRORLEVEL%
