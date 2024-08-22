param projectName string
param location string = resourceGroup().location
param environment string
param keyVaultName string
param appServicePlanId string
param adminPortalUrl string
param idsUrl string
param sqlConnectionStringSecretUriWithVersion string
param hangfireSqlConnectionStringSecretUriWithVersion string
param mapsApiKeySecretUriWithVersion string
param logAnalyticsWorkspaceId string 

var quizGptUrl = 'https://wapp-ssw-quizgpt-prod.azurewebsites.net/Quiz/SubmitAnswer'

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'ai-${projectName}-${environment}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspaceId
  }
}

resource kv 'Microsoft.KeyVault/vaults@2021-10-01' existing = {
  name: keyVaultName
}

resource notificationHubConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' existing = {
  parent: kv
  name: 'NotificationHub--ConnectionString'
}

resource notificationHubNameSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' existing = {
  parent: kv
  name: 'NotificationHub--Name'
}

resource contentStorageConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' existing = {
  parent: kv
  name: 'ContentStorageConnectionString'
}

resource graphClientSecretSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' existing = {
  parent: kv
  name: 'GraphClientSecret'
}

resource graphTenantSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' existing = {
  parent: kv
  name: 'GraphTenantId'
}

resource graphClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' existing = {
  parent: kv
  name: 'GraphClientId'
}

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-${projectName}-api-${environment}'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v6.0'
      alwaysOn: true
      connectionStrings: [
        {
          name: 'DefaultConnection'
          type: 'SQLServer'
          connectionString: '@Microsoft.KeyVault(SecretUri=${sqlConnectionStringSecretUriWithVersion})'
        }
        {
          name: 'HangfireConnection'
          type: 'SQLServer'
          connectionString: '@Microsoft.KeyVault(SecretUri=${hangfireSqlConnectionStringSecretUriWithVersion})'
        }
      ]
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'APPINSIGHTS_PROFILERFEATURE_VERSION'
          value: '1.0.0'
        }
        {
          name: 'APPINSIGHTS_SNAPSHOTFEATURE_VERSION'
          value: '1.0.0'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~2'
        }
        {
          name: 'DiagnosticServices_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'InstrumentationEngine_EXTENSION_VERSION'
          value: '~1'
        }
        {
          name: 'SnapshotDebugger_EXTENSION_VERSION'
          value: '~1'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_BaseExtensions'
          value: '~1'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'recommended'
        }
        {
          name: 'KeyVaultName'
          value: keyVaultName
        }
        {
          name: 'NotificationHub__ConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=${notificationHubConnectionStringSecret.properties.secretUri})'
        }
        {
          name: 'NotificationHub__Name'
          value: '@Microsoft.KeyVault(SecretUri=${notificationHubNameSecret.properties.secretUri})'
        }
        {
          name: 'CloudBlobProviderOptions__ContentStorageConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=${contentStorageConnectionStringSecret.properties.secretUri})'
        }
        {
          name: 'GraphSenderOptions__ClientId'
          value: '@Microsoft.KeyVault(SecretUri=${graphClientIdSecret.properties.secretUri})'
        }
        {
          name: 'GraphSenderOptions__Secret'
          value: '@Microsoft.KeyVault(SecretUri=${graphClientSecretSecret.properties.secretUri})'
        }
        {
          name: 'GraphSenderOptions__TenantId'
          value: '@Microsoft.KeyVault(SecretUri=${graphTenantSecret.properties.secretUri})'
        }
        {
          name: 'SMTPSettings__DefaultSender'
          value: 'info@ssw.com.au'
        }
        {
          name: 'SMTPSettings__DefaultSenderName'
          value: 'SSW'
        }
        {
          name: 'UserServiceOptions__StaffSmtpDomain'
          value: 'ssw.com.au'
        }
        {
          name: 'AllowedOrigin'
          value: adminPortalUrl
        }
        {
          name: 'SigningAuthority'
          value: idsUrl
        }
        {
          name: 'GPTServiceOptions__Url'
          value: quizGptUrl
        }
        {
          name: 'AzureMaps__Key'
          value: '@Microsoft.KeyVault(SecretUri=${mapsApiKeySecretUriWithVersion})'
        }
      ]
    }
  }
}

output appServiceName string = appService.name
output tenantId string = appService.identity.tenantId
output principalId string = appService.identity.principalId
