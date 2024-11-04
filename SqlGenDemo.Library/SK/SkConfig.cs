using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

namespace SqlGenDemo.Library.SK
{
   public class SkConfig
   {
      private IConfiguration config;
      private ILogger<SkConfig> log;
      private string azureOpenAiEndpoint = string.Empty;
      private string azureOpenKey = string.Empty;
      private string azureOpenAiModel = string.Empty;
      private string azureOpenAiDeployment = string.Empty;
      private string azureOpenEmbeddingAiModel = string.Empty;
      private string azureOpenAiEmbeddingDeployment = string.Empty;
      private string azureAiSearchEndpoint = string.Empty;
      private string azureAiSearchKey = string.Empty;
      private string azureAiSearchIndexName = string.Empty;
      private string azureOpenAiClassificationModel = string.Empty;
      private string azureOpenAiClassificationDeployment = string.Empty;
      private string tenantId = string.Empty;
      private string sqlConnection = string.Empty; 

      public SkConfig(ILogger<SkConfig> log, IConfiguration config)
      {
         this.config = config;
         this.log = log;
         GetSecretsFromKeyVault();
      }

      public string AzureOpenAiEndpoint
      {
         get
         {
            if (string.IsNullOrWhiteSpace(azureOpenAiEndpoint))
            {
               azureOpenAiEndpoint = config["AzureOpenAi:Endpoint"] ?? throw new ArgumentException("Missing AzureOpenAI:Endpoint config value");
            }
            return azureOpenAiEndpoint;
         }

      }
      public string AzureOpenAiKey
      {
         get
         {
            if (string.IsNullOrWhiteSpace(azureOpenKey))
            {
               if (string.IsNullOrWhiteSpace(config["AzureOpenAi:Key"]))
               {
                  log.LogInformation("AzureOpenAi:Key config is empty: using Default Azure Credential to connect to Azure Open AI");
               }
               else
               {
                  azureOpenKey = config["AzureOpenAi:Key"];
               }

            }
            return azureOpenKey;
         }
      }
      public string AzureOpenAiModel
      {
         get
         {
            if (string.IsNullOrWhiteSpace(azureOpenAiModel))
            {
               azureOpenAiModel = config["AzureOpenAi:Model"] ?? throw new ArgumentException("Missing AzureOpenAI:Model config value");
            }
            return azureOpenAiModel;
         }
      }
      public string AzureOpenAiDeployment
      {
         get
         {
            if (string.IsNullOrWhiteSpace(azureOpenAiDeployment))
            {
               azureOpenAiDeployment = config["AzureOpenAI:Deployment"] ?? throw new ArgumentException("Missing AzureOpenAI:Deployment config value");
            }
            return azureOpenAiDeployment;
         }
      }

      public string AzureOpenAiEmbeddingModel
      {
         get
         {
            if (string.IsNullOrWhiteSpace(azureOpenEmbeddingAiModel))
            {
               azureOpenEmbeddingAiModel = config["AzureOpenAi:EmbeddingModel"] ?? throw new ArgumentException("Missing AzureOpenAI:EmbeddingModel config value");
            }
            return azureOpenEmbeddingAiModel;
         }
      }
      public string AzureOpenAiEmbeddingDeployment
      {
         get
         {
            if (string.IsNullOrWhiteSpace(azureOpenAiEmbeddingDeployment))
            {
               azureOpenAiEmbeddingDeployment = config["AzureOpenAI:EmbeddingDeployment"] ?? throw new ArgumentException("Missing AzureOpenAI:EmbeddingDeployment config value");
            }
            return azureOpenAiEmbeddingDeployment;
         }
      }

      public string AzureOpenAiClassificationModel
      {
         get
         {
            if (string.IsNullOrWhiteSpace(azureOpenAiClassificationModel))
            {
               azureOpenAiClassificationModel = config["AzureOpenAi:ClassificationModel"] ?? throw new ArgumentException("Missing AzureOpenAI:ClassificationModel config value");
            }
            return azureOpenAiClassificationModel;
         }
      }
      public string AzureOpenAiClassificationDeployment
      {
         get
         {
            if (string.IsNullOrWhiteSpace(azureOpenAiClassificationDeployment))
            {
               azureOpenAiClassificationDeployment = config["AzureOpenAI:ClassificationDeployment"] ?? throw new ArgumentException("Missing AzureOpenAI:ClassificationDeployment config value");
            }
            return azureOpenAiClassificationDeployment;
         }
      }
      public string AzureAiSearchEndpoint
      {
         get
         {
            if (string.IsNullOrWhiteSpace(azureAiSearchEndpoint))
            {
               azureAiSearchEndpoint = config["AzureAiSearch:Endpoint"] ?? throw new ArgumentException("Missing AzureAiSearch:Endpoint config value");
            }
            return azureAiSearchEndpoint;
         }
      }

      public string AzureAiSearchKey
      {
         get
         {
            if (string.IsNullOrWhiteSpace(azureAiSearchKey))
            {
               if (string.IsNullOrWhiteSpace(config["AzureAiSearch:Key"]))
               {
                  log.LogInformation("AzureAiSearch:Key config is empty: using Default Azure Credential to connect to AI Search");
               }
               else
               {
                  azureAiSearchKey = config["AzureAiSearch:Key"];
               }
            }
            return azureAiSearchKey;
         }
      }

      public string AzureAiSearchIndexName
      {
         get
         {
            if (string.IsNullOrWhiteSpace(azureAiSearchIndexName))
            {
               azureAiSearchIndexName = config["AzureAiSearch:IndexName"] ?? throw new ArgumentException("Missing AzureAiSearch:IndexName config value");
            }
            return azureAiSearchIndexName;
         }
      }

      public LogLevel SemanticKernelLogLevel
      {
         get
         {
            if (Enum.TryParse<LogLevel>(config["SemanticKernelLogLevel"], out LogLevel level))
            {
               return level;
            }
            else
            {
               return LogLevel.Warning;
            }
         }
      }

      public string TenantId
      {
         get
         {
            if (string.IsNullOrWhiteSpace(this.tenantId))
            {
               tenantId = config["TenantId"];
            }
            return tenantId;
         }
      }

      public string SqlConnnectionString
      {
         get
         {
            if(string.IsNullOrWhiteSpace(sqlConnection))
            {
               sqlConnection = config["Database:ConnectionString"] ?? "";
            }
            return sqlConnection;
         }
      }

      private void GetSecretsFromKeyVault()
      {
         try
         {
            string keyVaultName = config["KeyVault:Name"] ?? "";
            var uri = new Uri($"https://{keyVaultName}.vault.azure.net/");
            SecretClient secretClient = new(uri, new DefaultAzureCredential(new DefaultAzureCredentialOptions() { TenantId = config["TenantId"] }));
            if (!string.IsNullOrWhiteSpace(config["KeyVault:SQLPasswordSecretName"]))
            {
               var sql = secretClient.GetSecret(config["KeyVault:SQLPasswordSecretName"]);
               if (sql.Value != null)
               {
                  var pw = sql.Value.Value;
                  sqlConnection = config["Database:ConnectionString"].Replace("<password>", pw);
               }
            }

            if (!string.IsNullOrWhiteSpace(config["KeyVault:AzureOpenAIKeySecretName"]))
            {
               var aiao = secretClient.GetSecret(config["KeyVault:AzureOpenAIKeySecretName"]);
               if (aiao.Value != null)
               {
                  azureOpenKey = aiao.Value.Value;
               }
            }

            if (!string.IsNullOrWhiteSpace(config["KeyVault:AiSearchKeySecretName"]))
            {
               var srch = secretClient.GetSecret(config["KeyVault:AiSearchKeySecretName"]);
               if (srch.Value != null)
               {
                  azureAiSearchKey = srch.Value.Value;
               }
            }
         }
         catch(Exception exe)
         {
            log.LogError($"Unable to set keys from Key Vault: {exe.Message}");


         }

      }
   }
}
