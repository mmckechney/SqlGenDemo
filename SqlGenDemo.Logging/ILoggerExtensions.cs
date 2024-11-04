using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
namespace SqlGenDemo.Logging
{
   public static partial class ILoggerExtensions
   {
      /// <summary>
      /// Do define the color of the message, wrap the colored words in curly braces, ending with a pipe (|) and color name or key name of your custom color dictionary.
      /// For example log.LogInformation("this is my {blue message|blue} and this is my {red message|red}");
      /// For string interpolation, escape with double curly braces, for example log.LogInformation($"this is my {{{obj.value}|blue}} and this is my {{{obj2.value}|red}}");
      /// </summary>
      /// <param name="logger"></param>
      /// <param name="message"></param>
      public static void LogInformation(this ILogger logger, string message)
      {
         logger.Log(LogLevel.Information, FormatMessages(message));
      }

      /// <summary>
      /// Do define the color of the message, wrap the colored words in curly braces, ending with a pipe (|) and color name or key name of your custom color dictionary.
      /// For example log.LogDebug("this is my {blue message|blue} and this is my {red message|red}");
      /// For string interpolation, escape with double curly braces, for example log.LogDebug($"this is my {{{obj.value}|blue}} and this is my {{{obj2.value}|red}}");
      /// </summary>
      /// <param name="logger"></param>
      /// <param name="message"></param>
      public static void LogDebug(this ILogger logger, string message)
      {
         logger.Log(LogLevel.Debug, FormatMessages(message));
      }

      /// <summary>
      /// Do define the color of the message, wrap the colored words in curly braces, ending with a pipe (|) and color name or key name of your custom color dictionary.
      /// For example log.LogError("this is my {blue message|blue} and this is my {red message|red}");
      /// For string interpolation, escape with double curly braces, for example log.LogError($"this is my {{{obj.value}|blue}} and this is my {{{obj2.value}|red}}");
      /// </summary>
      /// <param name="logger"></param>
      /// <param name="message"></param>
      public static void LogError(this ILogger logger, string message)
      {
         logger.Log(LogLevel.Error, FormatMessages(message));
      }

      /// <summary>
      /// Do define the color of the message, wrap the colored words in curly braces, ending with a pipe (|) and color name or key name of your custom color dictionary.
      /// For example log.LogWarning("this is my {blue message|blue} and this is my {red message|red}");
      /// For string interpolation, escape with double curly braces, for example log.LogWarning($"this is my {{{obj.value}|blue}} and this is my {{{obj2.value}|red}}");
      /// </summary>
      /// <param name="logger"></param>
      /// <param name="message"></param>
      public static void LogWarning(this ILogger logger, string message)
      {
         logger.Log(LogLevel.Warning, FormatMessages(message));
      }

      /// <summary>
      /// Do define the color of the message, wrap the colored words in curly braces, ending with a pipe (|) and color name or key name of your custom color dictionary.
      /// For example log.LogCritical("this is my {blue message|blue} and this is my {red message|red}");
      /// For string interpolation, escape with double curly braces, for example log.LogCritical($"this is my {{{obj.value}|blue}} and this is my {{{obj2.value}|red}}");
      /// </summary>
      /// <param name="logger"></param>
      /// <param name="message"></param>
      public static void LogCritical(this ILogger logger, string message)
      {
         logger.Log(LogLevel.Critical, FormatMessages(message));
      }

      /// <summary>
      /// Do define the color of the message, wrap the colored words in curly braces, ending with a pipe (|) and color name or key name of your custom color dictionary.
      /// For example log.LogTrace("this is my {blue message|blue} and this is my {red message|red}");
      /// For string interpolation, escape with double curly braces, for example log.LogTrace($"this is my {{{obj.value}|blue}} and this is my {{{obj2.value}|red}}");
      /// </summary>
      /// <param name="logger"></param>
      /// <param name="message"></param>
      public static void LogTrace(this ILogger logger, string message)
      {
         logger.Log(LogLevel.Trace, FormatMessages(message));
      }


      private static Regex curlyBraceRegex = new Regex(@"\{(.+?)\|(.+?)\}");

      private static string FormatMessages(string message)
      {
         string[] substrings = curlyBraceRegex.Split(message);
         List<string> formattedMessages = new List<string>();
         for (int i = 0; i < substrings.Length; i++)
         {
            if (i % 3 == 0)
            {
               // This is a non-matching string
               formattedMessages.Add(substrings[i] + "**COLOR:White|");
            }
            else if (i % 3 == 1)
            {
               // This is the first matching string
               // We append the second matching string (which is at i+1) and add it to the list
               formattedMessages.Add(substrings[i] + "**COLOR:" + substrings[i + 1] + "|");
               i++; // Skip the next iteration because we've already processed the second matching string
            }
         }
         return string.Join("", formattedMessages);
      }

      public static void LogInformation(this ILogger logger, string message, ConsoleColor color)
      {
         logger.LogInformation(FormatMessage(message, color));
      }

      public static void LogDebug(this ILogger logger, string message, ConsoleColor color)
      {
         logger.LogDebug(FormatMessage(message, color));
      }

      public static void LogError(this ILogger logger, string message, ConsoleColor color)
      {
         logger.LogError(FormatMessage(message, color));
      }

      public static void LogWarning(this ILogger logger, string message, ConsoleColor color)
      {
         logger.LogWarning(FormatMessage(message, color));
      }

      public static void LogCritical(this ILogger logger, string message, ConsoleColor color)
      {
         logger.LogCritical(FormatMessage(message, color));
      }

      public static void LogTrace(this ILogger logger, string message, ConsoleColor color)
      {
         logger.LogTrace(FormatMessage(message, color));
      }
      private static string FormatMessage(string message, ConsoleColor color)
      {
         return message + " **COLOR:" + color.ToString();
      }
   }
}
