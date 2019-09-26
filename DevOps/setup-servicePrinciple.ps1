param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()][string]$subscriptionId,
    
    # a name for our azure ad app
    [ValidateNotNullOrEmpty()][string]$appName = "AzureDeploySP"
)

Write-Host "Selecting Subscription $($subscriptionId)" -ForegroundColor Gray

# select correct subscription
az account set -s $subscriptionId
if(1 -eq $LASTEXITCODE)  {
	Write-Host "Unable to set the subscription :(~" -ForegroundColor Red
	Write-Host "Run 'az login' and select your Azure account." -ForegroundColor Red
	exit 1;
}

Write-Host "Creating AD App" -ForegroundColor Gray
# create an Azure AD app
az ad app create --display-name $appName --homepage "http://localhost/$($appName)" --identifier-uris "http://localhost/$($appName)"

# create a SP for the AD App
$sp = az ad sp create-for-rbac -n "https://$($appName)" | ConvertFrom-Json
Write-Host "SP.AppId: $($sp.appId)" -ForegroundColor Green
Write-Host "SP.DisplayName: $($sp.displayName)" -ForegroundColor Green
Write-Host "SP.Name: $($sp.name)" -ForegroundColor Green
Write-Host "SP.Password: $($sp.password)" -ForegroundColor Green
Write-Host "SP.Tenant: $($sp.tenant)" -ForegroundColor Green

Write-Host "Getting Service Principle appID" -ForegroundColor Cyan
# get the app id of the service principal
$servicePrincipalAppId = az ad sp list --display-name $appName --query "[].appId" | ConvertFrom-Json
Write-Host "Service Principle AppID: $($servicePrincipalAppId)" -ForegroundColor Gray
Write-Host "Service Principle AppID: $($servicePrincipalAppId[0])" -ForegroundColor Gray
Write-Host "Service Principle AppID: $($servicePrincipalAppId[1])" -ForegroundColor Gray

# get the tenant id
Write-Host "Getting Service Principle Tenant ID" -ForegroundColor Cyan
$tenantId = $(az account show --query tenantId -o tsv)
Write-Host "Service Principle Tenant AppID: $($tenantId)" -ForegroundColor Gray

# save all the stuff for next time
$credsFile = "$($PSScriptRoot)\azure.json"

@{ 
    "username"="$($servicePrincipalAppId[0])"; 
    "password"="$($sp.password)"; 
    "tenant"="$($tenantId)"; 
    "subscription"="$($subscriptionId)" 
} | ConvertTo-Json | Set-Content $credsFile
