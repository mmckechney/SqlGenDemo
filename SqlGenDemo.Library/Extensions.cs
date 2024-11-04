using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlGenDemo.Library
{
   public static class Extensions
   {

      public static string RemoveMarkdown(this string input)
      {
         string pattern = @"^```.*(?:\r?\n)?";
         string result = Regex.Replace(input, pattern, "", RegexOptions.Multiline);
         return result;
      }

   }
}
