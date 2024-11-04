using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SqlGenDemo.Library.SK;
using SqlGenDemo.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using System.Collections;
using System.Collections.Generic;


namespace SqlGenDemo.Library
{
   public class AiSqlGeneration
   {
      private readonly ILogger<AiSqlGeneration> log;
      private readonly IConfiguration config;
      private readonly SkKernel skKernel;
      private readonly SkMemory skMemory;
      private readonly SkConfig skConfig;
      private SkAgents agents;
      private AgentGroupChat chat;
      private List<ChatMessageContent> history = new();

      public AiSqlGeneration(ILogger<AiSqlGeneration> log, IConfiguration config, SkKernel skKernel, SkMemory skMemory, SkAgents agents, SkConfig skConfig)
      {
         this.log = log;
         this.config = config;
         this.skKernel = skKernel;
         this.skMemory = skMemory;
         this.agents = agents;
         this.skConfig = skConfig;

      }

      public async Task<(string conversation, string answer)> GenerateSQLQueryWithAgent(string query, int count = 0)
      {
         bool.TryParse(config["UseFunctionInvocationFilter"], out bool useSkFunctionInvocationFilter);

         StringBuilder convo = new();
         if (chat != null)
         {
            history = await chat.GetChatMessagesAsync().ToListAsync();
         }
         chat = await agents.SetUpGroupAgentChat(useSkFunctionInvocationFilter, new object[] { this });

         if (history.Count > 0)
         {
            chat.AddChatMessages(history.Where(h => !string.IsNullOrEmpty(h.Content)).ToList().Take(5).ToList());
         }

         chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, query));
         var item = $"# {AuthorRole.User}: '{query}'";
         convo.AppendLine(item);
         log.LogDebug(item);
         try 
         { 
            await foreach (ChatMessageContent content in chat.InvokeAsync())
            {
               item = $"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'";
               convo.AppendLine(item);
               log.LogDebug(item);
            }
            var msgs = chat.GetChatMessagesAsync();
            var answer = chat.GetChatMessagesAsync().FirstAsync().Result.Content.RemoveMarkdown();
            return (convo.ToString(), answer);
         }
         catch(Exception exe)
         {
            if (exe.Message.ToLower().Contains("The plugin collection does not contain a plugin and/or function with the specified names."))
            {
               return ("", "Sorry, something went wrong. Can you please rephrase the question and try again?");
            }else
            {
               log.LogError(exe.Message);
               return ("", exe.Message);
            }
         }
      }

      [KernelFunction("TableSchemas")]
      [Description("Retrieves the table schemas relevent to the user query")]
      [return: Description("Schema for one or more tables")]
      public async Task<string> GetTableSchemas([Description("User question")] string query)
      {
         var searchQuery = $"Get table schemas related to this question: {query}";
         string schemaText;
         List<string> tableSchema = await skMemory.SearchMemoryAsync(query, 10, .5);
         log.LogDebug($"Query: {query}");
         log.LogDebug("Schema Returned:");
         log.LogDebug(string.Join(Environment.NewLine, tableSchema.ToArray()));
         if (tableSchema.Count > 0)
         {
            schemaText = string.Join(Environment.NewLine, tableSchema.ToArray());
         }
         else
         {
            log.LogWarning("No results from Memory Index. Loading SQL file definitions");
            var files = new DirectoryInfo("C:\\Users\\mimcke\\OneDrive\\source\\repos\\~CodeDemos\\SqlGenDemo\\SqlGenDemo.Library\\SampleSchema").GetFiles("*.sql");
            schemaText = string.Join(Environment.NewLine, files.ToList().Select(f => File.ReadAllText(f.FullName)).ToArray());
         }

         return schemaText;
      }

      //TODO: Replace this with an actual Database call to return real data...
      [KernelFunction("RetrieveData")]
      [Description("Retrieves data from the database")]
      [return: Description("Dataset of information from the database")]
      public async Task<string> GetDataFromDatabase([Description("T-SQL Query")] string query)
      {

         //provide protection against updates or deletes in the query
         if (query.ToLower().Contains("update") || query.ToLower().Contains("delete") || query.ToLower().Contains("insert"))
         {
            log.LogWarning("Update or Delete statements are not allowed");
            return "Update or Delete statements are not allowed";
         }



         string connectionString = skConfig.SqlConnnectionString;
         StringBuilder result = new StringBuilder();

         try
         {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
               await connection.OpenAsync();
               using (SqlCommand command = new SqlCommand(query, connection))
               {
                  using (SqlDataReader reader = await command.ExecuteReaderAsync())
                  {
                     // Get column names
                     for (int i = 0; i < reader.FieldCount; i++)
                     {
                        result.Append(reader.GetName(i) + (i < reader.FieldCount - 1 ? "," : ""));
                     }
                     result.AppendLine();

                     // Get rows
                     while (await reader.ReadAsync())
                     {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                           result.Append(reader[i].ToString() + (i < reader.FieldCount - 1 ? "," : ""));
                        }
                        result.AppendLine();
                     }
                  }
               }
            }
         }
         catch (Exception ex)
         {
            log.LogError(ex, $"Error executing query: {query}");
            return "Error executing query: " + ex.Message;
         }

         return result.ToString();

        
      }


      public async Task<(int avaialble, int successful)> CreateDatabaseTables(DirectoryInfo schemaDir)
      {
         int counter = 0;
         string content;
         var files = schemaDir.GetFiles("*.sql");
         var total = files.Length;
         using (SqlConnection connection = new SqlConnection(skConfig.SqlConnnectionString))
         {
            await connection.OpenAsync();
            foreach (var file in files)
            {
               try
               {
                  content = File.ReadAllText(file.FullName);
                  using (SqlCommand command = new SqlCommand(content, connection))
                  {
                     var res = await command.ExecuteNonQueryAsync();
                     log.LogInformation($"Created table from {file.FullName}");
                     counter++;
                  }
               }
               catch (Exception exe)
               {
                  log.LogError($"Failed to create table from {file.FullName}. {exe.Message}");
               }
            }
         }
         return (total,counter);
      }

      public async Task<(int available, int count)> PopulateDatabaseTables(DirectoryInfo directory)
      {
         int counter = 0;
         string content;
         var files = directory.GetFiles("*.data");
         var total = files.Length;
         using (SqlConnection connection = new SqlConnection(skConfig.SqlConnnectionString))
         {
            await connection.OpenAsync();
            foreach (var file in files)
            {
               try
               {
                  content = File.ReadAllText(file.FullName);
                  using (SqlCommand command = new SqlCommand(content, connection))
                  {
                     var res = await command.ExecuteNonQueryAsync();
                     log.LogInformation($"Inserted data from from {file.FullName}");
                     counter++;
                  }
               }
               catch (Exception exe)
               {
                  log.LogError($"Failed to insert data from {file.FullName}. {exe.Message}");
               }
            }
         }
         return (total, counter);
      }
   }
}
