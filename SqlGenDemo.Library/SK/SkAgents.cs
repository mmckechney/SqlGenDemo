using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using SqlGenDemo.Library.SK;
using System.Collections.ObjectModel;
using System.Reflection;


namespace SqlGenDemo.Library.SK
{
   public partial class SkAgents
   {
      private IConfiguration _config;
      private ILogger log;
      private ILoggerFactory loggerFactory;
      private SkKernel skKernel;
      private SkConfig skConfig;
      private SkMemory skMemory;
      private AgentGroupChat chat;
      private Dictionary<string, OpenAIAssistantDefinition> agentDefinitions;
      public ReadOnlyDictionary<string, OpenAIAssistantDefinition> AgentDefinitions
      {
         get
         {
            return agentDefinitions.AsReadOnly();
         }
      }
      public SkAgents(ILogger<SkAgents> log, IConfiguration config, ILoggerFactory loggerFactory, SkKernel skKernel, SkConfig skConfig, SkMemory skMemory)
      {
         _config = config;
         this.log = log;
         this.loggerFactory = loggerFactory;
         this.skKernel = skKernel;
         this.skConfig = skConfig;
         agentDefinitions = new Dictionary<string, OpenAIAssistantDefinition>();
         this.skMemory = skMemory;
         InitAgentDefinitions();

      }

      public async Task<AgentGroupChat> SetUpGroupAgentChat(bool useFunctionInvocationFilter = false, params object[] plugins )
      {

         chat = new AgentGroupChat();
         var sqlGenerator = await CreateOpenAiAgent("SQLGenerator", useFunctionInvocationFilter,  plugins );
         chat.AddAgent(sqlGenerator);

         chat.ExecutionSettings = new()
         {
            TerminationStrategy = new ApprovalTerminationStrategy()//,
         };

         return chat;

      }

      public async Task<OpenAIAssistantAgent> CreateOpenAiAgent(string agentName, bool useFunctionInvocationFilter,  params object[] plugins)
      {
         agentName = agentName.ToLower();
         OpenAIAssistantDefinition definition;

         if (!agentDefinitions.ContainsKey(agentName))
         {
            log.LogError($"Agent definition {agentName} not found");
            return null;
         }
         else
         {
            definition = agentDefinitions[agentName];
         }

         var sk = skKernel.CreateKernel(useFunctionInvocationFilter, plugins);
  
         var prov = OpenAIClientProvider.ForAzureOpenAI(new System.ClientModel.ApiKeyCredential(skConfig.AzureOpenAiKey), new Uri(skConfig.AzureOpenAiEndpoint));
         var agent = await OpenAIAssistantAgent.CreateAsync(kernel: sk, clientProvider: prov, definition: definition);


         return agent;
      }

      public bool InitAgentDefinitions()
      {
         bool success = true;
         var assembly = Assembly.GetExecutingAssembly();
         var myNamespace = GetType().Namespace;
         var resources = assembly.GetManifestResourceNames().ToList();
         resources.ForEach(r =>
         {
            if (r.ToLower().EndsWith("yaml"))
            {
               if (r.Contains("Agents."))
               {
                  using StreamReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream(r)!);
                  var tmp = reader.ReadToEnd();
                  var opt = AgentYamlSerializer(tmp);
                  if (opt != null && !agentDefinitions.ContainsKey(opt.Name.ToLower()))
                  {
                     agentDefinitions.Add(opt.Name.ToLower(), opt);
                  }
                  else
                  {
                     success = false;
                  }
               }

            }
         });

         return success;
      }

      private OpenAIAssistantDefinition AgentYamlSerializer(string yaml)
      {
         try
         {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.LowerCaseNamingConvention.Instance)
                .Build();
            var objDynamic = deserializer.Deserialize<dynamic>(yaml);

            var agentDef = new OpenAIAssistantDefinition(skConfig.AzureOpenAiModel)
            {
               Instructions = objDynamic["instructions"],
               Name = objDynamic["name"],
               Description = objDynamic["description"],
               EnableCodeInterpreter = Convert.ToBoolean(objDynamic["enablecodeinterpreter"])
            };

            return agentDef;
         }
         catch (Exception exe)
         {
            log.LogError($"Error deserializing agent definition: {exe.Message}. {yaml}");
            return null;
         }
      }
   }
}
