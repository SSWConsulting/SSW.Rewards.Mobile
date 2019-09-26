param (
    [Parameter(Mandatory=$true)][string]$appCenterSecret
)

function ReplaceSecrets {
    param( [string]$templateName, [string]$outPath)
    $template = (Join-Path "ReactNative\android\templates" $templateName);
    $templateName = $templateName -replace ".template.","."
    (Get-Content -Path $template) `
        -replace "{APPCENTER_SECRET}", $appCenterSecret `
    | Set-Content -Path (Join-Path $outPath $templateName);
}

Write-Host "Modifying Android secrets"
$androidRoot = "ReactNative\android\app"

# copy/modify appcenter-config
ReplaceSecrets "appcenter-config.template.json" (Join-Path $androidRoot "src\main\assets\")
ReplaceSecrets "secrets.template.xml" (Join-Path $androidRoot "src\main\res\values\")