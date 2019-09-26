param (
    [Parameter(Mandatory=$true)][string]$appCenterSecret
)

function ReplaceSecrets {
    param( [string]$templateName, [string]$outPath)
    $template = Join-Path "ReactNative\android\templates" $templateName
    (Get-Content -Path $template) `
        -replace "{APPCENTER_SECRET}", $appCenterSecret `
    | Set-Content -Path (Join-Path $outPath $templateName)
}

Write-Host "Modifying Android secrets"
$androidRoot = "ReactNative\android\app"

# copy/modify appcenter-config
ReplaceSecrets "appcenter-config.json" (Join-Path $androidRoot "src\main\assets\")
ReplaceSecrets "secrets.xml" (Join-Path $androidRoot "src\main\res\values\")