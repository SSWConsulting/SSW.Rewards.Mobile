param keyVaultName string

param secretName string

@secure()
param secretValue string

resource kv 'Microsoft.KeyVault/vaults@2021-10-01' existing = {
  name: keyVaultName
}

resource secret 'Microsoft.KeyVault/vaults/secrets@2021-04-01-preview' = {
  name: secretName
  parent: kv
  properties: {
    value: secretValue
  }
}

output secretUriWithVersion string = secret.properties.secretUriWithVersion
