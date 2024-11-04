using SqlGenDemo.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
namespace SqlGenDemo.Library.SK
{
   public class SkFunctionInvocationFilter : IFunctionInvocationFilter
   {
      ILogger<SkFunctionInvocationFilter> log;
      public SkFunctionInvocationFilter(ILogger<SkFunctionInvocationFilter> log)
      {
         this.log = log;
      }
      public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
      {
         log.LogInformation("----------------");
         log.LogInformation($"{{INVOKING|DarkMagenta}}:\t{{{context.Function.Name}|Red}}");//{Environment.NewLine}{string.Join(Environment.NewLine, context.Arguments.Select(a => $"{{{a.Key}|Cyan}}:{{{a.Value.ToString().ToShortString()}|(230, 255, 255)}}"))}");
         await next(context);
         log.LogInformation("----------------");
         log.LogInformation($"{{INVOKED|DarkMagenta}}:\t{{{context.Function.Name}|Red}}{Environment.NewLine}{string.Join(Environment.NewLine, context.Arguments.Select(a => $"{{{a.Key}|Cyan}}:{a.Value.ToString()}"))}{Environment.NewLine}{{Result:|Yellow}}{context.Result.ToString()}");
      }

   }

   public static class Extension
   {
      public static string ToShortString(this string value)
      {
         var shortStr = value.Substring(0, Math.Min(value.Length, 100));
         if (shortStr.Length == 100) shortStr += "...";
         return shortStr;
      }
   }

}
