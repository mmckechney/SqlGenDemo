using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace SqlGenDemo.Library.SK
{
   public class SkKernel
   {
      ILogger<SkKernel> log;
      SkConfig skConfig;
      SkFunctionInvocationFilter skFunctionInvocationFilter;
      public readonly string DefaultPluginName = "YamlPrompts";
      public SkKernel(ILogger<SkKernel> log,  SkConfig skConfig, SkFunctionInvocationFilter skFunctionInvocationFilter)
      {
         this.log = log;
         this.skConfig = skConfig;
         this.skFunctionInvocationFilter = skFunctionInvocationFilter;

      }

      public Kernel CreateKernel(bool includeInvocationFilter, params object[] plugins)
      {
         var sk = CreateBaseKernel(includeInvocationFilter);

         foreach (var p in plugins)
         {
            sk.ImportPluginFromObject(p);
         }

         log.LogDebug($"Created new Kernel object with {skConfig.AzureOpenAiDeployment} deployment and {plugins.Count()} plugins");
         return sk;
      }
 
      private Kernel CreateBaseKernel(bool includeInvocationFilter)
      {
         var builder = Kernel.CreateBuilder();
         builder.AddAzureOpenAIChatCompletion(deploymentName: skConfig.AzureOpenAiDeployment, modelId: skConfig.AzureOpenAiModel,
               endpoint: skConfig.AzureOpenAiEndpoint, apiKey: skConfig.AzureOpenAiKey);
         
         builder.Services.AddLogging(a => { a.SetMinimumLevel(skConfig.SemanticKernelLogLevel); a.AddSimpleConsole(); });
         var sk = builder.Build();
         if (includeInvocationFilter)
         {
            sk.FunctionInvocationFilters.Add(skFunctionInvocationFilter);
         }
         return sk;
      }
   }
}
