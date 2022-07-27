param projectName string
param location string = resourceGroup().location
param environment string

param appServicePlanName string

resource hostingPlan 'Microsoft.Web/serverfarms@2022-03-01' existing = {
  name: appServicePlanName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'ai-${projectName}-${environment}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: 'app-${projectName}-api-${environment}'
  location: location
  properties: {
    serverFarmId: hostingPlan.id
    httpsOnly: true
    siteConfig: {
      alwaysOn: true
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
          name: 'XDT_MicrosoftApplicationInsights_Java'
          value: '1'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'recommended'
        }
      ]
    }
  }
}
