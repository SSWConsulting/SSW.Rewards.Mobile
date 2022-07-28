@echo off
setlocal

set configProject=SSW.Rewards\SSW.Rewards.WebAPI.csproj
set dbProject=SSW.Rewards.Persistence\SSW.Rewards.Persistence.csproj

dotnet ef %* --startup-project "%configProject%" --project "%dbProject%"

endlocal
