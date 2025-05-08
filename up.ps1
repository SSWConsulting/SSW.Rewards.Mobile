Write-Host "🏗️ Creating SSW Rewards docker environment..."

Write-Host "Checking for .env file..."

if (-not(Test-Path "./.env")) {
    Write-Host "🚧 Setting up docker environment..."

    $homeFolderPath = Resolve-Path ~

    $certsPath = "$homeFolderPath/.aspnet/https"

    $devCertPath = "$certsPath/WebAPI.pfx"

    if (-not(Test-Path $devCertPath)) {
        Write-Host "WebApi Developer certificate not found."
        Write-Host "Run the following commands, then run this script again."

        Write-Host "dotnet dev-certs https --clean"

        if ($IsWindows) {
            Write-Host "dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\WebAPI.pfx -p ThisPassword"
        }
        else {
            Write-Host "dotnet dev-certs https -ep ${HOME}/.aspnet/https/WebAPI.pfx -p ThisPassword"
        }

        Write-Host "dotnet dev-certs https --trust"

        if ($IsWindows) {
            Write-Host ""
            Write-Host "⚠️ Check that %USERPROFILE%\.aspnet\https folder exists before running above commands (copy & paste path into File Explorer)"
            Write-Host "⚠️ Check that it didn't create cert in {{ GIT_REPO_PATH }}\%USERPROFILE%\.aspnet\https instead of the expected path"
        }
        
        exit -1
    }

    "CERTS_PATH=$certsPath" | Out-File -Encoding utf8 -FilePath "./.env"
}

Write-Host "✅ Done!"

Write-Host "🏗️ Building docker images..."

docker-compose build

Write-Host "✅ Done!"

Write-Host "▶️ Starting docker containers..."

docker compose --profile all up -d