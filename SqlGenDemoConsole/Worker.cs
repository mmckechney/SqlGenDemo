using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.SemanticKernel;
using SqlGenDemo.Library;
using SqlGenDemo.Library.SK;
using SqlGenDemo.Logging;
using System;
using System.CommandLine.Parsing;
using System.Net.NetworkInformation;

namespace SqlGenDemoConsole
{
   internal class Worker : BackgroundService
   {
      private static ILogger<Worker> log;
      private static ILoggerFactory logFactory;
      private static IConfiguration config;
      private static StartArgs startArgs;
      private static Parser rootParser;
      private static AiSqlGeneration sqlgen;
      private static SkMemory skMemory;
      public Worker(ILogger<Worker> logger, ILoggerFactory loggerFactory, IConfiguration configuration, StartArgs sArgs, AiSqlGeneration sqlgen, SkMemory skMemory, SkKernel kernel)
      {
         log = logger;
         logFactory = loggerFactory;
         config = configuration;
         startArgs = sArgs;
         Worker.sqlgen = sqlgen;
         Worker.skMemory = skMemory;
      }



      protected async override Task ExecuteAsync(CancellationToken stoppingToken)
      {
         skMemory.InitMemory();
         Directory.SetCurrentDirectory(Path.GetDirectoryName(AppContext.BaseDirectory));
         rootParser = CommandBuilder.BuildCommandLine();
         int val = await rootParser.InvokeAsync(["-h"]);

         while (true)
         {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.Write("sqlgen> ");
            Console.ResetColor();
            var line = Console.ReadLine();
            if (line == null)
            {
               return;
            }

            if (line.Length == 0) line = "-h";
            val = await rootParser.InvokeAsync(line);
         }
      }

      internal static async Task AskQuestion(string[] question)
      {

         (string convo, string answer) = await sqlgen.GenerateSQLQueryWithAgent(string.Join(" ", question));
         log.LogInformation("-----");
         log.LogInformation(answer, ConsoleColor.DarkYellow);

         log.LogDebug("Chat History");
         log.LogDebug(convo, ConsoleColor.Cyan);

      }

      internal static async Task IndexToAISearch(DirectoryInfo directory)
      {
         log.LogInformation($"Sending contents of {directory.Name} to AI Search");
         await skMemory.SendDataToAISearch(directory);
         log.LogInformation("Indexing Complete.");
      }

      internal static async Task CreateDatabaseTables(DirectoryInfo directory)
      {
         (int available, int count) = await sqlgen.CreateDatabaseTables(directory);
         if(available == count)
         {
            log.LogInformation($"Created {count} tables out of the available {available} SQL files");
         }
         else
         {
            log.LogWarning($"Only created {count} tables out of the available {available} SQL files");
         }
      }

      internal static async Task PopulateDatabaseTables(DirectoryInfo directory)
      {
         (int available, int count) = await sqlgen.PopulateDatabaseTables(directory);
         if (available == count)
         {
            log.LogInformation($"Inserted data into {count} tables out of the available {available} Data files");
         }
         else
         {
            log.LogWarning($"Only inserted data into {count} tables out of the available {available} Data files");
         }
      }
   }
}
