param projectName string
param environment string

@description('Storage Account type')
@allowed([
  'Premium_LRS'
  'Premium_ZRS'
  'Standard_GRS'
  'Standard_GZRS'
  'Standard_LRS'
  'Standard_RAGRS'
  'Standard_RAGZRS'
  'Standard_ZRS'
])
param storageAccountType string = 'Standard_LRS'

@description('Location for the storage account.')
param location string = resourceGroup().location

@description('The name of the Storage Account')
param storageAccountName string = 'st${projectName}${take(environment,4)}${take(uniqueString(subscription().id, resourceGroup().name), 8)}'

param keyVaultName string
param now string

resource sa 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: take(storageAccountName, 24)
  location: location
  sku: {
    name: storageAccountType
  }
  kind: 'StorageV2'
  properties: {
  }
}

module accountKeySecret 'create-secrets.bicep' = {
  name: 'accountKeySecret-${now}'
  params:{
    keyVaultName: keyVaultName
    secretName: 'ContentStorageConnectionString'
    secretValue: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};AccountKey=${sa.listKeys().keys[0].value}'
  }
}

output storageAccountName string = sa.name
output storageAccountId string = sa.id
