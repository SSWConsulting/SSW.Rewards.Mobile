param (
    [Parameter(Mandatory=$true)][string]$appCenterSecret
)

function ReplaceSecrets {
    param( $templatePath )
    $outPath = $templatePath -replace ".template.",".";
    (Get-Content -Path $templatePath) `
        -replace "{APPCENTER_SECRET}", $appCenterSecret `
    | Set-Content -Path $outPath
}

Write-Host "Modifying Android secrets"
$androidRoot = "ReactNative\android\app"

# copy/modify appcenter-config
ReplaceSecrets (Join-Path $androidRoot "src\main\assets\appcenter-config.template.json")
ReplaceSecrets (Join-Path $androidRoot "src\main\assets\secrets.template.xml")
Move-Item -Path (Join-Path $androidRoot "src\main\assets\secrets.xml") -Destination (Join-Path $androidRoot "src\main\res\values\secrets.xml")