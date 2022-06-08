@echo off
setlocal

set configProject=SSW.Consulting\SSW.Consulting.WebAPI.csproj
set dbProject=SSW.Consulting.Persistence\SSW.Consulting.Persistence.csproj

dotnet ef %* --startup-project "%configProject%" --project "%dbProject%"

endlocal
