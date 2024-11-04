
param currentUserObjectId string

var deploymentEntropy = '3F2504E0-4F89-11D3-9A0C-0305E82C3302'
var roles = loadJsonContent('constants/roles.json')


resource keyVaultSecretUser 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: resourceGroup()
  name: roles.keyVaultSecretsUser
}

//Key Vault Secrests 
resource keyVaultsSecretAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' =  {
  name: guid(currentUserObjectId, keyVaultSecretUser.id, deploymentEntropy)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: keyVaultSecretUser.id
    principalId: currentUserObjectId
  }
}



