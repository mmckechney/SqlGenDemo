param location string = resourceGroup().location
param sqlServerName string
param sqlAdminUsername string
@secure()
param sqlAdminPassword string
param sqlPasswordSecretName string
param keyVaultName string
param miName string
param currentIpAddress string

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: miName
}

resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
    minimalTlsVersion: '1.2'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'Application'
      login: managedIdentity.name
      sid: managedIdentity.properties.clientId
      tenantId: subscription().tenantId
    }
  }
}

resource sqlserverFirewallRule 'Microsoft.Sql/servers/firewallRules@2021-11-01' = if(currentIpAddress != '') {
  parent: sqlServer
  name: '${sqlServer.name}_AllowIp'
  properties: {
    startIpAddress: currentIpAddress
    endIpAddress: currentIpAddress
  }
}

resource sqlserver_Pool 'Microsoft.Sql/servers/elasticPools@2021-11-01' = {
  parent: sqlServer
  name: '${sqlServer.name}-pool'  
  location: location
  sku: {
    name: 'BasicPool'
    tier: 'Basic'
    capacity: 50
  }
  properties: {
    maxSizeBytes: 5242880000
    perDatabaseSettings: {
      minCapacity: 0
      maxCapacity: 5
    }
    zoneRedundant: false
  }
}

//create a database in the server
resource sqlserverDatabase 'Microsoft.Sql/servers/databases@2021-11-01' = {
  parent: sqlServer
  name: 'SampleDb'
  location: location
   sku: {
    name: 'ElasticPool'
    tier: 'Basic'
    capacity: 0
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    elasticPoolId: sqlserver_Pool.id
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Geo'
    isLedgerOn: false
  }
}


 resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
   name: keyVaultName
 }

 resource sqlAdminSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: sqlPasswordSecretName
  properties: {
    value:  sqlAdminPassword
  }
} 


