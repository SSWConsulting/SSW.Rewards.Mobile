param projectName string
param location string = resourceGroup().location
param environment string

@description('Specifies the SKU to use for the key vault.')
param keyVaultSku object = {
  name: 'standard'
  family: 'A'
}

var keyVaultName = 'kv-${projectName}-${environment}'

resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' = {
  name: keyVaultName
  location: location
  properties: {
    enabledForDeployment: true
    enableRbacAuthorization: true
    tenantId: tenant().tenantId
    sku: keyVaultSku
    accessPolicies: [
    ]
  }
}

output keyVaultName string = keyVault.name
output keyVaultId string = keyVault.id
