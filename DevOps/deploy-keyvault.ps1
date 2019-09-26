param(
	[Parameter(Mandatory = $true)]
	[ValidateNotNullOrEmpty()][string]$ResourceGroupName,
	
	[Parameter(Mandatory = $true)]
	[ValidateNotNullOrEmpty()][string]$Location,
	
	[Parameter(Mandatory = $true)]
	[ValidateNotNullOrEmpty()][string]$Environment,
	
	[Parameter(Mandatory = $true)]
	[ValidateNotNullOrEmpty()][string]$Project
	
)

$keyVaultName = "$($Project.ToLower())-$($Environment.ToLower())".Replace(".", "");
$modifiedDate = (Get-Date).ToString('yyyy/MM/dd')

Write-Host "Creating KeyVault: $($keyVaultName)" -ForegroundColor Yellow
az keyvault create --name $keyVaultName --resource-group $ResourceGroupName --location $Location --enabled-for-template-deployment true --tags project=$Project environment=$Environment modifiedDate=$modifiedDate

Write-Host "Getting latest deployment for $($ResourceGroupName)" -ForegroundColor Yellow
$latestDeployments = (az group deployment list -g $ResourceGroupName --top 1) | ConvertFrom-Json
if (!$latestDeployments) {
	throw "Latest Deployments could not be found for Resource Group '$ResourceGroupName'."
}

if (!$latestDeployments[0]) {
	throw "Latest Deployment for Resource Group '$ResourceGroupName' is invalid!"
}

Write-Host "Got latest deployment..." -ForegroundColor Yellow
$lastDeployment = $latestDeployments[0].properties.outputs
if (!$lastDeployment) {
	throw "Lates deployment did not have any outputs!"
}

Write-Host "Adding secrets to KeyVault..." -ForegroundColor Yellow
$lastDeployment.PSObject.Properties | foreach-object {
	$field = $_.Name

	if($field.StartsWith("secret_")){
		$key = $field.Replace('secret_', '')
		$value = $_.Value.value
		Write-Host "Adding to keyvault: $($key)" -ForegroundColor Red
		az keyvault secret set --vault-name $keyVaultName --name $key --value $value | Out-Null
	}
}

Write-Host "Applying Access Policies to KeyVault..." -ForegroundColor Yellow

# TODO: Maybe we need to pull the appname from the arm template outputs?
# For now, recreate the app name just like it is done in the ARM template.
$appName = "$($Project.ToLower())-$($Environment.ToLower())".Replace(".", "");

$appIdentity = az webapp identity show -g $ResourceGroupName -n $appName | ConvertFrom-Json

az keyvault set-policy -g $ResourceGroupName -n $keyVaultName --object-id $appIdentity.principalId --secret-permissions list get
