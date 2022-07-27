
param projectName string = 'sswrewards'
param location string = resourceGroup().location

@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string

param appServicePlanName string

param sqlAdministratorsLoginName string
param sqlAdministratorsObjectId string

@secure()
@description('Specifies secrets to connect to apple https://docs.microsoft.com/en-us/azure/templates/microsoft.notificationhubs/namespaces/notificationhubs?tabs=bicep#apnscredentialproperties Sample: {"keyId":"key id","appName":"bundle id","appId":"team id","token":"token"}')
param notificationHubAppleCredential object = {}

@secure()
param notificationHubGoogleApiKey string = ''

param now string = utcNow('yyyyMMddHHmmss')

module keyVault 'modules/keyvault.bicep' = {
  name: 'kv-${now}'
  params: {
    projectName: projectName
    environment: environment
    location: location
  }
}

module sqlServer 'modules/sql.bicep' = {
  name: 'sql-${now}'
  params: {
    projectName: projectName
    environment: environment
    location: location
    sqlAdministratorsLoginName: sqlAdministratorsLoginName
    sqlAdministratorsObjectId: sqlAdministratorsObjectId
    keyVaultName: keyVault.outputs.keyVaultName
    now: now
  }
}

module appService 'modules/webapp.bicep' = {
  name: 'api-${now}'
  params: {
    appServicePlanName: appServicePlanName
    environment: environment
    location: location
    projectName: projectName
  }
}

module storage 'modules/storage.bicep' = {
  name: 'st-${now}'
  params: {
    environment: environment
    location: location
    projectName: projectName
    keyVaultName: keyVault.outputs.keyVaultName
    now: now
  }
}

module notificationhub 'modules/notificationhub.bicep' = {
  name: 'nh-${now}'
  params: {
    environment: environment
    location: location
    projectName: projectName
    keyVaultName: keyVault.outputs.keyVaultName
    now: now
    appleCredential: notificationHubAppleCredential
    googleApiKey: notificationHubGoogleApiKey
  }
}
