param projectName string
param environment string

@description('The location in which the Notification Hubs resources should be deployed.')
param location string = resourceGroup().location

param keyVaultName string
param now string

@secure()
@description('Specifies secrets to connect to apple https://docs.microsoft.com/en-us/azure/templates/microsoft.notificationhubs/namespaces/notificationhubs?tabs=bicep#apnscredentialproperties Sample: {"keyId":"key id","appName":"bundle id","appId":"team id","token":"token"}')
param appleCredential object

@secure()
param googleApiKey string

var namespaceName = 'ntf-${projectName}-${environment}'
var hubName = 'ntfns-${projectName}-${environment}'

resource namespace 'Microsoft.NotificationHubs/namespaces@2017-04-01' = {
  name: namespaceName
  location: location
  sku: {
    name: 'Free'
  }
}

resource notificationHub 'Microsoft.NotificationHubs/namespaces/notificationHubs@2017-04-01' = {
  name: hubName
  location: location
  parent: namespace
  properties: {
    apnsCredential: {
      properties: appleCredential
    }
    gcmCredential: {
      properties: {
        googleApiKey: googleApiKey
      }
    }
  }
}

module notificationHubNameSecret 'create-secrets.bicep' = {
  name: 'notificationHubNameSecret-${now}'
  params:{
    keyVaultName: keyVaultName
    secretName: 'NotificationHub--Name'
    secretValue: hubName
  }
}

module notificationHubConnectionStringSecret 'create-secrets.bicep' = {
  name: 'notificationHubConnectionStringSecret-${now}'
  params:{
    keyVaultName: keyVaultName
    secretName: 'NotificationHub--ConnectionString'
    secretValue: '${listKeys(resourceId('Microsoft.NotificationHubs/namespaces/notificationHubs/authorizationRules', namespaceName, hubName, 'DefaultFullSharedAccessSignature'), '2017-04-01').primaryConnectionString}'
  }
}
