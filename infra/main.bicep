targetScope = 'subscription'


param resourceGroupName string
param location string 
param aiServiceName string
param aiServiceSku string = 'basic'
param keyVaultName string
param sqlServerName string
param sqlAdminUsername string
@secure()
param sqlAdminPassword string
param sqlPasswordSecretName string
param currentUserGuid string
param aoaiServiceName string
param miName string
param aoaiLocation string
param currentIpAddress string
param aoaiKeyName string
param srchKeyName string



resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-07-01' = {
  name: resourceGroupName
  location: location
}

module aisearch 'aisearch.bicep' = {
  name: aiServiceName
  scope: resourceGroup
  params: {
    aiServiceName: aiServiceName
    aiServiceSku: aiServiceSku
    srchKeyName: srchKeyName
    keyVaultName: keyVaultName
  }
}

module sqlserver 'sqlserver.bicep' = {
  name: sqlServerName
  scope: resourceGroup
  params: {
    location: location
    sqlServerName: sqlServerName
    sqlAdminPassword: sqlAdminPassword
    sqlAdminUsername: sqlAdminUsername
    sqlPasswordSecretName: sqlPasswordSecretName
    keyVaultName: keyVaultName
    currentIpAddress : currentIpAddress
    miName: miName

  }
  dependsOn:[
      keyvault
]
}

//add keyvault
module keyvault 'keyvault.bicep' = {
  name: keyVaultName
  scope: resourceGroup
  params: {
    keyVaultName: keyVaultName
    location: location
    currentUserGuid: currentUserGuid
  }
}

module openai 'openai.bicep' = {
  name: aoaiServiceName
  scope: resourceGroup
  params: {
    managedIdentityId: managedIdentity.outputs.id
    name: aoaiServiceName
    location: aoaiLocation
    keyVaultName: keyVaultName
    aoaiKeyName: aoaiKeyName
    
  }
}
module managedIdentity 'managed-identity.bicep' = {
	name: miName
	scope: resourceGroup
	params: {
		name: miName
		location: location
	}
}

module roleassignments 'roleassignments.bicep' = {
  name: 'roleassignments'
  scope: resourceGroup
  params: {
    currentUserObjectId: currentUserGuid
  }
}


output aoaiEndpoint string = openai.outputs.endpoint
output keyVaultName string = keyvault.outputs.vaultName
output aiSearchEndpoint string = aisearch.outputs.endpoint
