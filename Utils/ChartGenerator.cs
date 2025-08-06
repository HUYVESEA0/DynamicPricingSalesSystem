using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicPricingSalesSystem.Utils
{
    public static class ChartGenerator
    {
        public static void DisplayBarChart(string title, Dictionary<string, decimal> data, int maxWidth = 50)
        {
            if (!data.Any())
            {
                ConsoleHelper.WriteWarning("No data to display.");
                return;
            }

            ConsoleHelper.WriteHeader(title);
            
            var maxValue = data.Values.Max();
            var maxLabelLength = data.Keys.Max(k => k.Length);

            foreach (var item in data.OrderByDescending(kvp => kvp.Value))
            {
                var barLength = maxValue > 0 ? (int)((item.Value / maxValue) * maxWidth) : 0;
                var bar = new string('█', barLength);
                var spaces = new string(' ', maxWidth - barLength);
                
                Console.WriteLine($"{item.Key.PadRight(maxLabelLength)} │{bar}{spaces}│ ${item.Value:F2}");
            }
        }

        public static void DisplayLineChart(string title, List<(string Label, decimal Value)> data, int height = 10, int width = 60)
        {
            if (!data.Any())
            {
                ConsoleHelper.WriteWarning("No data to display.");
                return;
            }

            ConsoleHelper.WriteHeader(title);
            
            var values = data.Select(d => d.Value).ToList();
            var minValue = values.Min();
            var maxValue = values.Max();
            var range = maxValue - minValue;

            if (range == 0)
            {
                Console.WriteLine("All values are the same.");
                return;
            }

            // Create chart grid
            var chart = new char[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    chart[i, j] = ' ';
                }
            }

            // Plot data points
            for (int i = 0; i < data.Count && i < width; i++)
            {
                var normalizedValue = (data[i].Value - minValue) / range;
                var yPos = height - 1 - (int)(normalizedValue * (height - 1));
                chart[yPos, i] = '●';

                // Connect points with lines
                if (i > 0)
                {
                    var prevNormalizedValue = (data[i - 1].Value - minValue) / range;
                    var prevYPos = height - 1 - (int)(prevNormalizedValue * (height - 1));
                    
                    // Draw simple line between points
                    var startY = Math.Min(prevYPos, yPos);
                    var endY = Math.Max(prevYPos, yPos);
                    
                    for (int y = startY; y <= endY; y++)
                    {
                        if (chart[y, i - 1] == ' ')
                            chart[y, i - 1] = '│';
                        if (chart[y, i] == ' ')
                            chart[y, i] = '│';
                    }
                }
            }

            // Display chart
            Console.WriteLine($"Max: ${maxValue:F2}");
            for (int i = 0; i < height; i++)
            {
                Console.Write("│");
                for (int j = 0; j < width; j++)
                {
                    Console.Write(chart[i, j]);
                }
                Console.WriteLine("│");
            }
            Console.WriteLine("└" + new string('─', width) + "┘");
            Console.WriteLine($"Min: ${minValue:F2}");

            // Display labels for key points
            Console.WriteLine("\nData Points:");
            for (int i = 0; i < Math.Min(data.Count, 10); i++)
            {
                Console.WriteLine($"{data[i].Label}: ${data[i].Value:F2}");
            }
        }

        public static void DisplayProgressChart(string title, List<(string Period, decimal Value, decimal Target)> data)
        {
            if (!data.Any())
            {
                ConsoleHelper.WriteWarning("No data to display.");
                return;
            }

            ConsoleHelper.WriteHeader(title);
            
            foreach (var item in data)
            {
                var percentage = item.Target > 0 ? (item.Value / item.Target) * 100 : 0;
                var displayPercentage = Math.Min(100, Math.Max(0, percentage));
                
                Console.WriteLine($"{item.Period.PadRight(12)} ");
                ConsoleHelper.DisplayProgressBar($"${item.Value:F0}/${item.Target:F0}", (double)displayPercentage);
                Console.WriteLine();
            }
        }

        public static void DisplayPieChart(string title, Dictionary<string, decimal> data)
        {
            if (!data.Any())
            {
                ConsoleHelper.WriteWarning("No data to display.");
                return;
            }

            ConsoleHelper.WriteHeader(title);
            
            var total = data.Values.Sum();
            var sortedData = data.OrderByDescending(kvp => kvp.Value).ToList();
            
            foreach (var item in sortedData)
            {
                var percentage = total > 0 ? (item.Value / total) * 100 : 0;
                var barLength = (int)(percentage / 2); // Scale down for display
                var bar = new string('█', barLength);
                
                Console.WriteLine($"{item.Key.PadRight(15)} │{bar.PadRight(50)}│ {percentage:F1}% (${item.Value:F2})");
            }
        }

        public static void DisplayTrendIndicator(decimal currentValue, decimal previousValue, string label = "")
        {
            var change = currentValue - previousValue;
            var changePercent = previousValue != 0 ? (change / previousValue) * 100 : 0;
            
            string indicator;
            ConsoleColor color;
            
            if (change > 0)
            {
                indicator = "▲";
                color = ConsoleColor.Green;
            }
            else if (change < 0)
            {
                indicator = "▼";
                color = ConsoleColor.Red;
            }
            else
            {
                indicator = "◄►";
                color = ConsoleColor.Yellow;
            }

            Console.ForegroundColor = color;
            Console.Write($"{indicator} ");
            Console.ResetColor();
            
            Console.Write($"{label} ${currentValue:F2} ");
            
            Console.ForegroundColor = color;
            Console.WriteLine($"({changePercent:+0.0;-0.0;0.0}%)");
            Console.ResetColor();
        }

        public static void DisplaySparkline(List<decimal> values, int width = 30)
        {
            if (!values.Any())
                return;

            var min = values.Min();
            var max = values.Max();
            var range = max - min;

            if (range == 0)
            {
                Console.WriteLine(new string('─', width));
                return;
            }

            var sparkChars = new[] { ' ', '▁', '▂', '▃', '▄', '▅', '▆', '▇', '█' };
            
            foreach (var value in values.Take(width))
            {
                var normalized = (value - min) / range;
                var charIndex = (int)(normalized * (sparkChars.Length - 1));
                charIndex = Math.Max(0, Math.Min(sparkChars.Length - 1, charIndex));
                Console.Write(sparkChars[charIndex]);
            }
            Console.WriteLine();
        }

        public static void DisplayCorrelationMatrix(Dictionary<string, Dictionary<string, double>> correlations)
        {
            var metrics = correlations.Keys.ToList();
            
            ConsoleHelper.WriteHeader("Correlation Matrix");
            
            // Header
            Console.Write("".PadRight(15));
            foreach (var metric in metrics)
            {
                Console.Write(metric.Substring(0, Math.Min(8, metric.Length)).PadRight(10));
            }
            Console.WriteLine();
            
            // Matrix
            foreach (var row in metrics)
            {
                Console.Write(row.Substring(0, Math.Min(14, row.Length)).PadRight(15));
                
                foreach (var col in metrics)
                {
                    var correlation = correlations.ContainsKey(row) && correlations[row].ContainsKey(col) 
                        ? correlations[row][col] 
                        : 0.0;
                    
                    var color = correlation switch
                    {
                        > 0.7 => ConsoleColor.Green,
                        > 0.3 => ConsoleColor.Yellow,
                        < -0.3 => ConsoleColor.Red,
                        _ => ConsoleColor.White
                    };
                    
                    Console.ForegroundColor = color;
                    Console.Write($"{correlation:F2}".PadRight(10));
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
        }
    }
}