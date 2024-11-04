param aiServiceName string
param aiServiceSku string = 'basic'
param keyVaultName string
param srchKeyName string

resource searchService 'Microsoft.Search/searchServices@2020-08-01' = {
  name: aiServiceName
  location: resourceGroup().location
  sku: {
    name: aiServiceSku
  }
  properties: {
    hostingMode: 'default'
  }
  identity: {
    type: 'SystemAssigned'
  }
}


resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}


resource aiSearchSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
 parent: keyVault
 name: srchKeyName
 properties: {
   value:  searchService.listAdminKeys().primaryKey
 }
} 

output endpoint string = 'https://${searchService.name}.search.windows.net'

