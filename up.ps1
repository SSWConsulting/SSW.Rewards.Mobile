Write-Host "🏗️ Creating SSW Rewards docker environment..."

Write-Host "Checking for .env file..."

if (-not(Test-Path "./.env")) {
    Write-Host "🚧 Setting up docker environment..."

    $homeFolderPath = Resolve-Path ~

    $certsPath = "$homeFolderPath/.aspnet/https"

    $devCertPath = "$certsPath/WebAPI.pfx"

    if (-not(Test-Path $devCertPath)) {
        Write-Host "Developer certificate not found."
        Write-Host "Run the followin commands, then run this script again."

        if ($IsWindows) {
            Write-Host "dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\WebAPI.pfx -p ThisPassword"
        }
        else {
            Write-Host "dotnet dev-certs https -ep ${HOME}/.aspnet/https/WebAPI.pfx -p ThisPassword"
        }

        Write-Host "dotnet dev-certs https --trust"

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