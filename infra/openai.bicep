@description('Name of the resource.')
param name string
@description('Location to deploy the resource. Defaults to the location of the resource group.')
param location string = resourceGroup().location
param managedIdentityId string
param keyVaultName string
param aoaiKeyName string


@description('List of model deployments.')
param deployments array = [
  {
    name:  'gpt-4o'
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
    }
    sku: {
      name: 'Standard'
      capacity:40
    }
  }
  {
    name: 'text-embedding-ada-002'
    model: {
      format: 'OpenAI'
      name: 'text-embedding-ada-002'
    }
    sku: {
      name: 'Standard'
      capacity: 40
    }
  }
]
@description('Whether to enable public network access. Defaults to Enabled.')
@allowed([
  'Enabled'
  'Disabled'
])
param publicNetworkAccess string = 'Enabled'

resource aoaiService 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = {
  name: name
  location: location
  kind: 'OpenAI'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    customSubDomainName: toLower(name)
    publicNetworkAccess: publicNetworkAccess
  }
  sku: {
    name: 'S0'
  }
}


resource deployment 'Microsoft.CognitiveServices/accounts/deployments@2023-10-01-preview' = [for deployment in deployments: {
  parent: aoaiService
  name: deployment.name
  properties: {
    model: deployment.?model ?? null
    raiPolicyName: deployment.?raiPolicyName ?? null
  }
  sku: deployment.?sku ?? {
    name: 'Standard'
    capacity: 50
  }
}]


resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

var aoaiKey = aoaiService.listKeys().key1
resource aoaiSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
 parent: keyVault
 name: aoaiKeyName
 properties: {
   value:  aoaiKey
 }
} 

@description('ID for the deployed Cognitive Services resource.')
output id string = aoaiService.id
@description('Name for the deployed Cognitive Services resource.')
output name string = aoaiService.name
@description('Endpoint for the deployed Cognitive Services resource.')
output endpoint string = aoaiService.properties.endpoint
@description('Host for the deployed Cognitive Services resource.')
output host string = split(aoaiService.properties.endpoint, '/')[2]
