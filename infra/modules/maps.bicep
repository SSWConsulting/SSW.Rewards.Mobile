param projectName string
param location string = resourceGroup().location
param environment string
param keyVaultName string
param now string

var mapsAccountName = 'maps-${projectName}-${environment}'

@description('The pricing tier SKU for the account.')
@allowed([
  'G2'
])
param pricingTier string = 'G2'

@description('The pricing tier for the account.')
@allowed([
  'Gen2'
])
param kind string = 'Gen2'

resource mapsAccount 'Microsoft.Maps/accounts@2023-06-01' = {
  name: mapsAccountName
  location: location
  sku: {
    name: pricingTier
  }
  kind: kind
}

module mapsApiKey 'create-secrets.bicep' = {
  name: 'azureMapsKey-${now}'
  params: {
    keyVaultName: keyVaultName
    secretName: 'AzureMapsKey'
    secretValue: mapsAccount.listKeys().primaryKey
  }
}
