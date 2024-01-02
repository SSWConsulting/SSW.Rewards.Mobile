// TIP: to maintain Kv Access Policies:
// https://ochzhen.com/blog/key-vault-access-policies-using-azure-bicep 

param keyVaultName string
param tenantId string
param objectId string

resource kv 'Microsoft.KeyVault/vaults@2021-10-01' existing = {
  name: keyVaultName
}

resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2019-09-01' = {
  name: 'add'
  parent: kv
  properties: {
    accessPolicies: [
      {
        tenantId: tenantId
        objectId: objectId
        permissions: {
          keys: [
            'get'
            'list'
          ]
          secrets: [
            'list'
            'get'
          ]
        }
      }
    ]
  }
}
