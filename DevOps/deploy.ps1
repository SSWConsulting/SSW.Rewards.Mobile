# Follow all the steps from https://markheath.net/post/create-service-principal-azure-cli to create your own service principal

# Here are a couple of other useful links
# https://lnx.azurewebsites.net/non-interactive-login-in-azure-cli-2-0/
# https://docs.microsoft.com/bs-latn-ba/cli/azure/authenticate-azure-cli?view=azure-cli-latest

param(
	[Parameter(Mandatory = $true)]
	[ValidateNotNullOrEmpty()][string]$ResourceGroupName,
	
	[Parameter(Mandatory = $true)]
	[ValidateNotNullOrEmpty()][string]$Location,

	[Parameter(Mandatory = $true)]
	[ValidateNotNullOrEmpty()][string]$Environment,
	
	[Parameter(Mandatory = $true)]
	[ValidateNotNullOrEmpty()][string]$Project,
	
	[Parameter(Mandatory = $true)]
	[ValidateNotNullOrEmpty()][string]$TemplateFile,

	[Parameter(Mandatory = $true)]
	[ValidateNotNullOrEmpty()][string]$ParametersFile
)

$credsFile = "azure.json"

if(Test-Path $credsFile) {
	$azure = Get-Content -Raw -Path $credsFile | ConvertFrom-Json
}
else {
	$azure = @{ 
		username = $null;
		password = $null;
		subscription = $null; 
		tenant = $null;
	};
}

if([string]::IsNullOrWhiteSpace($azure.username)) {
	$azure.username = Read-Host 'Username'
}

if([string]::IsNullOrWhiteSpace($azure.password)) {
	$azure.password = Read-Host 'Password' -AsSecureString
	$azure.password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($azure.password))
}

if([string]::IsNullOrWhiteSpace($azure.subscription)) {
	$azure.subscription = Read-Host "Subscription ID"
}

if([string]::IsNullOrWhiteSpace($azure.tenant)) {
	$azure.tenant = Read-Host "Tenant  ID"
}

Write-Host "Logging into Azure..." -ForegroundColor Cyan
az account clear

# Write-host "Username: $($azure.username)" -ForegroundColor Cyan
# Write-host "Password: $($azure.password)" -ForegroundColor Cyan
# Write-host "Tenant: $($azure.tenant)" -ForegroundColor Cyan

az login --service-principal -u $azure.username --password $azure.password --tenant $azure.tenant | Out-Null 
$login = az account show | ConvertFrom-Json

# if the login object is null, then login failed
$loginJson = $login | ConvertTo-Json
if ($null -eq $login -or [string]::IsNullOrWhiteSpace($loginJson)) {
	Write-Host "Unable to log into Azure :(~" -ForegroundColor Red
	Write-Host "Run 'az login' and then try deploying again." -ForegroundColor Red
	exit 1;
}

Write-Host "Logged into Azure" -ForegroundColor Cyan

Write-Host "Selecting your subscription" -ForegroundColor Cyan

az account set -s $azure.subscription
if(1 -eq $LASTEXITCODE)  {
	Write-Host "Unable to set the subscription :(~" -ForegroundColor Red
	Write-Host "Re-run the setup-servicePrinciple.ps1 to generate a valid configuration." -ForegroundColor Red
	exit 1;
}

.\deploy-template.ps1 `
	-TemplateFile $TemplateFile `
	-ParametersFile $ParametersFile `
	-SubscriptionId $azure.subscription `
	-ResourceGroupName $ResourceGroupName `
	-Location $Location