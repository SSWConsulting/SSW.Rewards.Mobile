@echo off
setlocal

set configProject=C:\Users\jpr17\src\Internal\SSW.Rewards\API\SSW.Rewards\SSW.Rewards.WebAPI.csproj
set dbProject=C:\Users\jpr17\src\Internal\SSW.Rewards\API\SSW.Rewards.Persistence\SSW.Rewards.Persistence.csproj

dotnet ef %* --startup-project "%configProject%" --project "%dbProject%"

endlocal
