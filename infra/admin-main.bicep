param projectName string = 'sswrewardsadmin'
param location string = resourceGroup().location

@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string

@description('The name of the storage account to use for site hosting.')
param storageAccountName string = 'st${projectName}${environment}${uniqueString(resourceGroup().id)}'

@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_ZRS'
  'Premium_LRS'
])
@description('The storage account sku name.')
param storageSku string = 'Standard_LRS'

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: take(storageAccountName, 24)
  location: location
  kind: 'StorageV2'
  sku: {
    name: storageSku
  }
}

output storageAccountName string = storageAccount.name
output storageAccountUrl string = storageAccount.properties.primaryEndpoints.web
