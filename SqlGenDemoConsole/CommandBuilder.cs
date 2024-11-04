using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;

namespace SqlGenDemoConsole
{
   internal class CommandBuilder
   {
      public static Parser BuildCommandLine()
      {

         var messageArg = new Argument<string[]>("question", "Natural Language Question") { Arity = ArgumentArity.ZeroOrMore };
         var indexCmd = new Command("index", "Push table schemas to Azure AI Search");

         var dirOpt = new Option<DirectoryInfo>(["--dir", "--directory"], "Directory containing table schema .sql files") { IsRequired = true };
         dirOpt.ExistingOnly();
         indexCmd.Add(dirOpt);
         indexCmd.Handler = CommandHandler.Create<DirectoryInfo>(Worker.IndexToAISearch);


         var createTablesCmd = new Command("create", "Create Tables in target database");
         createTablesCmd.Add(dirOpt);
         createTablesCmd.Handler = CommandHandler.Create<DirectoryInfo>(Worker.CreateDatabaseTables);


         var sqlOpt = new Option<DirectoryInfo>(["--dir", "--directory"], "Directory containing the table insert commands in .data files") { IsRequired = true };
         sqlOpt.ExistingOnly();
         var populateCmd = new Command("populate", "Populate database tables with sample data");
         populateCmd.Add(sqlOpt);
         populateCmd.Handler = CommandHandler.Create<DirectoryInfo>(Worker.PopulateDatabaseTables);


         RootCommand rootCommand = new RootCommand(description: $"SQL Generator Demo - uses Semantic Kernel and an Azure OpenAI Agent to generate a query and return data");
         rootCommand.Add(messageArg);
         rootCommand.AddCommand(indexCmd);
         rootCommand.AddCommand(createTablesCmd);
         rootCommand.AddCommand(populateCmd);

         rootCommand.Handler = CommandHandler.Create<string[]>(Worker.AskQuestion);

         var parser = new CommandLineBuilder(rootCommand)
              .UseDefaults()
              .UseHelp(ctx =>
              {
                 ctx.HelpBuilder.CustomizeLayout(_ => HelpBuilder.Default
                                    .GetLayout()
                                    .Prepend(
                                        _ => AnsiConsole.Write(new FigletText("SQL Generator Demo"))
                                    ));

              })
              .Build();

         return parser;
      }


   }
}
