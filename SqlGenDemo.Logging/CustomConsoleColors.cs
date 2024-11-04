namespace SqlGenDemo.Logging
{
   public class CustomConsoleColors
   {
      /// <summary>
      /// https://devblogs.microsoft.com/commandline/updating-the-windows-console-colors/
      /// </summary>
      public static Dictionary<ConsoleColor, (int, int, int)> StandardColorMapping = new Dictionary<ConsoleColor, (int, int, int)>
      {
          { ConsoleColor.Black, (12,12,12) },
          { ConsoleColor.DarkBlue, (0,55,218) },
          { ConsoleColor.DarkGreen, (19,161,14) },
          { ConsoleColor.DarkCyan, (58,150,221) },
          { ConsoleColor.DarkRed, (197,15,31) },
          { ConsoleColor.DarkMagenta, (136,23,152) },
          { ConsoleColor.DarkYellow, (193,156,0) },
          { ConsoleColor.Gray, (192, 192, 192) },
          { ConsoleColor.DarkGray, (128, 128, 128) },
          { ConsoleColor.Blue, (59,120,255) },
          { ConsoleColor.Green, (22,198,12) },
          { ConsoleColor.Cyan, (97,214,214) },
          { ConsoleColor.Red, (231,72,86) },
          { ConsoleColor.Magenta, (180,0,158) },
          { ConsoleColor.Yellow, (249,241,165) },
          { ConsoleColor.White, (242,242,242) }
      };

      public static void PrintCustomColorMap()
      {
         Console.WriteLine("Need help, use this Color Picker: https://www.bing.com/search?q=color+picker");
         Console.ResetColor();
      }



      internal static string EscapedRGBColor((int r, int g, int b) rgb)
      {
         return EscapedRGBColor(rgb.r, rgb.g, rgb.b);
      }
      internal static string EscapedRGBColor(int r, int g, int b)
      {
         return $"\x1b[38;2;{r};{g};{b}m";
      }
      public static string EscapedRGBColor(ConsoleColor color)
      {
         var mapped = StandardColorMapping[color];
         return EscapedRGBColor(mapped.Item1, mapped.Item2, mapped.Item3);
      }


      private static string BackgoundEscapedRGBColor(int r, int g, int b)
      {
         return $"\x1b[38;2;{r};{g};{b}m";
      }
      private static string BackgoundEscapedRGBColor((int r, int g, int b) rgb)
      {
         return BackgoundEscapedRGBColor(rgb.r, rgb.g, rgb.b);
      }
      internal static string BackgoundEscapedRGBColor(ConsoleColor color)
      {
         var mapped = StandardColorMapping[color];
         return BackgoundEscapedRGBColor(mapped.Item1, mapped.Item2, mapped.Item3);
      }
      internal static string ResetColor()
      {
         return "\x1b[0m";
      }
   }
}
