using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicPricingSalesSystem.UI.Console
{
    public class ConsoleUI
    {
        public static void DisplayHeader()
        {
            System.Console.Clear();
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            System.Console.WriteLine("║                    DYNAMIC PRICING + SALES MANAGEMENT SYSTEM                ║");
            System.Console.WriteLine("║                           AI-Powered Business Intelligence                    ║");
            System.Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            System.Console.ResetColor();
            System.Console.WriteLine();
        }

        public static void DisplayMainMenu()
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("┌─────────────────── MAIN MENU ───────────────────┐");
            System.Console.ResetColor();
            System.Console.WriteLine("│                                                  │");
            System.Console.WriteLine("│  1. 📊 Analytics Dashboard                       │");
            System.Console.WriteLine("│  2. 💰 Dynamic Pricing Engine                   │");
            System.Console.WriteLine("│  3. 🛒 Sales Management                         │");
            System.Console.WriteLine("│  4. 👥 Customer Management                      │");
            System.Console.WriteLine("│  5. 📦 Product Management                       │");
            System.Console.WriteLine("│  6. 📈 Reports & Analytics                      │");
            System.Console.WriteLine("│  7. ⚙️  System Settings                         │");
            System.Console.WriteLine("│  8. 🔄 Generate Sample Data                     │");
            System.Console.WriteLine("│  0. 🚪 Exit                                     │");
            System.Console.WriteLine("│                                                  │");
            System.Console.WriteLine("└──────────────────────────────────────────────────┘");
            System.Console.WriteLine();
        }

        public static void DisplaySubMenu(string title, List<string> options)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"┌─────────────────── {title.ToUpper()} ───────────────────┐");
            System.Console.ResetColor();
            System.Console.WriteLine("│                                                  │");
            
            for (int i = 0; i < options.Count; i++)
            {
                System.Console.WriteLine($"│  {i + 1}. {options[i],-43} │");
            }
            
            System.Console.WriteLine("│  0. ← Back to Main Menu                         │");
            System.Console.WriteLine("│                                                  │");
            System.Console.WriteLine("└──────────────────────────────────────────────────┘");
            System.Console.WriteLine();
        }

        public static int GetUserChoice(int maxChoice)
        {
            while (true)
            {
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write($"Enter your choice (0-{maxChoice}): ");
                System.Console.ResetColor();
                
                if (int.TryParse(System.Console.ReadLine(), out int choice) && choice >= 0 && choice <= maxChoice)
                {
                    return choice;
                }
                
                DisplayErrorMessage("Invalid choice. Please try again.");
            }
        }

        public static void DisplaySuccessMessage(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"✓ {message}");
            System.Console.ResetColor();
        }

        public static void DisplayErrorMessage(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"✗ {message}");
            System.Console.ResetColor();
        }

        public static void DisplayWarningMessage(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine($"⚠ {message}");
            System.Console.ResetColor();
        }

        public static void DisplayInfoMessage(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine($"ℹ {message}");
            System.Console.ResetColor();
        }

        public static void DisplayTable<T>(List<T> items, List<string> headers, List<Func<T, string>> valueSelectors)
        {
            if (!items.Any() || headers.Count != valueSelectors.Count)
            {
                DisplayWarningMessage("No data to display or invalid table configuration.");
                return;
            }

            // Calculate column widths
            var columnWidths = new List<int>();
            for (int i = 0; i < headers.Count; i++)
            {
                var maxContentWidth = items.Max(item => valueSelectors[i](item).Length);
                columnWidths.Add(Math.Max(headers[i].Length, maxContentWidth) + 2);
            }

            // Display table header
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.Write("┌");
            for (int i = 0; i < columnWidths.Count; i++)
            {
                System.Console.Write(new string('─', columnWidths[i]));
                if (i < columnWidths.Count - 1) System.Console.Write("┬");
            }
            System.Console.WriteLine("┐");

            // Display headers
            System.Console.Write("│");
            for (int i = 0; i < headers.Count; i++)
            {
                System.Console.Write($" {headers[i].PadRight(columnWidths[i] - 1)}");
                if (i < headers.Count - 1) System.Console.Write("│");
            }
            System.Console.WriteLine("│");

            // Display separator
            System.Console.Write("├");
            for (int i = 0; i < columnWidths.Count; i++)
            {
                System.Console.Write(new string('─', columnWidths[i]));
                if (i < columnWidths.Count - 1) System.Console.Write("┼");
            }
            System.Console.WriteLine("┤");
            System.Console.ResetColor();

            // Display data rows
            foreach (var item in items)
            {
                System.Console.Write("│");
                for (int i = 0; i < valueSelectors.Count; i++)
                {
                    var value = valueSelectors[i](item);
                    System.Console.Write($" {value.PadRight(columnWidths[i] - 1)}");
                    if (i < valueSelectors.Count - 1) System.Console.Write("│");
                }
                System.Console.WriteLine("│");
            }

            // Display bottom border
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.Write("└");
            for (int i = 0; i < columnWidths.Count; i++)
            {
                System.Console.Write(new string('─', columnWidths[i]));
                if (i < columnWidths.Count - 1) System.Console.Write("┴");
            }
            System.Console.WriteLine("┘");
            System.Console.ResetColor();
        }

        public static void DisplayMetricCard(string title, string value, string subtitle = "", ConsoleColor titleColor = ConsoleColor.Cyan)
        {
            System.Console.ForegroundColor = titleColor;
            System.Console.WriteLine($"┌─── {title} ───");
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.WriteLine($"│ {value}");
            if (!string.IsNullOrEmpty(subtitle))
            {
                System.Console.ForegroundColor = ConsoleColor.Gray;
                System.Console.WriteLine($"│ {subtitle}");
            }
            System.Console.ForegroundColor = titleColor;
            System.Console.WriteLine("└─────");
            System.Console.ResetColor();
            System.Console.WriteLine();
        }

        public static void DisplayProgressBar(int current, int total, string label = "")
        {
            var percentage = (double)current / total;
            var barLength = 30;
            var filledLength = (int)(barLength * percentage);

            System.Console.Write($"\r{label} [");
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.Write(new string('█', filledLength));
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            System.Console.Write(new string('░', barLength - filledLength));
            System.Console.ResetColor();
            System.Console.Write($"] {percentage:P0} ({current}/{total})");
        }

        public static string GetUserInput(string prompt, bool required = true)
        {
            while (true)
            {
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write($"{prompt}: ");
                System.Console.ResetColor();
                
                var input = System.Console.ReadLine()?.Trim();
                
                if (!required || !string.IsNullOrEmpty(input))
                {
                    return input ?? string.Empty;
                }
                
                DisplayErrorMessage("This field is required. Please enter a value.");
            }
        }

        public static decimal GetDecimalInput(string prompt, decimal minValue = 0, decimal maxValue = decimal.MaxValue)
        {
            while (true)
            {
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write($"{prompt}: ");
                System.Console.ResetColor();
                
                if (decimal.TryParse(System.Console.ReadLine(), out decimal value) && 
                    value >= minValue && value <= maxValue)
                {
                    return value;
                }
                
                DisplayErrorMessage($"Please enter a valid number between {minValue} and {maxValue}.");
            }
        }

        public static int GetIntInput(string prompt, int minValue = 0, int maxValue = int.MaxValue)
        {
            while (true)
            {
                System.Console.ForegroundColor = ConsoleColor.White;
                System.Console.Write($"{prompt}: ");
                System.Console.ResetColor();
                
                if (int.TryParse(System.Console.ReadLine(), out int value) && 
                    value >= minValue && value <= maxValue)
                {
                    return value;
                }
                
                DisplayErrorMessage($"Please enter a valid number between {minValue} and {maxValue}.");
            }
        }

        public static void WaitForKeyPress(string message = "Press any key to continue...")
        {
            System.Console.ForegroundColor = ConsoleColor.Gray;
            System.Console.WriteLine();
            System.Console.WriteLine(message);
            System.Console.ResetColor();
            System.Console.ReadKey(true);
        }

        public static bool ConfirmAction(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.Write($"{message} (y/N): ");
            System.Console.ResetColor();
            
            var response = System.Console.ReadLine()?.Trim().ToLower();
            return response == "y" || response == "yes";
        }

        public static void DisplayLoadingMessage(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.Write($"{message}");
            
            for (int i = 0; i < 3; i++)
            {
                System.Threading.Thread.Sleep(500);
                System.Console.Write(".");
            }
            
            System.Console.WriteLine();
            System.Console.ResetColor();
        }

        public static void DisplaySeparator(char character = '─', int length = 80)
        {
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            System.Console.WriteLine(new string(character, length));
            System.Console.ResetColor();
        }
    }
}