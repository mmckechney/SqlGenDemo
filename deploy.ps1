param(
    [string] $appName,
    [string] $location,
    [string] $aoaiLocation
)

function Get-SecurePassword {
    param(
        [int]$length
    )

    # Define character sets for the password
    $characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@?#"

    # Create a byte array to hold random bytes
    $byteArray = New-Object 'System.Byte[]' $length

    # Create a random number generator
    $rng = [System.Security.Cryptography.RNGCryptoServiceProvider]::Create()

    # Fill the byte array with random bytes
    $rng.GetBytes($byteArray)

    # Generate the password
    $password = -join ($byteArray | ForEach-Object { $characters[($_ % $characters.Length)] })

    return $password
}


$error.Clear()
$ErrorActionPreference = 'Stop'

$sqlAdminPassword = Get-SecurePassword -length 40
$abbrPath = 'infra/constants/abbreviations.json'
if(!(Test-Path $abbrPath)){
    Write-Host "JSON file not found at $abbrPath" -ForegroundColor Red
    exit
}
$secretNamePath = 'infra/constants/keyVaultSecretNames.json'
if(!(Test-Path $secretNamePath)){
    Write-Host "JSON file not found at $secretNamePath" -ForegroundColor Red
    exit
}

$bicepPath = "./infra/main.bicep" # convert this to a full file path
if(!(Test-Path $bicepPath)){
    Write-Host "Bicep file not found at $bicepPath" -ForegroundColor Red
    exit
}
$bicepPath = (Get-Item $bicepPath).FullName

$abbr =  Get-Content $abbrPath | ConvertFrom-Json
$kvSecretNames =  Get-Content $secretNamePath | ConvertFrom-Json
$key=$kvSecretNames.SQL_SERVER_PASSWORD
$aiaoKeyName=$kvSecretNames.AZURE_OPENAI_API_KEY
$srchKeyName =  $kvSecretNames.AI_SEARCH_KEY
$resourceGroupName = $abbr.resourceGroup + $appName
$aiSearchName = $abbr.aiSearch + $appName
$aiServiceSku = "basic"
$sqlServerName = $abbr.sqlDatabaseServer + $appName
$sqlAdminUsername = "admin-" + $appName
$kvName = $abbr.keyVault + $appName
$aoaiServiceName = $abbr.azureOpenAIService + $appName
$miName = $abbr.managedIdentity + $appName
$userIdGuid = az ad signed-in-user show -o tsv --query id
$tenantId = az  account show -o tsv --query tenantId

$ipAddress = (Invoke-WebRequest https://api.ipify.org/?format=text).Content.Trim()
Write-Host "Using IP Address: $ipAddress" -ForegroundColor DarkYellow

Write-Host "Resource Group Name: $resourceGroupName" -ForegroundColor DarkYellow
Write-Host "AI Search Name: $aiSearchName" -ForegroundColor DarkYellow
Write-Host "AI Service SKU: $aiServiceSku" -ForegroundColor DarkYellow
Write-Host "SQL Server Name: $sqlServerName" -ForegroundColor DarkYellow
Write-Host "SQL Admin Username: $sqlAdminUsername" -ForegroundColor DarkYellow
#Write-Host "SQL Admin Password: $sqlAdminPassword" -ForegroundColor DarkYellow
Write-Host "Key Vault Name: $kvName" -ForegroundColor DarkYellow
Write-Host "Azure Open AI Service Name: $aoaiServiceName" -ForegroundColor DarkYellow
Write-Host "Azure Open AI Service Location: $aoaiLocation" -ForegroundColor DarkYellow
Write-Host "Azure Open AI Service Key Name: $aiaoKeyName" -ForegroundColor DarkYellow
Write-Host "Current User Guid: $userIdGuid" -ForegroundColor DarkYellow
Write-Host "Tenant Id: $tenantId" -ForegroundColor DarkYellow
Write-Host "Manged Identity Name: $miName" -ForegroundColor DarkYellow

Write-host "Creating Resource Group and Deployment" -ForegroundColor Yellow

$result = az deployment sub create --name $appName --location $location  --template-file $bicepPath `
    --parameters resourceGroupName="$resourceGroupName" location="$location" `
    aiServiceName="$aiSearchName" aiServiceSku="$aiServiceSku" srchKeyName="$srchKeyName" `
    sqlServerName="$sqlServerName" sqlAdminUsername="$sqlAdminUsername" sqlAdminPassword="$sqlAdminPassword" sqlPasswordSecretName="$key" `
    keyVaultName="$kvName" `
    aoaiServiceName="$aoaiServiceName" aoaiLocation="$aoaiLocation" aoaiKeyName="$aiaoKeyName" `
     miName="$miName" currentIpAddress=$ipAddress `
    currentUserGuid="$userIdGuid" `
    --verbose

if($?){ 
    Write-Host "Deployment completed successfully" -ForegroundColor Green 
}
else{ 
    Write-Host "Deployment failed" -ForegroundColor Red
    exit
 }

 if(!$?){ exit }

$outputObj = $result | ConvertFrom-Json -Depth 10
$aoaiEndpoint = $outputObj.properties.outputs.aoaiEndpoint.value
$keyVaultName = $outputObj.properties.outputs.keyVaultName.value
$aiSearchEndpoint = $outputObj.properties.outputs.aiSearchEndpoint.value

$sqlConnection = az sql db show-connection-string -s $sqlServerName -n SampleDb --auth-type SqlPassword --client ado.net -o tsv
$sqlConnection = $sqlConnection.Replace("<username>", $sqlAdminUsername);

Write-Host "Azure Open AI Endpoint: $aoaiEndpoint" -ForegroundColor DarkYellow
Write-Host "Key Vault Name: $keyVaultName" -ForegroundColor DarkYellow
Write-Host "Azure AI Search Endpoint: $aiSearchEndpoint" -ForegroundColor DarkYellow
Write-Host "SQL Connection String: $sqlConnection" -ForegroundColor DarkYellow


$settings  = @{
 "AzureOpenAI" = @{
       "Endpoint" =  $aoaiEndpoint
       "Model" = "gpt-4o"
       "Deployment" = "gpt-4o"
       "EmbeddingModel"= "text-embedding-ada-002"
       "EmbeddingDeployment" = "text-embedding-ada-002"
       "Key" = ""
    }
    "KeyVault" = @{
      "Name" = $keyVaultName
      "SQLPasswordSecretName" = $key
      "AzureOpenAIKeySecretName" = $aiaoKeyName
      "AiSearchKeySecretName" = $srchKeyName
    }
    "AzureAiSearch" = @{
       "Endpoint" = $aiSearchEndpoint
       "IndexName" = "sample_index"
       "Key" = ""
    }
    "Database"= @{
       "ConnectionString" = $sqlConnection
    }
    "UseVolatileMemory" = $false
    "UseFunctionInvocationFilter" = $true
    "SemanticKernelLogLevel" = "Warning"
    "TenantId" = $tenantId

}

Write-Host -ForegroundColor Green "Creating console local.settings.json file"
$settingsJson = ConvertTo-Json $settings -Depth 100
$settingsJson | Out-File -FilePath ".\SqlGenDemoConsole\local.settings.json"





