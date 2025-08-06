using System;
using System.Linq;

namespace DynamicPricingSalesSystem.Utils
{
    public static class ConsoleHelper
    {
        public static void WriteTitle(string title)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($" {title.ToUpper().PadLeft((60 + title.Length) / 2)}");
            Console.WriteLine(new string('=', 60));
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void WriteHeader(string header)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n{header}");
            Console.WriteLine("-".PadRight(header.Length, '-'));
            Console.ResetColor();
        }

        public static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ {message}");
            Console.ResetColor();
        }

        public static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {message}");
            Console.ResetColor();
        }

        public static void WriteWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ {message}");
            Console.ResetColor();
        }

        public static void WriteInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ℹ {message}");
            Console.ResetColor();
        }

        public static string GetInput(string prompt, string defaultValue = "")
        {
            if (!string.IsNullOrEmpty(defaultValue))
            {
                Console.Write($"{prompt} [{defaultValue}]: ");
                var input = Console.ReadLine();
                return string.IsNullOrWhiteSpace(input) ? defaultValue : input;
            }
            else
            {
                Console.Write($"{prompt}: ");
                return Console.ReadLine() ?? string.Empty;
            }
        }

        public static int GetIntInput(string prompt, int defaultValue = 0)
        {
            Console.Write($"{prompt} [{defaultValue}]: ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;
                
            return int.TryParse(input, out int result) ? result : defaultValue;
        }

        public static decimal GetDecimalInput(string prompt, decimal defaultValue = 0)
        {
            Console.Write($"{prompt} [{defaultValue}]: ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;
                
            return decimal.TryParse(input, out decimal result) ? result : defaultValue;
        }

        public static bool GetBoolInput(string prompt, bool defaultValue = false)
        {
            var defaultText = defaultValue ? "y" : "n";
            Console.Write($"{prompt} [y/n] [{defaultText}]: ");
            var input = Console.ReadLine()?.ToLower();
            
            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;
                
            return input.StartsWith("y");
        }

        public static void PressAnyKeyToContinue()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static void DisplayTable<T>(System.Collections.Generic.List<T> items, 
            params (string Header, Func<T, object> ValueFunc)[] columns)
        {
            if (!items.Any())
            {
                WriteWarning("No data to display.");
                return;
            }

            // Calculate column widths
            var columnWidths = columns.Select(col => 
                Math.Max(col.Header.Length, 
                    items.Max(item => col.ValueFunc(item)?.ToString()?.Length ?? 0))).ToArray();

            // Display header
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < columns.Length; i++)
            {
                Console.Write(columns[i].Header.PadRight(columnWidths[i] + 2));
            }
            Console.WriteLine();
            
            // Display separator
            for (int i = 0; i < columns.Length; i++)
            {
                Console.Write(new string('-', columnWidths[i] + 2));
            }
            Console.WriteLine();
            Console.ResetColor();

            // Display data
            foreach (var item in items)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    var value = columns[i].ValueFunc(item)?.ToString() ?? "";
                    Console.Write(value.PadRight(columnWidths[i] + 2));
                }
                Console.WriteLine();
            }
        }

        public static void DisplayProgressBar(string label, double percentage)
        {
            const int barWidth = 40;
            var completed = (int)(percentage * barWidth / 100);
            var remaining = barWidth - completed;

            Console.Write($"{label}: [");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(new string('█', completed));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(new string('░', remaining));
            Console.ResetColor();
            Console.WriteLine($"] {percentage:F1}%");
        }

        public static void ClearCurrentLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
        }
    }
}