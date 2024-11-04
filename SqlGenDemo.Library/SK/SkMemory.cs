using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Memory;
using System.Text.Json;
namespace SqlGenDemo.Library.SK
{
   public class SkMemory
   {
      ISemanticTextMemory semanticMemory;
      ILogger<SkMemory> log;
      IConfiguration config;
      ILoggerFactory logFactory;
      SkConfig skConfig;
      private string collectionName = string.Empty;

      public ISemanticTextMemory SemanticMemory { get => semanticMemory; set => semanticMemory = value; }

      public SkMemory(ILogger<SkMemory> log, ILoggerFactory logFactory, IConfiguration config, SkConfig skConfig)
      {
         this.log = log;
         this.config = config;
         this.skConfig = skConfig;
         this.logFactory = logFactory;
         this.collectionName = skConfig.AzureAiSearchIndexName;
      }

      public void InitMemory()
      {

            var embeddingSvc = new AzureOpenAITextEmbeddingGenerationService(
                  deploymentName: skConfig.AzureOpenAiEmbeddingDeployment,
                  modelId: skConfig.AzureOpenAiEmbeddingModel,
                  endpoint: skConfig.AzureOpenAiEndpoint,
                  apiKey: skConfig.AzureOpenAiKey);



            IMemoryStore aiSearchStore = new AzureAISearchMemoryStore(endpoint: skConfig.AzureAiSearchEndpoint, apiKey: skConfig.AzureAiSearchKey);
            semanticMemory = new MemoryBuilder()
                .WithMemoryStore(aiSearchStore)
                .WithTextEmbeddingGeneration(embeddingSvc)
                .WithLoggerFactory(logFactory)
                .Build();
    
      }

      public async Task<bool> SendDataToAISearch(DirectoryInfo schemaDir)
      {
         int counter = 0;
         string content;
         var files = schemaDir.GetFiles("*.sql");
         foreach (var file in files)
         {
            content = File.ReadAllText(file.FullName); 

            await semanticMemory.SaveReferenceAsync(
               collection: collectionName, externalSourceName: "file",
               externalId: file.Name, description: content, text: content);

            counter++;
         }
         log.LogDebug($"Send {counter} table schemas from {schemaDir.Name} file to AI Search");
         return true;
      }

      public async Task<List<string>> SearchMemoryAsync(string query, int limit, double minRelevanceScore)
      {
         IAsyncEnumerable<MemoryQueryResult> memories = await SearchMemoryAsync(query, limit, minRelevanceScore, true);
         return await GetMemoryContents(memories);
      }
      private async Task<List<string>> GetMemoryContents(IAsyncEnumerable<MemoryQueryResult> memories)
      {
         List<string> contents = new();
         await foreach (var memoryResult in memories)
         {
            log.LogDebug("Memory Result = " + memoryResult.Metadata.Description);
            contents.Add(memoryResult.Metadata.Description);
         };

         return contents;
      }
      private async Task<IAsyncEnumerable<MemoryQueryResult>> SearchMemoryAsync(string query, int limit, double minRelevanceScore, bool withEmbeddings)
      {

         log.LogInformation($"AI Search Query: '{query}'");
         log.LogDebug($"AI Search settings:{Environment.NewLine}Collection: {collectionName}{Environment.NewLine}Endpoint:{semanticMemory.ToString()}");
         var memoryResults = semanticMemory.SearchAsync(collectionName, query, limit: limit, minRelevanceScore: minRelevanceScore, withEmbeddings: withEmbeddings);
         await foreach (MemoryQueryResult memoryResult in memoryResults)
         {
            if (memoryResult.Relevance > 0.9)
            {
               log.LogInformation($"Top Relevent Result @ {memoryResult.Relevance}: {memoryResult.Metadata.Description}", ConsoleColor.Cyan);
            }
         }

         return memoryResults;
      }
   }
}
