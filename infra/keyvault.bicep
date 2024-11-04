
param location string = resourceGroup().location
param keyVaultName string
param currentUserGuid string


var roles = loadJsonContent('constants/roles.json')

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' ={
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
  }
}

resource keyVaultSecretsOfficer 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: resourceGroup()
  name: roles.keyVaultSecretsOfficer
}

resource keyVaultSecretsOfficerAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().name, 'keyVaultSecretsOfficer', currentUserGuid)
  properties: {
    roleDefinitionId: keyVaultSecretsOfficer.id
    principalId: currentUserGuid
    principalType: 'User'
  }
}

output vaultName string = keyVault.name
